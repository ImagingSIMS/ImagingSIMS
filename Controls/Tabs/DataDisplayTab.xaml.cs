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

using Microsoft.Win32;

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Data;
using ImagingSIMS.Data.ClusterIdentification;
using ImagingSIMS.Data.Imaging;
using System.IO;
using ImagingSIMS.Controls.BaseControls;
using ImagingSIMS.Controls.ViewModels;
using System.Collections.Specialized;
using ImagingSIMS.Data.Rendering;
using ImagingSIMS.Common.Math;

namespace ImagingSIMS.Controls.Tabs
{
    public partial class DataDisplayTab : UserControl
    {
        public static readonly DependencyProperty BatchApplyProperty = DependencyProperty.Register("BatchApply",
            typeof(DataDisplayBatchApplyViewModel), typeof(DataDisplayTab));

        public DataDisplayBatchApplyViewModel BatchApply
        {
            get { return (DataDisplayBatchApplyViewModel)GetValue(BatchApplyProperty); }
            set { SetValue(BatchApplyProperty, value); }
        }

        ObservableCollection<Data3DDisplayViewModel> _displayItems;
        public ObservableCollection<Data3DDisplayViewModel> DisplayItems
        {
            get { return _displayItems; }
            set { _displayItems = value; }
        }

        public DataDisplayTab()
        {
            DisplayItems = new ObservableCollection<Data3DDisplayViewModel>();
            BatchApply = new DataDisplayBatchApplyViewModel();

            DisplayItems.CollectionChanged += DisplayItems_CollectionChanged;

            InitializeComponent();
        }
        public DataDisplayTab(ColorScaleTypes ColorScale)
        {
            DisplayItems = new ObservableCollection<Data3DDisplayViewModel>();
            BatchApply = new DataDisplayBatchApplyViewModel(ColorScale);

            DisplayItems.CollectionChanged += DisplayItems_CollectionChanged;

            InitializeComponent();
        }

        private void DisplayItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int max = 0;
            foreach(var item in DisplayItems)
            {
                if (item.LayerEnd > max)
                    max = item.LayerEnd;
            }

            BatchApply.LayerMaximum = max;
            BatchApply.LayerMinimum = 1;
        }

        public void AddDataSource(Data2D dataSource)
        {
            AddDataSource(new Data3D(new Data2D[] { dataSource }));
        }
        public void AddDataSource(Data2D dataSource, ColorScaleTypes colorScale)
        {
            AddDataSource(new Data3D(new Data2D[] { dataSource }), colorScale);
        }
        public void AddDataSource(Data2D dataSource, Color solidColorScale)
        {
            AddDataSource(new Data3D(new Data2D[] { dataSource }), solidColorScale);
        }

        public void AddDataSource(Data3D dataSource)
        {
            DisplayItems.Add(new Data3DDisplayViewModel(dataSource, BatchApply.ColorScale));
        }
        public void AddDataSource(Data3D dataSource, ColorScaleTypes colorScale)
        {
            DisplayItems.Add(new Data3DDisplayViewModel(dataSource, colorScale));
        }
        public void AddDataSource(Data3D dataSource, Color solidColorScale)
        {
            DisplayItems.Add(new Data3DDisplayViewModel(dataSource, solidColorScale));
        }

        public async Task AddDataSourceAsync(Data2D dataSource)
        {
            Data3DDisplayViewModel displayItem = Data3DDisplayViewModel.Empty;
            await displayItem.SetData3DDisplayItemAsync(new Data3D(new Data2D[] { dataSource }), BatchApply.ColorScale);
            DisplayItems.Add(displayItem);
        }
        public async Task AddDataSourceAsync(Data2D dataSource, ColorScaleTypes colorScale)
        {
            Data3DDisplayViewModel displayItem = Data3DDisplayViewModel.Empty;
            await displayItem.SetData3DDisplayItemAsync(new Data3D(new Data2D[] { dataSource }), colorScale);
            DisplayItems.Add(displayItem);
        }
        public async Task AddDataSourceAsync(Data2D dataSource, Color solidColorScale)
        {
            Data3DDisplayViewModel displayItem = Data3DDisplayViewModel.Empty;
            await displayItem.SetData3DDisplayItemAsync(new Data3D(new Data2D[] { dataSource }), solidColorScale);
            DisplayItems.Add(displayItem);
        }

