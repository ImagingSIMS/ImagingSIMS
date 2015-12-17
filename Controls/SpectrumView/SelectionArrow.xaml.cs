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

namespace ImagingSIMS.Controls.SpectrumView
{
    /// <summary>
    /// Interaction logic for SelectionArrow.xaml
    /// </summary>
    public partial class SelectionArrow : UserControl
    {
        public static readonly DependencyProperty ArrowColorProperty = DependencyProperty.Register("ArrowColor",
            typeof(SolidColorBrush), typeof(SelectionArrow));

        public SolidColorBrush ArrowColor
        {
            get { return (SolidColorBrush)GetValue(ArrowColorProperty); }
            set { SetValue(ArrowColorProperty, value); }
        }

        public SelectionArrow()
        {
            ArrowColor = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            InitializeComponent();
        }
    }
}
