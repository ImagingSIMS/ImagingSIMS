using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for CropPreview.xaml
    /// </summary>
    public partial class CropPreview : UserControl
    {
        public static readonly DependencyProperty OriginalImageProperty = DependencyProperty.Register("OriginaImage",
            typeof(BitmapSource), typeof(CropPreview));
        public static readonly DependencyProperty PixelStartXProperty = DependencyProperty.Register("PixelStartX",
            typeof(int), typeof(CropPreview));
        public static readonly DependencyProperty PixelStartYProperty = DependencyProperty.Register("PixelStartY",
            typeof(int), typeof(CropPreview));
        public static readonly DependencyProperty PixelWidthProperty = DependencyProperty.Register("PixelWidth",
            typeof(int), typeof(CropPreview));
        public static readonly DependencyProperty PixelHeightProperty = DependencyProperty.Register("PixelHeight",
            typeof(int), typeof(CropPreview));

        public BitmapSource OriginalImage
        {
            get { return (BitmapSource)GetValue(OriginalImageProperty); }
            set
            {
                SetValue(OriginalImageProperty, value);
                originalImage.Source = OriginalImage;
                Recalculate();
            }
        }
        public int PixelStartX
        {
            get { return (int)GetValue(PixelStartXProperty); }
            set { SetValue(PixelStartXProperty, value); }
        }
        public int PixelStartY
        {
            get { return (int)GetValue(PixelStartYProperty); }
            set { SetValue(PixelStartYProperty, value); }
        }
        public int PixelWidth
        {
            get { return (int)GetValue(PixelWidthProperty); }
            set { SetValue(PixelWidthProperty, value); }
        }
        public int PixelHeight
        {
            get { return (int)GetValue(PixelHeightProperty); }
            set { SetValue(PixelHeightProperty, value); }
        }
        public CropPreview()
        {
            InitializeComponent();

            AddHandler(MoveThumb.ThumbMovedEvent, new RoutedEventHandler(OnThumbMoved));
            AddHandler(ResizeThumb.ThumbResizedEvent, new RoutedEventHandler(OnThumbResized));
        }

        public Control designerItem
        {
            get
            {
                return (Control)contentControl;
            }
        }

        public void AutoCenter()
        {
            double width = this.ActualWidth;
            double height = this.ActualHeight;

            Canvas.SetLeft(designerItem, (width / 2) - (designerItem.Width / 2));
            Canvas.SetTop(designerItem, (height / 2) - (designerItem.Height / 2));

            Recalculate();
        }
        public void AutoHalf()
        {
            double width = this.ActualWidth;
            double height = this.ActualHeight;

            designerItem.Width = width / 2;
            designerItem.Height = height / 2;

            Recalculate();
        }
        protected void OnThumbMoved(object sender, RoutedEventArgs e)
        {
            Recalculate();
        }
        protected void OnThumbResized(object sender, RoutedEventArgs e)
        {
            Recalculate();
        }
        private void Recalculate()
        {
            if (OriginalImage == null) return;

            int sizeX = OriginalImage.PixelWidth;
            int sizeY = OriginalImage.PixelHeight;

            double imgWidth = originalImage.Width;
            double imgHeight = originalImage.Height;

            double rectWidth = designerItem.Width;
            double rectHeight = designerItem.Height;

            int pixelsX = (int)(((double)sizeX) * rectWidth / imgWidth);
            int pixelsY = (int)(((double)sizeY) * rectHeight / imgHeight);

            double rectLeft = Canvas.GetLeft(designerItem);
            double rectTop = Canvas.GetTop(designerItem);

            int startX = (int)(((double)sizeX) * rectLeft / imgWidth);
            int startY = (int)(((double)sizeY) * rectTop / imgHeight);

            PixelStartX = startX;
            PixelStartY = startY;
            PixelWidth = pixelsX;
            PixelHeight = pixelsY;
        }

        private void DockPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DockPanel dp = (DockPanel)sender;
                if (dp == null) return;

                if (dp.Children == null || dp.Children.Count == 0) return;

                Image image = (Image)dp.Children[0];
                if (image == null) return;

                DataObject obj = new DataObject(DataFormats.Bitmap, image.Source);
                DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy);
            }
            else
            {
                base.OnMouseMove(e);
            }
        }
    }

    public class MoveThumb : Thumb
    {
        public static readonly RoutedEvent ThumbMovedEvent = EventManager.RegisterRoutedEvent("ThumbMoved",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MoveThumb));
        public event RoutedEventHandler ThumbMoved
        {
            add { AddHandler(ThumbMovedEvent, value); }
            remove { RemoveHandler(ThumbMovedEvent, value); }
        }

        double _leftPoint;
        double _topPoint;

        public double LeftPoint
        {
            get { return _leftPoint; }
        }
        public double TopPoint
        {
            get { return _topPoint; }
        }

        public Control designerItem
        {
            get
            {
                Control item = this.DataContext as Control;
                return item;
            }
        }

        public MoveThumb()
        {
            DragDelta += MoveThumb_DragDelta;
        }

        void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (designerItem == null) return;

            double left = Canvas.GetLeft(designerItem);
            double top = Canvas.GetTop(designerItem);

            double newLeft = left + e.HorizontalChange;
            double newTop = top + e.VerticalChange;

            if (newLeft >= 0 && newLeft + designerItem.ActualWidth <= 450)
            {
                Canvas.SetLeft(designerItem, newLeft);
                _leftPoint = newLeft;
            }
            if (newTop >= 0 && newTop + designerItem.ActualHeight <= 450)
            {
                Canvas.SetTop(designerItem, newTop);
                _topPoint = newTop;
            }

            RaiseEvent(new RoutedEventArgs(MouseMoveEvent, this));
        }
    }

    public class ResizeThumb : Thumb
    {
        public static readonly RoutedEvent ThumbResizedEvent = EventManager.RegisterRoutedEvent("ThumbResized",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ResizeThumb));
        public event RoutedEventHandler ThumbMoved
        {
            add { AddHandler(ThumbResizedEvent, value); }
            remove { RemoveHandler(ThumbResizedEvent, value); }
        }

        public Control designerItem
        {
            get
            {
                Control item = this.DataContext as Control;
                return item;
            }
        }

        public ResizeThumb()
        {
            DragDelta += ResizeThumb_DragDelta;
        }

        void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (designerItem == null) return;

            double deltaVertical, deltaHorizontal;
            double newHeight, newWidth;
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Bottom:
                    deltaVertical = -e.VerticalChange;
                    newHeight = designerItem.ActualHeight - deltaVertical;
                    if (newHeight < 10) break;
                    if (newHeight >= 10 && newHeight < 450 - Canvas.GetTop(designerItem)) designerItem.Height = newHeight;
                    break;
                case VerticalAlignment.Top:
                    deltaVertical = e.VerticalChange;
                    if (designerItem.Height - deltaVertical < 10) break;
                    double newLoc = Canvas.GetTop(designerItem) + deltaVertical;
                    if (newLoc >= 0 && newLoc < 450)
                    {
                        Canvas.SetTop(designerItem, newLoc);
                        designerItem.Height -= deltaVertical;
                    }
                    break;
                default:
                    break;
            }
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    deltaHorizontal = e.HorizontalChange;
                    if (designerItem.Width - deltaHorizontal < 10) break;
                    double newLoc = Canvas.GetLeft(designerItem) + deltaHorizontal;
                    if (newLoc >= 0 && newLoc < 450)
                    {
                        Canvas.SetLeft(designerItem, newLoc);
                        designerItem.Width -= deltaHorizontal;
                    }
                    break;
                case HorizontalAlignment.Right:
                    deltaHorizontal = -e.HorizontalChange;
                    newWidth = designerItem.ActualWidth - deltaHorizontal;
                    if (newWidth < 10) break;
                    if (newWidth >= 10 && newWidth < 450 - Canvas.GetLeft(designerItem)) designerItem.Width = newWidth;
                    break;
                default:
                    break;
            }

            RaiseEvent(new RoutedEventArgs(ThumbResizedEvent, this));
            e.Handled = true;
        }
    }
}
