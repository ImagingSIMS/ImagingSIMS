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
using System.Globalization;

namespace ImagingSIMS.Common.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox : Window
    {
        public static readonly DependencyProperty IconTypeProperty = DependencyProperty.Register("IconType",
            typeof(DialogIcon), typeof(DialogBox));

        public DialogIcon IconType
        {
            get { return (DialogIcon)GetValue(IconTypeProperty); }
            set { SetValue(IconTypeProperty, value); }
        }

        public DialogBox()
        {
        }
        public DialogBox(string Message1, string Message2, string Header, DialogIcon Icon)
        {
            IconType = Icon;

            InitializeComponent();

            this.Title = Header;
            msg1.Text = Message1;
            msg2.Text = Message2;

            Loaded += DialogBox_Loaded;
            SizeChanged += DialogBox_SizeChanged;

            HideCancel();
        }        
        public DialogBox(string Message1, string Message2, string Header, DialogIcon Icon, bool ShowCancel)
        {
            IconType = Icon;

            InitializeComponent();

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

        public static bool? Show(string Message1, string Message2, string Header, DialogIcon Icon)
        {
            DialogBox db = new DialogBox(Message1, Message2, Header, Icon);
            db.ShowDialog();
            return db.DialogResult;
        }        
        public static bool? Show(string Message1, string Message2, string Header, DialogIcon Icon, bool ShowCancel)
        {
            DialogBox db = new DialogBox(Message1, Message2, Header, Icon, ShowCancel);
            db.ShowDialog();
            return db.DialogResult;
        }
    }

    public enum DialogIcon
    {
        Alert, Blocked, Error, Help, Information,
        Invalid, Offline, Ok, Pause, Run, SecurityWarning, 
        Stop, Suppressed, Warning, WarningGray
    }
    public enum DialogBoxButtons { OK, OKCancel }

    //class DialogBoxIconToImageConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        try
    //        {
    //            DialogBoxIcon iconType = (DialogBoxIcon)value;

    //            switch (iconType)
    //            {
    //                case DialogBoxIcon.Alert:
    //                    break;
    //                case DialogBoxIcon.Blocked:
    //                    break;
    //                case DialogBoxIcon.Error:
    //                    break;
    //                case DialogBoxIcon.Help:
    //                    break;
    //                case DialogBoxIcon.Information:
    //                    break;
    //                case DialogBoxIcon.Invalid:
    //                    break;
    //                case DialogBoxIcon.Ok:
    //                    break;
    //                case DialogBoxIcon.Pause:
    //                    break;
    //                case DialogBoxIcon.Run:
    //                    break;
    //                case DialogBoxIcon.SecurityWarning:
    //                    break;
    //                case DialogBoxIcon.Error:
    //                    break;
    //                case DialogBoxIcon.ErrorSquare:
    //                    break;
    //                case DialogBoxIcon.Suppressed:
    //                    break;
    //                case DialogBoxIcon.Warning:
    //                    break;
    //                case DialogBoxIcon.WarningGray:
    //                    break;
    //            }
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    //public static class IconHelper
    //{
    //    public static Bitmap GetIcon(DialogBoxIcon icon)
    //    {
    //        switch (icon)
    //        {
    //            case DialogBoxIcon.AlertBlue:
    //                return Properties.Resources.Alert.ToBitmap();
    //            case DialogBoxIcon.AlertGray:
    //                return Properties.Resources.AlertGrey.ToBitmap();
    //            case DialogBoxIcon.Blocked:
    //                return Properties.Resources.Blocked.ToBitmap();
    //            case DialogBoxIcon.CheckGray:
    //                return Properties.Resources.OKGrey.ToBitmap();
    //            case DialogBoxIcon.Pause:
    //                return Properties.Resources.PauseGrey.ToBitmap();
    //            case DialogBoxIcon.Run:
    //                return Properties.Resources.Run.ToBitmap();
    //            case DialogBoxIcon.SecurityOk:
    //                return Properties.Resources.SecurityOKy.ToBitmap();
    //            case DialogBoxIcon.Suppressed:
    //                return Properties.Resources.Suppressed.ToBitmap();
    //            case DialogBoxIcon.WarningGray:
    //                return Properties.Resources.WarningGrey.ToBitmap();
    //            case DialogBoxIcon.Help:
    //                return Properties.Resources.Help.ToBitmap();
    //            case DialogBoxIcon.Ok:
    //                return Properties.Resources.OK.ToBitmap();
    //            case DialogBoxIcon.Information:
    //                return Properties.Resources.Information.ToBitmap();
    //            case DialogBoxIcon.Error:
    //                return Properties.Resources.Critical.ToBitmap();
    //            case DialogBoxIcon.Warning:
    //                return Properties.Resources.Warning.ToBitmap();
    //            default:
    //                return Properties.Resources.Information.ToBitmap();
    //        }
    //    }
    //}
}
