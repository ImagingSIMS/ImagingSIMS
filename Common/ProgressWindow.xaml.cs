using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImagingSIMS.Common.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        bool continuous;

        public ProgressWindow(string Message, string Header)
        {
            InitializeComponent();

            this.Title = Header;
            msg.Text = Message;

            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.LargeChange = 1;

            progressBar.Value = progressBar.Minimum;
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;

            Loaded += ProgressWindow_Loaded;
            SizeChanged += ProgressWindow_SizeChanged;

            buttonOK.IsEnabled = false;

            ProgressWindowManager.AddToInstances(this);
        }        
        public ProgressWindow(string Message, string Header, bool ContinuousStyle)
        {
            InitializeComponent();

            this.Title = Header;
            msg.Text = Message;

            continuous = ContinuousStyle;

            if (ContinuousStyle)
            {
                progressBar.IsIndeterminate = true;
                TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
            }
            else
            {                
                progressBar.Minimum = 0;
                progressBar.Maximum = 100;
                progressBar.LargeChange = 1;

                progressBar.Value = progressBar.Minimum;
                TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;
            }

            Loaded += ProgressWindow_Loaded;
            SizeChanged += ProgressWindow_SizeChanged;

            buttonOK.IsEnabled = false;

            ProgressWindowManager.AddToInstances(this);
        }
        public ProgressWindow(string Message, string Header, int ProgressMin, int ProgressMax, int ProgressStep)
        {
            InitializeComponent();

            this.Title = Header;
            msg.Text = Message;

            progressBar.Minimum = ProgressMin;
            progressBar.Maximum = ProgressMax;
            progressBar.LargeChange = ProgressStep;

            progressBar.Value = progressBar.Minimum;
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Normal;

            Loaded += ProgressWindow_Loaded;
            SizeChanged += ProgressWindow_SizeChanged;

            buttonOK.IsEnabled = false;

            ProgressWindowManager.AddToInstances(this);
        }

        void ProgressWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void ProgressWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DialogResult = true;
                this.Close();
            }
            catch (InvalidOperationException)
            {
                this.Close();
            }
        }

        public void UpdateProgress(int Percentage)
        {
            progressBar.Value = Percentage;
            TaskbarItemInfo.ProgressValue = (double)Percentage / 100d;
            
        }
        public void UpdateProgress(ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            TaskbarItemInfo.ProgressValue = (double)e.ProgressPercentage / 100d;
        }
        public void UpdateMessage(string Message)
        {
            msg.Text = Message;
        }

        public void ProgressFinished()
        {
            if (continuous)
            {
                progressBar.IsIndeterminate = false;
                progressBar.Value = progressBar.Maximum;
            }
            this.Close();
        }
        public void ProgressFinished(string Message)
        {
            msg.Text = Message;
            if (continuous)
            {
                progressBar.IsIndeterminate = false;
                progressBar.Value = progressBar.Maximum;
            }
            else
            {
                progressBar.Value = progressBar.Maximum;
            }
            buttonOK.IsEnabled = true;
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            TaskbarItemInfo.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;

            ProgressWindowManager.RemoveFromInstances(this);
        }
    }

    public static class ProgressWindowManager
    {
        static List<ProgressWindow> _instances = new List<ProgressWindow>();

        public static void AddToInstances(ProgressWindow instance)
        {
            _instances.Add(instance);
        }

        public static void RemoveFromInstances(ProgressWindow instance)
        {
            if (_instances == null) return;

            if (_instances.Contains(instance))
            {
                _instances.Remove(instance);
            }
        }

        public static void DisposeAll()
        {
            if (_instances == null) return;

            ProgressWindow[] instances = new ProgressWindow[_instances.Count];
            for (int i = 0; i < _instances.Count; i++)
            {
                instances[i] = _instances[i];
            }

            _instances.Clear();
            _instances = null;

            for (int i = 0; i < instances.Length; i++)
            {
                instances[i].Close();
                instances[i] = null;
            }

        }
    }
}
