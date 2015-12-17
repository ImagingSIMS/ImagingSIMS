using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Common.Math;
using ImagingSIMS.Data;
using ImagingSIMS.Data.ClusterIdentification;
using ImagingSIMS.Data.Imaging;
using System.Globalization;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for ClusterTab.xaml
    /// </summary>
    public partial class ClusterTab : UserControl
    {
        CancellationTokenSource _cancellationTokenSource;

        public static readonly DependencyProperty FoundClustersProperty = DependencyProperty.Register("FoundClusters",
            typeof(FoundClusters), typeof(ClusterTab), new FrameworkPropertyMetadata(FoundClusters.Empty));
        public static readonly DependencyProperty ParametersProperty = DependencyProperty.Register("Parameters",
            typeof(ClusterTabParameters), typeof(ClusterTab));
        public static readonly DependencyProperty SmoothWindowSizeProperty = DependencyProperty.Register("SmoothWindowSize",
            typeof(int), typeof(ClusterTab));
        public static readonly DependencyProperty DataInputProperty = DependencyProperty.Register("DataInput",
            typeof(Data2D), typeof(ClusterTab));
        public static readonly DependencyProperty IsIdentifyingProperty = DependencyProperty.Register("IsIdentifying",
            typeof(bool), typeof(ClusterTab));
        public static readonly DependencyProperty ClusterImagePixelTextProperty = DependencyProperty.Register("ClusterImagePixelText",
            typeof(string), typeof(ClusterTab));
        public static readonly DependencyProperty ClusterImageClusterTextProperty = DependencyProperty.Register("ClusterImageClusterText",
            typeof(string), typeof(ClusterTab));

        public FoundClusters FoundClusters
        {
            get{return (FoundClusters)GetValue(FoundClustersProperty);}
            set{SetValue(FoundClustersProperty,value);}
        }
        public ClusterTabParameters Parameters
        {
            get { return (ClusterTabParameters)GetValue(ParametersProperty); }
            set { SetValue(ParametersProperty, value); }
        }
        public int SmoothWindowSize
        {
            get { return (int)GetValue(SmoothWindowSizeProperty); }
            set { SetValue(SmoothWindowSizeProperty, value); }
        }
        public Data2D DataInput
        {
            get { return (Data2D)GetValue(DataInputProperty); }
            set { SetValue(DataInputProperty, value); }
        }
        public bool IsIdentifying
        {
            get { return (bool)GetValue(IsIdentifyingProperty); }
            set { SetValue(IsIdentifyingProperty, value); }
        }
        public string ClusterImagePixelText
        {
            get { return (string)GetValue(ClusterImagePixelTextProperty); }
            set { SetValue(ClusterImagePixelTextProperty, value); }
        }
        public string ClusterImageClusterText
        {
            get { return (string)GetValue(ClusterImageClusterTextProperty); }
            set { SetValue(ClusterImageClusterTextProperty, value); }
        }

        public ClusterTab()
        {
            Parameters = new ClusterTabParameters();

            SmoothWindowSize = 5;

            ClusterImageClusterText = "Left click a cluster to identify it";
            ClusterImagePixelText = "Mouse over the image to determine pixel number";

            InitializeComponent();
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            if (_cancellationTokenSource != null && IsIdentifying)
                _cancellationTokenSource.Cancel();
        }
        private void openImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Bitmap images (.bmp)|*.bmp";
            if (ofd.ShowDialog() != true) return;

            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(ofd.FileName, UriKind.Absolute);
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bmp.EndInit();

            Parameters.InputImageSource = bmp as BitmapSource;
        }
        private async void identify_Click(object sender, RoutedEventArgs e)
        {
            if (Parameters.InputImageSource == null)
            {
                DialogBox.Show("No input image selected.", "Load an input image and try again.", "Identify", DialogBoxIcon.Stop);
                return;
            }

            Data2D d = ImageHelper.ConvertToData2D(Parameters.InputImageSource);

            Mouse.OverrideCursor = Cursors.Wait;

            bool[,] mask = new bool[d.Width, d.Height];
            int threshold = Parameters.PixelThreshold;
            bool invert = Parameters.Invert;

            for (int x = 0; x < d.Width; x++)
            {
                for (int y = 0; y < d.Height; y++)
                {
                    if (invert)
                    {
                        mask[x, y] = d[x, y] <= threshold;
                    }
                    else
                    {
                        mask[x, y] = d[x, y] >= threshold;
                    }
                }
            }

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                IsIdentifying = true;
                FoundClusters = await FoundClusters.FindClustersAsync(mask, _cancellationTokenSource, Parameters.MinPixelArea, Parameters.IntraparticleDistance);
            }
            catch (OperationCanceledException)
            {
                ClosableTabItem.SendStatusUpdate(this, "Cluster identifcation cancelled by user.");
            }
            finally
            {
                IsIdentifying = false;
                Mouse.OverrideCursor = Cursors.Arrow;
            }            
        }
        private void saveImage_Click(object sender, RoutedEventArgs e)
        {
            if (FoundClusters != null || FoundClusters.NumberClusters == 0)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Bitmap images (.bmp)|*.bmp";
                if (sfd.ShowDialog() != null) return;

                bool[,] mask = FoundClusters.MaskArray;
                BitmapSource outputImage = ImageHelper.CreateColorScaleImage((Data2D)mask, ColorScaleTypes.Gray);

                outputImage.Save(sfd.FileName);
            }
        }
        private void saveClusterArray_Click(object sender, RoutedEventArgs e)
        {
            if (FoundClusters == null || FoundClusters.NumberClusters == 0)
            {
                DialogBox.Show("No found cluster data.",
                    "Load an image and identify clusters first, then try again.", "Clusters", DialogBoxIcon.Stop);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Cluster Array Data Files (.cluster)|*.cluster";

            if (sfd.ShowDialog() != true) return;

            try
            {
                //FoundClusters.SaveTextArray(sfd.FileName);
                FoundClusters.SaveBinaryArray(sfd.FileName);
                DialogBox.Show("Cluster data saved sucessfully!", sfd.FileName, "Clusters", DialogBoxIcon.GreenCheck);
            }
            catch (Exception ex)
            {
                DialogBox.Show("Could not save array data.", ex.Message, "Clusters", DialogBoxIcon.Stop);
                return;
            }
        }
        private async void smooth_Click(object sender, RoutedEventArgs e)
        {
            if (FoundClusters == null)
            {
                DialogBox.Show("No found cluster data.",
                    "Load an image and identify clusters first, then try again.", "Clusters", DialogBoxIcon.Stop);
                return;
            }

            Mouse.OverrideCursor = Cursors.Wait;

            bool[,] originalMask = FoundClusters.MaskArray;
            int width = originalMask.GetLength(0);
            int height = originalMask.GetLength(1);

            float[,] data = new float[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    data[x, y] = originalMask[x, y] ? 1 : 0;
                }
            }

            int windowSize = SmoothWindowSize;
            Data2D smoothed = new Data2D(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int startX = x - windowSize / 2;
                    int startY = y - windowSize / 2;

                    List<float> values = new List<float>();
                    for (int a = 0; a < windowSize; a++)
                    {
                        for (int b = 0; b < windowSize; b++)
                        {
                            int locX = startX + a;
                            int locY = startY + b;

                            if (locX < 0 || locX >= width ||
                                locY < 0 || locY >= height)
                                continue;

                            values.Add(data[locX, locY]);
                        }
                    }

                    smoothed[x, y] = MathEx.Average(values.ToArray());
                }
            }

            float cutoff = 1f / (SmoothWindowSize * SmoothWindowSize);

            bool[,] smoothedBool = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (smoothed[x, y] >= cutoff)
                    {
                        smoothed[x, y] = 1.0f;
                        smoothedBool[x, y] = true;
                    }
                    else
                    {
                        smoothed[x, y] = 0.0f;
                        smoothedBool[x, y] = false;
                    }
                }
            }

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                IsIdentifying = true;
                FoundClusters = await FoundClusters.FindClustersAsync(smoothedBool, _cancellationTokenSource, Parameters.MinPixelArea, Parameters.IntraparticleDistance);
            }
            catch (OperationCanceledException)
            {
                ClosableTabItem.SendStatusUpdate(this, "Cluster identifcation cancelled by user.");
            }
            finally
            {
                IsIdentifying = false;
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
        private void generateStats_Click(object sender, RoutedEventArgs e)
        {
            if (FoundClusters == null)
            {
                DialogBox.Show("No clusters found.",
                    "Run the cluster analysis and try again.", "Stats", DialogBoxIcon.Stop);
                return;
            }
            if (DataInput == null)
            {
                DialogBox.Show("No data selected.", 
                    "Drop a data table to analyze and try agein.", "Stats", DialogBoxIcon.Stop);
                return;
            }

            List<CountedClusterStatistic> stats;
            try
            {
                stats = FoundClusters.GenerateStats(DataInput);
            }
            catch (Exception ex)
            {
                DialogBox.Show("Could not generate statistics from data source.", ex.Message, "Stats", DialogBoxIcon.Stop);
                return;
            }

            listViewStats.ItemsSource = stats;

            float wAverage = FoundClusters.WeightedAverage(stats);
            float wStdDev = FoundClusters.WeightedStdDev(stats, wAverage);

            textBlockWeightedStats.Text = string.Format("Weighted Average: {0}\nWeighted Std Dev: {1}", wAverage, wStdDev);
        }
        private void copyStats_Click(object sender, RoutedEventArgs e)
        {
            string headers = "Cluster\tCenter\tMin\tMax\tTotal Counts\tAverage\tStdDev\tPixels\n";

            StringBuilder sb = new StringBuilder(headers);

            List<CountedClusterStatistic> stats = listViewStats.ItemsSource as List<CountedClusterStatistic>;
            if(stats == null)
            {
                DialogBox.Show("Could not copy stats results.", "Generate stats and try again.", "Stats", DialogBoxIcon.Stop);
                return;
            }

            foreach (CountedClusterStatistic stat in stats)
            {
                sb.AppendFormat("{0}\t({1}, {2})\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\n",
                    stat.ClusterNumber, stat.CenterX, stat.CenterY, stat.Minimum,
                    stat.Maximum, stat.TotalCounts, stat.Average, stat.StdDev, stat.NumberPixels);
            }

            float wAverage = FoundClusters.WeightedAverage(stats);
            float wStdDev = FoundClusters.WeightedStdDev(stats, wAverage);

            sb.AppendFormat("\n\nWeighted Average:\t\t{0}\nWeighted Std Dev:\t\t{1}", wAverage, wStdDev);

            //Clipboard.SetText(sb.ToString());
            Clipboard.SetDataObject(sb.ToString());
        }

        public void DropImage(DisplayImage image)
        {
            Data2D data = ImageHelper.ConvertToData2D(image.Source as BitmapSource);
            Parameters.InputImageSource = ImageHelper.CreateColorScaleImage(data, ColorScaleTypes.Gray);
        }
        public void DropData(Data2D data)
        {
            Parameters.InputImageSource = ImageHelper.CreateColorScaleImage(data, ColorScaleTypes.Gray);
        }
        private void imageInput_Drop(object sender, DragEventArgs e)
        {
            Data2D d = e.Data.GetData("Data2D") as Data2D;
            if (d == null) return;

            DropData(d);
        }
        private void dataInput_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Data2D"))
            {
                Data2D d = e.Data.GetData("Data2D") as Data2D;
                DataInput = d;

                e.Handled = true;
            }
        }
        private void removeCluster_Click(object sender, RoutedEventArgs e)
        {
            List<Cluster> toRemove = new List<Cluster>();
            foreach (object item in listViewResults.SelectedItems)
            {
                Cluster cluster = item as Cluster;
                if (cluster == null) continue;
                
                toRemove.Add(cluster);
            }

            foreach (Cluster cluster in toRemove)
            {
                if (FoundClusters.Clusters.Contains(cluster))
                {
                    FoundClusters.Clusters.Remove(cluster);
                }
            }
        }
        private void mergeClusters_Click(object sender, RoutedEventArgs e)
        {
            if(listViewResults.SelectedItems.Count == 0)
            {
                DialogBox.Show("No clusters selected.", 
                    "Select two or more clusters to merge and try again.", "Merge", DialogBoxIcon.Stop);
                return;
            }
            if(listViewResults.SelectedItems.Count == 1)
            {
                DialogBox.Show("Only one clusters selected.",
                    "Select two or more clusters to merge and try again.", "Merge", DialogBoxIcon.Stop);
                return;
            }

            List<Cluster> toMerge = new List<Cluster>();
            foreach(object item in listViewResults.SelectedItems)
            {
                Cluster cluster = item as Cluster;
                if (cluster == null) continue;

                toMerge.Add(cluster);
            }

            Cluster merged = Cluster.MergeClusters(toMerge);

            for (int i = 0; i < toMerge.Count; i++)
            {
                Cluster toRemove = toMerge[i];

                if (i == 0)
                {
                    int index = FoundClusters.Clusters.IndexOf(toRemove);
                    FoundClusters.Clusters.Insert(index, merged);
                }

                FoundClusters.Clusters.Remove(toRemove);
            }
        }

        private void imageOutput_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DataObject obj = new DataObject("FoundClusters", FoundClusters);
                DragDrop.DoDragDrop(sender as DependencyObject, obj, DragDropEffects.Copy);
            }

            Point pixelLocation = getPixelLocation(e.GetPosition(imageOutput));
            ClusterImagePixelText = $"Pixel location: ({pixelLocation.X.ToString("0")}, {pixelLocation.Y.ToString("0")})";
        }
        private void imageOutput_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton== MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed)
            {
                Point pixelLocation = getPixelLocation(e.GetPosition(imageOutput));

                Cluster foundCluster = FoundClusters.FindClusterByPoint((int)pixelLocation.X, (int)pixelLocation.Y);

                if (foundCluster == null)
                {
                    ClusterImageClusterText = $"No cluster found at point ({pixelLocation.X.ToString("0")}, {pixelLocation.Y.ToString("0")})";
                }
                else
                {
                    if (e.RightButton == MouseButtonState.Pressed)
                    {
                        listViewResults.SelectedItem = foundCluster;
                    }
                    ClusterImageClusterText = $"Cluster number #{foundCluster.ClusterNumber} found at point ({pixelLocation.X.ToString("0")}, {pixelLocation.Y.ToString("0")})";
                }
            }
        }

        private Point getPixelLocation(Point imageCoordinates)
        {
            if (FoundClusters == null) return new Point(0, 0);

            return new Point()
            {
                X = imageCoordinates.X * FoundClusters.ColorMask.Width / imageOutput.ActualWidth,
                Y = imageCoordinates.Y * FoundClusters.ColorMask.Height / imageOutput.ActualHeight
            };
        }
    }

    public class ClusterTabParameters : INotifyPropertyChanged
    {
        bool _invert;
        int _pixelThreshold;
        int _intraparticleDistance;
        int _minPixelArea;
        BitmapSource _inputImageSource;

        public bool Invert
        {
            get { return _invert; }
            set
            {
                if (_invert != value)
                {
                    _invert = value;
                    NotifyPropertyChanged("Invert");
                }
            }
        }
        public int PixelThreshold
        {
            get { return _pixelThreshold; }
            set
            {
                if (_pixelThreshold != value)
                {
                    _pixelThreshold = value;
                    NotifyPropertyChanged("PixelThreshold");
                }
            }
        }
        public int IntraparticleDistance
        {
            get { return _intraparticleDistance; }
            set
            {
                if (_intraparticleDistance != value)
                {
                    _intraparticleDistance = value;
                    NotifyPropertyChanged("InterparticleDistance");
                }
            }
        }
        public int MinPixelArea
        {
            get { return _minPixelArea; }
            set
            {
                if (_minPixelArea != value)
                {
                    _minPixelArea = value;
                    NotifyPropertyChanged("MinPixelArea");
                }
            }
        }
        public BitmapSource InputImageSource
        {
            get { return _inputImageSource; }
            set
            {
                if (_inputImageSource != value)
                {
                    _inputImageSource = value;
                    NotifyPropertyChanged("InputImageSource");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ClusterTabParameters()
        {
            Invert = false;
            PixelThreshold = 25;
            IntraparticleDistance = 25;
            MinPixelArea = 50;
        }
    }

    public class NumberClustersToEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            FoundClusters foundClusters = value as FoundClusters;

            if (foundClusters == null) return false;

            return foundClusters.NumberClusters > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
