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
using System.Windows.Navigation;
using System.Windows.Shapes;

using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for PeakPreview.xaml
    /// </summary>
    public partial class PeakPreview : UserControl
    {
        public static readonly DependencyProperty PeakProperty = DependencyProperty.Register("Peak",
            typeof(Peak), typeof(PeakPreview), new FrameworkPropertyMetadata(new Peak(), peakChangedCallback));
        public static readonly DependencyProperty PeakViewProperty = DependencyProperty.Register("PeakPreview",
            typeof(PeakPreviewViewModel), typeof(PeakPreview), new FrameworkPropertyMetadata(new PeakPreviewViewModel(), peakViewChangedCallback));

        public Peak Peak
        {
            get { return (Peak)GetValue(PeakProperty); }
            set { SetValue(PeakProperty, value); }
        }
        public PeakPreviewViewModel PeakView
        {
            get { return (PeakPreviewViewModel)GetValue(PeakViewProperty); }
            set { SetValue(PeakViewProperty, value); }
        }

        public PeakPreview()
        {
            InitializeComponent();
        }

        void updateGeometry()
        {
            if (PeakView == null || PeakView.Peak == null) return;

            polylinePeak.Points.Clear();

            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            double[] data = PeakView.Peak.Intensities;
            double[] masses = PeakView.Peak.Masses;

            double massRange = masses[data.Length - 1] - masses[0];
            double intensityRange = PeakView.YAxisEnd;

            // Draw spectrum
            polylinePeak.Points.Add(new Point(0, canvasHeight));
            
            for (int i = 0; i < data.Length; i++)
            {
                double x = ((masses[i] - PeakView.XAxisStart) * canvasWidth) / massRange;
                double y = canvasHeight - ((data[i] / intensityRange) * canvasHeight);

                polylinePeak.Points.Add(new Point(x, y));
            }

            polylinePeak.Points.Add(new Point(canvasWidth, canvasHeight));

            // Draw FWHM outline
            polyLineFWHM.Points.Clear();

            int fwhmStart = Peak.HalfMaxStart;
            int fwhmEnd = Peak.HalfMaxEnd;

            double fwhmXStart = ((masses[Peak.HalfMaxStart] - PeakView.XAxisStart) * canvasWidth) / massRange;
            polyLineFWHM.Points.Add(new Point(fwhmXStart, canvasHeight));

            for (int i = fwhmStart; i <= fwhmEnd; i++)
            {
                double x = ((masses[i] - PeakView.XAxisStart) * canvasWidth) / massRange;
                double y = canvasHeight - ((data[i] / intensityRange) * canvasHeight);

                polyLineFWHM.Points.Add(new Point(x, y));
            }
            
            double fwhmXEnd = ((masses[Peak.HalfMaxEnd] - PeakView.XAxisStart) * canvasWidth) / massRange;
            polyLineFWHM.Points.Add(new Point(fwhmXEnd, canvasHeight));            
        }

        static void peakViewChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PeakPreview p = d as PeakPreview;
            if (p == null) return;

            p.updateGeometry();
        }
        static void peakChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PeakPreview p = d as PeakPreview;
            if (p == null) return;

            p.PeakView.Peak = e.NewValue as Peak;
            p.updateGeometry();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            updateGeometry();
        }
    }

    public class PeakPreviewViewModel : INotifyPropertyChanged
    {
        Peak _peak;
        double _xAxisStart;
        double _xAxisEnd;
        double _yAxisStart;
        double _yAxisEnd;
        double _xAxisMid;

        public Peak Peak
        {
            get { return _peak; }
            set
            {
                if(_peak!= value)
                {
                    _peak = value;
                    setViewData(value);
                    NotifyPropertyChanged("Peak");
                }
            }
        }
        public double XAxisStart
        {
            get { return _xAxisStart; }
            set
            {
                if(_xAxisStart != value)
                {
                    _xAxisStart = value;
                    NotifyPropertyChanged("XAxisStart");
                }
            }
        }
        public double XAxisEnd
        {
            get { return _xAxisEnd; }
            set
            {
                if (_xAxisEnd != value)
                {
                    _xAxisEnd = value;
                    NotifyPropertyChanged("XAxisEnd");
                }
            }
        }
        public double YAxisStart
        {
            get { return _yAxisStart; }
            set
            {
                if (_yAxisStart != value)
                {
                    _yAxisStart = value;
                    NotifyPropertyChanged("YAxisStart");
                }
            }
        }
        public double YAxisEnd
        {
            get { return _yAxisEnd; }
            set
            {
                if (_yAxisEnd != value)
                {
                    _yAxisEnd = value;
                    NotifyPropertyChanged("YAxisEnd");
                }
            }
        }
        public double XAxisMid
        {
            get { return _xAxisMid; }
            set
            {
                if(_xAxisMid != value)
                {
                    _xAxisMid = value;
                    NotifyPropertyChanged("XAxisMid");
                }
            }
        }

        public PeakPreviewViewModel()
        {
            Peak = new Peak();
        }

        private void setViewData(Peak peak)
        {
            if (peak == null) return;

            XAxisStart = peak.Start;
            YAxisStart = 0;
            XAxisEnd = peak.End;
            YAxisEnd = peak.MaxIntensity * 1.25;
            XAxisMid = (XAxisEnd + XAxisStart) / 2;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
