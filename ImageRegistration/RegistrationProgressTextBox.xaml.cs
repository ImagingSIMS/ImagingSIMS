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

namespace ImagingSIMS.ImageRegistration
{
    /// <summary>
    /// Interaction logic for RegistrationProgressTextBox.xaml
    /// </summary>
    public partial class RegistrationProgressTextBox : UserControl
    {
        public static readonly DependencyProperty TraceTextProperty = DependencyProperty.Register("TraceText",
            typeof(string), typeof(RegistrationProgressTextBox));
        public static readonly DependencyProperty IsRegistrationCompleteProperty = DependencyProperty.Register("IsRegistrationComplete",
            typeof(bool), typeof(RegistrationProgressTextBox));

        public string TraceText
        {
            get { return (string)GetValue(TraceTextProperty); }
            set { SetValue(TraceTextProperty, value); }
        }
        public bool IsRegistrationComplete
        {
            get { return (bool)GetValue(IsRegistrationCompleteProperty); }
            set { SetValue(IsRegistrationCompleteProperty, value); }
        }

        public RegistrationProgressTextBox()
        {
            IsRegistrationComplete = false;
            fifo = new StringBuilder(2 * displaySize);

            InitializeComponent();
        }

        StringBuilder fifo;
        const int displaySize = 10000;
        public void UpdateTextBox(string s)
        {
            TraceText = getString(s);
            textBox.ScrollToEnd();
        }
        private string getString(string s)
        {
            if (s.Length > displaySize)
            {
                fifo.Clear();
                fifo.Append(s, s.Length - displaySize, displaySize);
                return fifo.ToString();
            }
            else if (fifo.Length + s.Length > fifo.Capacity)
            {
                fifo.Remove(0, fifo.Length + s.Length - displaySize);
            }
            fifo.Append(s);
            if (fifo.Length <= displaySize)
            {
                return fifo.ToString();
            }
            return fifo.ToString(fifo.Length - displaySize, displaySize);
        }
    }
}
