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

        private Bitmap GetDialogIcon(DialogBoxIcon Type)
        {
            Uri baseUri = new Uri("pack://application:,,,/Resources/Resources.resx");
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
    public enum RatioDropResult { Numerator, Denominator, Cancel }
}
