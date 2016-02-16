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
    public partial class FusionDropBox : Window
    {
        public FusionDropResult DropResult { get; private set; }

        public FusionDropBox()
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

        private void buttonSEM_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            DropResult = FusionDropResult.SEM;
            this.Close();
        }

        private void buttonSIMS_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            DropResult = FusionDropResult.SIMS;
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            DropResult = FusionDropResult.Cancel;
            this.Close();
        }
    }
    public enum FusionDropResult { SEM, SIMS, Cancel }
}
