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
using ImagingSIMS.Controls.ViewModels;
using ImagingSIMS.ImageRegistration;

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for RegistrationParametersControl.xaml
    /// </summary>
    public partial class RegistrationParametersControl : UserControl
    {
        public static readonly DependencyProperty RegistrationParametersProperty = DependencyProperty.Register("RegistrationParameters",
            typeof(RegistrationParametersViewModel), typeof(RegistrationParametersControl));

        public RegistrationParametersViewModel RegistrationParameters
        {
            get { return (RegistrationParametersViewModel)GetValue(RegistrationParametersProperty); }
            set { SetValue(RegistrationParametersProperty, value); }
        }

        public RegistrationParametersControl()
        {
            RegistrationParameters = new RegistrationParametersViewModel();

            InitializeComponent();
        }
    }
}
