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
    /// Interaction logic for HighlightableImage.xaml
    /// </summary>
    public partial class HighlightableImage : UserControl
    {
        public static readonly DependencyProperty BrushSizeProperty = DependencyProperty.Register("BrushSize",
            typeof(int), typeof(HighlightableImage), new FrameworkPropertyMetadata(1));
        public static readonly DependencyProperty Data2DSourceProperty = DependencyProperty.Register("Data2DSource",
            typeof(Data2D), typeof(HighlightableImage), new FrameworkPropertyMetadata(new Data2D(1, 1, 0),
                FrameworkPropertyMetadataOptions.None, updateDataSource));
        public static readonly DependencyProperty ColorScaleProperty = DependencyProperty.Register("ColorScale",
            typeof(ColorScaleTypes), typeof(HighlightableImage), new FrameworkPropertyMetadata(ColorScaleTypes.ThermalWarm, 
                FrameworkPropertyMetadataOptions.None, updateImage));
        public static readonly DependencyProperty SelectionColorProperty = DependencyProperty.Register("SelectionColor",
            typeof(Color), typeof(HighlightableImage), new FrameworkPropertyMetadata(Color.FromArgb(255, 0, 255, 0),
                FrameworkPropertyMetadataOptions.None, updateImage));
        public static readonly DependencyProperty DisplayImageSource = DependencyProperty.Register("DisplayImage",
            typeof(BitmapSource), typeof(HighlightableImage));
        public static readonly DependencyProperty DisplayTextLine1Property = DependencyProperty.Register("DisplayTextLine1",
            typeof(string), typeof(HighlightableImage), new FrameworkPropertyMetadata(""));
        public static readonly DependencyProperty DisplayTextLine2Property = DependencyProperty.Register("DisplayTextLine2",
            typeof(string), typeof(HighlightableImage), new FrameworkPropertyMetadata(""));

        public int BrushSize
        {
            get { return (int)GetValue(BrushSizeProperty); }
            set { SetValue(BrushSizeProperty, value); }
        }
        public Data2D Data2DSource
        {
            get { return (Data2D)GetValue(Data2DSourceProperty); }
            set { SetValue(Data2DSourceProperty, value); }
        }
        public ColorScaleTypes ColorScale
        {
            get { return (ColorScaleTypes)GetValue(ColorScaleProperty); }
            set { SetValue(ColorScaleProperty, value); }
        }
        public Color SelectionColor
        {
            get { return (Color)GetValue(SelectionColorProperty); }
            set { SetValue(SelectionColorProperty, value); }
        }
        public BitmapSource DisplayImage
        {
            get { return (BitmapSource)GetValue(DisplayImageSource); }
            set { SetValue(DisplayImageSource, value); }
        }
        public string DisplayTextLine1
        {
            get { return (string)GetValue(DisplayTextLine1Property); }
            set { SetValue(DisplayTextLine1Property, value); }
        }
        public string DisplayTextLine2
        {
            get { return (string)GetValue(DisplayTextLine2Property); }
            set { SetValue(DisplayTextLine2Property, value); }
        }

        public HighlightableImage()
        {
            InitializeComponent();
        }

        public static void updateImage(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            HighlightableImage source = obj as HighlightableImage;
            if (source == null) return;

            source.updateImage();
        }
        public static void updateDataSource(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            HighlightableImage source = obj as HighlightableImage;
            if (source == null) return;

            Data2D d = source.Data2DSource;
            int width = d.Width;
            int height = d.Height;            

            if (source._highlightMask == null || width != source._highlightMask.GetLength(0)
                    || height != source._highlightMask.GetLength(1))
            {
                source._highlightMask = new bool[width, height];
            }

            source.updateImage();
        }
        public void updateImage()
        {
            Data2D d = Data2DSource;
            int width = d.Width;
            int height = d.Height;

            Data3D colorChannel = new Data3D(width, height, 4);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (_highlightMask[x, y])
                    {
                        colorChannel[x, y] = SelectionColor.ToFloatArray();
                    }
                    else
                    {
                        colorChannel[x, y] = ColorScales.FromScale(
                            Data2DSource[x, y], Data2DSource.Maximum,
                            ColorScale).ToFloatArray();
                    }
                }
            }

            DisplayImage = ImageGenerator.Instance.Create(colorChannel);
        }

        private void buttonShowColor_MouseEnter(object sender, RoutedEventArgs e)
        {
            popupSolidColorScale.IsOpen = true;
        }

        #region Highlighting
        bool _isHighlighting;
        bool _isUnHighlighting;

        bool[,] _highlightMask;

        private void imageHost_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isHighlighting = true;
            _isUnHighlighting = false;
        }
        private void imageHost_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isHighlighting = false;
        }
        private void imageHost_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isHighlighting = false;
            _isUnHighlighting = true;
        }
        private void imageHost_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isUnHighlighting = false;
        }
        private void imageHost_MouseMove(object sender, MouseEventArgs e)
        {
            int xPixel = 0;
            int yPixel = 0;

            Data2D d = Data2DSource;
            if (d != null)
            {
                int width = d.Width;
                int height = d.Height;

                xPixel = (int)(e.GetPosition(imageHost).X * width / imageHost.ActualWidth);
                yPixel = (int)(e.GetPosition(imageHost).Y * height / imageHost.ActualHeight);

                if (_isHighlighting)
                {
                    if (xPixel < 0 || xPixel >= d.Width || yPixel < 0 || yPixel >= d.Height) return;

                    DisplayTextLine2 = string.Format("Highlighting X:{0} Y:{1}", xPixel, yPixel);
                    highlight(xPixel, yPixel, BrushSize);

                    updateImage();
                }

                else if (_isUnHighlighting)
                {
                    if (xPixel < 0 || xPixel >= d.Width || yPixel < 0 || yPixel >= d.Height) return;

                    DisplayTextLine2 = string.Format("Unhighlighting X:{0} Y:{1}", xPixel, yPixel);
                    unHighlight(xPixel, yPixel, BrushSize);

                    updateImage();
                }
            }

            DisplayTextLine2 = string.Format("Data Position X:{0} Y:{1}\nControl Position X:{2} Y:{3}",
                xPixel, yPixel, e.GetPosition(imageHost).X, e.GetPosition(imageHost).Y);            
        }

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            int width = _highlightMask.GetLength(0);
            int height = _highlightMask.GetLength(1);
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _highlightMask[x, y] = false;
                }    
            }

            updateImage();
        }        
        private void highlight(int x, int y, int brushSize = 1)
        {
            for (int a = 0; a < BrushSize; a++)
            {
                for (int b = 0; b < BrushSize; b++)
                {
                    int xIndex = (x - (BrushSize / 2)) + a;
                    int yIndex = (y - (BrushSize / 2)) + b;

                    if (xIndex < 0 || xIndex >= Data2DSource.Width ||
                        yIndex < 0 || yIndex >= Data2DSource.Height) continue;

                    _highlightMask[xIndex, yIndex] = true;
                }
            }
        }
        private void unHighlight(int x, int y, int brushSize = 1)
        {
            for (int a = 0; a < BrushSize; a++)
            {
                for (int b = 0; b < BrushSize; b++)
                {
                    int xIndex = (x - (BrushSize / 2)) + a;
                    int yIndex = (y - (BrushSize / 2)) + b;

                    if (xIndex < 0 || xIndex >= Data2DSource.Width ||
                        yIndex < 0 || yIndex >= Data2DSource.Height) continue;

                    _highlightMask[xIndex, yIndex] = false;
                }
            }
        }
        #endregion
    }
}
