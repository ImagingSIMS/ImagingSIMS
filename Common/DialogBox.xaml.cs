using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Drawing;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImagingSIMS.Common.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox : Window
    {
        public DialogBox()
        {
        }
        public DialogBox(string Message1, string Message2, string Header, DialogBoxIcon Icon)
        {
            InitializeComponent();
            displayIcon.BorderVisibility = Visibility.Hidden;
            displayIcon.SetImage(GetDialogIcon(Icon));

            this.Title = Header;
            msg1.Text = Message1;
            msg2.Text = Message2;

            Loaded += DialogBox_Loaded;
            SizeChanged += DialogBox_SizeChanged;

            HideCancel();
        }        
        public DialogBox(string Message1, string Message2, string Header, DialogBoxIcon Icon, bool ShowCancel)
        {
            InitializeComponent();
            displayIcon.BorderVisibility = Visibility.Hidden;
            displayIcon.SetImage(GetDialogIcon(Icon));

            this.Title = Header;
            msg1.Text = Message1;
            msg2.Text = Message2;

            Loaded += DialogBox_Loaded;
            SizeChanged += DialogBox_SizeChanged;

            if (!ShowCancel)
            {
                HideCancel();
            }
        }

        void DialogBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        void DialogBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        private Bitmap GetDialogIcon(DialogBoxIcon Type)
        {
            Uri baseUri = new Uri("pack://application:,,,/Resources/Resources.resx");
            switch (Type)
            {
                case DialogBoxIcon.BlueQuestion:
                    return Properties.Resources.BlueQuestion;
                case DialogBoxIcon.Bubble:
                    return Properties.Resources.Bubble;
                case DialogBoxIcon.CyanCheck:
                    return Properties.Resources.CyanCheck;
                case DialogBoxIcon.GreenCheck:
                    return Properties.Resources.GreenCheck;
                case DialogBoxIcon.Information:
                    return Properties.Resources.Information;
                case DialogBoxIcon.RedQuestion:
                    return Properties.Resources.RedQuestion;
                case DialogBoxIcon.Stop:
                    return Properties.Resources.Stop;
                case DialogBoxIcon.Warning:
                    return Properties.Resources.Warning;
                default:
                    return Properties.Resources.Information;
            }
        }

        private void HideCancel()
        {
            buttonCancel.Visibility = Visibility.Collapsed;
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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

        public static bool? Show(string Message1, string Message2, string Header, DialogBoxIcon Icon)
        {
            DialogBox db = new DialogBox(Message1, Message2, Header, Icon);
            db.ShowDialog();
            return db.DialogResult;
        }        
        public static bool? Show(string Message1, string Message2, string Header, DialogBoxIcon Icon, bool ShowCancel)
        {
            DialogBox db = new DialogBox(Message1, Message2, Header, Icon, ShowCancel);
            db.ShowDialog();
            return db.DialogResult;
        }
    }

    public enum DialogBoxIcon
    {
        BlueQuestion, Bubble, CyanCheck, GreenCheck,
        Information, RedQuestion, Stop, Warning
    }
    public enum DialogBoxButtons { OK, OKCancel }
}
