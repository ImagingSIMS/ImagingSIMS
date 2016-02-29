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
    public class Data2DDisplayBatchApplyViewModel : INotifyPropertyChanged
    {
        ColorScaleTypes _colorScale;
        Color _solidColorScale;

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

        public Data2DDisplayBatchApplyViewModel()
        {
            ColorScale = ColorScaleTypes.ThermalWarm;
            SolidColorScale = Color.FromArgb(255, 255, 255, 255);
        }
        public Data2DDisplayBatchApplyViewModel(ColorScaleTypes ColorScale)
        {
            this.ColorScale = ColorScale;
            this.SolidColorScale = Color.FromArgb(255, 255, 255, 255);
        }
        public Data2DDisplayBatchApplyViewModel(Color SolidColorScale)
        {
            this.ColorScale = ColorScaleTypes.Solid;
            this.SolidColorScale = SolidColorScale;
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
