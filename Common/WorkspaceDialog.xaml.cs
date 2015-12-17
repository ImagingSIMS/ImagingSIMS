using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImagingSIMS.Common.Dialogs
{
    /// <summary>
    /// Interaction logic for FusionDropBox.xaml
    /// </summary>
    public partial class WorkspaceDialog : Window
    {
        public WorkspaceResult WorkspaceResult { get; private set; }

        public WorkspaceDialog()
        {
            InitializeComponent();

            Loaded += FusionDropBox_Loaded;
            SizeChanged += FusionDropBox_SizeChanged;

            displayIcon.ShowBorder(false);
            displayIcon.SetImage(GetDialogIcon(DialogBoxIcon.BlueQuestion));
        }

        void FusionDropBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void FusionDropBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        private void buttonMerge_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            WorkspaceResult = WorkspaceResult.Merge;
            this.Close();
        }

        private void buttonOverwrite_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            WorkspaceResult = WorkspaceResult.Overwrite;
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            WorkspaceResult = WorkspaceResult.Cancel;
            this.Close();
        }

        private Bitmap GetDialogIcon(DialogBoxIcon Type)
        {
            //Uri baseUri = new Uri("pack://application:,,,/Resources/Resources.resx");
            switch (Type)
            {
                case DialogBoxIcon.BlueQuestion:
                    return Properties.Resources.BlueQuestion;
                case DialogBoxIcon.Bubble:
                    return Properties.Resources.Bubble;
                case DialogBoxIcon.CyanCheck:
                    return Properties.Resources.CyanCheck;
                case DialogBoxIcon.GreenCheck:
                    return Properties.Resources.GreenCheck;
                case DialogBoxIcon.Information:
                    return Properties.Resources.Information;
                case DialogBoxIcon.RedQuestion:
                    return Properties.Resources.RedQuestion;
                case DialogBoxIcon.Stop:
                    return Properties.Resources.Stop;
                case DialogBoxIcon.Warning:
                    return Properties.Resources.Warning;
                default:
                    return Properties.Resources.Information;
            }
        }
    }
    public enum WorkspaceResult { Merge, Overwrite, Cancel }
}
