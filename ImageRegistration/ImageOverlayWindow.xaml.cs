using System;
using System.Collections.Generic;
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

namespace ImagingSIMS.ImageRegistration
{
    /// <summary>
    /// Interaction logic for ImageOverlayWindow.xaml
    /// </summary>
    public partial class ImageOverlayWindow : Window
    {
        public static readonly DependencyProperty IsDebugProperty = DependencyProperty.Register("IsDebug",
            typeof(bool), typeof(ImageOverlayWindow));
        public static readonly DependencyProperty FixedImageSourceProperty = DependencyProperty.Register("FixedImageSource",
            typeof(BitmapSource), typeof(ImageOverlayWindow));
        public static readonly DependencyProperty MovingImageSourceProperty = DependencyProperty.Register("MovingImageSource",
            typeof(BitmapSource), typeof(ImageOverlayWindow));

        public bool IsDebug
        {
            get { return (bool)GetValue(IsDebugProperty); }
            set { SetValue(IsDebugProperty, value); }
        }
        public BitmapSource FixedImageSource
        {
            get { return (BitmapSource)GetValue(FixedImageSourceProperty); }
            set { SetValue(FixedImageSourceProperty, value); }
        }
        public BitmapSource MovingImageSource
        {
            get { return (BitmapSource)GetValue(MovingImageSourceProperty); }
            set { SetValue(MovingImageSourceProperty, value); }
        }

        public event ParametersGeneratedEventHandler ParametersGenerated;        

        public ImageOverlayWindow()
        {
            InitializeComponent();

#if DEBUG
            IsDebug = true;
#endif

            ImageOverlayWindowManager.AddWindow(this);
        }

        private void generate_Click(object sender, RoutedEventArgs e)
        {
            if (MovingImageSource == null || FixedImageSource == null)
                return;

            // Need to scale the offset to the actual number of pixels in the image. Since the lower res
            // will eventually be upscaled to match the high res, I use the moving image for the scaling
            // Also, the translation needs to be scaled to match the scale transform
            double translationX = transformTranslate.X * MovingImageSource.Width / movingImage.ActualWidth / transformScale.ScaleX;
            double translationY = transformTranslate.Y * MovingImageSource.Height / movingImage.ActualHeight / transformScale.ScaleX;

            RegistrationParameters parameters = new RegistrationParameters()
            {
                // ITK angle is the negative of what the ScaleTransform is set to
                Angle = -transformRotate.Angle,
                
                // In addition, the direction of the translation is the opposite in ITK
                TranslationX = -translationX,
                TranslationY = -translationY,

                // ITK scale is FixedImage/MovingImage so the scale needs to be set to the inverse of the
                // ScaleTransform value
                Scale = 1 / transformScale.ScaleX,
            };

            if (ParametersGenerated != null)
            {
                ParametersGenerated(this, new ParametersGeneratedEventArgs(parameters));
            }
        }

        private void reset_Click(object sender, RoutedEventArgs e)
        {
            transformTranslate.X = 0;
            transformTranslate.Y = 0;
            transformScale.ScaleX = 1;
            transformScale.ScaleY = 1;
            transformRotate.Angle = 0;
        }
        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        bool _isMouseDown;
        bool _isTranslating;
        bool _isRotating;
        Point _lastMousePoint;
        private void movingImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isMouseDown = true;
                _isTranslating = true;
                _isRotating = false;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                _isMouseDown = true;
                _isTranslating = false;
                _isRotating = true;

            }
            else return;

            _lastMousePoint = e.GetPosition(this);

            UIElement element = (UIElement)sender;
            element.CaptureMouse();
        }
        private void movingImage_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            _isTranslating = false;
            _isRotating = false;

            UIElement element = (UIElement)sender;
            element.ReleaseMouseCapture();
        }
        private void movingImage_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
            _isTranslating = false;
            _isRotating = false;
        }
        private void movingImage_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;

            Point currentPoint = e.GetPosition(this);

            double deltaX = currentPoint.X - _lastMousePoint.X;
            double deltaY = currentPoint.Y - _lastMousePoint.Y;

            if (_isTranslating)
            {
                transformTranslate.X += deltaX;
                transformTranslate.Y += deltaY;
            }
            else if (_isRotating)
            {
                transformRotate.Angle += deltaX / 5;
            }
            else return;

            _lastMousePoint = currentPoint;
        }

        private void movingImage_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double ticks = e.Delta;
            double scale = transformScale.ScaleX;

            if (ticks > 0)
            {
                scale *= (10d / 9d);
            }
            else if (ticks < 0)
            {
                scale *= (9d / 10d);
            }

            transformScale.ScaleX = scale;
            transformScale.ScaleY = scale;
        }

        private void userControl_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ImageOverlayWindowManager.RemoveWindow(this);
        }
    }

    public class ParametersGeneratedEventArgs : EventArgs
    {
        public RegistrationParameters Paramaters;
        public ParametersGeneratedEventArgs(RegistrationParameters parameters)
        {
            Paramaters = parameters;
        }
    }
    public delegate void ParametersGeneratedEventHandler(object sender, ParametersGeneratedEventArgs e);

    public class TransformToTranslateConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string baseString = "Translation X: {0} Y: {1}";
            TranslateTransform transform = value as TranslateTransform;
            if (transform == null) 
                return string.Format(baseString, "x.xx", "x.xx");

            return string.Format(baseString, 
                transform.X.ToString("0.00"), transform.Y.ToString("0.00"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
       
        }
    }
    public class TransformToRotateConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string baseString = "Angle: {0}";
            RotateTransform transform = value as RotateTransform;
            if (transform == null)
                return string.Format(baseString, "x.xx");

            return string.Format(baseString, transform.Angle.ToString("0.00"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class TransformToScaleConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string baseString = "Scale: {0}";
            ScaleTransform transform = value as ScaleTransform;
            if (transform == null)
                return string.Format(baseString, "x.xx");

            return string.Format(baseString, transform.ScaleX.ToString("0.00"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public static class ImageOverlayWindowManager
    {
        static List<ImageOverlayWindow> _windows = new List<ImageOverlayWindow>();

        public static void AddWindow(ImageOverlayWindow window)
        {
            _windows.Add(window);
        }
        public static void RemoveWindow(ImageOverlayWindow window)
        {
            if (_windows == null) return;

            if(_windows.Contains(window))
            {
                _windows.Remove(window);
            }
        }
        public static void DisposeAll()
        {
            if (_windows == null) return;

            ImageOverlayWindow[] windows = new ImageOverlayWindow[_windows.Count];
            for (int i = 0; i < _windows.Count; i++)
            {
                windows[i] = _windows[i];
            }

            _windows.Clear();
            _windows = null;

            for (int i = 0; i < windows.Length; i++)
            {
                windows[i].Close();
                windows[i] = null;
            }
        }
    }
}
