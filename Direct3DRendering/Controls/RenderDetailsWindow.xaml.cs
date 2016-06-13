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
using ImagingSIMS.Direct3DRendering.ViewModels;
using SharpDX.Direct3D11;

namespace ImagingSIMS.Direct3DRendering.Controls
{
    /// <summary>
    /// Interaction logic for RenderDetailsWindow.xaml
    /// </summary>
    public partial class RenderDetailsWindow : Window
    {
        public static readonly DependencyProperty DeviceProperty = DependencyProperty.Register("Device",
            typeof(Device), typeof(RenderDetailsWindow));
        public static readonly DependencyProperty RenderWindowProperty = DependencyProperty.Register("RenderWindow",
            typeof(RenderWindow), typeof(RenderDetailsWindow));
        public static readonly DependencyProperty RenderWindowViewProperty = DependencyProperty.Register("RenderWindowView",
            typeof(RenderingViewModel), typeof(RenderDetailsWindow));

        public Device Device
        {
            get { return (Device)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }
        public RenderWindow RenderWindow
        {
            get { return (RenderWindow)GetValue(RenderWindowProperty); }
            set { SetValue(RenderWindowProperty, value); }
        }
        public RenderingViewModel RenderWindowView
        {
            get { return (RenderingViewModel)GetValue(RenderWindowViewProperty); }
            set { SetValue(RenderWindowViewProperty, value); }
        }

        public ScreenCaptureDisplay ScreenCaptureDisplay
        {
            get { return screenCaptureDisplay; }
            set { screenCaptureDisplay = value; }
        }

        public RenderDetailsWindow()
        {
            InitializeComponent();
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if ((Button)sender == buttonRecord)
            {
                RenderWindowView.IsRecording = true;

                buttonRecord.IsEnabled = false;
                buttonStop.IsEnabled = true;
                buttonSnap.IsEnabled = false;
            }
            else if ((Button)sender == buttonStop)
            {
                RenderWindowView.IsRecording = false;

                buttonRecord.IsEnabled = true;
                buttonStop.IsEnabled = false;
                buttonSnap.IsEnabled = true;
            }
            else if ((Button)sender == buttonSnap)
            {
                RenderWindowView.GetSnapshot = true;
            }
        }
        private void CameraReset_Click(object sender, RoutedEventArgs e)
        {
            RenderWindow.ResetCamera();
        }
        private void CameraReverse_Click(object sender, RoutedEventArgs e)
        {
            RenderWindow.ReverseCamera();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Device != null) shaderDisplay.LoadShaders(Device);
        }
    }
}
