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
using System.Windows.Shapes;

namespace ImagingSIMS.ImageRegistration
{
    /// <summary>
    /// Interaction logic for RegistrationProgressWindow.xaml
    /// </summary>
    public partial class RegistrationProgressWindow : Window
    {
        public RegistrationProgressTextBox TextBox
        {
            get { return textBox; }
            set { textBox = value; }
        }

        public RegistrationProgressWindow()
        {
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
