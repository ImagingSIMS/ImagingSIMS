using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for RegImage.xaml
    /// </summary>
    public partial class RegImage : UserControl
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(RegImageViewModel), typeof(RegImage));

        public RegImageViewModel ViewModel
        {
            get { return (RegImageViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public IEnumerable<ControlPoint> SelectedPoints
        {
            get
            {
                return ViewModel.ControlPoints.Select(p => new ControlPoint()
                {
                    Id = p.Id,
                    X = p.CoordX,
                    Y = p.CoordY
                });
            }
        }

        public RegImage()
        {
            ViewModel = new RegImageViewModel();

            InitializeComponent();
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = sender as Image;
            if (image == null) return;

            ViewModel.VisualWidth = image.ActualWidth;
            ViewModel.VisualHeight = image.ActualHeight;

            ViewModel.RepositionPoints();
            ViewModel.RepoisitonRregionOfInterest();
        }

        private void ControlPointSelection_ControlPointDragging(object sender, ControlPointDragRoutedEventArgs e)
        {
            var selection = e.Source as ControlPointSelection;
            if (selection == null) return;

            var point = selection.ControlPoint;

            // TODO: Drag
            var coord = e.DragArgs.GetPosition(gridImageHost);

            point.VisualX = coord.X - (ControlPointSelection.TargetWidth / 2);
            point.VisualY = coord.Y - (ControlPointSelection.TargetHeight / 2);
            ViewModel.SetMatrixCoordinates(point);
        }
        private void ControlPointSelection_ControlPointRemoved(object sender, RoutedEventArgs e)
        {
            var selection = e.Source as ControlPointSelection;
            if (selection == null) return;

            var point = selection.ControlPoint;

            if (ViewModel.ControlPoints.Contains(point))
            {
                ViewModel.ControlPoints.Remove(point);
            }
        }

        private void ClearSelection_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ControlPoints)
                e.CanExecute = ViewModel.ControlPoints.Count > 0;

            else if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ROI)
                e.CanExecute = ViewModel.RegionOfInterest.HasRegionOfInterest;
        }
        private void ClearSelection_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ControlPoints)
                ViewModel.ControlPoints.Clear();

            else if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ROI)
                ViewModel.RegionOfInterest.ClearRegionOfInterest();
        }

        private void regImageControl_DragEnter(object sender, DragEventArgs e)
        {
            ViewModel.IsDropTarget = true;
        }
        private void regImageControl_DragLeave(object sender, DragEventArgs e)
        {
            ViewModel.IsDropTarget = false;
        }
        private void regImageControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Bitmap))
            {
                var bs = e.Data.GetData(DataFormats.Bitmap) as BitmapSource;
                if (bs == null) return;

                ViewModel.BaseImage = bs;

                e.Handled = true;
            }

            else if (e.Data.GetDataPresent("DisplayImage"))
            {
                var image = e.Data.GetData("DisplayImage") as DisplayImage;
                if (image == null) return;

                var bs = image.Source as BitmapSource;
                if (bs == null) return;

                ViewModel.BaseImage = bs;

                e.Handled = true;
            }

            if (e.Data.GetDataPresent("Data2D"))
            {
                var data = e.Data.GetData("Data2D") as Data2D;
                if (data == null) return;

                var bs = ImageGenerator.Instance.Create(data, ColorScaleTypes.ThermalCold);

                ViewModel.BaseImage = bs;

                e.Handled = true;
            }

            ViewModel.IsDropTarget = false;
        }

        bool _isRoiDragging;
        private void gridImageHost_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var coord = e.GetPosition(gridImageHost);

                if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ControlPoints)
                {

                    ViewModel.AddNewPoint(coord);
                }
                else
                {
                    ViewModel.RegionOfInterest.Reset(coord);
                    ViewModel.SetMatrixCoordinates(ViewModel.RegionOfInterest);
                    _isRoiDragging = true;
                }
            }
        }
        private void gridImageHost_MouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ROI && _isRoiDragging)
            {
                var coord = e.GetPosition(gridImageHost);
                ViewModel.RegionOfInterest.DragTo(coord);
                ViewModel.SetMatrixCoordinates(ViewModel.RegionOfInterest);
            }
        }
        private void gridImageHost_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(ViewModel.SelectionMode == RegistrationImageSelectionMode.ROI)
            {
                _isRoiDragging = false;
            }
        }
        private void gridImageHost_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ViewModel.SelectionMode == RegistrationImageSelectionMode.ROI)
            {
                _isRoiDragging = false;
            }
        }
    }

    public class RegImageViewModel : INotifyPropertyChanged
    {
        BitmapSource _baseImage;
        ObservableCollection<ControlPointViewModel> _controlPoints;
        int _baseWidth;
        int _baseHeight;
        Color _selectionColor;
        double _visualWidth;
        double _visualHeight;
        bool _clearPointsOnRegistration;
        bool _isDropTarget;
        RegistrationImageSelectionMode _selectionMode;
        RegionOfInterestViewModel _regionOfInterest;

        public BitmapSource BaseImage
        {
            get { return _baseImage; }
            set
            {
                if(_baseImage != value)
                {
                    _baseImage = value;
                    NotifyPropertyChanged();
                    OnBaseImageChanged();
                }
            }
        }
        public ObservableCollection<ControlPointViewModel> ControlPoints
        {
            get { return _controlPoints; }
            set
            {
                if(_controlPoints != value)
                {
                    _controlPoints = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int BaseWidth
        {
            get { return _baseWidth; }
            set
            {
                if(_baseWidth != value)
                {
                    _baseWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int BaseHeight
        {
            get { return _baseHeight; }
            set
            {
                if(_baseHeight != value)
                {
                    _baseHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Color SelectionColor
        {
            get { return _selectionColor; }
            set
            {
                if(_selectionColor != value)
                {
                    _selectionColor = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double VisualWidth
        {
            get { return _visualWidth; }
            set
            {
                if(_visualWidth != value)
                {
                    _visualWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double VisualHeight
        {
            get { return _visualHeight; }
            set
            {
                if(_visualHeight != value)
                {
                    _visualHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool ClearPointsOnRegistration
        {
            get { return _clearPointsOnRegistration; }
            set
            {
                if (_clearPointsOnRegistration != value)
                {
                    _clearPointsOnRegistration = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsDropTarget
        {
            get { return _isDropTarget; }
            set
            {
                if(_isDropTarget != value)
                {
                    _isDropTarget = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public RegistrationImageSelectionMode SelectionMode
        {
            get { return _selectionMode; }
            set
            {
                if(_selectionMode != value)
                {
                    _selectionMode = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public RegionOfInterestViewModel RegionOfInterest
        {
            get { return _regionOfInterest; }
            set
            {
                if (_regionOfInterest != value)
                {
                    _regionOfInterest = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void OnBaseImageChanged()
        {
            BaseWidth = (int)BaseImage.Width;
            BaseHeight = (int)BaseImage.Height;

            if (ControlPoints.Count > 0)
            {
                foreach (var point in ControlPoints)
                {
                    SetMatrixCoordinates(point);
                }
            }

            if (RegionOfInterest.HasRegionOfInterest)
            {
                SetMatrixCoordinates(RegionOfInterest);
            }
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RegImageViewModel()
        {
            ControlPoints = new ObservableCollection<ControlPointViewModel>();
            SelectionColor = Color.FromArgb(255, 128, 255, 0);

            SelectionMode = RegistrationImageSelectionMode.ControlPoints;
            RegionOfInterest = new RegionOfInterestViewModel();

            var testImage = new Data2D(256, 256);
            for (int x = 0; x < 256; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    testImage[x, y] = x + y;
                }
            }
            BaseImage = ImageGenerator.Instance.Create(testImage, ColorScaleTypes.ThermalCold);
        }

        public void AddNewPoint(Point controlLocation)
        {
            var nextId = GetNextControlPointIndex();

            var controlPoint = new ControlPointViewModel(nextId)
            {
                VisualX = controlLocation.X - (ControlPointSelection.TargetWidth / 2),
                VisualY = controlLocation.Y - (ControlPointSelection.TargetHeight / 2)
            };
            SetMatrixCoordinates(controlPoint);
            ControlPoints.Add(controlPoint);
        }
        private int GetNextControlPointIndex()
        {
            int i = 1;
            while (ControlPoints.Select(p => p.Id).Contains(i))
            {
                i++;
            }
            return i;
        }

        public void RepositionPoints()
        {
            foreach (var point in ControlPoints)
            {
                point.SetVisualCoordinate(VisualWidth, VisualHeight, BaseWidth, BaseHeight);
            }
        }
        public void RepoisitonRregionOfInterest()
        {
            RegionOfInterest.SetVisualCoordinate(VisualWidth, VisualHeight, BaseWidth, BaseHeight);
        }
        public void SetMatrixCoordinates(ControlPointViewModel point)
        {
            point.SetMatrixCoordinate(VisualWidth, VisualHeight, BaseWidth, BaseHeight);
        }
        public void SetMatrixCoordinates(RegionOfInterestViewModel regionOfInterest)
        {
            regionOfInterest.SetMatrixCoordinate(VisualWidth, VisualHeight, BaseWidth, BaseHeight);
        }
        public void SetVisualCoordinates(ControlPointViewModel point)
        {
            point.SetVisualCoordinate(VisualWidth, VisualHeight, BaseWidth, BaseHeight);
        }
        public void SetVisualCoordinates(RegionOfInterestViewModel regionOfInterest)
        {
            regionOfInterest.SetVisualCoordinate(VisualWidth, VisualHeight, BaseWidth, BaseHeight);
        }
    }

    public class RegionOfInterestViewModel : INotifyPropertyChanged
    {
        Point _dragStart;

        double _left;
        double _top;
        double _height;
        double _width;

        double _visualLeft;
        double _visualTop;
        double _visualHeight;
        double _visualWidth;

        bool _hasRegionOfInterest;

        public double Left
        {
            get { return _left; }
            set
            {
                if(_left != value)
                {
                    _left = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Top
        {
            get { return _top; }
            set
            {
                if (_top != value)
                {
                    _top = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    NotifyPropertyChanged();
                }
            }

        }

        public double VisualLeft
        {
            get { return _visualLeft; }
            set
            {
                if (_visualLeft != value)
                {
                    _visualLeft = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double VisualTop
        {
            get { return _visualTop; }
            set
            {
                if (_visualTop != value)
                {
                    _visualTop = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double VisualWidth
        {
            get { return _visualWidth; }
            set
            {
                if (_visualWidth != value)
                {
                    _visualWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double VisualHeight
        {
            get { return _visualHeight; }
            set
            {
                if(_visualHeight != value)
                {
                    _visualHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool HasRegionOfInterest
        {
            get { return _hasRegionOfInterest; }
            set
            {
                if (_hasRegionOfInterest != value)
                {
                    _hasRegionOfInterest = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Rect Rectangle
        {
            get { return new Rect(Left, Top, Width, Height); }
        }

        public RegionOfInterestViewModel()
        {
            _dragStart = new Point();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Reset(Point startPoint)
        {
            VisualLeft = startPoint.X;
            VisualTop = startPoint.Y;
            VisualWidth = 0;
            VisualHeight = 0;

            _dragStart = startPoint;

            HasRegionOfInterest = true;
        }
        public void DragTo(Point endPoint)
        {
            if (_dragStart == null) return;

            VisualLeft = Math.Min(_dragStart.X, endPoint.X);
            VisualTop = Math.Min(_dragStart.Y, endPoint.Y);

            VisualWidth = Math.Abs(_dragStart.X - endPoint.X);
            VisualHeight = Math.Abs(_dragStart.Y - endPoint.Y);

            HasRegionOfInterest = true;
        }

        public void SetVisualCoordinate(double visualElementWidth,
            double visualElementHeight, int matrixWidth, int matrixHeight)
        {
            VisualLeft = visualElementWidth * Left / matrixWidth;
            VisualTop = visualElementHeight * Top / matrixHeight;
            VisualWidth = visualElementWidth * Width / matrixWidth;
            VisualHeight = visualElementHeight * Height / matrixHeight;
        }
        public void SetMatrixCoordinate(double visualElementWidth,
            double visualElementHeight, int matrixWidth, int matrixHeight)
        {
            Left = VisualLeft * matrixWidth / visualElementWidth;
            Top = VisualTop * matrixHeight / visualElementHeight;
            Width = VisualWidth * matrixWidth / visualElementWidth;
            Height = VisualHeight * matrixHeight / visualElementHeight;
        }

        public void ClearRegionOfInterest()
        {
            _dragStart = new Point();

            HasRegionOfInterest = false;

            VisualLeft = 0;
            VisualTop = 0;
            VisualWidth = 0;
            VisualHeight = 0;

            Left = 0;
            Top = 0;
            Width = 0;
            Height = 0;
        }
    }

    public enum RegistrationImageSelectionMode
    {
        [Description("Control Points")]
        ControlPoints,

        ROI
    }
}
