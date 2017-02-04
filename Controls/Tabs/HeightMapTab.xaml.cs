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
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for HeightMapTab.xaml
    /// </summary>
    public partial class HeightMapTab : UserControl, IDroppableTab
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
            HeightData = ImageGenerator.Instance.ConvertToData2D(bs);
        }
        public void SetColor(BitmapSource bs)
        {
            ColorDataImageSource = bs;
            ColorData = ImageGenerator.Instance.ConvertToData3D(bs);
        }

        public void HandleDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                var bs = e.Data.GetData(DataFormats.Bitmap) as BitmapSource;
                if (bs == null) return;

                var hmdb = new HeightMapDropBox();
                if (hmdb.ShowDialog() != true) return;

                HeightMapDropResult dropResult = hmdb.DropResult;
                if (dropResult == HeightMapDropResult.Height)
                {
                    HeightDataImageSource = bs;
                    HeightData = ImageGenerator.Instance.ConvertToData2D(bs);
                }
                else if (dropResult == HeightMapDropResult.Color)
                {
                    ColorDataImageSource = bs;
                    ColorData = ImageGenerator.Instance.ConvertToData3D(bs);
                }

                e.Handled = true;
            }
            else if (e.Data.GetDataPresent("BitmapSource"))
            {
                var bs = e.Data.GetData("BitmapSource") as BitmapSource;
                if (bs == null) return;

                var hmdb = new HeightMapDropBox();
                if (hmdb.ShowDialog() != true) return;

                HeightMapDropResult dropResult = hmdb.DropResult;
                if (dropResult == HeightMapDropResult.Height)
                {
                    HeightDataImageSource = bs;
                    HeightData = ImageGenerator.Instance.ConvertToData2D(bs);
                }
                else if (dropResult == HeightMapDropResult.Color)
                {
                    ColorDataImageSource = bs;
                    ColorData = ImageGenerator.Instance.ConvertToData3D(bs);
                }

                e.Handled = true;
            }

            else if (e.Data.GetDataPresent("DisplayImage"))
            {
                var image = e.Data.GetData("DisplayImage") as DisplayImage;
                if (image == null) return;

                var bs = image.Source as BitmapSource;
                if (bs == null) return;

                HeightMapDropBox hmdb = new HeightMapDropBox();
                if (hmdb.ShowDialog() != true) return;

                HeightMapDropResult dropResult = hmdb.DropResult;
                if (dropResult == HeightMapDropResult.Height)
                {
                    HeightDataImageSource = bs;
                    HeightData = ImageGenerator.Instance.ConvertToData2D(bs);
                }
                else if (dropResult == HeightMapDropResult.Color)
                {
                    ColorDataImageSource = bs;
                    ColorData = ImageGenerator.Instance.ConvertToData3D(bs);
                }

                e.Handled = true;
            }

            else if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Handled = false;
            }
        }
    }
}
