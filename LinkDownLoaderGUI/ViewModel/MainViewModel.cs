using Flurl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using HtmlAgilityPack;
using LinkDownLoaderGUI.Messages;
using LinkDownLoaderGUI.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LinkDownLoaderGUI.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        private DownloadOptions _downloadOptions;
        private string _logText;
        private int _filesProceed;
        private int _filesCount;
        private ConcurrentQueue<FileLink> _fileLinks;
        private string _remainFileCount;
        private bool _started;

        private CancellationTokenSource _cts;
        private CancellationToken _token;

        public DownloadOptions DownloadOptions
        {
            get => _downloadOptions;
            set => Set<DownloadOptions>(() => this.DownloadOptions, ref _downloadOptions, value);
        }

        public string LogText
        {
            get => _logText;
            set => Set<string>(() => this.LogText, ref _logText, value);
        }

        public int FilesProceed
        {
            get => _filesProceed;
            set => Set<int>(() => this.FilesProceed, ref _filesProceed, value);
        }

        public int FilesCount
        {
            get => _filesCount;
            set => Set<int>(() => this.FilesCount, ref _filesCount, value);
        }

        public string RemainFilesCount
        {
            get => _remainFileCount;
            set => Set<string>(() => this.RemainFilesCount, ref _remainFileCount, value);
        }

        public bool Started
        {
            get => _started;
            set
            {
                Set<bool>(() => this.Started, ref _started, value);
                GetLinksCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand GetLinksCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
        public RelayCommand ShowOpenFolderDialogCommand { get; set; }

        public MainViewModel()
        {
            DownloadOptions = new DownloadOptions
            {
                HttpAddress = @"http://www.krishnapath.org/prabhupada-audio/",
                DownloadDirectory = @"I:\Download",
                Mask = @"*.zip",
                ThreadCount = 5
            };
            _fileLinks = new ConcurrentQueue<FileLink>();
            GetLinksCommand = new RelayCommand(
                () => DownloadFilesAsync(DownloadOptions),
                () => !Started,
                true);
            CancelCommand = new RelayCommand(() => CancelDownload(),
                () => Started,
                true);
            ShowOpenFolderDialogCommand = new RelayCommand(
                () => ShowOpenFolderDialog());

            DispatcherHelper.Initialize();
            Started = false;
        }

        private void CancelDownload()
        {
            _cts.Cancel();
            Started = false;
        }

        private void ShowOpenFolderDialog()
        {
            var message = new OpenFolderMessage(
                true,
                AppDomain.CurrentDomain.BaseDirectory,
                result =>
                {
                    DownloadOptions.DownloadDirectory = result;
                });
            Messenger.Default.Send(message);
        }

        private async void DownloadFilesAsync(DownloadOptions options)
        {
            Started = true;

            _cts = new CancellationTokenSource();
            _token = _cts.Token;

            await GetFileLinksAsync(options);

            if (_fileLinks.IsEmpty)
                return;

            FilesCount = _fileLinks.Count;

            DispatcherHelper.CheckBeginInvokeOnUI(
              () =>
              {
                  RemainFilesCount = string.Concat("Скачано: 0" + " из " + FilesCount, "\n");
              });

            var threadList = new List<Task>();

            for (int i = 0; i < DownloadOptions.ThreadCount && !_fileLinks.IsEmpty; i++)
            {
                if (_token.IsCancellationRequested)
                    break;
                threadList.Add(DowloadOneFileAsync(_token));
            }

            await Task.WhenAny(threadList);

            var t = threadList.FirstOrDefault(p => p.IsCompleted);

            threadList.Remove(t);

            while (!_fileLinks.IsEmpty)
            {
                if (_token.IsCancellationRequested)
                    break;
                threadList.Add(DowloadOneFileAsync(_token));

                await Task.WhenAny(threadList);

                var task = threadList.FirstOrDefault(p => p.IsCompleted);

                threadList.Remove(task);
            }

            await Task.WhenAll(threadList);

            DispatcherHelper.CheckBeginInvokeOnUI(
             () =>
             {
                 RemainFilesCount = string.Concat("Скачано: "+ FilesProceed + " из " + FilesCount, ". Окончание закачки\n");
             });
            Started = false;
        }

        private async Task DowloadOneFileAsync(CancellationToken token)
        {
            _fileLinks.TryDequeue(out FileLink link);
            WebClient wc = new WebClient();

            if (token.IsCancellationRequested)
            {
                LogText = string.Concat(LogText, "Отмена", "\n");
                return;
            }
            /* DispatcherHelper.CheckBeginInvokeOnUI(
                 () =>
                 {
                     LogText = string.Concat(LogText, "Копирование: " + link.OutputName, "\n");
                 });*/
            try
            {
                await wc.DownloadFileTaskAsync(link.LinkName, Path.Combine(link.OutputDirectory, link.OutputName));
            }
            catch (Exception ex)
            {
                DispatcherHelper.CheckBeginInvokeOnUI(
              () =>
              {
                  LogText = string.Concat(LogText, ex.Message, "\n");
              });
            }

            DispatcherHelper.CheckBeginInvokeOnUI(
               () =>
               {
                   LogText = string.Concat(LogText, "Окончание: " + link.OutputName, "\n");
               });
            DispatcherHelper.CheckBeginInvokeOnUI(
               () =>
               {
                   FilesProceed++;
                   RemainFilesCount = string.Concat("Скачано: " + FilesProceed + " из " + FilesCount);
               });
        }

        private async Task GetFileLinksAsync(DownloadOptions options)
        {
            HtmlWeb hw = new HtmlWeb();
            var doc = await hw.LoadFromWebAsync(options.HttpAddress);

            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                var fileName = Path.GetFileName(link.Attributes["href"].Value);
                var linkName = link.Attributes["href"].Value.StartsWith("/")
                    ? Url.Combine(Url.GetRoot(options.HttpAddress), link.Attributes["href"].Value)
                    : link.Attributes["href"].Value;

                if (FitsMask(fileName, options.Mask))
                    _fileLinks.Enqueue(new FileLink
                    {
                        LinkName = linkName,
                        OutputName = fileName,
                        OutputDirectory = options.DownloadDirectory
                    });
            }
        }

        private bool FitsMask(string sFileName, string sFileMask)
        {
            Regex mask = new Regex(sFileMask.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));
            return mask.IsMatch(sFileName);
        }


    }
}