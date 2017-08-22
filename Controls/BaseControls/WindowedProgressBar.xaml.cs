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
    public partial class WindowedProgressBar : UserControl
    {
        public static DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel",
            typeof(WindowedProgressBarViewModel), typeof(WindowedProgressBar));
        public static DependencyProperty IsProgressRunningProperty = DependencyProperty.Register("IsProgressRunning",
            typeof(bool), typeof(WindowedProgressBar));

        IProgress<int> _currentProgressReporter;

        public WindowedProgressBarViewModel ViewModel
        {
            get { return (WindowedProgressBarViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public bool IsProgressRunning
        {
            get { return (bool)GetValue(IsProgressRunningProperty); }
            set { SetValue(IsProgressRunningProperty, value); }
        }

        public WindowedProgressBar()
        {
            ViewModel = new WindowedProgressBarViewModel();

            InitializeComponent();
        }

        public async Task StartProgressAsync(IProgress<int> progressReporter)
        {
            _currentProgressReporter = progressReporter;

            //_currentProgressReporter
        }
    }
}
