using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using Microsoft.Win32;

using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.Common.Controls;

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for RegistrationInputImage.xaml
    /// </summary>
    public partial class RegistrationInputImage : UserControl
    {
        public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register("ImageWidth",
            typeof(double), typeof(RegistrationInputImage));
        public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register("ImageHeight",
            typeof(double), typeof(RegistrationInputImage));
        public static readonly DependencyProperty OriginalImageSourceProperty = DependencyProperty.Register("OriginalImageSource",
            typeof(BitmapSource), typeof(RegistrationInputImage), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None, originalImageChanged));
        public static readonly DependencyProperty PointsOverlayImageSourceProperty = DependencyProperty.Register("PointsOverlayImageSource",
            typeof(BitmapSource), typeof(RegistrationInputImage));
        public static readonly DependencyProperty IsPointBasedProperty = DependencyProperty.Register("IsPointBased",
            typeof(bool), typeof(RegistrationInputImage), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, isPointBasedChanged));
        public static readonly DependencyProperty SelectionColorProperty = DependencyProperty.Register("SelectionColor",
            typeof(Color), typeof(RegistrationInputImage), new FrameworkPropertyMetadata(Color.FromArgb(255, 0, 255, 0), FrameworkPropertyMetadataOptions.None, updateOverlayImage));
        public static readonly DependencyProperty PointSourceProperty = DependencyProperty.Register("PointSource",
            typeof(PointSource), typeof(RegistrationInputImage), new FrameworkPropertyMetadata(PointSource.Selection));
        public static readonly DependencyProperty CanSelectROIProperty = DependencyProperty.Register("CanSelectROI",
            typeof(bool), typeof(RegistrationInputImage), new FrameworkPropertyMetadata(true));

        public double ImageWidth
        {
            get { return (double)GetValue(ImageWidthProperty); }
            set { SetValue(ImageWidthProperty, value); }
        }
        public double ImageHeight
        {
            get { return (double)GetValue(ImageHeightProperty); }
            set { SetValue(ImageHeightProperty, value); }
        }
        public BitmapSource OriginalImageSource
        {
            get { return (BitmapSource)GetValue(OriginalImageSourceProperty); }
            set { SetValue(OriginalImageSourceProperty, value); }
        }
        public BitmapSource PointsOverlayImageSource
        {
            get { return (BitmapSource)GetValue(PointsOverlayImageSourceProperty); }
            set { SetValue(PointsOverlayImageSourceProperty, value); }
        }
        public bool IsPointBased
        {
            get { return (bool)GetValue(IsPointBasedProperty); }
            set { SetValue(IsPointBasedProperty, value); }
        }
        public Color SelectionColor
        {
            get { return (Color)GetValue(SelectionColorProperty); }
            set { SetValue(SelectionColorProperty, value); }
        }
        public PointSource PointSource
        {
            get { return (PointSource)GetValue(PointSourceProperty); }
            set { SetValue(PointSourceProperty, value); }
        }
        public bool CanSelectROI
        {
            get { return (bool)GetValue(CanSelectROIProperty); }
            set { SetValue(CanSelectROIProperty, value); }
        }

        Data3D _dataSource;
        ObservableCollection<Point> _selectedPoints;

        public ObservableCollection<Point> SelectedPoints
        {
            get { return _selectedPoints; }
            set { _selectedPoints = value; }
        }
        public ObservableCollection<Point> SelectedPointsNormalized
        {
            get
            {
                ObservableCollection<Point> normalized = new ObservableCollection<Point>();

                int pixelWidth = _dataSource.Width;
                int pixelHeight = _dataSource.Height;

                foreach (Point p in SelectedPoints)
                {
                    normalized.Add(new Point(p.X / pixelWidth, p.Y / pixelHeight));
                }

                return normalized;
            }
        }
        public Point ROITopLeft
        {
            get { return new Point(roiRectangle.Margin.Left, roiRectangle.Margin.Top); }
        }
        public Point ROIBottomRight
        {
            get 
            {
                // Check if width/height are NaN because there will be no width or height to the rectangle
                // until one is first drawn or cleared
                double width = double.IsNaN(roiRectangle.Width) ? originalImage.ActualWidth : roiRectangle.Width;
                double height = double.IsNaN(roiRectangle.Height) ? originalImage.ActualHeight : roiRectangle.Height;

                return new Point(roiRectangle.Margin.Left + width, roiRectangle.Margin.Top + height);
            }
        }
        public Point ROITopLeftNormalized
        {
            get 
            {
                Point topLeft = ROITopLeft;
                return new Point(topLeft.X / originalImage.ActualWidth, topLeft.Y / originalImage.ActualHeight);
            }
        }
        public Point ROIBottomRightNormalized
        {
            get
            {
                Point botRight = ROIBottomRight;
                return new Point(botRight.X / originalImage.ActualWidth, botRight.Y / originalImage.ActualHeight);
            }
        }

        public RegistrationInputImage()
        {
            _selectedPoints = new ObservableCollection<Point>();
            _selectedPoints.CollectionChanged += _selectedPoints_CollectionChanged;

            SelectionColor = Color.FromArgb(255, 0, 255, 0);

            InitializeComponent();
        }

        public void ClearPoints()
        {
            SelectedPoints.Clear();
        }

        #region On Changes
        public static void updateOverlayImage(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            RegistrationInputImage r = obj as RegistrationInputImage;
            if (r == null) return;

            r.updateOverlayImage();
        }
        private void updateOverlayImage()
        {
            if (_dataSource == null) return;

            int brushSize = _dataSource.Width / 50;

            Data3D imagePreview = new Data3D(_dataSource.Width, _dataSource.Height, _dataSource.Depth);
            bool[,] pointMask = new bool[_dataSource.Width, _dataSource.Height];

            foreach (Point p in _selectedPoints)
            {
                int px = (int)p.X;
                int py = (int)p.Y;

                int startX = px - (brushSize / 2);
                int startY = py - (brushSize / 2);

                for (int x = 0; x < brushSize; x++)
                {
                    for (int y = 0; y < brushSize; y++)
                    {
                        if (startX + x < 0 || startX + x >= _dataSource.Width ||
                            startY + y < 0 || startY + y >= _dataSource.Height) continue;

                        pointMask[startX + x, startY + y] = true;
                    }
                }
            }

            var colorArray = ((Color)SelectionColor).ToFloatArray();

            for (int x = 0; x < _dataSource.Width; x++)
            {
                for (int y = 0; y < _dataSource.Height; y++)
                {
                    if (pointMask[x, y])
                    {
                        imagePreview[x, y] = colorArray;
                    }
                    else
                    {
                        imagePreview[x, y] = new float[4] { 0, 0, 0, 1 };
                    }
                }
            }

            PointsOverlayImageSource = ImageGenerator.Instance.Create(imagePreview);
        }
        public static void originalImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            RegistrationInputImage r = obj as RegistrationInputImage;
            if (r == null) return;

            r.originalImageChanged(r.OriginalImageSource, true);
        }
        private void originalImageChanged(BitmapSource image, bool clearPoints)
        {
            if (image == null) return;

            if (clearPoints) this.ClearPoints();

            _dataSource = ImageGenerator.Instance.ConvertToData3D(image);

        }
        public static void isPointBasedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }
        void _selectedPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            updateOverlayImage();
        }
        #endregion

        #region Mouse Events
        bool _isLeftMouseDown;
        bool _isRightMouseDown;
        Point _rectangleStartPoint;
        private void gridMouseCapture_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            UIElement element = (UIElement)sender;

            if(e.ChangedButton == MouseButton.Left)
            {
                _isLeftMouseDown = true;
                _isRightMouseDown = false;

                if (IsPointBased)
                {
                    addPoint(e.GetPosition(originalImage));
                }
            }
            else if(e.ChangedButton==MouseButton.Right)
            {
                _isLeftMouseDown = false;
                _isRightMouseDown = true;

                element.CaptureMouse();

                if (!CanSelectROI) return;

                Point location = e.GetPosition(originalImage);
                _rectangleStartPoint = location;
                roiRectangle.Visibility = Visibility.Hidden;
                roiRectangle.Margin = new Thickness(location.X, location.Y, 0, 0);
            }
        }
        private void gridMouseCapture_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isLeftMouseDown) return;

            if (_isRightMouseDown)
            {
                if (!CanSelectROI) return;

                Point currentPoint = e.GetPosition(originalImage);
                double x1 = _rectangleStartPoint.X;
                double x2 = currentPoint.X;
                double y1 = _rectangleStartPoint.Y;
                double y2 = currentPoint.Y;

                roiRectangle.Visibility = Visibility.Visible;
                roiRectangle.Margin = new Thickness(Math.Min(x1, x2), Math.Min(y1, y2), 0, 0);
                roiRectangle.Width = Math.Abs(x1 - x2);
                roiRectangle.Height = Math.Abs(y1 - y2);
            }
        }
        private void gridMouseCapture_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            UIElement element = (UIElement)sender;

            if (e.ChangedButton == MouseButton.Left)
            {
                _isLeftMouseDown = false;
                _isRightMouseDown = false;

                element.ReleaseMouseCapture();
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                _isLeftMouseDown = false;
                _isRightMouseDown = false;

                element.ReleaseMouseCapture();
            }
        }
        #endregion

        private void addPoint(Point position)
        {
            double width = originalImage.ActualWidth;
            double height = originalImage.ActualHeight;

            int pixelWidth = _dataSource.Width;
            int pixelHeight = _dataSource.Height;

            double x = position.X * pixelWidth / width;
            double y = position.Y * pixelHeight / height;

            SelectedPoints.Add(new Point(x, y));
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Point Set File (.pts)|*.pts";

            if (!sfd.ShowDialog() == true) return;

            PointSet.PointSetToFile(SelectedPointsNormalized, sfd.FileName);
        }
        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            this.ClearPoints();
        }
        private void buttonClearROI_Click(object sender, RoutedEventArgs e)
        {
            roiRectangle.Margin = new Thickness(0, 0, 0, 0);
            roiRectangle.Width = originalImage.ActualWidth;
            roiRectangle.Height = originalImage.ActualHeight;
            roiRectangle.Visibility = Visibility.Hidden;
        }
    }
}