        public async Task AddDataSourceAsync(Data3D dataSource)
        {
            Data3DDisplayViewModel displayItem = Data3DDisplayViewModel.Empty;
            await displayItem.SetData3DDisplayItemAsync(dataSource, BatchApply.ColorScale);
            DisplayItems.Add(displayItem);
        }
        public async Task AddDataSourceAsync(Data3D dataSource, ColorScaleTypes colorScale)
        {
            Data3DDisplayViewModel displayItem = Data3DDisplayViewModel.Empty;
            await displayItem.SetData3DDisplayItemAsync(dataSource, colorScale);
            DisplayItems.Add(displayItem);
        }
        public async Task AddDataSourceAsync(Data3D dataSource, Color solidColorScale)
        {
            Data3DDisplayViewModel displayItem = Data3DDisplayViewModel.Empty;
            await displayItem.SetData3DDisplayItemAsync(dataSource, solidColorScale);
            DisplayItems.Add(displayItem);
        }

        public async Task AddDataSourceAsync(List<Data2D> dataSources)
        {
            foreach (Data2D dataSource in dataSources)
            {
                await AddDataSourceAsync(dataSource);
            }
        }
        public async Task AddDataSourceAsync(List<Data3D> dataSources)
        {
            foreach(Data3D dataSource in dataSources)
            {
                await AddDataSourceAsync(dataSource);
            }
        }

        private void cmSelectAll_Click(object sender, RoutedEventArgs e)
        {
            itemsControl.SelectAll();
        }
        private void cmClearAll_Click(object sender, RoutedEventArgs e)
        {
            itemsControl.UnselectAll();
        }

        public void SaveImageSeries()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            Nullable<bool> result = sfd.ShowDialog();

            if (result != true) return;

            bool multiple = DisplayItems.Count > 0;
            int counter = 0;

            Dictionary<Data3DDisplayViewModel, string> notSaved = new Dictionary<Data3DDisplayViewModel, string>();

            foreach (Data3DDisplayViewModel displayItem in DisplayItems)
            {
                string filePath = sfd.FileName;
                if (multiple)
                {
                    filePath = filePath.Insert(filePath.Length - 4,
                        "_" + (++counter).ToString());
                }

                BitmapSource src = displayItem.DisplayImageSource as BitmapSource;

                if (src == null)
                {
                    notSaved.Add(displayItem, "Invalid ImageSource");
                    continue;
                }

                try
                {
                    saveImage((BitmapSource)displayItem.DisplayImageSource, filePath);
                }
                catch (Exception ex)
                {
                    notSaved.Add(displayItem, ex.Message);
                }
            }

