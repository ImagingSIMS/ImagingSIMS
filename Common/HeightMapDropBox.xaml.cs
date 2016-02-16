using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for HeightMapDropBox.xaml
    /// </summary>
    public partial class HeightMapDropBox : Window
    {
        public HeightMapDropResult DropResult { get; private set; }

        public HeightMapDropBox()
        {
            InitializeComponent();

            Loaded += HeightMapDropBox_Loaded;
            SizeChanged += HeightMapDropBox_SizeChanged;
        }

        void HeightMapDropBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void HeightMapDropBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        private void buttonHeight_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            DropResult = HeightMapDropResult.Height;
            this.Close();
        }

        private void buttonColor_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            DropResult = HeightMapDropResult.Color;
            this.Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            DropResult = HeightMapDropResult.Cancel;
            this.Close();
        }
    }
    public enum HeightMapDropResult { Height, Color, Cancel };
}
