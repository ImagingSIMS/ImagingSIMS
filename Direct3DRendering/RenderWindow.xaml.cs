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
using Direct3DRendering.ViewModels;
using ImagingSIMS.Common.Controls;
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
            typeof(RenderingViewModel), typeof(RenderWindow));
        public RenderingViewModel RenderWindowView
        {
            get { return (RenderingViewModel)GetValue(RenderWindowViewProperty); }
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
            RenderWindowView = new RenderingViewModel();

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

            // Set initial colors based on volume data

            // Check to see if this call is running on async thread
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                for (int i = 0; i < Volumes.Count; i++)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RenderWindowView.VolumeColors[i] = Volumes[i].Color.ToNotifiableColor();
                    });
                }
            }
            else
            {
                for (int i = 0; i < Volumes.Count; i++)
                {
                    RenderWindowView.VolumeColors[i] = Volumes[i].Color.ToNotifiableColor();
                }
            }
        
        }
        private void setData(List<RenderIsosurface> Isosurfaces)
        {
            if (Isosurfaces.Count > 8) throw new ArgumentException("Rendering is only suppoerted for a maximum of 8 volumes.");

            _renderer = new IsosurfaceRenderer(this);

            initializeRenderer();

            ((IsosurfaceRenderer)_renderer).SetData(Isosurfaces);

            // Set initial colors

            // Check to see if this call is running on an async thread
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                for (int i = 0; i < Isosurfaces.Count; i++)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RenderWindowView.VolumeColors[i] = Isosurfaces[i].InitialColor.ToNotifiableColor();
                    });
                }
            }
            else
            {
                for (int i = 0; i < Isosurfaces.Count; i++)
                {
                    RenderWindowView.VolumeColors[i] = Isosurfaces[i].InitialColor.ToNotifiableColor();
                }
            }
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
}
