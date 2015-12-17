using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Common.Math;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for CropTab.xaml
    /// </summary>
    public partial class CropTab : UserControl
    {
        ProgressWindow pw;

        ObservableCollection<Data2D> _availableTables;

        public static readonly DependencyProperty RemoveOriginalProperty = DependencyProperty.Register("RemoveOriginal",
            typeof(bool), typeof(CropTab));
        public static readonly DependencyProperty IsCroppingProperty = DependencyProperty.Register("IsCropping",
            typeof(bool), typeof(CropTab));
        public static readonly DependencyProperty PositionTextProperty = DependencyProperty.Register("PositionText",
            typeof(string), typeof(CropTab));

        public bool RemoveOriginal
        {
            get { return (bool)GetValue(RemoveOriginalProperty); }
            set { SetValue(RemoveOriginalProperty, value); }
        }
        public bool IsCropping
        {
            get { return (bool)GetValue(IsCroppingProperty); }
            set { SetValue(IsCroppingProperty, value); }
        }
        public string PositionText
        {
            get { return (string)GetValue(PositionTextProperty); }
            set { SetValue(PositionTextProperty, value); }
        }

        public ObservableCollection<Data2D> AvailableTables
        {
            get { return _availableTables; }
            set { _availableTables = value; }
        }
        public CropTab()
        {
            AvailableTables = new ObservableCollection<Data2D>();

            InitializeComponent();
        }

        public void SetTables(ObservableCollection<Data2D> AvailableTables)
        {
            this.AvailableTables = AvailableTables;
            listAvailable.ItemsSource = AvailableTables;
        }

        private void buttonDoCrop_Click(object sender, RoutedEventArgs e)
        {
            if (listAvailable.SelectedItems.Count == 0)
            {
                DialogBox db = new DialogBox("No data tables selected.", "Select one or more data tables to crop.",
                    "Crop", DialogBoxIcon.Stop);
                db.ShowDialog();
                return;
            }
            int width = ((Data2D)listAvailable.SelectedItems[0]).Width;
            int height = ((Data2D)listAvailable.SelectedItems[0]).Height;
            foreach (object obj in listAvailable.SelectedItems)
            {
                Data2D d = (Data2D)obj;
                if (d.Width != width || d.Height != height)
                {
                    DialogBox db = new DialogBox("Invalid table dimensions.", "One or more of the selected tables does not match the dimensions of the others.",
                    "Crop", DialogBoxIcon.Stop);
                    db.ShowDialog();
                    return;
                }
            }
            if (preview.PixelStartX + preview.PixelWidth > width
                || preview.PixelStartY + preview.PixelHeight > height)
            {
                DialogBox db = new DialogBox("Invalid crop placement.", "Make sure the crop area is completely within the available area.",
                    "Crop", DialogBoxIcon.Stop);
                db.ShowDialog();
                return;
            }
            if (preview.PixelWidth <= 0 || preview.PixelHeight <= 0)
            {
                DialogBox db = new DialogBox("Invalid crop placement.", "The width and/or height is not valid.",
                      "Crop", DialogBoxIcon.Stop);
                db.ShowDialog();
                return;
            }
            if (preview.PixelStartX < 0 || preview.PixelStartY < 0)
            {
                DialogBox db = new DialogBox("Invalid crop placement.", "The start point is not valid.",
                      "Crop", DialogBoxIcon.Stop);
                db.ShowDialog();
                return;
            }

            Data3D toCrop = new Data3D();
            foreach (object obj in listAvailable.SelectedItems)
            {
                Data2D d = (Data2D)obj;
                if (d == null) continue;

                toCrop.AddLayer(d);
            }

            if (toCrop.Depth == 0)
            {
                DialogBox db = new DialogBox("No data tables selected.", "Select one or more data tables to crop.",
                       "Crop", DialogBoxIcon.Stop);
                db.ShowDialog();
                return;
            }

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.DoWork += bw_DoWork;

            pw = new ProgressWindow("Cropping tables. Please wait.", "Crop");
            pw.Show();

            CropArgs a = new CropArgs()
            {
                ToCrop = toCrop,
                CropStartX = preview.PixelStartX,
                CropStartY = preview.PixelStartY,
                CropWidth = preview.PixelWidth,
                CropHeight = preview.PixelHeight,
                RemoveOriginal = RemoveOriginal
            };
            IsCropping = true;
            bw.RunWorkerAsync(a);
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            Data3D tablesToAdd = new Data3D();
            CropArgs a = (CropArgs)e.Argument;

            int totalSteps = a.ToCrop.Depth * a.CropWidth * a.CropHeight;
            int pos = 0;

            foreach (Data2D dt in a.ToCrop.Layers)
            {
                Data2D croppped = new Data2D(a.CropWidth, a.CropHeight);
                croppped.DataName = dt.DataName + " Cropped";

                for (int x = 0; x < a.CropWidth; x++)
                {
                    for (int y = 0; y < a.CropHeight; y++)
                    {
                        int origX = a.CropStartX + x;
                        int origY = a.CropStartY + y;

                        croppped[x, y] = dt[origX, origY];
                        pos++;
                    }
                    bw.ReportProgress(Percentage.GetPercent(pos * 100, totalSteps));
                }
                tablesToAdd.AddLayer(croppped);
            }
            CropResult r = new CropResult()
            {
                ToAdd = tablesToAdd,
                ToRemove = a.ToCrop,
                RemoveOriginal = a.RemoveOriginal
            };
            e.Result = r;
        }
        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pw.UpdateProgress(e.ProgressPercentage);
        }
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsCropping = false;

            pw.ProgressFinished();

            BackgroundWorker bw = sender as BackgroundWorker;
            bw.RunWorkerCompleted -= bw_RunWorkerCompleted;
            bw.ProgressChanged -= bw_ProgressChanged;
            bw.DoWork -= bw_DoWork;
            bw.Dispose();

            CropResult r = (CropResult)e.Result;

            List<KeyValuePair<Data2D,string>> notAdded = new List<KeyValuePair<Data2D,string>>();
            foreach (Data2D d in r.ToAdd.Layers)
            {
                if (AvailableTables.Contains(d))
                {
                    notAdded.Add(new KeyValuePair<Data2D,string>(d, "Table already present in the collection."));
                    continue;
                }
                try
                {
                    AvailableTables.Add(d);
                }
                catch(Exception ex)
                {
                    notAdded.Add(new KeyValuePair<Data2D, string>(d, ex.Message));
                }
            }
            
            ClosableTabItem.SendStatusUpdate(this, "Crop operation complete");
            if (notAdded.Count > 0)
            {
                string list = "";
                foreach (KeyValuePair<Data2D, string> kvp in notAdded)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list.Remove(list.Length - 2, 2);

                DialogBox db = new DialogBox("The following tables were not added to the collection:", list, "Crop", DialogBoxIcon.Warning);
                db.ShowDialog();
            }

            if (!r.RemoveOriginal) return;

            notAdded.Clear();

            foreach (Data2D d in r.ToRemove.Layers)
            {
                if (!AvailableTables.Contains(d))
                {
                    notAdded.Add(new KeyValuePair<Data2D, string>(d, "Table not fount in the collection."));
                    continue;
                }
                try
                {
                    AvailableTables.Remove(d);
                }
                catch (Exception ex)
                {
                    notAdded.Add(new KeyValuePair<Data2D, string>(d, ex.Message));
                }
            }

            if (notAdded.Count > 0)
            {
                string list = "";
                foreach (KeyValuePair<Data2D, string> kvp in notAdded)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list += "\nUnless there was a different error above, the cropped tables were added. This only applies to tables being removed.";

                DialogBox db = new DialogBox("The following tables were not removed from the collection:", list, "Crop", DialogBoxIcon.Warning);
                db.ShowDialog();
            }
        }
        private void buttonCenter_Click(object sender, RoutedEventArgs e)
        {
            preview.AutoCenter();
        }
        private void buttonHalf_Click(object sender, RoutedEventArgs e)
        {
            preview.AutoHalf();
        }

        private void MenuItemPreview_Click(object sender, RoutedEventArgs e)
        {
            List<Data2D> data = new List<Data2D>();
            foreach (object obj in listAvailable.SelectedItems)
            {
                Data2D d = (Data2D)obj;
                if (d == null) continue;

                data.Add(d);
            }

            if (data.Count == 0) return;

            int width = data[0].Width;
            int height = data[0].Height;

            foreach (Data2D d in data)
            {
                if (d.Width != width || d.Height != height)
                {
                    DialogBox db = new DialogBox("Invalid table dimensions.", 
                        "One or more of the selected tables does not match the dimensions of the others and a preview image cannot be created.",
                        "Crop", DialogBoxIcon.Stop);
                    db.ShowDialog();
                    return;
                }
            }

            Data3D tables = new Data3D(data.ToArray<Data2D>());

            preview.OriginalImage = ImageHelper.CreateColorScaleImage(tables.Summed, ColorScaleTypes.ThermalWarm);
        }

        private void selectionThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
        }

        private void preview_MouseMove(object sender, MouseEventArgs e)
        {
            int xPixel = 0;
            int yPixel = 0;

            Data2D d = (Data2D)listAvailable.SelectedItem;
            if (d != null)
            {
                int width = d.Width;
                int height = d.Height;

                xPixel = (int)((e.GetPosition(preview).X * (width / preview.ActualWidth)));
                yPixel = (int)((e.GetPosition(preview).Y * (height / preview.ActualHeight)));

                PositionText = string.Format("Data Position X:{0} Y:{1}", xPixel, yPixel);
            }            
        }
    }

    internal struct CropArgs
    {
        public Data3D ToCrop;
        public int CropStartX;
        public int CropStartY;
        public int CropWidth;
        public int CropHeight;
        public bool RemoveOriginal;
    }
    internal struct CropResult
    {
        public Data3D ToRemove;
        public Data3D ToAdd;
        public bool RemoveOriginal;
    }
    public class ResizableThumb : Thumb
    {
    }
}
