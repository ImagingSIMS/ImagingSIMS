using System;
using System.Windows;
using System.Xml;

namespace ImagingSIMS.Common.Dialogs
{
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        Exception reportedException;

        public ExceptionWindow()
        {
        }
        public ExceptionWindow(Exception Exception)
        {
            InitializeComponent();

            Loaded += ExceptionWindow_Loaded;
            SizeChanged += ExceptionWindow_SizeChanged;

            this.Title = "Unhandled Exception";
            reportedException = Exception;

            CreateText(Exception);
        }

        void ExceptionWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void ExceptionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        private void buttonCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textBoxOutput.Text, TextDataFormat.UnicodeText);
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void CreateText(Exception ex)
        {
            msg1.Text = "The program has encountered an exception and needs to close.\n\n" +
                "Before closing, the program will attempt to autosave the current workspace. If successful, you can load it on the next startup.";
            msg2.Text = ex.Message;
            
            string path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"ImagingSIMS\exceptions");
            if(!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string fileName = System.IO.Path.Combine(path, DateTime.Now.ToFileTime().ToString() + ".xml");
            
            string outputText = "";

            using (XmlWriter xWriter = XmlWriter.Create(fileName))
            {
                xWriter.WriteStartDocument();
                xWriter.WriteStartElement("UnhandledException");
                outputText += "Unhandled Exception\n";
                outputText += "------------------------------------------\n";

                xWriter.WriteStartElement("Information");
                outputText += "Information\n";
                outputText += "\n";

                string date = DateTime.Now.ToShortDateString();
                string time = DateTime.Now.ToShortTimeString();
                xWriter.WriteElementString("EventTime", string.Format("Date: {0} Time: {1}", date, time));
                outputText += string.Format("Event Date: {0} Time: {1}\n", date, time);
                outputText += "\n";

                bool architecture = Environment.Is64BitOperatingSystem;
                bool procArchitect = Environment.Is64BitProcess;
                OperatingSystem os = Environment.OSVersion;
                Version clrVersion = Environment.Version;
                xWriter.WriteStartElement("SystemInformation");
                xWriter.WriteElementString("Windows_version", os.ToString());
                outputText += "Windows Version: " + os.ToString() +"\n";
                xWriter.WriteElementString("CLR_version", clrVersion.ToString());
                outputText += "CLR Version: " + clrVersion.ToString() + "\n";
                xWriter.WriteElementString("Is_64-bit_architecture", architecture.ToString());
                outputText += "Is 64-bit: " + architecture.ToString() + "\n";
                xWriter.WriteElementString("Is_64-bit_process", procArchitect.ToString());
                outputText += "Is 64-bit process: " + procArchitect.ToString() +"\n";
                xWriter.WriteEndElement();
                outputText += "\n";

                xWriter.WriteEndElement();

                xWriter.WriteStartElement("Exception");
                outputText += "Primary Exception\n";
                outputText += "----------------------------------------------------\n";
                outputText += "\n";
                xWriter.WriteElementString("Type", ex.GetType().Name);
                outputText += "Type: " + ex.GetType().ToString() + "\n";
                xWriter.WriteElementString("Source", ex.Source);
                outputText += "Source: " + ex.Source + "\n";
                xWriter.WriteElementString("Message", ex.Message);
                outputText += ex.Message + "\n";
                outputText += "\n";
                xWriter.WriteStartElement("StackTrace");
                outputText += "Stack:\n";
                string[] lines = ex.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                int i = lines.Length;
                foreach (string line in lines)
                {
                    xWriter.WriteElementString("Line_" + i.ToString(), line.Remove(0, 5));
                    outputText += "Line " + i.ToString() + "->" + line.Remove(0, 5) + "\n";
                    i--;
                }
                xWriter.WriteEndElement();
                xWriter.WriteElementString("HasInnerException", (ex.InnerException != null).ToString());
                xWriter.WriteEndElement();

                if (ex.InnerException != null)
                {
                    xWriter.WriteStartElement("Exception");
                    outputText += "Inner Exception\n";
                    outputText += "----------------------------------------------------\n";
                    outputText += "\n";
                    xWriter.WriteElementString("Type", ex.GetType().Name);
                    outputText += "Type: " + ex.GetType().ToString() + "\n";
                    xWriter.WriteElementString("Source", ex.InnerException.Source);
                    outputText += "Source: " + ex.Source + "\n";
                    xWriter.WriteElementString("Message", ex.InnerException.Message);
                    outputText += ex.Message + "\n";
                    outputText += "\n";
                    xWriter.WriteStartElement("StackTrace");
                    outputText += "Stack:\n";
                    if (ex.InnerException.StackTrace != null)
                    {
                        string[] linesinner = ex.InnerException.StackTrace.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                        int iinner = linesinner.Length;
                        foreach (string line in linesinner)
                        {
                            xWriter.WriteElementString("Line_" + iinner.ToString(), line.Remove(0,5));
                            outputText += "Line " + iinner.ToString() + "->" + line.Remove(0, 5) + "\n";
                            iinner--;
                        }
                    }
                    xWriter.WriteEndElement();
                    xWriter.WriteEndElement();
                }

                xWriter.WriteEndElement();
                xWriter.WriteEndDocument();

                outputText += "\n";
                outputText += "----------------------------------------------------\n";
                outputText += "End of exception information dump\n";
                outputText += "\n";
                outputText += @"A crash dump can be found in the current user's AppData\Roaming folder.";
            }
            textBoxOutput.Text = outputText;
        }
    }
}
