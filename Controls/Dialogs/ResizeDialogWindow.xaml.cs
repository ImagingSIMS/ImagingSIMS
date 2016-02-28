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

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;

namespace ImagingSIMS.Controls.Dialogs
{
    /// <summary>
    /// Interaction logic for ResizeDialogWindow.xaml
    /// </summary>
    public partial class ResizeDialogWindow : Window
    {
        public static readonly DependencyProperty OriginalWidthProperty = DependencyProperty.Register("OriginalWidth",
            typeof(int), typeof(ResizeDialogWindow));
        public static readonly DependencyProperty OriginalHeightProperty = DependencyProperty.Register("OriginalHeight",
            typeof(int), typeof(ResizeDialogWindow));
        public static readonly DependencyProperty ResizedWidthProperty = DependencyProperty.Register("ResizedWidth",
            typeof(int), typeof(ResizeDialogWindow));
        public static readonly DependencyProperty ResizedHeightProperty = DependencyProperty.Register("ResizedHeight",
            typeof(int), typeof(ResizeDialogWindow));
        public static readonly DependencyProperty ResziePreviewImageSourceProperty = DependencyProperty.Register("ResziePreviewImageSource",
            typeof(BitmapSource), typeof(ResizeDialogWindow));
        public static readonly DependencyProperty DoCropProperty = DependencyProperty.Register("DoCrop",
            typeof(bool), typeof(ResizeDialogWindow));
        public static readonly DependencyProperty CropStartXProperty = DependencyProperty.Register("CropStartX",
            typeof(int), typeof(ResizeDialogWindow));
        public static readonly DependencyProperty CropStartYProperty = DependencyProperty.Register("CropStartY",
            typeof(int), typeof(ResizeDialogWindow));

        public int OriginalWidth
        {
            get { return (int)GetValue(OriginalWidthProperty); }
            set { SetValue(OriginalWidthProperty, value); }
        }
        public int OriginalHeight
        {
            get { return (int)GetValue(OriginalHeightProperty); }
            set { SetValue(OriginalHeightProperty, value); }
        }
        public int ResizedWidth
        {
            get { return (int)GetValue(ResizedWidthProperty); }
            set { SetValue(ResizedWidthProperty, value); }
        }
        public int ResizedHeight
        {
            get { return (int)GetValue(ResizedHeightProperty); }
            set { SetValue(ResizedHeightProperty, value); }
        }
        public BitmapSource ResizePreviewImageSource
        {
            get { return (BitmapSource)GetValue(ResziePreviewImageSourceProperty); }
            set { SetValue(ResziePreviewImageSourceProperty, value); }
        }
        public bool DoCrop
        {
            get { return (bool)GetValue(DoCropProperty); }
            set { SetValue(DoCropProperty, value); }
        }
        public int CropStartX
        {
            get { return (int)GetValue(CropStartXProperty); }
            set { SetValue(CropStartXProperty, value); }
        }
        public int CropStartY
        {
            get { return (int)GetValue(CropStartYProperty); }
            set { SetValue(CropStartYProperty, value); }
        }

        public ResizeDialogArgs ResizeResult
        {
            get { return new ResizeDialogArgs(ResizePreviewImageSource, 
                ResizedWidth, ResizedHeight, DoCrop, CropStartX, CropStartY); }
        }
        

        public ResizeDialogWindow()
        {
            InitializeComponent();
        }
        public ResizeDialogWindow(ResizeDialogArgs ResizeArgs)
        {
            OriginalWidth = ResizeArgs.OriginalWidth;
            OriginalHeight = ResizeArgs.OriginalHeight;
            ResizedWidth = ResizeArgs.ResizedWidth;
            ResizedHeight = ResizeArgs.ResizedHeight;
            ResizePreviewImageSource = ResizeArgs.ImageToResize;

            InitializeComponent();
        }

