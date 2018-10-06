using GalaSoft.MvvmLight;

namespace LinkDownLoaderGUI.Model
{
    public class DownloadOptions : ObservableObject
    {
        private string _httpAddress;
        private string _downloadDirectory;
        private string _mask;
        private int _threadCount;

        public string HttpAddress
        {
            get => _httpAddress;
            set => Set<string>(() => this.HttpAddress, ref _httpAddress, value);
        }

        public string DownloadDirectory
        {
            get => _downloadDirectory;
            set => Set<string>(() => this.DownloadDirectory, ref _downloadDirectory, value);
        }

        public string Mask
        {
            get => _mask;
            set => Set<string>(() => this.Mask, ref _mask, value);
        }

        public int ThreadCount
        {
            get => _threadCount;
            set => Set<int>(() => this.ThreadCount, ref _threadCount, value);
        }
    }
}
