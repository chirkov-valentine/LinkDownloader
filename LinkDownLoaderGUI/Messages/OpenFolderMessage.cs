using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDownLoaderGUI.Messages
{
    public class OpenFolderMessage
    {
        public string Filter { get; private set; }
        public Action<string> OpenFeedBack { get; private set; }
        public  OpenFolderMessage(string filter, Action<string> openFeedBack)
        {
            Filter = filter;
            OpenFeedBack = openFeedBack;
        }
    }
}