        private void buttonOK_Click(object sender, RoutedEventArgs e)
        {
            if (DoCrop)
            {
                if (ResizedWidth + CropStartX > OriginalWidth)
                {
                    DialogBox db = new DialogBox("Invalid width.",
                        string.Format("Resized width ({0}) falls outside the bounds of the original image.", ResizedWidth, OriginalWidth),
                        "Resize", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                }
                if (ResizedHeight + CropStartY > OriginalHeight)
                {
                    DialogBox db = new DialogBox("Invalid height.",
                        string.Format("Resized height ({0}) falls outside the bounds of the original image.", ResizedHeight, OriginalHeight),
                        "Resize", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                }
            }
            else
            {
                if (ResizedWidth <= OriginalWidth)
                {
                    DialogBox db = new DialogBox("Invalid width.",
                        string.Format("Resized width ({0}) must be greater than the original width ({1}).", ResizedWidth, OriginalWidth),
                        "Resize", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                }
                if (ResizedHeight <= OriginalHeight)
                {
                    DialogBox db = new DialogBox("Invalid height.",
                        string.Format("Resized height ({0}) must be greater than the original height ({1}).", ResizedHeight, OriginalHeight),
                        "Resize", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                }
            }

            string msg1 = "";
            string msg2 = "";
            string hdr = "";
            if (DoCrop)
            {
                msg1 = string.Format(
                    "Confirm crop arguments:\n\nStart X: {0} Resized width: {1}\nStart Y: {2} Resized height: {3}",
                    CropStartX, ResizedWidth, CropStartY, ResizedHeight);
                msg2 = "Click OK to perform crop or Cancel to review the parameters.";
                hdr = "Crop";
            }
            else
            {
                msg1 = string.Format(
                    "Confirm resize arguments:\n\nOriginal width: {0} Resized width: {1}\nOriginal height: {2} Resized height: {3}",
                    OriginalWidth, ResizedWidth, OriginalHeight, ResizedHeight);
                msg2 = "Click OK to perform resize or Cancel to review the parameters.";
                hdr = "Resize";
            }

            DialogBox db_confirm = new DialogBox(msg1, msg2, hdr, DialogBoxIcon.Help);
            if (db_confirm.ShowDialog() != true) return;

            this.DialogResult = true;
            this.Close();
        }
        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }

    public struct ResizeDialogArgs
    {
        public int OriginalWidth;
        public int OriginalHeight;
        public int ResizedWidth;
        public int ResizedHeight;
        public bool DoCrop;
        public int CropStartX;
        public int CropStartY;
        public BitmapSource ImageToResize;

        public ResizeDialogArgs(BitmapSource ImageToResize)
        {
            this.ImageToResize = ImageToResize;
            this.OriginalHeight = ImageToResize.PixelHeight;
            this.OriginalWidth = ImageToResize.PixelWidth;
            this.ResizedHeight = this.OriginalHeight;
            this.ResizedWidth = this.OriginalWidth;
            this.DoCrop = false;
            this.CropStartX = 0;
            this.CropStartY = 0;
        }
        public ResizeDialogArgs(BitmapSource ImageToResize, int ResizedWidth, int ResizedHeight)
        {
            this.ImageToResize = ImageToResize;
            this.OriginalHeight = ImageToResize.PixelHeight;
            this.OriginalWidth = ImageToResize.PixelWidth;
            this.ResizedHeight = ResizedHeight;
            this.ResizedWidth = ResizedWidth;
            this.DoCrop = false;
            this.CropStartX = 0;
            this.CropStartY = 0;
        }
        public ResizeDialogArgs(BitmapSource ImageToResize, int ResizedWidth, int ResizedHeight, bool DoCrop, int CropStartX, int CropStartY)
        {
            this.ImageToResize = ImageToResize;
            this.OriginalHeight = ImageToResize.PixelHeight;
            this.OriginalWidth = ImageToResize.PixelWidth;
            this.ResizedHeight = ResizedHeight;
            this.ResizedWidth = ResizedWidth;
            this.DoCrop = DoCrop;
            this.CropStartX = CropStartX;
            this.CropStartY = CropStartY;
        }
    }
}
