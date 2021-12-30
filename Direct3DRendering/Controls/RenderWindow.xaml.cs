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
using ImagingSIMS.Direct3DRendering.DrawingObjects;
using ImagingSIMS.Direct3DRendering.Renderers;
using ImagingSIMS.Direct3DRendering.ViewModels;
using ImagingSIMS.Common.Controls;
using SharpDX;
using SharpDX.Windows;

using Color = SharpDX.Color;

namespace ImagingSIMS.Direct3DRendering.Controls
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

        public static readonly DependencyProperty IsDebugProperty = DependencyProperty.Register("IsDebug",
            typeof(bool), typeof(RenderWindow));
        public static readonly DependencyProperty RenderWindowViewProperty = DependencyProperty.Register("RenderWindowView",
            typeof(RenderingViewModel), typeof(RenderWindow));

        public bool IsDebug
        {
            get { return (bool)GetValue(IsDebugProperty); }
            set { SetValue(IsDebugProperty, value); }
        }
        public RenderingViewModel RenderWindowView
        {
            get { return (RenderingViewModel)GetValue(RenderWindowViewProperty); }
            set { SetValue(RenderWindowViewProperty, value); }
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
            IsDebug = true;
#endif
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopRendering();
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

            // Set default rendering parameters

            // Check to see if this call is running on an async thread
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RenderWindowView = RenderingViewModel.DefaultHeightMapParameters;
                });

            }
            else
            {
                RenderWindowView = RenderingViewModel.DefaultHeightMapParameters;
            }

            initializeRenderer();

            ((HeightMapRenderer)_renderer).SetData(ColorData, HeightData, new Vector3(2, 2, 0.50f));            
        }
        private void setData(params RenderVolume[] Volumes)
        {
            if (Volumes.Length > 8) throw new ArgumentException("Rendering is only supported for a maximum of 8 volumes.");

            _renderer = new VolumeRenderer(this);

            // Set default rendering parameters and
            // initial colors based on volume data

            // Check to see if this call is running on an async thread
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RenderWindowView = RenderingViewModel.DefaultVolumeParameters;
                    RenderWindowView.ScalingZ = 20;
                });
                for (int i = 0; i < Volumes.Length; i++)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RenderWindowView.VolumeColors[i] = Volumes[i].Color;
                    });
                }
            }
            else
            {
                RenderWindowView = RenderingViewModel.DefaultVolumeParameters;
                RenderWindowView.ScalingZ = 20;
                for (int i = 0; i < Volumes.Length; i++)
                {
                    RenderWindowView.VolumeColors[i] = Volumes[i].Color;
                }
            }

            initializeRenderer();

            ((VolumeRenderer)_renderer).SetData(new List<RenderVolume>(Volumes));   
        }
        private void setData(List<RenderVolume> Volumes)
        {
            if (Volumes.Count > 8) throw new ArgumentException("Rendering is only supported for a maximum of 8 volumes.");

            _renderer = new VolumeRenderer(this);

            // Set default rendering parameters and
            // initial colors based on volume data

            // Check to see if this call is running on async thread
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RenderWindowView = RenderingViewModel.DefaultVolumeParameters;
                    RenderWindowView.ScalingZ = 20;
                });
                for (int i = 0; i < Volumes.Count; i++)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RenderWindowView.VolumeColors[i] = Volumes[i].Color;
                    });
                }
            }
            else
            {
                RenderWindowView = RenderingViewModel.DefaultVolumeParameters;
                RenderWindowView.ScalingZ = 20;
                for (int i = 0; i < Volumes.Count; i++)
                {
                    RenderWindowView.VolumeColors[i] = Volumes[i].Color;
                }
            }

            initializeRenderer();

            ((VolumeRenderer)_renderer).SetData(new List<RenderVolume>(Volumes));
        }
        private void setData(List<RenderIsosurface> Isosurfaces)
        {
            if (Isosurfaces.Count > 8) throw new ArgumentException("Rendering is only supported for a maximum of 8 volumes.");

            _renderer = new IsosurfaceRenderer(this);

            // Set default rendering parameters and
            // initial colors based on isosurface data

            // Check to see if this call is running on an async thread
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RenderWindowView = RenderingViewModel.DefaultIsosurfaceParameters;
                    RenderWindowView.ScalingZ = 1;
                });
                for (int i = 0; i < Isosurfaces.Count; i++)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RenderWindowView.VolumeColors[i] = Isosurfaces[i].InitialColor;
                    });
                }
            }
            else
            {
                RenderWindowView = RenderingViewModel.DefaultIsosurfaceParameters;
                RenderWindowView.ScalingZ = 1;
                for (int i = 0; i < Isosurfaces.Count; i++)
                {
                    RenderWindowView.VolumeColors[i] = Isosurfaces[i].InitialColor;
                }
            }

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

        public void OnZScalingChanged()
        {
            if (Renderer is VolumeRenderer)
            {
                Dispatcher.Invoke(() =>
                {
                    ((VolumeRenderer)Renderer).CreateVolumeVertices(RenderWindowView.ScalingZ);
                });                
            }
            else if (Renderer is IsosurfaceRenderer)
            {
                Dispatcher.Invoke(() =>
                {
                    ((IsosurfaceRenderer)Renderer).CreateVolumeVertices(RenderWindowView.ScalingZ);
                });
            }
        }

        public void SetTransferFunction(byte[] Function)
        {

        }
        public void BeginRendering()
        {
            _framerate = new FramerateCounter();

            RenderWindowView.IsRenderLoaded = true;

            if (IsWindowActivated)
            {
                _renderer.EnsureInputAcquired();
            }

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

        public static readonly DependencyProperty IsWindowFocusedProperty = DependencyProperty.Register("IsWindowFocused",
            typeof(bool), typeof(RenderWindow));
        public static readonly DependencyProperty IsWindowActivatedProperty = DependencyProperty.Register("IsWindowActivated",
            typeof(bool), typeof(RenderWindow), new PropertyMetadata(false, onWindowActivatedChanged));

        internal event WindowActivatedChangedEventHandler WindowActivatedChanged;

        public bool IsWindowFocused
        {
            get { return (bool)GetValue(IsWindowFocusedProperty); }
            set { SetValue(IsWindowFocusedProperty, value); }
        }
        public bool IsWindowActivated
        {
            get { return (bool)GetValue(IsWindowActivatedProperty); }
            set { SetValue(IsWindowActivatedProperty, value); ; }
        }

        private static void onWindowActivatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RenderWindow r = d as RenderWindow;
            if (d == null) return;

            bool isActivated = (bool)e.NewValue;

            r.WindowActivatedChanged?.Invoke(r, new WindowActivatedChangedEventArgs(isActivated));
        }

        private void renderWindow_Activated(object sender, EventArgs e)
        {
            IsWindowActivated  = true;
        }
        private void renderWindow_Deactivated(object sender, EventArgs e)
        {
            IsWindowActivated = false;
        }
        private void Window_LostFocus(object sender, RoutedEventArgs e)
        {
            IsWindowFocused = false;
        }
        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            IsWindowFocused = true;
        }
    }

    internal delegate void WindowActivatedChangedEventHandler(object sender, WindowActivatedChangedEventArgs e);
    internal class WindowActivatedChangedEventArgs : EventArgs
    {
        internal bool IsWindowActivated { get; set; }

        internal WindowActivatedChangedEventArgs(bool isWindowActivated):
            base()
        {
            IsWindowActivated = isWindowActivated;
        }
    }
}
