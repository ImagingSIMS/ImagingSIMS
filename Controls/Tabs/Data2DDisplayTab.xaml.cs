﻿using System;
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

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for Data2DDisplayTab.xaml
    /// </summary>
    public partial class Data2DDisplayTab : UserControl
    {
        public static readonly DependencyProperty BatchApplyProperty = DependencyProperty.Register("BatchApply",
            typeof(Data2DDisplayBatchApplyViewModel), typeof(Data2DDisplayTab));
        public Data2DDisplayBatchApplyViewModel BatchApply
        {
            get { return (Data2DDisplayBatchApplyViewModel)GetValue(BatchApplyProperty); }
            set { SetValue(BatchApplyProperty, value); }
        }

        ObservableCollection<Data2DDisplayViewModel> _displayItems;
        public ObservableCollection<Data2DDisplayViewModel> DisplayItems
        {
            get { return _displayItems; }
            set { _displayItems = value; }
        }

        public Data2DDisplayTab()
        {
            DisplayItems = new ObservableCollection<Data2DDisplayViewModel>();
            BatchApply = new Data2DDisplayBatchApplyViewModel();

            InitializeComponent();
        }
        public Data2DDisplayTab(ColorScaleTypes ColorScale)
        {
            DisplayItems = new ObservableCollection<Data2DDisplayViewModel>();

            BatchApply = new Data2DDisplayBatchApplyViewModel(ColorScale);

            InitializeComponent();
        }
        public Data2DDisplayTab(List<Data2D> DataSources)
        {
            DisplayItems = new ObservableCollection<Data2DDisplayViewModel>();

            foreach (Data2D d in DataSources)
            {
                DisplayItems.Add(new Data2DDisplayViewModel(d, ColorScaleTypes.ThermalWarm));
            }

            BatchApply = new Data2DDisplayBatchApplyViewModel();

            InitializeComponent();
        }
        public Data2DDisplayTab(List<Data2D> DataSources, ColorScaleTypes ColorScale)
        {
            DisplayItems = new ObservableCollection<Data2DDisplayViewModel>();

            foreach (Data2D d in DataSources)
            {
                DisplayItems.Add(new Data2DDisplayViewModel(d, ColorScale));
            }

            BatchApply = new Data2DDisplayBatchApplyViewModel(ColorScale);

            InitializeComponent();
        }
        public Data2DDisplayTab(List<Data2D> DataSources, Color SolidColor)
        {
            DisplayItems = new ObservableCollection<Data2DDisplayViewModel>();

            foreach (Data2D d in DataSources)
            {
                DisplayItems.Add(new Data2DDisplayViewModel(d, SolidColor));
            }

            BatchApply = new Data2DDisplayBatchApplyViewModel(SolidColor);

            InitializeComponent();
        }

        public void AddDataSource(Data2D dataSource)
        {
            DisplayItems.Add(new Data2DDisplayViewModel(dataSource, BatchApply.ColorScale));
        }
        public async Task AddDataSourceAsync(Data2D dataSource)
        {
            Data2DDisplayViewModel displayItem = Data2DDisplayViewModel.Empty;
            await displayItem.SetData2DDisplayItemAsync(dataSource, BatchApply.ColorScale);
            DisplayItems.Add(displayItem);
        }
        public async Task AddDataSourceAsync(List<Data2D> dataSources)
        {
            foreach(Data2D dataSource in dataSources)
            {
                await AddDataSourceAsync(dataSource);
            }
        }

        private void buttonShowColor_MouseEnter(object sender, RoutedEventArgs e)
        {
            popupSolidColorScale.IsOpen = true;
        }
        private void buttonApply_Click(object sender, RoutedEventArgs e)
        {
            ColorScaleTypes scale = BatchApply.ColorScale;
            Color solid = BatchApply.SolidColorScale;

            Button button = sender as Button;
            if (sender == null) return;

            if (sender == buttonApplyAll)
            {
                foreach (object o in itemsControl.Items)
                {
                    Data2DDisplayViewModel d = o as Data2DDisplayViewModel;
                    if (d == null) continue;

                    d.ColorScale = scale;
                    if (scale == ColorScaleTypes.Solid)
                    {
                        d.SolidColorScale = solid;
                    }
                }
            }
            else if (button == buttonApplySelected)
            {
                foreach (object o in itemsControl.SelectedItems)
                {
                    Data2DDisplayViewModel d = o as Data2DDisplayViewModel;
                    if (d == null) continue;

                    d.ColorScale = scale;
                    if (scale == ColorScaleTypes.Solid)
                    {
                        d.SolidColorScale = solid;
                    }
                }
            }
        }
        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (sender == null) return;

            if(sender == buttonResetScale)
            {
                foreach(object o in itemsControl.Items)
                {
                    Data2DDisplayViewModel d = o as Data2DDisplayViewModel;
                    if (d == null) return;

                    d.Saturation = d.InitialSaturation;
                }
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

            Dictionary<Data2DDisplayViewModel, string> notSaved = new Dictionary<Data2DDisplayViewModel, string>();

            foreach(Data2DDisplayViewModel displayItem in DisplayItems)
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
                DialogBox.Show("Image(s) saved successfully!", sfd.FileName, "Save", DialogBoxIcon.Ok);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (Data2DDisplayViewModel item in notSaved.Keys)
                {
                    sb.AppendLine(item.DataSource.DataName + string.Format("({0})", notSaved[item]));
                }
                DialogBox.Show("One or more images were not saved successfully and are listed below. Any other images have been saved.",
                    sb.ToString(), "Save", DialogBoxIcon.Error);
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
            List<Data2D> selectedTables = new List<Data2D>();
            foreach (Data2DDisplayViewModel displayItem in itemsControl.SelectedItems)
            {
                selectedTables.Add(displayItem.DataSource);
            }

            if (selectedTables.Count == 0)
            {
                DialogBox.Show("No tables selected.", "Select one or more tables to expand and try again.", "Expand", DialogBoxIcon.Error);
            }

            foreach (Data2D d in selectedTables)
            {
                Data2D expanded = d.ExpandIntensity(windowSize);
                AddDataSource(expanded);
            }
        }

        private void removeItem_Click(object sender, RoutedEventArgs e)
        {
            Data2DDisplay display = e.Source as Data2DDisplay;
            if (display == null) return;

            Data2DDisplayViewModel displayItem = display.DisplayItem;
            if(displayItem!= null)
            {
                if (DisplayItems.Contains(displayItem))
                {
                    DisplayItems.Remove(displayItem);
                }
            }
        }

        public static readonly DependencyProperty AnalysisOutputTextProperty = DependencyProperty.Register("AnalysisOutputText",
            typeof(string), typeof(Data2DDisplayTab));
        public string AnalysisOutputText
        {
            get { return (string)GetValue(AnalysisOutputTextProperty); }
            set { SetValue(AnalysisOutputTextProperty, value); }
        }

        private void analyzePixels_Click(object sender, RoutedEventArgs e)
        {
            Data2DDisplay display = e.Source as Data2DDisplay;
            if (display == null) return;

            Data2D d = display.DisplayItem.DataSource;
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
        }

        public BitmapSource GetOverlay()
        {

            int imgCount = itemsControl.SelectedItems.Count;
            if (imgCount == 0)
            {
                DialogBox db = new DialogBox("No images selected", "Select two or more images to overlay and try again.",
                    "Overlay", DialogBoxIcon.Error);
                db.ShowDialog();
                return null;
            }
            if (imgCount == 1)
            {
                DialogBox db = new DialogBox("Only one image selected", "Select two or more images to overlay and try again.",
                    "Overlay", DialogBoxIcon.Error);
                db.ShowDialog();
                return null;
            }
            BitmapSource[] toOverlay = new BitmapSource[imgCount];

            int i = 0;
            foreach (Data2DDisplayViewModel displayItem in itemsControl.SelectedItems)
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
                    "Overlay", DialogBoxIcon.Error);
                db.ShowDialog();
                return null;
            }

            return Overlay.CreateOverlay(toOverlay);

        }
    }
}