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
        public static DependencyProperty IconTypeProperty = DependencyProperty.Register("IconType",
            typeof(DialogIcon), typeof(Dialog));
        public static DependencyProperty LinkTargetProperty = DependencyProperty.Register("LinkTarget",
            typeof(string), typeof(Dialog));

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
        public DialogIcon IconType
        {
            get { return (DialogIcon)GetValue(IconTypeProperty); }
            set { SetValue(IconTypeProperty, value); }
        }
        public string LinkTarget
        {
            get { return (string)GetValue(LinkTargetProperty); }
            set { SetValue(LinkTargetProperty, value); }
        }

        ObservableCollection<string> _dialogOptions;
        public ObservableCollection<string> DialogOptions
        {
            get { return _dialogOptions; }
            set { _dialogOptions = value; }
        }

        public string DialogResponse;

        public Dialog(string topMessage, string bottomMessage, string header, DialogIcon icon)
        {
            this.TopMessage = topMessage;
            this.BottomMessage = bottomMessage;
            this.Header = header;
            this.IconType = icon;
            this.DialogOptions = new ObservableCollection<string>(Dialogs.DialogOptions.OK);

            InitializeComponent();
        }
        public Dialog(string topMessage, string bottomMessage, string header, DialogIcon icon, ObservableCollection<string> dialogOptions)
        {
            this.TopMessage = topMessage;
            this.BottomMessage = bottomMessage;
            this.Header = header;
            this.IconType = icon;
            this.DialogOptions = dialogOptions;

            InitializeComponent();
        }
        public Dialog(string topMessage, string bottomMessage, string header, DialogIcon icon, List<string> dialogOptions)
        {
            this.TopMessage = topMessage;
            this.BottomMessage = bottomMessage;
            this.Header = header;
            this.IconType = icon;
            this.DialogOptions = new ObservableCollection<string>(dialogOptions);

            InitializeComponent();
        }
        public Dialog(string topMessage, string bottomMessage, string header, DialogIcon icon, string[] dialogOptions)
        {
            this.TopMessage = topMessage;
            this.BottomMessage = bottomMessage;
            this.Header = header;
            this.IconType = icon;
            this.DialogOptions = new ObservableCollection<string>(dialogOptions);

            InitializeComponent();
        }
        public Dialog(string topMessage, string bottomMessage, string header, DialogIcon icon, string linkTarget)
        {
            this.TopMessage = topMessage;
            this.BottomMessage = bottomMessage;
            this.Header = header;
            this.IconType = icon;
            this.DialogOptions = new ObservableCollection<string>(Dialogs.DialogOptions.OK);
            this.LinkTarget = linkTarget;

            InitializeComponent();
        }
        public Dialog(string topMessage, string bottomMessage, string header, DialogIcon icon, ObservableCollection<string> dialogOptions, string linkTarget)
        {
            this.TopMessage = topMessage;
            this.BottomMessage = bottomMessage;
            this.Header = header;
            this.IconType = icon;
            this.DialogOptions = dialogOptions;
            this.LinkTarget = linkTarget;

            InitializeComponent();
        }
        public Dialog(string topMessage, string bottomMessage, string header, DialogIcon icon, List<string> dialogOptions, string linkTarget)
        {
            this.TopMessage = topMessage;
            this.BottomMessage = bottomMessage;
            this.Header = header;
            this.IconType = icon;
            this.DialogOptions = new ObservableCollection<string>(dialogOptions);
            this.LinkTarget = linkTarget;

            InitializeComponent();
        }
        public Dialog(string topMessage, string bottomMessage, string header, DialogIcon icon, string[] dialogOptions, string linkTarget)
        {
            this.TopMessage = topMessage;
            this.BottomMessage = bottomMessage;
            this.Header = header;
            this.IconType = icon;
            this.DialogOptions = new ObservableCollection<string>(dialogOptions);
            this.LinkTarget = linkTarget;

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
        private void msg_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TextBlock tb = sender as TextBlock;

                DataObject toDrag = new DataObject(typeof(string), tb.Text);
                DragDrop.DoDragDrop(tb, toDrag, DragDropEffects.Copy);
            }
        }

        public static string Show(string topMessage, string bottomMessage, string header, DialogIcon icon)
        {
            Dialog d = new Dialog(topMessage, bottomMessage, header, icon);
            d.ShowDialog();
            return d.DialogResponse;
        }
        public static string Show(string topMessage, string bottomMessage, string header, DialogIcon icon, List<string> dialogOptions)
        {
            Dialog d = new Dialog(topMessage, bottomMessage, header, icon, dialogOptions);
            d.ShowDialog();
            return d.DialogResponse;
        }
        public static string Show(string topMessage, string bottomMessage, string header, DialogIcon icon, ObservableCollection<string> dialogOptions)
        {
            Dialog d = new Dialog(topMessage, bottomMessage, header, icon, dialogOptions);
            d.ShowDialog();
            return d.DialogResponse;
        }
        public static string Show(string topMessage, string bottomMessage, string header, DialogIcon icon, string[] dialogOptions)
        {
            Dialog d = new Dialog(topMessage, bottomMessage, header, icon, dialogOptions);
            d.ShowDialog();
            return d.DialogResponse;
        }
        public static string Show(string topMessage, string bottomMessage, string header, DialogIcon icon, string linkTarget)
        {
            Dialog d = new Dialog(topMessage, bottomMessage, header, icon, linkTarget);
            d.ShowDialog();
            return d.DialogResponse;
        }
        public static string Show(string topMessage, string bottomMessage, string header, DialogIcon icon, List<string> dialogOptions, string linkTarget)
        {
            Dialog d = new Dialog(topMessage, bottomMessage, header, icon, dialogOptions, linkTarget);
            d.ShowDialog();
            return d.DialogResponse;
        }
        public static string Show(string topMessage, string bottomMessage, string header, DialogIcon icon, ObservableCollection<string> dialogOptions, string linkTarget)
        {
            Dialog d = new Dialog(topMessage, bottomMessage, header, icon, dialogOptions, linkTarget);
            d.ShowDialog();
            return d.DialogResponse;
        }
        public static string Show(string topMessage, string bottomMessage, string header, DialogIcon icon, string[] dialogOptions, string linkTarget)
        {
            Dialog d = new Dialog(topMessage, bottomMessage, header, icon, dialogOptions, linkTarget);
            d.ShowDialog();
            return d.DialogResponse;
        }

        public static class HeightMapDropDialog
        {
            public static string Show()
            {
                Dialog d = new Dialog("You are attempting to drop an image into a depth rendering.",
                    "Is this height data or color data?", "Height Map", DialogIcon.Help, 
                    Dialogs.DialogOptions.Custom("Height Data", "Color Data", "Cancel"));
                d.ShowDialog();
                return d.DialogResponse;
            }
        }
        public static class FusionDropDialog
        {
            public static string Show()
            {
                Dialog d = new Dialog("You are attempting to drop an image into a fusion tab.",
                    "Is this a high or low resolution image?", "Fusion", DialogIcon.Help,
                    Dialogs.DialogOptions.Custom("High Res", "Low Res", "Cancel"));
                d.ShowDialog();
                return d.DialogResponse;
            }
        }
        public static class RegistrationDropDialog
        {
            public static string Show()
            {
                Dialog d = new Dialog("You are attempting to drop an image into a transform tab.",
                    "Is this a moving or a fixed image?", "Registration", DialogIcon.Help,
                    Dialogs.DialogOptions.Custom("Moving", "Fixed", "Cancel"));
                d.ShowDialog();
                return d.DialogResponse;
            }
        }
        public static class Workspace
        {
            public static string Show()
            {
                Dialog d = new Dialog("The current workspace is already in use.",
                    "Do you wish to overwrite or merge the current data with the saved workspace?", "Load Workspace", DialogIcon.Help,
                    Dialogs.DialogOptions.Custom("Overwrite", "Merge", "Cancel"));
                d.ShowDialog();
                return d.DialogResponse;
            }
        }
    }

    public static class DialogOptions
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
