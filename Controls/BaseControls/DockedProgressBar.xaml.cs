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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImagingSIMS.Controls.ViewModels;

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for WindowedProgressBar.xaml
    /// </summary>
    public partial class DockedProgressBar : UserControl
    {
        public static DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(DockedProgressBarViewModel), typeof(DockedProgressBar));

        Progress<ProgressValue> _currentProgressReporter;

        public DockedProgressBarViewModel ViewModel
        {
            get { return (DockedProgressBarViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public DockedProgressBar()
        {
            ViewModel = new DockedProgressBarViewModel();

            InitializeComponent();
        }

        public void StartProgressReporting(Progress<ProgressValue> progressReporter)
        {
            _currentProgressReporter = progressReporter;
            _currentProgressReporter.ProgressChanged += OnProgressChanged;

            ViewModel.IsProgressRunning = true;
        }

        private void OnProgressChanged(object sender, ProgressValue e)
        {
            if (!string.IsNullOrEmpty(e.Message))
            {
                ViewModel.Message = e.Message;
            }
            ViewModel.Progress = e.Progress;
        }

        public void CompleteProgressReporting()
        {
            _currentProgressReporter.ProgressChanged -= OnProgressChanged;
            _currentProgressReporter = null;

            ViewModel.IsProgressRunning = false;
        }
    }

    public class ProgressValue
    {
        public string Message { get; set; }
        public int Progress { get; set; }

        public ProgressValue(int progress)
        {
            Progress = progress;
            Message = "";
        }
        public ProgressValue(int step, int totalSteps)
        {
            Progress = (int)(step * 100d / totalSteps);
            Message = "";
        }
        public ProgressValue(int progress, string message)
        {
            Progress = progress;
            Message = message;
        }
        public ProgressValue(int step, int totalSteps, string message)
        {
            Progress = (int)(step * 100d / totalSteps);
            Message = message;
        }
    }

    public static class ApplicationProgressManager
    {
        static DockedProgressBar _progressControl;

        public static DockedProgressBar Current
        {
            get { return _progressControl; }
        }

        static ApplicationProgressManager()
        {

        }

        public static void RegisterProgressControl(DockedProgressBar progressControl)
        {
            _progressControl = progressControl;
        }
    }
}
