using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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

namespace ImagingSIMS.Common.Controls
{
    /// <summary>
    /// Interaction logic for CollapsedColorSelector.xaml
    /// </summary>
    public partial class CollapsedColorSelector : UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor",
            typeof(NotifiableColor), typeof(CollapsedColorSelector));
        public static readonly DependencyProperty ColorSlidersVisibleProperty = DependencyProperty.Register("ColorSlidersVisible",
            typeof(bool), typeof(CollapsedColorSelector));
        public static readonly DependencyProperty IsAlphaEnabledProperty = DependencyProperty.Register("IsAlphaEnabled",
            typeof(bool), typeof(CollapsedColorSelector));

        public NotifiableColor SelectedColor
        {
            get { return (NotifiableColor)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        public bool ColorSlidersVisible
        {
            get { return (bool)GetValue(ColorSlidersVisibleProperty); }
            set { SetValue(ColorSlidersVisibleProperty, value); }
        }
        public bool IsAlphaEnabled
        {
            get { return (bool)GetValue(IsAlphaEnabledProperty); }
            set { SetValue(IsAlphaEnabledProperty, value); }
        }

        public CollapsedColorSelector()
        {
            SelectedColor = NotifiableColor.FromArgb(255, 0, 0, 0);

            IsAlphaEnabled = true;
            
            InitializeComponent();
        }
    }

    public class ColorToComponentScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                Color c = (Color)value;
                string p = ((string)parameter).ToLower();

                switch (p)
                {
                    case "a":
                        return new LinearGradientBrush(Color.FromArgb(0, c.R, c.G, c.B), 
                            Color.FromArgb(255, c.R, c.G, c.B), new Point(0, 0.5), new Point(1, 0.5));
                    case "r":
                        return new LinearGradientBrush(Color.FromArgb(c.A, 0, c.G, c.B),
                            Color.FromArgb(c.A, 255, c.G, c.B), new Point(0, 0.5), new Point(1, 0.5));
                    case "g":
                        return new LinearGradientBrush(Color.FromArgb(c.A, c.R, 0, c.B),
                            Color.FromArgb(c.A, c.R, 255, c.B), new Point(0, 0.5), new Point(1, 0.5));
                    case "b":
                        return new LinearGradientBrush(Color.FromArgb(c.A, c.R, c.G, 0),
                            Color.FromArgb(c.A, c.R, c.G, 255), new Point(0, 0.5), new Point(1, 0.5));
                    default:
                        return new LinearGradientBrush(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 0, 0), 0);
                }

            }
            catch (Exception)
            {
                return new LinearGradientBrush(Color.FromArgb(255, 0, 0, 0), Color.FromArgb(255, 0, 0, 0), 0);
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class BooleanToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                bool b = (bool)value;

                if (b)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
            catch (Exception)
            {
                return Visibility.Visible;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotifiableColor : INotifyPropertyChanged
    {
        byte _a;
        byte _r;
        byte _g;
        byte _b;

        public byte A
        {
            get { return _a; }
            set
            {
                if(_a != value)
                {
                    _a = value;
                    NotifyPropertyChanged("A");
                    NotifyPropertyChanged("Color");
                }
            }
        }
        public byte R
        {
            get { return _r; }
            set
            {
                if (_r != value)
                {
                    _r = value;
                    NotifyPropertyChanged("R");
                    NotifyPropertyChanged("Color");
                }
            }
        }
        public byte G
        {
            get { return _g; }
            set
            {
                if (_g != value)
                {
                    _g = value;
                    NotifyPropertyChanged("G");
                    NotifyPropertyChanged("Color");
                }
            }
        }
        public byte B
        {
            get { return _b; }
            set
            {
                if (_b != value)
                {
                    _b = value;
                    NotifyPropertyChanged("B");
                    NotifyPropertyChanged("Color");
                }
            }
        }
        public Color Color
        {
            get { return Color.FromArgb(_a, _r, _g, _b); }
            set
            {
                A = value.A;
                R = value.R;
                G = value.G;
                B = value.B; 
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static NotifiableColor FromArgb(byte a, byte r, byte g, byte b)
        {
            return new NotifiableColor()
            {
                A = a,
                R = r,
                G = g,
                B = b
            };
        }
        public static implicit operator Color(NotifiableColor c)
        {
            return Color.FromArgb(c._a, c._r, c._g, c._b);
        }
        public static implicit operator NotifiableColor(Color c)
        {
            return new NotifiableColor()
            {
                _a = c.A,
                _r = c.R,
                _g = c.G,
                _b = c.B
            };
        }

        public static NotifiableColor Black
        {
            get { return FromArgb(255, 0, 0, 0); }
        }
        public static NotifiableColor White
        {
            get
            {
                return new NotifiableColor()
                {
                    R = 255,
                    G = 255,
                    B = 255,
                    A = 255
                };
            }
        }
    }
}
