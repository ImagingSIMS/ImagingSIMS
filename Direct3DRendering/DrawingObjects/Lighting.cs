using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Common.Controls;
using SharpDX;

namespace ImagingSIMS.Direct3DRendering.DrawingObjects
{
    public class PointLightSource : INotifyPropertyChanged
    {
        bool _isEnabled;
        Vector4 _location;
        NotifiableColor _color;
        float _intensity;

        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                if(_isEnabled != value)
                {
                    _isEnabled = value;
                    NotifyPropertyChanged("IsEnabled");
                }
            }
        }
        public Vector4 Location
        {
            get { return _location; }
            set
            {
                if(_location != value)
                {
                    _location = value;
                    NotifyPropertyChanged("Location");
                }
            }
        }
        public NotifiableColor Color
        {
            get { return _color; }
            set
            {
                if(_color != value)
                {
                    _color = value;
                    NotifyPropertyChanged("Color");
                }
            }
        }
        public float Intensity
        {
            get { return _intensity; }
            set
            {
                if (_intensity != value)
                {
                    _intensity = value;
                    NotifyPropertyChanged("Intensity");
                }
            }
        }

        public string LocationString
        {
            get { return $"X: {_location.X.ToString("+0;-#")} Y: {_location.Y.ToString("+0;-#")} Z: {_location.Z.ToString("+0;-#")}"; }
        }

        public PointLightSource()
        {
            Location = new Vector4(0);
            Color = NotifiableColor.White;
        }
        public PointLightSource(Vector4 location)
        {
            Location = location;
            Color = NotifiableColor.White;
        }
        public PointLightSource(Vector4 location, float intensity)
        {
            Location = location;
            Intensity = intensity;
            Color = NotifiableColor.White;
        }
        public PointLightSource(Vector4 location, float intensity, bool isEnabled)
        {
            IsEnabled = true;
            Location = location;
            Intensity = intensity;
            Color = NotifiableColor.White;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
