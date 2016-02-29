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
using System.Windows.Navigation;
using System.Windows.Shapes;

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Controls.Tabs;
using ImagingSIMS.Data;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for SEMTab.xaml
    /// </summary>
    public partial class SEMTab : UserControl
    {        
        ObservableCollection<SEM> _semImages;

        public ObservableCollection<SEM> SEMImages
        {
            get { return _semImages; }
            set { _semImages = value; }
        }

        public SEMTab()
        {
            _semImages = new ObservableCollection<SEM>();
            InitializeComponent();

            colorPicker.SetColor(Color.FromArgb(255, 0, 0, 0));
        }

        private void Copy()
        {
            BitmapSource[] sources = new BitmapSource[itemsControl.SelectedItems.Count];
            foreach (object obj in itemsControl.SelectedItems)
            {
                SEM sem = (SEM)obj;
                if (sem == null) return;

                BitmapSource bs = (BitmapSource)sem.SEMImage.Source;
                Clipboard.SetImage(bs);
                ClosableTabItem.SendStatusUpdate(this, "SEM copied to clipboard.");
            }
        }
        private void Flip(bool Horizontal)
        {
            foreach (object obj in itemsControl.SelectedItems)
            {
                SEM sem = (SEM)obj;
                if (sem == null) return;

                Image image = sem.SEMImage;
                BitmapSource bs = (BitmapSource)image.Source;
                TransformedBitmap transformedImage = new TransformedBitmap();
                transformedImage.BeginInit();
                transformedImage.Source = bs;
                if (Horizontal) transformedImage.Transform = new ScaleTransform(-1, 1);
                else transformedImage.Transform = new ScaleTransform(1, -1);
                transformedImage.EndInit();

                image.Source = transformedImage;
            } 
        }
        private void Rotate(bool Clockwise)
        {
            foreach (object obj in itemsControl.SelectedItems)
            {
                SEM sem = (SEM)obj;
                if (sem == null) return;

                Image image = sem.SEMImage;
                BitmapSource bs = (BitmapSource)image.Source;
                TransformedBitmap transformedImage = new TransformedBitmap();
                transformedImage.BeginInit();
                transformedImage.Source = bs;
                if (Clockwise) transformedImage.Transform = new RotateTransform(90);
                else transformedImage.Transform = new RotateTransform(270);
                transformedImage.EndInit();

                image.Source = transformedImage;
            }
        }
        private void Save()
        {
            if (itemsControl.SelectedItems.Count == 0)
            {
                DialogBox db = new DialogBox("No images selected.", "Select one or more images to save.",
                    "Save", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            Nullable<bool> result = sfd.ShowDialog();
            if (result != true) return;

            List<KeyValuePair<BitmapSource, Exception>> notSaved = new List<KeyValuePair<BitmapSource, Exception>>();

            bool multiple = itemsControl.SelectedItems.Count > 1;
            for (int i = 0; i < itemsControl.SelectedItems.Count; i++)
            {
                string filePath = sfd.FileName;
                if (multiple)
                {
                    filePath = filePath.Insert(filePath.Length - 4, "_" + (i + 1).ToString());
                }
                using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
                {
                    BitmapSource bs = itemsControl.SelectedItems[i] as BitmapSource;
                    try
                    {
                        BitmapEncoder encoder = new PngBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(bs));
                        encoder.Save(fileStream);
                    }
                    catch (Exception ex)
                    {
                        notSaved.Add(new KeyValuePair<BitmapSource, Exception>(bs, ex));
                    }
                }
            }

            if (notSaved.Count > 0)
            {
                string list = "";
                foreach (KeyValuePair<BitmapSource, Exception> kvp in notSaved)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value.Message);
                }

                DialogBox db = new DialogBox("The following images were not saved:", list, "Save", DialogBoxIcon.Warning);
                db.ShowDialog();
            }
            else
            {
                DialogBox db = new DialogBox("Image(s) saved successfully!", sfd.FileName, "Save", DialogBoxIcon.Ok);
                db.ShowDialog();
            }
        }

        public void CallEvent(ImageTabEvent EventType)
        {
            switch (EventType)
            {
                case ImageTabEvent.Copy:
                    Copy();
                    break;
                case ImageTabEvent.FlipHorizontal:
                    Flip(true);
                    break;
                case ImageTabEvent.FlipVertical:
                    Flip(false);
                    break;
                case ImageTabEvent.RotateClock:
                    Rotate(true);
                    break;
                case ImageTabEvent.RotateCounter:
                    Rotate(false);
                    break;
                case ImageTabEvent.Save:
                    Save();
                    break;
            }
        }
        public void ResetImage()
        {
            foreach (object obj in itemsControl.SelectedItems)
            {
                SEM sem = (SEM)obj;
                if (sem == null) return;

                sem.RedrawImage(sem.MaxRawValue);
                sem.IntensityScale = sem.MaxRawValue;
            }
        }

        private void DockPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DockPanel dp = (DockPanel)sender;
                if (dp == null) return;

                if (dp.Children == null || dp.Children.Count == 0) return;
                Image image = (Image)dp.Children[0];
                if (image == null) return;

                DataObject obj = new DataObject(DataFormats.Bitmap, image.Source);
                DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy);
            }
            else
            {
                base.OnMouseMove(e);
            }
        }
        private void DockPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (itemsControl.SelectedItems.Count == 0) return;

            foreach (object obj in itemsControl.SelectedItems)
            {
                SEM sem = (SEM)obj;
                if (sem == null) return;

                int currentScale = sem.IntensityScale;
                int ticks = e.Delta;
                double newScaleValue = currentScale + (((double)sem.MaxRawValue / 1000d) * (double)ticks);
                if (newScaleValue >= int.MaxValue) newScaleValue = int.MaxValue - 1;

                if (newScaleValue < 0) newScaleValue = 1;
                if (newScaleValue == currentScale) return;

                sem.IntensityScale = (int)newScaleValue;
            }
        }
    }
}
