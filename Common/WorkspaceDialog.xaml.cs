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
    }
    public enum WorkspaceResult { Merge, Overwrite, Cancel }
}
