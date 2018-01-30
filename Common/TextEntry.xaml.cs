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

namespace ImagingSIMS.Common.Dialogs
{
    /// <summary>
    /// Interaction logic for TextEntry.xaml
    /// </summary>
    public partial class TextEntryDialog : Window
    {
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message",
            typeof(string), typeof(TextEntryDialog));
        public static readonly DependencyProperty EnteredTextProperty = DependencyProperty.Register("EnteredText",
            typeof(string),typeof(TextEntryDialog));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }
        public string EnteredText
        {
            get { return (string)GetValue(EnteredTextProperty); }
            set { SetValue(EnteredTextProperty, value); }
        }

        public TextEntryDialog()
        {
            InitializeComponent();
        }
        public TextEntryDialog(string Message)
        {
            InitializeComponent();

            this.Message = Message;
        }
        public TextEntryDialog(string Message, string OriginalText)
        {
            InitializeComponent();

            this.Message = Message;
            EnteredText = OriginalText;

            textBoxEntry.SelectAll();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBoxEntry.Focus();
            textBoxEntry.SelectAll();
        }
    }
}
