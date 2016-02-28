using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for ColorScalePreview.xaml
    /// </summary>
    public partial class ColorScalePreview : UserControl
    {
        public ColorScalePreview()
        {
            InitializeComponent();

            CreateScaleSamples();
        }

        private void CreateScaleSamples()
        {
            Data2D d = new Data2D(200, 35);
            for (int x = 0; x < 200; x++)
            {
                for (int y = 0; y < 35; y++)
                {
                    d[x, y] = x;
                }
            }

            previewScaleThermalWarm.Source = ImageHelper.CreateColorScaleImage(d, ColorScaleTypes.ThermalWarm);
            previewScaleThermalCold.Source = ImageHelper.CreateColorScaleImage(d, ColorScaleTypes.ThermalCold);
            previewScaleNeon.Source = ImageHelper.CreateColorScaleImage(d, ColorScaleTypes.Neon);
            previewScaleRetro.Source = ImageHelper.CreateColorScaleImage(d, ColorScaleTypes.Retro);
        }

        private void menuItemWarm_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource imageSource = previewScaleThermalWarm.Source as BitmapSource;
            if (imageSource == null) return;

            Clipboard.SetImage(imageSource);
        }
        private void menuItemCold_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource imageSource = previewScaleThermalCold.Source as BitmapSource;
            if (imageSource == null) return;

            Clipboard.SetImage(imageSource);
        }
        private void menuItemNeon_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource imageSource = previewScaleNeon.Source as BitmapSource;
            if (imageSource == null) return;

            Clipboard.SetImage(imageSource);
        }
        private void menuItemRetro_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource imageSource = previewScaleRetro.Source as BitmapSource;
            if (imageSource == null) return;

            Clipboard.SetImage(imageSource);
        }
    }
}
