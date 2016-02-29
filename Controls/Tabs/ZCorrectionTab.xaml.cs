using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Interaction logic for ZCorrectionTab.xaml
    /// </summary>
    public partial class ZCorrectionTab : UserControl
    {
        ProgressWindow _pw;

        ObservableCollection<ZCorrection> _zCorrectionBases;
        public ObservableCollection<ZCorrection> ZCorrectionBases
        {
            get { return _zCorrectionBases; }
            set { _zCorrectionBases = value; }
        }

        public ZCorrectionTab()
        {
            ZCorrectionBases = new ObservableCollection<ZCorrection>();

            InitializeComponent();

            tbThreshold.Text = "10";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }

        private void buttonPreview_Click(object sender, RoutedEventArgs e)
        {
            List<Data2D> selectedTables = AvailableHost.AvailableTablesSource.GetSelectedTables();
            if (selectedTables.Count == 0) return;

            Data2D summed = Data2D.Sum(selectedTables);

            imagePreview.ChangeDataSource(summed);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<Data2D> selectedTables = AvailableHost.AvailableTablesSource.GetSelectedTables();

            if (selectedTables.Count == 0)
            {
                DialogBox db = new DialogBox("No tables selected.", "Select two or more tables to correct and try again.",
                    "Z-Correction", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }
            if (selectedTables.Count == 1)
            {
                DialogBox db = new DialogBox("Insufficient tables selected.", "Select two or more tables to correct and try again.",
                    "Z-Correction", DialogBoxIcon.Error);
                db.ShowDialog();
                return;
            }

            int width = 0;
            int height = 0;

            for (int i = 0; i < selectedTables.Count; i++)
            {
                if (width == 0) width = selectedTables[i].Width;
                if (height == 0) height = selectedTables[i].Height;

                if (selectedTables[i].Width != width)
                {
                    DialogBox db = new DialogBox("Invalid table dimensions.", "Not all selected tables have the same dimensions.",
                         "Z-Correction", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                }
                if (selectedTables[i].Height != height)
                {
                    DialogBox db = new DialogBox("Invalid table dimensions.", "Not all selected tables have the same dimensions.",
                         "Z-Correction", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                }
            }

            //args[0]: true for mask correction false for threshold correction
            //args[1]: tables to correct
            //args[2]: mask for mask correction or threshold value for threshold correction
            object[] args = new object[3] { false, selectedTables, null };

            if(radioMask.IsChecked == true)
            {
                args[0] = true;

                if (listViewMasks.SelectedItems.Count == 0)
                {
                    DialogBox db = new DialogBox("No correction base selected.", "Select a base to use for the ZCorrection and try again.",
                       "Z-Correction", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                }
                
                args[2] = listViewMasks.SelectedItem as ZCorrection;
            }
            else if(radioThreshold.IsChecked == true)
            {
                args[0] = false;

                if (tbThresholdValue.Text == null || tbThresholdValue.Text == "")
                {
                    DialogBox db = new DialogBox("No threshold value specified.", "Enter an integer value for the threshold and try again.",
                       "Z-Correction", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                } 
                
                int threshold = -1;
                if (!int.TryParse(tbThresholdValue.Text, out threshold))
                {
                    DialogBox db = new DialogBox("Invalid threshold value.", "Enter an integer value for the threshold and try again.",
                       "Z-Correction", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                }

                if (threshold < 0)
                {
                    DialogBox db = new DialogBox("Invalid threshold value.", "Enter an integer value for the threshold and try again.",
                       "Z-Correction", DialogBoxIcon.Error);
                    db.ShowDialog();
                    return;
                }

                args[2] = threshold;
            }
            
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = false;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.DoWork += bw_DoWork;

            _pw = new ProgressWindow("Performing Z-Correction. Please wait.", "Z-Correction");
            _pw.Show();

            bw.RunWorkerAsync(args);
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            //object[] args = (object[])e.Argument;

            //List<Data2D> data = (List<Data2D>)args[1];
            //bool doMaskCorrection = (bool)args[0];

            //List<Data2D> corrected = new List<Data2D>();

            //if (doMaskCorrection)
            //{
            //    ZCorrection mask = (ZCorrection)args[2];
            //    corrected = ZCorrection.ZCorrect(data, mask, sender as BackgroundWorker);

            //    for (int i = 0; i < data.Count; i++)
            //    {
            //        corrected[i].DataName = data[i].DataName + string.Format(" - ZCorrected ({0})", mask.MaskName);
            //    }
            //}
            //else
            //{
            //    int threshold = (int)args[2];
            //    corrected = _ZCorrection.ZCorrect(data, threshold, sender as BackgroundWorker);

            //    for (int i = 0; i < data.Count; i++)
            //    {
            //        corrected[i].DataName = data[i].DataName + string.Format(" - ZCorrected ({0})", threshold);
            //    }
            //}
            
            //e.Result = corrected;
            e.Result = null;
        }
        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _pw.UpdateProgress(e.ProgressPercentage);
        }
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _pw.ProgressFinished("Correction Complete!");

            List<Data2D> corrected = (List<Data2D>)e.Result;

            BackgroundWorker bw = sender as BackgroundWorker;

            bw.RunWorkerCompleted -= bw_RunWorkerCompleted;
            bw.ProgressChanged -= bw_ProgressChanged;
            bw.DoWork -= bw_DoWork;
            bw.Dispose();

            if(corrected == null)
            {
                DialogBox.Show("No tables were returned.",
                    "The Z-Correction operation did not result in any corrected tables.", "Z-Correction", DialogBoxIcon.Error);
                return;
            }
            AvailableHost.AvailableTablesSource.AddTables(corrected);

            ClosableTabItem.SendStatusUpdate(this, "Z-Correction complete.");
        }

        private void buttonCreateMask_Click(object sender, RoutedEventArgs e)
        {
            //List<Data2D> selectedTables = new List<Data2D>();

            //foreach (object obj in listViewTables.SelectedItems)
            //{
            //    Data2D d = obj as Data2D;
            //    if (d != null) selectedTables.Add(d);
            //}

            //if (selectedTables.Count == 0)
            //{
            //    DialogBox db = new DialogBox("No tables selected.", "Select two or more tables to create a base and try again.",
            //        "Z-Correction", DialogBoxIcon.Error);
            //    db.ShowDialog();
            //    return;
            //}
            //if (selectedTables.Count == 1)
            //{
            //    DialogBox db = new DialogBox("Insufficient tables selected.", "Select two or more tables to create a base and try again.",
            //        "Z-Correction", DialogBoxIcon.Error);
            //    db.ShowDialog();
            //    return;
            //}

            //int threshold = 0;
            //if (!int.TryParse(tbThreshold.Text, out threshold))
            //{
            //    DialogBox db = new DialogBox("Invalid threshold value.", "Enter a valid integer and try again.",
            //             "Z-Correction", DialogBoxIcon.Error);
            //    db.ShowDialog();
            //    return;
            //}

            //string maskName = tbMaskName.Text;
            //if (maskName==null||maskName =="")
            //{
            //    DialogBox db = new DialogBox("No specified mask name.", "Enter a name for the base and try again.",
            //             "Z-Correction", DialogBoxIcon.Error);
            //    db.ShowDialog();
            //    return;
            //}

            //ZCorrection mask = new ZCorrection(maskName, selectedTables, threshold);
            //ZCorrectionBases.Add(mask);
        }
    }
}
