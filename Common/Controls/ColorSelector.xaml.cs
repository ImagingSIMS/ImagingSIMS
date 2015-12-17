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

namespace ImagingSIMS.Common.Controls
{
    /// <summary>
    /// Interaction logic for ColorSelector.xaml
    /// </summary>
    public partial class ColorSelector : UserControl
    {
        public static readonly DependencyProperty RProperty = DependencyProperty.Register("R",
            typeof(byte), typeof(ColorSelector), new FrameworkPropertyMetadata((byte)0, FrameworkPropertyMetadataOptions.None,
                new PropertyChangedCallback(OnValueChanged)));
        public static readonly DependencyProperty GProperty = DependencyProperty.Register("G",
            typeof(byte), typeof(ColorSelector), new FrameworkPropertyMetadata((byte)0, FrameworkPropertyMetadataOptions.None,
                new PropertyChangedCallback(OnValueChanged)));
        public static readonly DependencyProperty BProperty = DependencyProperty.Register("B",
            typeof(byte), typeof(ColorSelector), new FrameworkPropertyMetadata((byte)0, FrameworkPropertyMetadataOptions.None,
                new PropertyChangedCallback(OnValueChanged)));
        public static readonly DependencyProperty AProperty = DependencyProperty.Register("A",
            typeof(byte), typeof(ColorSelector), new FrameworkPropertyMetadata((byte)255, FrameworkPropertyMetadataOptions.None,
                new PropertyChangedCallback(OnValueChanged)));
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor",
            typeof(Color), typeof(ColorSelector), new FrameworkPropertyMetadata(Color.FromArgb(255, 0, 0, 0),
                FrameworkPropertyMetadataOptions.None, new PropertyChangedCallback(OnColorChanged)));
        public static readonly DependencyProperty IsAlphaEnabledProeprty = DependencyProperty.Register("IsAlphaEnabled",
            typeof(bool), typeof(ColorSelector));

        public byte R
        {
            get { return (byte)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }
        public byte G
        {
            get { return (byte)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }
        public byte B
        {
            get { return (byte)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }
        public byte A
        {
            get { return (byte)GetValue(AProperty); }
            set { SetValue(AProperty, value); }
        }

        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        public bool IsAlphaEnabled
        {
            get { return (bool)GetValue(IsAlphaEnabledProeprty); }
            set { SetValue(IsAlphaEnabledProeprty, value); }
        }
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector c = d as ColorSelector;
            if (c == null) return;

            c.Update();
        }
        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorSelector c = d as ColorSelector;
            if (c == null) return;

            c.SetColor(c.SelectedColor);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ColorSelector()
        {
            this.InitializeComponent();

            SelectedColor = Color.FromArgb(255, 0, 0, 0);

            IsAlphaEnabled = true;

            Loaded += ColorPicker_Loaded;
            SizeChanged += ColorPicker_SizeChanged;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ColorSelector(Color Original)
        {
            this.InitializeComponent();

            SelectedColor = Original;

            IsAlphaEnabled = true;

            Loaded += ColorPicker_Loaded;
            SizeChanged += ColorPicker_SizeChanged;
        }

        void ColorPicker_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            Update();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        public void LockAlpha()
        {
            IsAlphaEnabled = false;
        }
        public void LockAlpha(int Value)
        {
            slideA.Value = Value;
            IsAlphaEnabled = false;
        }
        public void UnlockAlpha()
        {
            IsAlphaEnabled = true;
        }

        public void SetColor(Color Color)
        {
            A = Color.A;
            R = Color.R;
            G = Color.G;
            B = Color.B;
        }
        private void Update()
        {
            LinearGradientBrush lgR = new LinearGradientBrush(Color.FromArgb(SelectedColor.A, (byte)0, SelectedColor.G, SelectedColor.B),
                Color.FromArgb(SelectedColor.A, (byte)255, SelectedColor.G, SelectedColor.B),
                new Point(0, 0.5), new Point(1, 0.5));
            LinearGradientBrush lgG = new LinearGradientBrush(Color.FromArgb(SelectedColor.A, SelectedColor.R, (byte)0, SelectedColor.B),
                Color.FromArgb(SelectedColor.A, SelectedColor.R, (byte)255, SelectedColor.B),
                new Point(0, 0.5), new Point(1, 0.5));
            LinearGradientBrush lgB = new LinearGradientBrush(Color.FromArgb(SelectedColor.A, SelectedColor.R, SelectedColor.G, (byte)0),
                Color.FromArgb(SelectedColor.A, SelectedColor.R, SelectedColor.G, (byte)255),
                new Point(0, 0.5), new Point(1, 0.5));
            LinearGradientBrush lgA = new LinearGradientBrush(Color.FromArgb((byte)0, SelectedColor.R, SelectedColor.G, SelectedColor.B),
                Color.FromArgb((byte)255, SelectedColor.R, SelectedColor.G, SelectedColor.B),
                new Point(0, 0.5), new Point(1, 0.5));

            slideR.Background = lgR;
            slideG.Background = lgG;
            slideB.Background = lgB;
            slideA.Background = lgA;

            SelectedColor = Color.FromArgb((byte)A, (byte)R, (byte)G, (byte)B);
            //rect.SetColor(SelectedColor);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            tbA.GotFocus += tb_GotFocus;
            tbR.GotFocus += tb_GotFocus;
            tbG.GotFocus += tb_GotFocus;
            tbB.GotFocus += tb_GotFocus;
        }

        void tb_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }
    }

    public class ColorChannelValidation : ValidationRule
    {
        public ColorChannelValidation()
        {
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int cValue = 0;

            if (((string)value).Length > 0)
            {
                if (!int.TryParse((string)value, out cValue))
                {
                    return new ValidationResult(false, "Invalid input: Non-integer value.");
                }
            }
            else
            {
                return new ValidationResult(false, "No input.");
            }

            if (cValue < 0)
            {
                return new ValidationResult(false, "Value must be greater than 0.");
            }
            else if (cValue > 255)
            {
                return new ValidationResult(false, "Value must be less than or equal to 255");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
