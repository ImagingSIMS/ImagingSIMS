using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Windows;

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Common.Registry;
using ImagingSIMS.Data;

namespace ImagingSIMS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Check if COM Exception
            var comException = e.Exception as System.Runtime.InteropServices.COMException;
            if (comException != null)
            {
                // Handle bug with WPF Clipboard handling
                if (comException.ErrorCode == -2147221040)
                {
                    e.Handled = true;
                    return;
                }
            }


            ExceptionWindow ew = new ExceptionWindow(e.Exception);
            ew.ShowDialog();

            ImagingSIMS.MainApplication.MainWindow mw = Application.Current.MainWindow as ImagingSIMS.MainApplication.MainWindow;
            
            //Only attempt to autosave workspace if it's actually dirty
            if (mw.Workspace.IsDirty)
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"ImagingSIMS\autosave\", DateTime.Now.ToFileTime().ToString() + ".wks");

                if (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }

                RegSettings registry = new RegSettings();
                string result = "";
                if (!registry.SaveCrashSettings(path, out result))
                {
                    DialogBox db = new DialogBox("The program was unable to save the crash settings to thre registry.",
                        "An attempt to save the crash file will be made. If the save is successful, you can find the file at {0}, however the program may not automatically load it on the next startup.",
                        "Autosave", DialogBoxIcon.Stop);
                    db.ShowDialog();
                }

                try
                {
                    mw.Workspace.Save(path);

                    Environment.Exit(-1);
                }
                catch (Exception)
                {
                    DialogBox db = new DialogBox("The program could not finish the autosave.",
                        "This is most likely due to corrupt data or the original cause of the crash.", "Autosave", DialogBoxIcon.Stop);
                    db.ShowDialog();
                    Environment.Exit(-2);
                }
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
        }
    }
}
