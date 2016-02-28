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

namespace ImagingSIMS.Controls.BaseControls.SpectrumView
{
    /// <summary>
    /// Interaction logic for ColorGradient.xaml
    /// </summary>
    public partial class ColorGradient : UserControl
    {
        public GradientType Gradient { get; set; }

        public ColorGradient()
        {
            InitializeComponent();

            Loaded += ColorGradient_Loaded;
            SizeChanged += ColorGradient_SizeChanged;
        }

        void ColorGradient_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void ColorGradient_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
    }

    public enum GradientType { Thermal, None }
}
