using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ImagingSIMS.Common.Dialogs
{
    /// <summary>
    /// Interaction logic for TraceListenerWindow.xaml
    /// </summary>
    public partial class TraceListenerWindow : Window, IDisposable
    {
        public static readonly DependencyProperty TraceTextProperty = DependencyProperty.Register("TraceText",
            typeof(string), typeof(TraceListenerWindow));

        public string TraceText
        {
            get { return (string)GetValue(TraceTextProperty); }
            set { SetValue(TraceTextProperty, value); }
        }

        WindowTraceListener _traceListener;

        public WindowTraceListener TraceListener
        {
            get { return _traceListener; }
        }

        #region IDisposable
        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                if (_traceListener != null) _traceListener.Dispose();
            }
            disposed = true;
        }
        #endregion
        public TraceListenerWindow()
        {
            InitializeComponent();

            _traceListener = new WindowTraceListener();
            _traceListener.TraceTextUpdated += _traceListener_TraceTextUpdated;
        }

        void _traceListener_TraceTextUpdated(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    TraceText = ((WindowTraceListener)sender).TraceText;
                    if (textBox != null) textBox.ScrollToEnd();
                }
            ));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_traceListener != null)
            {
                Trace.Listeners.Remove(_traceListener);
                _traceListener.Dispose();
            }
        }

        private void menuItemClear_Click(object sender, RoutedEventArgs e)
        {
            _traceListener.ClearTraceText();
        }
    }

    public class WindowTraceListener : TraceListener
    {
        string _traceText;

        public event TraceTextUpdatedEventHandler TraceTextUpdated;

        public string TraceText
        {
            get { return _traceText; }
        }

        public WindowTraceListener()
            : base()
        {
            _traceText = "";
        }
        public WindowTraceListener(string Name)
            :base(Name)
        {
            _traceText = "";
        }

        public override void Write(string message)
        {
            _traceText += message;
            if (TraceTextUpdated != null) TraceTextUpdated(this, EventArgs.Empty);
        }
        public override void WriteLine(string message)
        {
            _traceText += string.Format("{0}-{1}: {2}\n", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), message);
            if (TraceTextUpdated != null) TraceTextUpdated(this, EventArgs.Empty);
        }

        public void ClearTraceText()
        {
            _traceText = "";
            if (TraceTextUpdated != null) TraceTextUpdated(this, EventArgs.Empty);
        }
    }

    public delegate void TraceTextUpdatedEventHandler(object sender, EventArgs e);
}
