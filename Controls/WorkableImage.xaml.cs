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
    /// Interaction logic for WorkableImage.xaml
    /// </summary>
    public partial class WorkableImage : UserControl
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource",
            typeof(ImageSource), typeof(WorkableImage));
        public static readonly DependencyProperty IsWorkingProperty = DependencyProperty.Register("IsWorking",
            typeof(bool), typeof(WorkableImage));

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
        public bool IsWorking
        {
            get { return (bool)GetValue(IsWorkingProperty); }
            set { SetValue(IsWorkingProperty, value); }
        }

        public WorkableImage()
        {
            InitializeComponent();
        }
    }
}
