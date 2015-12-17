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

namespace ImagingSIMS.Common.Controls
{
    /// <summary>
    /// Interaction logic for ColorRectangle.xaml
    /// </summary>
    public partial class PreviewRectangle : UserControl
    {
        public static DependencyProperty PreviewColorProperty = DependencyProperty.Register("PreviewColor",
            typeof(Color), typeof(PreviewRectangle));

        public Color PreviewColor
        {
            get { return (Color)GetValue(PreviewColorProperty); }
            set { SetValue(PreviewColorProperty, value); }
        }

        public PreviewRectangle()
        {
            InitializeComponent();
        }
    }
}
