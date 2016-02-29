using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImagingSIMS.Controls.ViewModels
{
    public class ClusterTabViewModel : INotifyPropertyChanged
    {
        bool _invert;
        int _pixelThreshold;
        int _intraparticleDistance;
        int _minPixelArea;
        BitmapSource _inputImageSource;

        public bool Invert
        {
            get { return _invert; }
            set
            {
                if (_invert != value)
                {
                    _invert = value;
                    NotifyPropertyChanged("Invert");
                }
            }
        }
        public int PixelThreshold
        {
            get { return _pixelThreshold; }
            set
            {
                if (_pixelThreshold != value)
                {
                    _pixelThreshold = value;
                    NotifyPropertyChanged("PixelThreshold");
                }
            }
        }
        public int IntraparticleDistance
        {
            get { return _intraparticleDistance; }
            set
            {
                if (_intraparticleDistance != value)
                {
                    _intraparticleDistance = value;
                    NotifyPropertyChanged("InterparticleDistance");
                }
            }
        }
        public int MinPixelArea
        {
            get { return _minPixelArea; }
            set
            {
                if (_minPixelArea != value)
                {
                    _minPixelArea = value;
                    NotifyPropertyChanged("MinPixelArea");
                }
            }
        }
        public BitmapSource InputImageSource
        {
            get { return _inputImageSource; }
            set
            {
                if (_inputImageSource != value)
                {
                    _inputImageSource = value;
                    NotifyPropertyChanged("InputImageSource");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ClusterTabViewModel()
        {
            Invert = false;
            PixelThreshold = 25;
            IntraparticleDistance = 25;
            MinPixelArea = 50;
        }
    }
}
