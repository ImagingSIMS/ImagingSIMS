using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

using ImagingSIMS.Common;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

using Microsoft.Win32;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for Data2DDisplay.xaml
    /// </summary>
    public partial class Data2DDisplay : UserControl
    {
        public static readonly DependencyProperty DisplayItemProperty = DependencyProperty.Register("DisplayItem",
            typeof(Data2DDisplayItem), typeof(Data2DDisplay));

        public event RoutedEventHandler GeneratePixelStats
        {
            add { AddHandler(GeneratePixelsStatsEvent, value); }
            remove { RemoveHandler(GeneratePixelsStatsEvent, value); }
        }
        public static readonly RoutedEvent GeneratePixelsStatsEvent = EventManager.RegisterRoutedEvent("PixelStatsGenerated",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Data2DDisplay));

        public event RoutedEventHandler RemoveItemClick
        {
            add { AddHandler(RemoveItemClickEvent, value); }
            remove { RemoveHandler(RemoveItemClickEvent, value); }
        }
        public static readonly RoutedEvent RemoveItemClickEvent = EventManager.RegisterRoutedEvent("RemoveItemClick",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Data2DDisplay));

        public Data2DDisplayItem DisplayItem
        {
            get { return (Data2DDisplayItem)GetValue(DisplayItemProperty); }
            set { SetValue(DisplayItemProperty, value); }
        }

        public Data2DDisplay()
        {
            InitializeComponent();
        }

        private void buttonShowColor_MouseEnter(object sender, RoutedEventArgs e)
        {
            popupSolidColorScale.IsOpen = true;
        }
        private void buttonPixelStats_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            RaiseEvent(new RoutedEventArgs(GeneratePixelsStatsEvent, this));
        }

        #region ScrollViewer
        Point? lastCenterPositionOnTarget;
        Point? lastMousePositionOnTarget;
        Point? lastDragPoint;

        double scale;

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
            {
                Point? targetBefore = null;
                Point? targetNow = null;

                if (!lastMousePositionOnTarget.HasValue)
                {
                    if (lastCenterPositionOnTarget.HasValue)
                    {
                        var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
                        Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, image);

                        targetBefore = lastCenterPositionOnTarget;
                        targetNow = centerOfTargetNow;
                    }
                }
                else
                {
                    targetBefore = lastMousePositionOnTarget;
                    targetNow = Mouse.GetPosition(image);

                    lastMousePositionOnTarget = null;
                }

                if (targetBefore.HasValue)
                {
                    double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
                    double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

                    double multiplicatorX = e.ExtentWidth / image.Width;
                    double multiplicatorY = e.ExtentHeight / image.Height;

                    double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
                    double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

                    if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
                    {
                        return;
                    }

                    scrollViewer.ScrollToHorizontalOffset(newOffsetX);
                    scrollViewer.ScrollToVerticalOffset(newOffsetY);
                }
            }
        }
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            if (scrollViewer == null) return;

            lastMousePositionOnTarget = Mouse.GetPosition(image);

            double originalWidth = image.ActualWidth / scale;
            double originalHeight = image.ActualHeight / scale;

            if (e.Delta > 0)
            {
                scale += 0.25f;
            }
            if (e.Delta < 0)
            {
                scale -= 0.25f;
            }

            if (scale < 1) scale = 1;

            DisplayItem.ImageTransformedWidth = originalWidth * scale;
            DisplayItem.ImageTransformedHeight = originalHeight * scale;

            var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
            lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, image);
        }
        private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void ScrollViewer_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePos = e.GetPosition(scrollViewer);
            if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
            {
                scrollViewer.Cursor = Cursors.SizeAll;
                lastDragPoint = mousePos;
                Mouse.Capture(scrollViewer);
            }
        }
        private void ScrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void ScrollViewer_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            scrollViewer.Cursor = Cursors.Arrow;
            scrollViewer.ReleaseMouseCapture();
            lastDragPoint = null;

            e.Handled = true;
        }
        private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DisplayImage di = new DisplayImage(DisplayItem.DisplayImageSource, DisplayItem.DataSource.DataName);
                DataObject obj = new DataObject("DisplayImage", di);
                DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy);
                return;
            }
            if (lastDragPoint.HasValue)
            {
                Point posNow = e.GetPosition(scrollViewer);

                double dX = posNow.X - lastDragPoint.Value.X;
                double dY = posNow.Y - lastDragPoint.Value.Y;

                lastDragPoint = posNow;

                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
            }
        }
        #endregion

        private void scrollViewer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void cmCopyScale_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = imageColorScale.Source as BitmapSource;
            if (source == null) return;

            Clipboard.SetImage(source);

            // ContextMenu doesn't have logical ClosableTabItem parent so this won't send the status message
            // and result in an exception
            //ClosableTabItem.SendStatusUpdate(this, "Color scale copied to clipboard.");
        }
        private void cmCopyImage_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source = DisplayItem.DisplayImageSource as BitmapSource;
            if (source == null) return;

            Clipboard.SetImage(source);

            // ContextMenu doesn't have logical ClosableTabItem parent so this won't send the status message
            // and result in an exception
            //ClosableTabItem.SendStatusUpdate(this, "Image copied to clipboard.");
        }

        private void contentButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            RaiseEvent(new RoutedEventArgs(RemoveItemClickEvent, this));
        }
        private void contentButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";

            if (sfd.ShowDialog() != true) return;

            BitmapSource source = DisplayItem.DisplayImageSource as BitmapSource;
            if (source == null) return;

            source.Save(sfd.FileName);
        }
        private void contentButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource bs = image.Source as BitmapSource;
            if (bs == null) throw new ArgumentException("Invalid ImageSource");

            Clipboard.SetImage(bs);
        }
    }

    public class Data2DDisplayItem : INotifyPropertyChanged
    {
        Data2D _dataSource;
        ImageSource _displayImageSource;
        double _saturation;
        ColorScaleTypes _colorScale;
        Color _solidColorScale;
        double _imageTransformedWidth;
        double _imageTransformedHeight;

        public Data2D DataSource
        {
            get { return _dataSource; }
            set
            {
                if (_dataSource != value)
                {
                    _dataSource = value;
                    NotifyPropertyChanged("DataSource");
                    Redraw();
                }
            }
        }
        public ImageSource DisplayImageSource
        {
            get { return _displayImageSource; }
            set
            {
                if (_displayImageSource != value)
                {
                    _displayImageSource = value;
                    NotifyPropertyChanged("DisplayImageSource");
                }
            }
        }
        public double Saturation
        {
            get { return _saturation; }
            set
            {
                if (_saturation != value)
                {
                    _saturation = value;
                    NotifyPropertyChanged("Saturation");
                    Redraw();
                }
            }
        }
        public ColorScaleTypes ColorScale
        {
            get { return _colorScale; }
            set
            {
                if (_colorScale != value)
                {
                    _colorScale = value;
                    NotifyPropertyChanged("ColorScale");
                    Redraw();
                }
            }
        }
        public Color SolidColorScale
        {
            get { return _solidColorScale; }
            set
            {
                if (_solidColorScale != value)
                {
                    _solidColorScale = value;
                    NotifyPropertyChanged("SolidColorScale");
                    Redraw();
                }
            }
        }
        public double ImageTransformedWidth
        {
            get { return _imageTransformedWidth; }
            set
            {
                if (_imageTransformedWidth != value)
                {
                    _imageTransformedWidth = value;
                    NotifyPropertyChanged("ImageTransformedWidth");
                }
            }
        }
        public double ImageTransformedHeight
        {
            get { return _imageTransformedHeight; }
            set
            {
                if (_imageTransformedHeight != value)
                {
                    _imageTransformedHeight = value;
                    NotifyPropertyChanged("ImageTransformedHeight");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Data2DDisplayItem(Data2D DataSource, ColorScaleTypes ColorScale)
        {
            this.ImageTransformedHeight = 225d;
            this.ImageTransformedWidth = 225d;

            this.SolidColorScale = Color.FromArgb(255, 255, 255, 255);

            this.DataSource = DataSource;
            this.ColorScale = ColorScale;
            this.Saturation = (int)DataSource.Maximum;
        }
        public Data2DDisplayItem(Data2D DataSource, Color SolidColorScale)
        {
            this.ImageTransformedHeight = 225d;
            this.ImageTransformedWidth = 225d;

            this.ColorScale = ColorScaleTypes.Solid;
            this.SolidColorScale = SolidColorScale;

            this.DataSource = DataSource;
            this.Saturation = (int)DataSource.Maximum;
        }

        private void Redraw()
        {
            if (DataSource == null) return;
            if (ColorScale == ColorScaleTypes.Solid)
            {
                DisplayImageSource = ImageHelper.CreateSolidColorImage(DataSource, (float)Saturation, SolidColorScale);
            }
            else
            {
                DisplayImageSource = ImageHelper.CreateColorScaleImage(DataSource, (float)Saturation, ColorScale);
            }
        }
    }

    public class ColorScaleToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null) return false;
                ColorScaleTypes c = (ColorScaleTypes)value;

                return c == ColorScaleTypes.Solid;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }
}