            if (notSaved.Count == 0)
            {
                DialogBox.Show("Image(s) saved successfully!", sfd.FileName, "Save", DialogIcon.Ok);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (Data3DDisplayViewModel item in notSaved.Keys)
                {
                    sb.AppendLine(item.DataSource.DataName + string.Format("({0})", notSaved[item]));
                }
                DialogBox.Show("One or more images were not saved successfully and are listed below. Any other images have been saved.",
                    sb.ToString(), "Save", DialogIcon.Error);
            }
        }
        private void saveImage(BitmapSource src, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(src));

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        public void ExpandPixels(int windowSize)
        {
            List<Data3D> selectedTables = new List<Data3D>();
            foreach (Data3DDisplayViewModel displayItem in itemsControl.SelectedItems)
            {
                selectedTables.Add(displayItem.DataSource);
            }

            if (selectedTables.Count == 0)
            {
                DialogBox.Show("No tables selected.", "Select one or more tables to expand and try again.", "Expand", DialogIcon.Error);
            }

            foreach (Data3D d in selectedTables)
            {
                Data3D expanded = d.ExpandIntensity(windowSize);
                AddDataSource(expanded);
            }
        }

        private void removeItem_Click(object sender, RoutedEventArgs e)
        {
            Data3DDisplay display = e.Source as Data3DDisplay;
            if (display == null) return;

            Data3DDisplayViewModel displayItem = display.DisplayItem;
            if (displayItem != null)
            {
                if (DisplayItems.Contains(displayItem))
                {
                    DisplayItems.Remove(displayItem);
                }
            }
        }

        public static readonly DependencyProperty AnalysisOutputTextProperty = DependencyProperty.Register("AnalysisOutputText",
            typeof(string), typeof(DataDisplayTab));
        public string AnalysisOutputText
        {
            get { return (string)GetValue(AnalysisOutputTextProperty); }
            set { SetValue(AnalysisOutputTextProperty, value); }
        }

        private void analyzePixels_Click(object sender, RoutedEventArgs e)
        {
            Data3DDisplay display = e.Source as Data3DDisplay;
            if (display == null) return;

            Data2D d = display.DisplayItem.ViewableDataSource;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Name: {d.DataName} (UID {d.UniqueID})");
            sb.AppendLine($"Width: {d.Width} Height: {d.Height}");
            sb.AppendLine($"Minimum value: {d.Minimum} Maximum value: {d.Maximum}");
            sb.AppendLine($"Mean: {d.Mean} Standard deviation: {d.StdDev}");
            sb.AppendLine($"Total counts: {d.TotalCounts}");
            sb.AppendLine($"Non-zero pixels: {d.NonSparseMatrix.Count} Non-zero percentage: {(d.NonSparseMatrix.Count * 100 / (d.Width * d.Height))}%");
            sb.AppendLine($"Non-zero mean: {d.NonSparseMean} Non-zero standard deviation: {d.NonSparseStdDev}");

            AnalysisOutputText = sb.ToString();

            expanderStatsOutput.IsExpanded = true;
            tabOutput.SelectedItem = tabItemAnalysis;
        }
        private void calculateRatios_Click(object sender, RoutedEventArgs e)
        {
            string[] labels = DisplayItems.Select(di => di.DataSource.DataName).ToArray();
            float[] values = DisplayItems.Select(di => (float)di.ViewableDataSource.TotalCounts).ToArray();

            ratioGrid.SetValues(labels, values);

            expanderStatsOutput.IsExpanded = true;
            tabOutput.SelectedItem = tabItemRatios;
        }
        private void copyRatios_Click(object sender, RoutedEventArgs e)
        {
            ratioGrid.CopyRatios();
        }

        public BitmapSource GetOverlay()
        {

            int imgCount = itemsControl.SelectedItems.Count;
            if (imgCount == 0)
            {
                DialogBox db = new DialogBox("No images selected", "Select two or more images to overlay and try again.",
                    "Overlay", DialogIcon.Error);
                db.ShowDialog();
                return null;
            }
            if (imgCount == 1)
            {
                DialogBox db = new DialogBox("Only one image selected", "Select two or more images to overlay and try again.",
                    "Overlay", DialogIcon.Error);
                db.ShowDialog();
                return null;
            }
            BitmapSource[] toOverlay = new BitmapSource[imgCount];

            int i = 0;
            foreach (Data3DDisplayViewModel displayItem in itemsControl.SelectedItems)
            {
                toOverlay[i] = displayItem.DisplayImageSource as BitmapSource;
                i++;
            }

            int width = -1;
            int height = -1;
            bool proceed = true;

            foreach (BitmapSource bs in toOverlay)
            {
                if (width == -1) width = (int)bs.PixelWidth;
                if (height == -1) height = (int)bs.PixelHeight;

                if ((int)bs.Width != width || (int)bs.Height != height)
                {
                    proceed = false;
                    break;
                }
            }

            if (!proceed)
            {
                DialogBox db = new DialogBox("Invalid image dimensions",
                    "One or more images selected does not match the rest of the collection in width or height.",
                    "Overlay", DialogIcon.Error);
                db.ShowDialog();
                return null;
            }

            return Overlay.CreateOverlay(toOverlay);

        }

        private void ApplyColorScale_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(e.Parameter == null)
            {
                applyColorScaleAll();
            }
            else
            {
                string parameter = (string)e.Parameter;

                if (!string.IsNullOrEmpty(parameter))
                {
                    if (parameter.ToLower() == "all") applyColorScaleAll();
                    else if (parameter.ToLower() == "selected") applyColorScaleSelected();
                }
                else
                {
                    applyColorScaleAll();
                }
            }
        }
        private void applyColorScaleAll()
        {
            ColorScaleTypes scale = BatchApply.ColorScale;
            Color solid = BatchApply.SolidColorScale;

            foreach (object o in itemsControl.Items)
            {
                Data3DDisplayViewModel d = o as Data3DDisplayViewModel;
                if (d == null) continue;

                d.ColorScale = scale;
                if (scale == ColorScaleTypes.Solid)
                {
                    d.SolidColorScale = solid;
                }

                d.SetLayers(BatchApply.LayerStart, BatchApply.LayerEnd);
            }
        }
        private void applyColorScaleSelected()
        {
            ColorScaleTypes scale = BatchApply.ColorScale;
            Color solid = BatchApply.SolidColorScale;

            foreach (object o in itemsControl.SelectedItems)
            {
                Data3DDisplayViewModel d = o as Data3DDisplayViewModel;
                if (d == null) continue;

                d.ColorScale = scale;
                if (scale == ColorScaleTypes.Solid)
                {
                    d.SolidColorScale = solid;
                }

                d.SetLayers(BatchApply.LayerStart, BatchApply.LayerEnd);
            }
        }

        private void SaveItems_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            Nullable<bool> result = sfd.ShowDialog();

            if (result != true) return;

            bool multiple = DisplayItems.Count > 0;
            int counter = 0;

            Dictionary<Data3DDisplayViewModel, string> notSaved = new Dictionary<Data3DDisplayViewModel, string>();

            foreach (Data3DDisplayViewModel displayItem in DisplayItems)
            {
                string filePath = sfd.FileName;
                if (multiple)
                {
                    filePath = filePath.Insert(filePath.Length - 4,
                        "_" + (++counter).ToString());
                }

                BitmapSource src = displayItem.DisplayImageSource as BitmapSource;

                if (src == null)
                {
                    notSaved.Add(displayItem, "Invalid ImageSource");
                    continue;
                }

                try
                {
                    saveImage((BitmapSource)displayItem.DisplayImageSource, filePath);
                }
                catch (Exception ex)
                {
                    notSaved.Add(displayItem, ex.Message);
                }
            }

            if (notSaved.Count == 0)
            {
                DialogBox.Show("Image(s) saved successfully!", sfd.FileName, "Save", DialogIcon.Ok);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (Data3DDisplayViewModel item in notSaved.Keys)
                {
                    sb.AppendLine(item.DataSource.DataName + string.Format("({0})", notSaved[item]));
                }
                DialogBox.Show("One or more images were not saved successfully and are listed below. Any other images have been saved.",
                    sb.ToString(), "Save", DialogIcon.Error);
            }

        }
        private void ResetSaturations_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            foreach (object o in itemsControl.Items)
            {
                Data3DDisplayViewModel d = o as Data3DDisplayViewModel;
                if (d == null) return;

                d.Saturation = d.InitialSaturation;
            }
        }
        private void ApplyLayerRange_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int layerStart = BatchApply.LayerStart;
            int layerEnd = BatchApply.LayerEnd;

            foreach (object o in itemsControl.Items)
            {
                Data3DDisplayViewModel d = o as Data3DDisplayViewModel;
                if (d == null) continue;

                d.SetLayers(layerStart, layerEnd);
            }
        }

        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(e.Parameter == null)
            {
                itemsControl.SelectAll();
                return;
            }

            string p = e.Parameter.ToString();
            if (p.ToLower() == "clear")
            {
                itemsControl.UnselectAll();
                return;
            }
            else itemsControl.SelectAll();
        }

        private async void GetSelectedVolumes_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Check bin size
            if(BatchApply.LayerBinSize == 0)
            {
                DialogBox.Show("Invalid bin size.", "Bin size must be a positive integer", "Volume", DialogIcon.Stop);
                return;
            }

            // Check color scales

            List<Data3DDisplayViewModel> toConvert = new List<Data3DDisplayViewModel>();
            List<Data3DDisplayViewModel> notSolid = new List<Data3DDisplayViewModel>();

            foreach (var obj in itemsControl.SelectedItems)
            {
                Data3DDisplayViewModel d = obj as Data3DDisplayViewModel;
                if (d == null)
                    continue;

                toConvert.Add(d);

                if (d.ColorScale != ColorScaleTypes.Solid)
                    notSolid.Add(d);
            }

            if(toConvert.Count == 0)
            {
                DialogBox.Show("No objects selected", 
                    "Select one or more of displays to convert and try again.", "Volume", DialogIcon.Stop);
                return;
            }
            
            if(notSolid.Count != 0)
            {
                string list = string.Empty;
                foreach (var item in notSolid)
                {
                    list += "\n" + item.DataSource.DataName;
                }

                bool? result = DialogBox.Show("The following displays do not have solid color scales set:" + list,
                    "Click OK to proceed, which will result in those volumes having a color scale set to white, or click Cancel to adjust the settings.",
                    "Volume", DialogIcon.Information);
                if (result != true)
                    return;
            }

            ProgressWindow pw = new ProgressWindow("Creating volumes. Please wait...", "Volumes");
            pw.Show();

            List<Volume> volumes = new List<Volume>();
            Dictionary<Data3DDisplayViewModel, string> notConverted = new Dictionary<Data3DDisplayViewModel, string>();

            int pos = 0;
            foreach (var item in toConvert)
            {
                try
                {
                    volumes.Add(await item.GetSelectedVolumeAsync(BatchApply.LayerBinSize));
                }
                catch(ArgumentException ARex)
                {
                    notConverted.Add(item, ARex.Message);
                }
                catch(Exception ex)
                {
                    string message = ex.Message;
                    if (ex.InnerException != null) message += ": " + ex.InnerException.Message;

                    notConverted.Add(item, message);
                }

                pw.UpdateProgress(Percentage.GetPercent(++pos, toConvert.Count));
            }

            pw.Close();
            pw = null;

            if(notConverted.Count != 0)
            {
                string list = string.Empty;
                foreach (var item in notConverted)
                {
                    list += $"\n{item.Key.DataSource.DataName}: {item.Value}";
                }
                Dialog.Show("The following objects could not be converted to volumes:" + list,
                    "Those not listed above were successfully converted.", "Volume", DialogIcon.Stop);                
            }

            AvailableHost.AvailableVolumesSource.AddVolumes(volumes);
        }
        private async void GetSummedLayers_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            List<Data3DDisplayViewModel> toSum = new List<Data3DDisplayViewModel>();

            foreach (var obj in itemsControl.SelectedItems)
            {
                Data3DDisplayViewModel d = obj as Data3DDisplayViewModel;
                if (d == null)
                    continue;

                toSum.Add(d);
            }

            if(toSum.Count == 0)
            {
                DialogBox.Show("No objects selected to sum",
                    "Select one or more of the displays and try again.", "Sum", DialogIcon.Stop);
                return;
            }

            ProgressWindow pw = new ProgressWindow("Summing displays. Please wait...", "Sum");
            pw.Show();

            List<Data2D> summed = new List<Data2D>();
            Dictionary<Data3DDisplayViewModel, string> notSummed = new Dictionary<Data3DDisplayViewModel, string>();

            int pos = 0;
            foreach (var item in toSum)
            {
                try
                {
                    summed.Add(await item.SumSelectedLayersAsync());
                }
                catch(Exception ex)
                {
                    string message = ex.Message;
                    if (ex.InnerException != null) message += ": " + ex.InnerException.Message;

                    notSummed.Add(item, message);
                }

                pw.UpdateProgress(Percentage.GetPercent(++pos, toSum.Count));
            }

            pw.Close();
            pw = null;

            if (notSummed.Count != 0)
            {
                string list = string.Empty;
                foreach (var item in notSummed)
                {
                    list += $"\n{item.Key.DataSource.DataName}: {item.Value}";
                }
                Dialog.Show("The following objects could not be summed:" + list,
                    "Those not listed above were successfully summed.", "Sum", DialogIcon.Stop);
            }

            AvailableHost.AvailableTablesSource.AddTables(summed);
        }
    }
}
