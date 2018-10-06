using Flurl.Http;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HtmlAgilityPack;
using LinkDownLoaderGUI.Model;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
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

        public ICommand GetLinksCommand { get; private set; }

        public MainViewModel()
        {
            DownloadOptions = new DownloadOptions
            {
                ThreadCount = 1
            };
            GetLinksCommand = new RelayCommand(() => DownloadFilesAsync(DownloadOptions));
        }

        private async void DownloadFilesAsync(DownloadOptions options)
        {
            var list = await GetFileLinksAsync(options);
            DowloadOneFileAsync(list[0]);

        }

        private async void DowloadOneFileAsync(FileLink fileLink)
        {
            var path = await fileLink.LinkName.DownloadFileAsync(DownloadOptions.DownloadDirectory, fileLink.OutputName);
            // увеличить progressBar
        }

        private async Task<List<FileLink>> GetFileLinksAsync(DownloadOptions options)
        {
            HtmlWeb hw = new HtmlWeb();
            var doc = await hw.LoadFromWebAsync(options.HttpAddress);

            var result = new List<FileLink>();
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                var fileName = Path.GetFileName(link.Attributes["href"].Value);
                if (FitsMask(fileName, options.Mask))
                    result.Add( new FileLink
                    {
                        LinkName = link.Attributes["href"].Value,
                        OutputName = fileName
                    });
            }
            return result;
        }

        private bool FitsMask(string sFileName, string sFileMask)
        {
            Regex mask = new Regex(sFileMask.Replace(".", "[.]").Replace("*", ".*").Replace("?", "."));
            return mask.IsMatch(sFileName);
        }


    }
}