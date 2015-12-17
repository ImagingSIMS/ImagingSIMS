using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImagingSIMS.Common.Controls
{
    /// <summary>
    /// Interaction logic for ProgressUpdaterControl.xaml
    /// </summary>
    public partial class ProgressUpdaterControl : UserControl, IDisposable
    {
        public static readonly DependencyProperty MessageTextProperty = DependencyProperty.Register("MessageText",
            typeof(string), typeof(ProgressUpdaterControl));
        public static readonly DependencyProperty IsContinuousProperty = DependencyProperty.Register("IsContinuous",
            typeof(bool), typeof(ProgressUpdaterControl));
        public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register("Progress",
            typeof(int), typeof(ProgressUpdaterControl));
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive",
            typeof(bool), typeof(ProgressUpdaterControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, onIsActiveChanged));

        public static readonly RoutedEvent FadeInEvent = EventManager.RegisterRoutedEvent("FadeIn", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ProgressUpdaterControl));
        public static readonly RoutedEvent FadeOutEvent = EventManager.RegisterRoutedEvent("FadeOut", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ProgressUpdaterControl));

        public event RoutedEventHandler FadeIn
        {
            add { AddHandler(FadeInEvent, value); }
            remove { RemoveHandler(FadeInEvent, value); }
        }
        public event RoutedEventHandler FadeOut
        {
            add { AddHandler(FadeOutEvent, value); }
            remove { RemoveHandler(FadeOutEvent, value); }
        }

        Timer _timer;

        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }
        public bool IsContinuous
        {
            get { return (bool)GetValue(IsContinuousProperty); }
            set { SetValue(IsContinuousProperty, value); }
        }
        public int Progress
        {
            get { return (int)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public ProgressUpdaterControl()
        {
            MessageText = String.Empty;
            IsContinuous = false;
            Progress = 0;
            IsActive = false;

            _timer = new Timer(2000);
            _timer.Elapsed += _timer_Elapsed;            

            InitializeComponent();

            ProgressUpdater.RegisterControl(this);
        }

        private static void onIsActiveChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ProgressUpdaterControl control = source as ProgressUpdaterControl;
            if (source == null) return;

            control.onIsActiveChanged();
        }
        private void onIsActiveChanged()
        {
            if (IsActive)
            {
                RaiseEvent(new RoutedEventArgs(FadeInEvent, this));
            }
            else
            {
                RaiseEvent(new RoutedEventArgs(FadeOutEvent, this));
            }
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            Dispatcher.Invoke(() => 
            { 
                IsActive = false;
                
            });
        }
        
        public void FinishProgress()
        {
            _timer.Start();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_timer != null)
                        _timer.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ProgressUpdaterControl() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public static class ProgressUpdater
    {
        static List<ProgressUpdaterControl> _registeredControls = new List<ProgressUpdaterControl>();

        public static void StartProgress(string message, bool continuous = false)
        {
            foreach (ProgressUpdaterControl control in _registeredControls)
            {
                control.IsActive = true;
                control.MessageText = message;
                control.Progress = 0;
                control.IsContinuous = continuous;
            }
        }
        public static void UpdateProgress(int percentage)
        {
            foreach (ProgressUpdaterControl control in _registeredControls)
            {
                control.Progress = percentage;
            }
        }
        public static void FinishProgress(string message)
        {
            foreach (ProgressUpdaterControl control in _registeredControls)
            {
                control.MessageText = message;
                control.Progress = 100;
                control.FinishProgress();
            }
        }

        public static void RegisterControl(ProgressUpdaterControl control)
        {
            _registeredControls.Add(control);
        }
        public static void UnregisterCotnrol(ProgressUpdaterControl control)
        {
            if (control == null) return;

            if (_registeredControls.Contains(control))
            {
                _registeredControls.Remove(control);
            }
        }
    }
}
