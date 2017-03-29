using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImagingSIMS.Controls.BaseControls;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls.ViewModels
{
    public class ControlPointImageViewModel : INotifyPropertyChanged
    {
        Stack<Data2D> _undoHistory;
        Stack<Data2D> _redoHistory;

        bool _isRegistered;
        Data2D _dataSource;
        BitmapSource _displayImage;
        double _saturation;
        double _initialSaturation;
        ColorScaleTypes _colorScale;
        Color _solidColorScale;
        double _threshold;
        double _initialThreshold;

        public bool IsRegistered
        {
            get { return _isRegistered; }
            set
            {
                if (_isRegistered != value)
                {
                    _isRegistered = value;
                    NotifyPropertyChanged();
                }
            }
        }
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
        public Stack<Data2D> UndoHistory
        {
            get { return _undoHistory; }
            set
            {
                if (_undoHistory != value)
                {
                    _undoHistory = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Stack<Data2D> RedoHistory
        {
            get { return _redoHistory; }
            set
            {
                if (_redoHistory != value)
                {
                    _redoHistory = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Data2D DataSource
        {
            get { return _dataSource; }
            set
            {
                if (_dataSource != value)
                {
                    _dataSource = value;
                    NotifyPropertyChanged();
                    OnDataSourceChanged();
                }
            }
        }
        public BitmapSource DisplayImage
        {
            get { return _displayImage; }
            set
            {
                if (_displayImage != value)
                {
                    _displayImage = value;
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
                    Redraw();
                }
            }
        }
        public double InitialSaturation
        {
            get { return _initialSaturation; }
            set
            {
                if (_initialSaturation != value)
                {
                    _initialSaturation = value;
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
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
                    NotifyPropertyChanged();
                    Redraw();
                }
            }
        }
        public double Threshold
        {
            get { return _threshold; }
            set
            {
                if (_threshold != value)
                {
                    _threshold = value;
                    NotifyPropertyChanged();
                    Redraw();
                }
            }
        }
        public double InitialThreshold
        {
            get { return _initialThreshold; }
            set
            {
                if (_initialThreshold != value)
                {
                    _initialThreshold = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ObservableCollection<ControlPointViewModel> ControlPoints
        {
            get { return _controlPoints; }
            set
            {
                if (_controlPoints != value)
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
                if (_baseWidth != value)
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
                if (_baseHeight != value)
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
                if (_selectionColor != value)
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
                if (_visualHeight != value)
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
                if (_isDropTarget != value)
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
                if (_selectionMode != value)
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

        private void OnDataSourceChanged()
        {
            BaseWidth = DataSource.Width;
            BaseHeight = DataSource.Height;

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

            Saturation = DataSource.Maximum;
            InitialSaturation = DataSource.Maximum;
            Threshold = DataSource.Minimum;
            InitialSaturation = DataSource.Minimum;

            Redraw();
        }
        private void Redraw()
        {
            if (DataSource == null) return;

            if (ColorScale == ColorScaleTypes.Solid)
            {
                DisplayImage = ImageGenerator.Instance.Create(DataSource, SolidColorScale, (float)Saturation, (float)Threshold);
            }
            else
            {
                DisplayImage = ImageGenerator.Instance.Create(DataSource, ColorScale, (float)Saturation, (float)Threshold);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ControlPointImageViewModel()
        {
            UndoHistory = new Stack<Data2D>();
            RedoHistory = new Stack<Data2D>();

            ControlPoints = new ObservableCollection<ControlPointViewModel>();
            SelectionColor = Color.FromArgb(255, 128, 255, 0);

            SelectionMode = RegistrationImageSelectionMode.ControlPoints;
            RegionOfInterest = new RegionOfInterestViewModel();

            SolidColorScale = Color.FromArgb(255, 255, 255, 255);
            ColorScale = ColorScaleTypes.ThermalCold;
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

        public void ChangeDataSource(Data2D dataSource)
        {
            var previousDataSource = DataSource;

            _redoHistory.Clear();
            _undoHistory.Push(previousDataSource);

            DataSource = dataSource;
        }
    }
}
