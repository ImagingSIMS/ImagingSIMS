using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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
            typeof(Color), typeof(CollapsedColorSelector), new PropertyMetadata(new Color(), SelectedColorChanged_Callback));
        public static readonly DependencyProperty ColorViewModelProperty = DependencyProperty.Register("ColorViewModel",
            typeof(NotifiableColorViewModel), typeof(CollapsedColorSelector));
        public static readonly DependencyProperty ColorSlidersVisibleProperty = DependencyProperty.Register("ColorSlidersVisible",
            typeof(bool), typeof(CollapsedColorSelector));
        public static readonly DependencyProperty IsAlphaEnabledProperty = DependencyProperty.Register("IsAlphaEnabled",
            typeof(bool), typeof(CollapsedColorSelector));

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        public NotifiableColorViewModel ColorViewModel
        {
            get { return (NotifiableColorViewModel)GetValue(ColorViewModelProperty); }
            set { SetValue(ColorViewModelProperty, value); }
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
            IsAlphaEnabled = true;
            ColorViewModel = new NotifiableColorViewModel();
            ColorViewModel.ColorChanged += ColorViewModel_ColorChanged;
            
            InitializeComponent();
        }

        private static void SelectedColorChanged_Callback(object sender, DependencyPropertyChangedEventArgs e)
        {
            var selector = sender as CollapsedColorSelector;
            if (selector == null) return;

            try
            {
                var newColor = (Color)e.NewValue;
                selector.SelectedColorChanged_Callback(newColor);
            }
            catch (Exception)
            {
                return;
            }
        }
        private void SelectedColorChanged_Callback(Color newColor)
        {
            if(newColor != ColorViewModel.Color)
            {
                ColorViewModel.A = newColor.A;
                ColorViewModel.R = newColor.R;
                ColorViewModel.G = newColor.G;
                ColorViewModel.B = newColor.B;
            }
        }
        private void ColorViewModel_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            SelectedColor = e.NewColor;
        }
    }

    public class NotifiableColorViewModel : INotifyPropertyChanged
    {
        byte _a;
        byte _r;
        byte _g;
        byte _b;

        Color _previousColor;

        public byte A
        {
            get { return _a; }
            set
            {
                if(_a != value)
                {
                    _a = value;
                    NotifyPropertyChanged();
                    NotifyColorChanged();
                }
            }
        }
        public byte R
        {
            get { return _r; }
            set
            {
                if(_r != value)
                {
                    _r = value;
                    NotifyPropertyChanged();
                    NotifyColorChanged();

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
                    NotifyPropertyChanged();
                    NotifyColorChanged();

                }
            }
        }
        public byte B
        {
            get { return _b; }
            set
            {
                if(_b != value)
                {
                    _b = value;
                    NotifyPropertyChanged();
                    NotifyColorChanged();

                }
            }
        }

        public Color Color
        {
            get { return Color.FromArgb(A, R, G, B); }
        }

        public NotifiableColorViewModel()
        {
            A = 255;
            R = 0;
            G = 0;
            B = 0;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public event ColorChangedEventHandler ColorChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyColorChanged()
        {
            var newColor = Color;
            if (newColor == _previousColor) return;

            ColorChanged?.Invoke(this, new ColorChangedEventArgs(_previousColor, newColor));

            _previousColor = newColor;
        }
    }

    public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs e);
    public class ColorChangedEventArgs : EventArgs
    {
        public Color OldColor { get; set; }
        public Color NewColor { get; set; }

        public ColorChangedEventArgs(Color oldColor, Color newColor)
            : base()
        {
            OldColor = oldColor;
            NewColor = newColor;
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
}
