using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls.ViewModels
{
    public class DataDisplayBatchApplyViewModel : INotifyPropertyChanged
    {
        ColorScaleTypes _colorScale;
        Color _solidColorScale;
        int _layerStart;
        int _layerEnd;
        int _layerMinimum;
        int _layerMaximum;

        public ColorScaleTypes ColorScale
        {
            get { return _colorScale; }
            set
            {
                if (_colorScale != value)
                {
                    _colorScale = value;
                    NotifyPropertyChanged("ColorScale");
                }
            }
        }
        public Color SolidColorScale
        {
            get { return _solidColorScale; }
            set
            {
                if (_solidColorScale != value)
                {
                    _solidColorScale = value;
                    NotifyPropertyChanged("SolidColorScale");
                }
            }
        }
        public int LayerStart
        {
            get { return _layerStart; }
            set
            {
                if(_layerStart != value)
                {
                    _layerStart = value;
                    NotifyPropertyChanged("LayerStart");
                }
            }
        }
        public int LayerEnd
        {
            get { return _layerEnd; }
            set
            {
                if(_layerEnd != value)
                {
                    _layerEnd = value;
                    NotifyPropertyChanged("LayerEnd");
                }
            }
        }
        public int LayerMinimum
        {
            get { return _layerMinimum; }
            set
            {
                if(_layerMinimum != value)
                {
                    _layerMinimum = value;
                    NotifyPropertyChanged("LayerMinimum");
                }
            }
        }
        public int LayerMaximum
        {
            get { return _layerMaximum; }
            set
            {
                if(_layerMaximum != value)
                {
                    _layerMaximum = value;
                    NotifyPropertyChanged("LayerMaximum");
                }
            }
        }

        public DataDisplayBatchApplyViewModel()
        {
            ColorScale = ColorScaleTypes.ThermalWarm;
            SolidColorScale = Color.FromArgb(255, 255, 255, 255);

            LayerStart = 1;
            LayerEnd = 1;
        }
        public DataDisplayBatchApplyViewModel(ColorScaleTypes ColorScale)
        {
            this.ColorScale = ColorScale;
            this.SolidColorScale = Color.FromArgb(255, 255, 255, 255);

            LayerStart = 1;
            LayerEnd = 1;
        }
        public DataDisplayBatchApplyViewModel(Color SolidColorScale)
        {
            this.ColorScale = ColorScaleTypes.Solid;
            this.SolidColorScale = SolidColorScale;

            LayerStart = 1;
            LayerEnd = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
