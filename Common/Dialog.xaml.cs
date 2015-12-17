using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for Dialog.xaml
    /// </summary>
    public partial class Dialog : Window
    {
        public static DependencyProperty TopMessageProperty = DependencyProperty.Register("TopMessage",
            typeof(string), typeof(Dialog));
        public static DependencyProperty BottomMessageProperty = DependencyProperty.Register("BottomMessage",
            typeof(string), typeof(Dialog));
        public static DependencyProperty HeaderProperty = DependencyProperty.Register("Header",
            typeof(string), typeof(Dialog));
        public static DependencyProperty IconSourceProperty = DependencyProperty.Register("IconSource",
            typeof(ImageSource), typeof(Dialog));

        public string TopMessage
        {
            get { return (string)GetValue(TopMessageProperty); }
            set { SetValue(TopMessageProperty, value); }
        }
        public string BottomMessage
        {
            get { return (string)GetValue(BottomMessageProperty); }
            set { SetValue(BottomMessageProperty, value); }
        }
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public ImageSource IconSource
        {
            get { return (ImageSource)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }

        ObservableCollection<string> _dialogOptions;
        public ObservableCollection<string> DialogOptions
        {
            get { return _dialogOptions; }
            set { _dialogOptions = value; }
        }

        public string DialogResponse;

        public Dialog()
        {
            DialogOptions = new ObservableCollection<string>();
            DialogResponse = "None";

            TopMessage = "Sample Text Top";
            BottomMessage = "Sample Text Bottom";
            Header = "Sample Header";
            IconSource = GetIconSource(DialogBoxIcon.Information);

            InitializeComponent();
        }
        public Dialog(string TopMessage, string BottomMessage, string Header, DialogBoxIcon Icon)
        {
            this.TopMessage = TopMessage;
            this.BottomMessage = BottomMessage;
            this.Header = Header;
            this.IconSource = GetIconSource(Icon);
            this.DialogOptions = new ObservableCollection<string>(DialogOptionChoices.OK);

            InitializeComponent();
        }

        private void buttonDialogOption_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b != null)
            {
                DialogResponse = b.Content.ToString();
            }

            this.Close();
        }
        private ImageSource GetIconSource(DialogBoxIcon icon)
        {
            string iconPath = "";
            switch (icon)
            {
                case DialogBoxIcon.BlueQuestion:
                    iconPath = "Images/BlueQuestion.png";
                    break;
                case DialogBoxIcon.Bubble:
                    iconPath = "Images/Bubble.png";
                    break;
                case DialogBoxIcon.CyanCheck:
                    iconPath = "Images/CyanCheck.png";
                    break;
                case DialogBoxIcon.GreenCheck:
                    iconPath = "Images/GreenCheck.png";
                    break;
                case DialogBoxIcon.Information:
                    iconPath = "Images/Information.png";
                    break;
                case DialogBoxIcon.RedQuestion:
                    iconPath = "Images/RedQuestion.png";
                    break;
                case DialogBoxIcon.Stop:
                    iconPath = "Images/Stop.png";
                    break;
                case DialogBoxIcon.Warning:
                    iconPath = "Images/Warning.png";
                    break;
                default: 
                    iconPath = "Images/Information.png";
                    break;
            }
            BitmapImage src = new BitmapImage();

            src.BeginInit();
            src.UriSource = new Uri(iconPath, UriKind.RelativeOrAbsolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            src.EndInit();

            return src;
        }

        private void msg_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TextBlock tb = sender as TextBlock;

                DataObject toDrag = new DataObject(typeof(string), tb.Text);
                DragDrop.DoDragDrop(tb, toDrag, DragDropEffects.Copy);
            }
        }
    }

    public static class DialogOptionChoices
    {
        public static string[] OK
        {
            get { return new string[1] { "OK" }; }
        }
        public static string[] OKCancel
        {
            get { return new string[2] { "OK", "Cancel" }; }
        }
        public static string[] YesNo
        {
            get { return new string[2] { "Yes", "No" }; }
        }
        public static string[] Custom(params string[] CustomOptions)
        {
            return CustomOptions;
        }
    }
}
