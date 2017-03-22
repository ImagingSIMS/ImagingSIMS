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

using Microsoft.Win32;

using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for PointSelectImage.xaml
    /// </summary>
    public partial class PointSelectImage : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource",
            typeof(BitmapSource), typeof(PointSelectImage));
        public static readonly DependencyProperty SelectionColorProperty = DependencyProperty.Register("SelectionColor",
            typeof(Color), typeof(PointSelectImage), new FrameworkPropertyMetadata(Color.FromArgb(255, 0, 255, 0), FrameworkPropertyMetadataOptions.None,
                updateImage));
        public static readonly DependencyProperty OriginalImageProperty = DependencyProperty.Register("OriginalImage",
            typeof(BitmapSource), typeof(PointSelectImage), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None,
                setOriginalImage));
        public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register("ImageWidth",
            typeof(double), typeof(PointSelectImage), new FrameworkPropertyMetadata(1d));
        public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register("ImageHeight",
            typeof(double), typeof(PointSelectImage), new FrameworkPropertyMetadata(1d));
        public static readonly DependencyProperty PointSourceProperty = DependencyProperty.Register("PointSource",
            typeof(PointSource), typeof(PointSelectImage), new FrameworkPropertyMetadata(PointSource.Selection));

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

        public BitmapSource ImageSource
        {
            get { return (BitmapSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
        public Color SelectionColor
        {
            get { return (Color)GetValue(SelectionColorProperty); }
            set { SetValue(SelectionColorProperty, value); }
        }
        public BitmapSource OriginalImage
        {
            get { return (BitmapSource)GetValue(OriginalImageProperty); }
            set { SetValue(OriginalImageProperty, value); }
        }
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
        public PointSource PointSource
        {
            get { return (PointSource)GetValue(PointSourceProperty); }
            set { SetValue(PointSourceProperty, value); }
        }

        public PointSelectImage()
        {
            _selectedPoints = new ObservableCollection<Point>();
            _selectedPoints.CollectionChanged += _selectedPoints_CollectionChanged;
            
            InitializeComponent();
        }

        public static void setOriginalImage(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            PointSelectImage sender = obj as PointSelectImage;
            if (sender == null) return;

            sender.setOriginalImage(sender.OriginalImage, true);
        }
        private void setOriginalImage(BitmapSource Image, bool ClearPoints)
        {
            if (ClearPoints) this.ClearPoints();

            if (Image == null) return;

            _dataSource = ImageGenerator.Instance.ConvertToData3D(Image);
            updateImage();
        }

        public void ClearPoints()
        {
            SelectedPoints.Clear();
        }

        void _selectedPoints_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            updateImage();
        }

        private void buttonShowColor_MouseEnter(object sender, RoutedEventArgs e)
        {
            popupSolidColorScale.IsOpen = true;
        }
        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            ClearPoints();
        }
        private void Grid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                double width = image.ActualWidth;
                double height = image.ActualHeight;

                int pixelWidth = _dataSource.Width;
                int pixelHeight = _dataSource.Height;

                Point position = e.GetPosition(image);

                double x = position.X * pixelWidth / width;
                double y = position.Y * pixelHeight / height;

                SelectedPoints.Add(new Point(x, y));
            }
        }

        private void updateImage()
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

            for (int x = 0; x < _dataSource.Width; x++)
            {
                for (int y = 0; y < _dataSource.Height; y++)
                {
                    if (pointMask[x, y])
                    {
                        imagePreview[x, y] = SelectionColor.ToFloatArray();
                    }
                    else
                    {
                        imagePreview[x, y] = _dataSource[x, y];
                    }
                }
            }

            ImageSource = ImageGenerator.Instance.Create(imagePreview);
        }
        public static void updateImage(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            PointSelectImage source = obj as PointSelectImage;
            if (source == null) return;

            source.updateImage();
        }

        private void buttonShowColor_Click(object sender, RoutedEventArgs e)
        {
            popupSolidColorScale.IsOpen = true;
        }
        private void buttonSave_Click(object sender, RoutedEventArgs e) 
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Point Set File (.pts)|*.pts";

            if (!sfd.ShowDialog() == true) return;

            PointSet.PointSetToFile(SelectedPointsNormalized, sfd.FileName);
        }

        private void cmCopy_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            if (mi == null) return;

            if (mi == cmCopy)
            {
                if (OriginalImage != null)
                {
                    Clipboard.SetImage(OriginalImage);
                }
            }
            else if (mi == cmCopyPoints)
            {
                if (ImageSource != null)
                {
                    Clipboard.SetImage(ImageSource);
                }
            }
        }
    }

    public enum PointSource
    {
        Selection,
        [Description("Last Set")]
        LastSet,
        [Description("From File")]
        FromFile,
    }
}
