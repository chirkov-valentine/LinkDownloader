using System;
using System.IO;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using LinkDownLoaderGUI.Messages;
using System.Windows.Forms;

namespace LinkDownLoaderGUI.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            SimpleIoc.Default.Register<MainViewModel>();
            Messenger.Default.Register<OpenFolderMessage>(this, (message) => OpenMessageFolderMessageHandler(message));
        }

        private void OpenMessageFolderMessageHandler(OpenFolderMessage message)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowNewFolderButton = message.ShowNewFolderButton;
            dialog.SelectedPath = message.StartPath;
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                message.OpenFeedBack?.Invoke(dialog.SelectedPath);
            }
        }

        public MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}