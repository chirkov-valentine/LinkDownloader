using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDownLoaderGUI.Messages
{
    public class OpenFolderMessage
    {
        public bool ShowNewFolderButton { get; private set; }
        public string StartPath { get; set; }
        public Action<string> OpenFeedBack { get; private set; }
        public  OpenFolderMessage(bool showNewFolderButton, string startPath, Action<string> openFeedBack)
        {
            ShowNewFolderButton = showNewFolderButton;
            StartPath = startPath;
            OpenFeedBack = openFeedBack;
        }
    }
}
