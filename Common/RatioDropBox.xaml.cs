using System;
using System.Drawing;
using System.Windows;

namespace ImagingSIMS.Common.Dialogs
{
    /// <summary>
    /// Interaction logic for RatioDropBox.xaml
    /// </summary>
    public partial class RatioDropBox : Window
    {
        public RatioDropResult DropResult { get; private set; }

        public RatioDropBox()
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

        private void buttonNum_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            DropResult = RatioDropResult.Numerator;
            this.Close();
        }

        private void buttonDen_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            DropResult = RatioDropResult.Denominator;
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            DropResult = RatioDropResult.Cancel;
            this.Close();
        }

    }
    public enum RatioDropResult { Numerator, Denominator, Cancel }
}
