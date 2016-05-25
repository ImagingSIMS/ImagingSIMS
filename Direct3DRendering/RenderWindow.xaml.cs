using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

using SharpDX;
using SharpDX.Windows;

using Color = SharpDX.Color;
using Point = SharpDX.Point;
using Texture2D = SharpDX.Toolkit.Graphics.Texture2D;

namespace Direct3DRendering
{
    /// <summary>
    /// Interaction logic for RenderWindow.xaml
    /// </summary>
    public partial class RenderWindow : Window
    {
        RenderDetailsWindow _renderDetailsWindow;
        RenderControl _renderControl;
        Renderer _renderer;

        FramerateCounter _framerate;

        public RenderControl RenderControl
        {
            get { return _renderControl; }
        }
        public Renderer Renderer
        {
            get { return _renderer; }
        }

        public static readonly DependencyProperty RenderWindowViewProperty = DependencyProperty.Register("RenderWindowView",
            typeof(RenderWindowViewModel), typeof(RenderWindow));
        public RenderWindowViewModel RenderWindowView
        {
            get { return (RenderWindowViewModel)GetValue(RenderWindowViewProperty); }
            set { SetValue(RenderWindowViewProperty, value); }
        }

        bool _windowActivated;
        private bool canControlCamera
        {
            get
            {
                return _windowActivated;
            }
        }

        public RenderWindow()
        {
            RenderWindowView = new RenderWindowViewModel();

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _renderControl = new RenderControl();

            formsHost.Child = _renderControl;
            formsHost.Focus();

#if DEBUG || DEBUG_DEVICE
            RenderWindowView.ShowAxes = true;
            RenderWindowView.ShowBoundingBox = true;
#endif
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopRendering();
        }

        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {

        }
        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        private void menuItemShowRenderControls_Click(object sender, RoutedEventArgs e)
        {
            if (_renderDetailsWindow != null) return;

            _renderDetailsWindow = new RenderDetailsWindow();
            _renderDetailsWindow.Closed += _renderDetailsWindow_Closed;
            _renderDetailsWindow.Owner = this;
            _renderDetailsWindow.RenderWindow = this;
            _renderDetailsWindow.RenderWindowView = RenderWindowView;
            _renderDetailsWindow.Device = _renderer.Device;
            _renderDetailsWindow.Show();
        }

        private void _renderDetailsWindow_Closed(object sender, EventArgs e)
        {
            RenderWindowView.IsRecording = false;
            RenderWindowView.GetSnapshot = false;
            _renderDetailsWindow.Closed -= _renderDetailsWindow_Closed;
            _renderDetailsWindow = null;
        }

        private void setData(float[,] HeightData, Color[,] ColorData)
        {
            _renderer = new HeightMapRenderer(this);

            initializeRenderer();

            ((HeightMapRenderer)_renderer).SetData(ColorData, HeightData, new Vector3(2, 2, 0.50f));
        }
        private void setData(params RenderVolume[] Volumes)
        {
            if (Volumes.Length > 8) throw new ArgumentException("Rendering is only supported for a maximum of 8 volumes.");

            _renderer = new VolumeRenderer(this);

            initializeRenderer();

            ((VolumeRenderer)_renderer).SetData(new List<RenderVolume>(Volumes));
            
        }
        private void setData(List<RenderVolume> Volumes)
        {
            if (Volumes.Count > 8) throw new ArgumentException("Rendering is only supported for a maximum of 8 volumes.");

            _renderer = new VolumeRenderer(this);

            initializeRenderer();

            ((VolumeRenderer)_renderer).SetData(new List<RenderVolume>(Volumes));
            
        }
        private void setData(List<RenderIsosurface> Isosurfaces)
        {
            if (Isosurfaces.Count > 8) throw new ArgumentException("Rendering is only suppoerted for a maximum of 8 volumes.");

            _renderer = new IsosurfaceRenderer(this);

            initializeRenderer();

            ((IsosurfaceRenderer)_renderer).SetData(Isosurfaces);
        }

        public Task SetDataAsync(float[,] HeightData, Color[,] ColorData)
        {
            return Task.Run(() => setData(HeightData, ColorData));
        }
        public Task SetDataAsync(params RenderVolume[] Volumes)
        {
            return Task.Run(() => setData(Volumes));
        }
        public Task SetDataAsync(List<RenderVolume> Volumes)
        {
            return Task.Run(() => setData(Volumes));
        }
        public Task SetDataAsync(List<RenderIsosurface> Isosurfaces)
        {
            return Task.Run(() => setData(Isosurfaces));
        }

        private void initializeRenderer()
        {
            _renderer.InitializeRenderer();
            _renderControl.SizeChanged += (sender_, args) =>
                _renderer.NeedsResize = true;
        }

