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

namespace Direct3DRendering
{
    /// <summary>
    /// Interaction logic for PointLightSourceControl.xaml
    /// </summary>
    public partial class PointLightSourceControl : UserControl
    {
        public static readonly DependencyProperty PointSourceProperty = DependencyProperty.Register("PointSource",
            typeof(PointLightSource), typeof(PointLightSourceControl));

        public PointLightSource PointSource
        {
            get { return (PointLightSource)GetValue(PointSourceProperty); }
            set { SetValue(PointSourceProperty, value); }
        }
        public PointLightSourceControl()
        {
            PointSource = new PointLightSource();
            InitializeComponent();
        }
    }
}
