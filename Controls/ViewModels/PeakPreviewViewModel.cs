using System.ComponentModel;
using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.Controls.ViewModels
{
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
                if (_peak != value)
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
                if (_xAxisStart != value)
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
                if (_xAxisMid != value)
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
