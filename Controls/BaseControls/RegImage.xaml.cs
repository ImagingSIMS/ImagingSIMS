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

        public RegImage()
        {
            ViewModel = new RegImageViewModel();

            InitializeComponent();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var coord = e.GetPosition(grid_host);

            var controlPoint = new ControlPointViewModel(3, coord.X, coord.Y);
            ViewModel.ControlPoints.Add(controlPoint);
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

        private void OnBaseImageChanged()
        {
            BaseWidth = (int)BaseImage.Width;
            BaseHeight = (int)BaseImage.Height;

            ControlPoints.Clear();
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
            BaseImage = ImageHelper.CreateColorScaleImage(testImage, ColorScaleTypes.ThermalCold);
        }
    }
}