        public void SetTransferFunction(byte[] Function)
        {

        }
        public void BeginRendering()
        {
            _framerate = new FramerateCounter();

            RenderWindowView.IsRenderLoaded = true;

            RenderLoop.Run(_renderControl, () =>
                {
                    if (_renderer == null) return;

                    _renderer.Update(RenderWindowView.TargetYAxisOrbiting);

                    RenderWindowView.CameraDirection = _renderer.Camera.Direction;
                    RenderWindowView.CameraPosition = _renderer.Camera.Position;
                    RenderWindowView.CameraUp = _renderer.Camera.Up;
                    RenderWindowView.FPS = _framerate.GetFramerate();

                    if (RenderWindowView.RenderType != _renderer.RenderType)
                    {
                        RenderWindowView.RenderType = _renderer.RenderType;
                    }

                    _renderer.Draw();

                    if (RenderWindowView.IsRecording)
                    {
                        int width = 0;
                        int height = 0;

                        try
                        {
                            byte[] buffer = _renderer.GetCurrentCapture(out width, out height);
                            if (buffer != null)
                            {
                                _renderDetailsWindow.ScreenCaptureDisplay.TryCapture(buffer, width, height, RenderWindowView.FPS);
                            }
                        }
                        catch(OutOfMemoryException)
                        {
                            RenderWindowView.IsRecording = false;
                            MessageBox.Show("An OutOfMemoryException has been registered and capturing has been stopped." +
                                " In order to continue recording, please first save all captures and clear the list. Then, resume recording.", 
                                "OutOfMemoryException", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Exclamation);
                        }
                    }
                    if (RenderWindowView.GetSnapshot)
                    {
                        int width = 0;
                        int height = 0;

                        try
                        {
                            byte[] buffer = _renderer.GetCurrentCapture(out width, out height);
                            if (buffer != null)
                            {
                                _renderDetailsWindow.ScreenCaptureDisplay.AddCapture(buffer, width, height);
                                RenderWindowView.GetSnapshot = false;
                            }
                        }
                        catch (OutOfMemoryException)
                        {
                            RenderWindowView.IsRecording = false;
                            MessageBox.Show("An OutOfMemoryException has been registered and capturing has been stopped." +
                                " In order to continue recording, please first save all captures and clear the list. Then, resume recording.",
                                "OutOfMemoryException",
                                MessageBoxButton.OK,
                                MessageBoxImage.Exclamation);
                        }
                    }
                });
        }

        public void StopRendering()
        {
            _renderer.Dispose();
            _renderControl.Dispose();
            formsHost.Dispose();
        }

        public void ReverseCamera()
        {
            _renderer.Camera.ReverseCamera();
        }
        public void ResetCamera()
        {
            _renderer.Camera.ResetCamera();
        }

        private void renderWindow_Activated(object sender, EventArgs e)
        {
            _windowActivated = true;
        }
                private void renderWindow_Deactivated(object sender, EventArgs e)
        {
            _windowActivated = false;
        }
    }

    public class RenderWindowViewModel : INotifyPropertyChanged
    {
        bool _isRenderLoaded;
        bool _renderIsosurfaces;
        bool _showAxes;
        bool _showBoundingBox;
        bool _showCoordinateBox;
        float _coordinateBoxTransparency;
        bool _isRecording;
        bool _getSnapshot;
        Color _backColor;
        float _brightness;
        float[] _volumeAlphas;
        float[] _isosurfaceValues;
        bool _targetYAxisOrbiting;
        float _heightMapHeight;
        RenderType _renderType;
        Vector3 _cameraDirection;
        Vector3 _cameraPosition;
        Vector3 _cameraUp;
        double _fps;

