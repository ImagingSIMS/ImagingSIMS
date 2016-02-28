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

namespace ImagingSIMS.Controls.BaseControls
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
}
