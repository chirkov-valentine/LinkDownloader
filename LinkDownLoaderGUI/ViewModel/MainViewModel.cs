using GalaSoft.MvvmLight;
using LinkDownLoaderGUI.Model;

namespace LinkDownLoaderGUI.ViewModel
{

    public class MainViewModel : ViewModelBase
    {
        private DownloadOptions _downloadOptions;

        public DownloadOptions DownloadOptions
        {
            get => _downloadOptions;
            set => Set<DownloadOptions>(() => this.DownloadOptions, ref _downloadOptions, value);
        }
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }
    }
}