        public bool IsRenderLoaded
        {
            get { return _isRenderLoaded; }
            set
            {
                if (_isRenderLoaded != value)
                {
                    _isRenderLoaded = value;
                    NotifyPropertyChanged("IsRenderLoaded");
                }
            }
        }
        public bool RenderIsosurfaces
        {
            get { return _renderIsosurfaces; }
            set
            {
                if (_renderIsosurfaces != value)
                {
                    _renderIsosurfaces = value;
                    NotifyPropertyChanged("RenderIsosurfaces");
                }
            }
        }
        public bool ShowAxes
        {
            get { return _showAxes; }
            set
            {
                if (_showAxes != value)
                {
                    _showAxes = value;
                    NotifyPropertyChanged("ShowAxes");
                }
            }
        }
        public bool ShowBoundingBox
        {
            get { return _showBoundingBox; }
            set
            {
                if (_showBoundingBox != value)
                {
                    _showBoundingBox = value;
                    NotifyPropertyChanged("ShowBoundingBox");
                }
            }
        }
        public bool ShowCoordinateBox
        {
            get { return _showCoordinateBox; }
            set
            {
                if (_showCoordinateBox != value)
                {
                    _showCoordinateBox = value;
                    NotifyPropertyChanged("ShowCoordinateBox");
                }
            }
        }
        public float CoordinateBoxTransparency
        {
            get { return _coordinateBoxTransparency; }
            set
            {
                if (_coordinateBoxTransparency != value)
                {
                    _coordinateBoxTransparency = value;
                    NotifyPropertyChanged("CoordinateBoxTransparency");
                }
            }
        }
        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                if (_isRecording != value)
                {
                    _isRecording = value;
                    NotifyPropertyChanged("IsRecording");
                }
            }
        }
        public bool GetSnapshot
        {
            get { return _getSnapshot; }
            set
            {
                if (_getSnapshot != value)
                {
                    _getSnapshot = value;
                    NotifyPropertyChanged("GetSnapshot");
                }
            }
        }
        public Color BackColor
        {
            get { return _backColor; }
            set
            {
                if (_backColor != value)
                {
                    _backColor = value;
                    NotifyPropertyChanged("BackColor");
                }
            }
        }
        public float Brightness
        {
            get { return _brightness; }
            set
            {
                if (_brightness != value)
                {
                    _brightness = value;
                    NotifyPropertyChanged("Brightness");
                }
            }
        }
        public float[] VolumeAlphas
        {
            get { return _volumeAlphas; }
            set
            {
                if (_volumeAlphas != value)
                {
                    _volumeAlphas = value;
                    NotifyPropertyChanged("VolumeAlphas");
                }
            }
        }
        public float[] IsosurfaceValues
        {
            get { return _isosurfaceValues; }
            set
            {
                if (_isosurfaceValues != value)
                {
                    _isosurfaceValues = value;
                    NotifyPropertyChanged("IsosurfaceValues");
                }
            }
        }
        public bool TargetYAxisOrbiting
        {
            get { return _targetYAxisOrbiting; }
            set
            {
                if (_targetYAxisOrbiting != value)
                {
                    _targetYAxisOrbiting = value;
                    NotifyPropertyChanged("TargetYAxisOrbiting");
                }
            }
        }
        public float HeightMapHeight
        {
            get { return _heightMapHeight; }
            set 
            {
                if (_heightMapHeight != value)
                {
                    _heightMapHeight = value;
                    NotifyPropertyChanged("HeightMapHeight");
                }
            }
        }
        public RenderType RenderType
        {
            get { return _renderType; }
            set
            {
                if (_renderType != value)
                {
                    _renderType = value;
                    NotifyPropertyChanged("RenderType");
                }
            }
        }
        public Vector3 CameraDirection
        {
            get { return _cameraDirection; }
            set
            {
                if (_cameraDirection != value)
                {
                    _cameraDirection = value;
                    NotifyPropertyChanged("CameraDirection");
                }
            }
        }
        public Vector3 CameraPosition
        {
            get { return _cameraPosition; }
            set
            {
                if (_cameraPosition != value)
                {
                    _cameraPosition = value;
                    NotifyPropertyChanged("CameraPosition");
                }
            }
        }
        public Vector3 CameraUp
        {
            get { return _cameraUp; }
            set
            {
                if (_cameraUp != value)
                {
                    _cameraUp = value;
                    NotifyPropertyChanged("CameraUp");
                }
            }
        }
        public double FPS
        {
            get { return _fps; }
            set
            {
                if (_fps != value)
                {
                    _fps = value;
                    NotifyPropertyChanged("FPS");
                }
            }
        }

        public RenderWindowViewModel()
        {
            RenderIsosurfaces = false;

            Brightness = 1.0f;

            ShowCoordinateBox = false;
            CoordinateBoxTransparency = 1.0f;
            VolumeAlphas = new float[8] 
            {
                1.0f, 1.0f, 1.0f, 
                1.0f, 1.0f, 1.0f, 
                1.0f, 1.0f 
            };
            IsosurfaceValues = new float[8]
            {
                0.01f, 0.01f, 0.01f,
                0.01f, 0.01f, 0.01f,
                0.01f, 0.01f
            };

            HeightMapHeight = 1.0f;

            BackColor = new Color(0, 0, 0, 255);

            IsRenderLoaded = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class BoolVisInvertedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
        {
            try
            {
                if (value == null) return false;

                bool b = (bool)value;

                if (b)
                {
                    return System.Windows.Visibility.Collapsed;
                }

                else return System.Windows.Visibility.Visible;
            }
            catch (InvalidCastException)
            {
                return System.Windows.Visibility.Collapsed;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return System.Windows.Visibility.Collapsed;
        }
    }
    public class RenderTypeToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
        {
            try
            {
                RenderType r = (RenderType)value;

                string p = (string)parameter;

                return r.ToString() == p;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
