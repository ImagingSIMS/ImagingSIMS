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
        }
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                var coord = e.GetPosition(gridImageHost);

                ViewModel.AddNewPoint(coord);
            }
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

        private void ClearPoints_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel.ControlPoints.Count > 0;
        }
        private void ClearPoints_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.ControlPoints.Clear();
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
            ViewModel.IsDropTarget = false;
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
        bool _clearPointsOnImageChanged;
        bool _clearPointsOnRegistration;
        bool _isDropTarget;

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
        public bool ClearPointsOnImageChanged
        {
            get { return _clearPointsOnImageChanged; }
            set
            {
                if(_clearPointsOnImageChanged != value)
                {
                    _clearPointsOnImageChanged = value;
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

        private void OnBaseImageChanged()
        {
            BaseWidth = (int)BaseImage.Width;
            BaseHeight = (int)BaseImage.Height;

            if (ClearPointsOnImageChanged)
            {
                ControlPoints.Clear();
            }

            else
            {
                foreach (var point in ControlPoints)
                {
                    SetMatrixCoordinates(point);
                }
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
        public void SetMatrixCoordinates(ControlPointViewModel point)
        {
            point.SetMatrixCoordinate(VisualWidth, VisualHeight, BaseWidth, BaseHeight);
        }
        public void SetVisualCoordinates(ControlPointViewModel point)
        {
            point.SetVisualCoordinate(VisualWidth, VisualHeight, BaseWidth, BaseHeight);
        }
    }
}
