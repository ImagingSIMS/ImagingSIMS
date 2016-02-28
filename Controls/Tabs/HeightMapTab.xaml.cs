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

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for HeightMapTab.xaml
    /// </summary>
    public partial class HeightMapTab : UserControl
    {
        public static readonly DependencyProperty HeightDataImageSourceProperty = DependencyProperty.Register("HeightDataImageSource",
            typeof(BitmapSource), typeof(HeightMapTab));
        public static readonly DependencyProperty ColorDataImageSourceProperty = DependencyProperty.Register("ColorDataImageSource",
            typeof(BitmapSource), typeof(HeightMapTab));
        public static readonly DependencyProperty HeightDataProperty = DependencyProperty.Register("HeightData",
            typeof(Data2D), typeof(HeightMapTab));
        public static readonly DependencyProperty ColorDataProperty = DependencyProperty.Register("ColorData",
            typeof(Data3D), typeof(HeightMapTab));

        public BitmapSource HeightDataImageSource
        {
            get { return (BitmapSource)GetValue(HeightDataImageSourceProperty); }
            set { SetValue(HeightDataImageSourceProperty, value); }
        }
        public BitmapSource ColorDataImageSource
        {
            get { return (BitmapSource)GetValue(ColorDataImageSourceProperty); }
            set { SetValue(ColorDataImageSourceProperty, value); }
        }
        public Data2D HeightData
        {
            get { return (Data2D)GetValue(HeightDataProperty); }
            set { SetValue(HeightDataProperty, value); }
        }
        public Data3D ColorData
        {
            get { return (Data3D)GetValue(ColorDataProperty); }
            set { SetValue(ColorDataProperty, value); }
        }

        public HeightMapTab()
        {
            InitializeComponent();
        }

        public void SetHeight(BitmapSource bs)
        {
            HeightDataImageSource = bs;
            HeightData = ImageHelper.ConvertToData2D(bs);
        }
        public void SetColor(BitmapSource bs)
        {
            ColorDataImageSource = bs;
            ColorData = ImageHelper.ConvertToData3D(bs);
        }

    }
}
