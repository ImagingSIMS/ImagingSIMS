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

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for WaitingSpinner.xaml
    /// </summary>
    public partial class WaitingSpinner : UserControl
    {
        public static readonly DependencyProperty IsSpinningProperty = DependencyProperty.Register("IsSpinning",
            typeof(bool), typeof(WaitingSpinner));

        public bool IsSpinning
        {
            get { return (bool)GetValue(IsSpinningProperty); }
            set { SetValue(IsSpinningProperty, value); }
        }

        public WaitingSpinner()
        {
            InitializeComponent();
        }
    }
}
