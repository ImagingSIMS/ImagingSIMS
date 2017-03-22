using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Common.Math;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for CropTab.xaml
    /// </summary>
    public partial class CropTab : UserControl
    {
        ProgressWindow pw;

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

        public CropTab()
        {
            InitializeComponent();
        }

        private void buttonDoCrop_Click(object sender, RoutedEventArgs e)
        {
            List<Data2D> selected = AvailableHost.AvailableTablesSource.GetSelectedTables();
            if (selected.Count == 0)
            {
                DialogBox db = new DialogBox("No data tables selected.", "Select one or more data tables to crop.",
                    "Crop", DialogIcon.Error);
                db.ShowDialog();
                return;
            }
            int width = selected[0].Width;
            int height = selected[0].Height;
            foreach(Data2D d in selected)
            {
                if (d.Width != width || d.Height != height)
                {
                    DialogBox db = new DialogBox("Invalid table dimensions.", "One or more of the selected tables does not match the dimensions of the others.",
                    "Crop", DialogIcon.Error);
                    db.ShowDialog();
                    return;
                }
            }
            if (preview.PixelStartX + preview.PixelWidth > width
                || preview.PixelStartY + preview.PixelHeight > height)
            {
                DialogBox db = new DialogBox("Invalid crop placement.", "Make sure the crop area is completely within the available area.",
                    "Crop", DialogIcon.Error);
                db.ShowDialog();
                return;
            }
            if (preview.PixelWidth <= 0 || preview.PixelHeight <= 0)
            {
                DialogBox db = new DialogBox("Invalid crop placement.", "The width and/or height is not valid.",
                      "Crop", DialogIcon.Error);
                db.ShowDialog();
                return;
            }
            if (preview.PixelStartX < 0 || preview.PixelStartY < 0)
            {
                DialogBox db = new DialogBox("Invalid crop placement.", "The start point is not valid.",
                      "Crop", DialogIcon.Error);
                db.ShowDialog();
                return;
            }

            Data3D toCrop = new Data3D();
            foreach (Data2D d in selected)
            {
                toCrop.AddLayer(d);
            }

            if (toCrop.Depth == 0)
            {
                DialogBox db = new DialogBox("No data tables selected.", "Select one or more data tables to crop.",
                       "Crop", DialogIcon.Error);
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

            AvailableHost.AvailableTablesSource.AddTables(r.ToAdd.Layers);
            
            ClosableTabItem.SendStatusUpdate(this, "Crop operation complete");

            if (!r.RemoveOriginal) return;

            AvailableHost.AvailableTablesSource.RemoveTables(r.ToRemove.Layers);
        }
        private void buttonCenter_Click(object sender, RoutedEventArgs e)
        {
            preview.AutoCenter();
        }
        private void buttonHalf_Click(object sender, RoutedEventArgs e)
        {
            preview.AutoHalf();
        }

        private void buttonPreview_Click(object sender, RoutedEventArgs e)
        {
            List<Data2D> data = AvailableHost.AvailableTablesSource.GetSelectedTables();
            if (data.Count == 0) return;

            int width = data[0].Width;
            int height = data[0].Height;

            foreach (Data2D d in data)
            {
                if (d.Width != width || d.Height != height)
                {
                    DialogBox db = new DialogBox("Invalid table dimensions.",
                        "One or more of the selected tables does not match the dimensions of the others and a preview image cannot be created.",
                        "Crop", DialogIcon.Error);
                    db.ShowDialog();
                    return;
                }
            }

            Data3D tables = new Data3D(data.ToArray<Data2D>());

            preview.OriginalImage = ImageGenerator.Instance.Create(tables.Summed, ColorScaleTypes.ThermalWarm);
        }

        private void selectionThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
        }

        private void preview_MouseMove(object sender, MouseEventArgs e)
        {
            int xPixel = 0;
            int yPixel = 0;

            Data2D d = AvailableHost.AvailableTablesSource.GetAvailableTables().FirstOrDefault();
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
}
