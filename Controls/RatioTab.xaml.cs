using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Common;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Fusion;
using ImagingSIMS.Data.Imaging;
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

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for RatioTab.xaml
    /// </summary>
    public partial class RatioTab : UserControl
    {
        ProgressWindow _pw;

        public static readonly DependencyProperty InputDataProperty = DependencyProperty.Register("InputData",
            typeof(RatioTabViewModel), typeof(RatioTab));
        
        public RatioTabViewModel InputData
        {
            get { return (RatioTabViewModel)GetValue(InputDataProperty); }
            set { SetValue(InputDataProperty, value); }
        }

        public RatioTab()
        {
            InputData = new RatioTabViewModel();

            InitializeComponent();
        }

        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (sender == null) return;

            foreach(object o in listAvailable.SelectedItems)
            {
                Data2D d = o as Data2D;
                if (d == null) continue;

                if (b == buttonAddNumerator)
                {
                    InputData.NumeratorTables.Add(d);
                }
                else if (b == buttonAddDenomintor)
                {
                    InputData.DenominatorTables.Add(d);
                }
            }
        }
        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (sender == null) return;

            List<Data2D> selected = new List<Data2D>();

            if (b == buttonRemoveNumerator)
            {
                foreach(object o in listNumerator.SelectedItems)
                {
                    Data2D d = o as Data2D;
                    if (d == null) continue;

                    selected.Add(d);
                }
                foreach(Data2D d in selected)
                {
                    if (InputData.NumeratorTables.Contains(d))
                        InputData.NumeratorTables.Remove(d);
                }
            }
            else if (b == buttonRemoveDenominator)
            {
                foreach (object o in listDenominator.SelectedItems)
                {
                    Data2D d = o as Data2D;
                    if (d == null) continue;

                    selected.Add(d);
                }
                foreach (Data2D d in selected)
                {
                    if (InputData.DenominatorTables.Contains(d))
                        InputData.DenominatorTables.Remove(d);
                }
            }
        }
        private void buttonInvert_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Data2D> temp = InputData.NumeratorTables;
            InputData.NumeratorTables = InputData.DenominatorTables;
            InputData.DenominatorTables = temp;
        }
        private void buttonClearAll_Click(object sender, RoutedEventArgs e)
        {
            InputData.NumeratorTables.Clear();
            InputData.DenominatorTables.Clear();
        }

        public void SetHighResImage(BitmapSource bs)
        {
            InputData.HighRes = bs;            

            InputData.FuseImagesFirst = true;
        }

        private void buttonClearHighRes_Click(object sender, RoutedEventArgs e)
        {
            InputData.HighRes = null;
        }
        private void buttonLoadHighRes_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            if (b == null) return;

            OpenFileDialog ofd = FileDialogs.OpenImageDialog;
            ofd.Multiselect = false;
            if (ofd.ShowDialog() != true) return;

            BitmapImage src = new BitmapImage();

            src.BeginInit();
            src.UriSource = new Uri(ofd.FileName, UriKind.Absolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            src.EndInit();
            
            InputData.HighRes = src;
        }

        private void buttonDoRatio_Click(object sender, RoutedEventArgs e)
        {
            // Validate input parameters
            if(InputData.NumeratorTables.Count == 0)
            {
                DialogBox.Show("No numerator tables selected.",
                    "Please select one or more tables for the ratio numerator.", "Ratio", DialogBoxIcon.Stop);
                return;
            }
            if(InputData.DenominatorTables.Count == 0)
            {
                DialogBox.Show("No denominator tables selected.",
                    "Please select one or more tables for the ratio denominator.", "Ratio", DialogBoxIcon.Stop);
                return;
            }
            if (string.IsNullOrEmpty(InputData.OutputBaseName))
            {
                DialogBox.Show("No output base name specified.", 
                    "Please enter a name (or base name) for the generated table(s).", "Ratio", DialogBoxIcon.Stop);
                return;
            }

            if (InputData.DoCrossRatio && InputData.NumeratorTables.Count != InputData.DenominatorTables.Count)
            {
                DialogBox.Show("Invalid number of numerator and denominator tables.",
                    "In order to perform a cross ratio calculation, the number of numerator tables must equal the number of denominator tables.",
                    "Ratio", DialogBoxIcon.Stop);
                return;
            }

            // Validate table dimensions
            int numWidth = 0;
            int numHeight = 0;
            foreach(Data2D d in InputData.NumeratorTables)
            {
                if (numWidth == 0) numWidth = d.Width;
                if (numHeight == 0) numHeight = d.Height;

                if (numWidth != d.Width || numHeight != d.Height)
                {
                    DialogBox.Show("Invalid table dimensions.",
                        "One or more numerator tables does not match the dimensions of the others.", "Ratio", DialogBoxIcon.Stop);
                    return;
                }
            }

            int denWidth = 0;
            int denHeight = 0;
            foreach(Data2D d in InputData.DenominatorTables)
            {
                if (denWidth == 0) denWidth = d.Width;
                if (denHeight == 0) denHeight = d.Height;

                if(denWidth != d.Width || denHeight != d.Height)
                {
                    DialogBox.Show("Invalid table dimensions.",
                        "One or more denominator tables does not match the dimensions of the others.", "Ratio", DialogBoxIcon.Stop);
                    return;
                }
            }

            if(numWidth != denWidth || numHeight != denHeight)
            {
                DialogBox.Show("Invalid table dimensions.",
                    "The dimensions of the numerator table(s) must match that of the denominator table(s).", "Ratio", DialogBoxIcon.Stop);
                return;
            }

            // Validate fusion
            if (InputData.FuseImagesFirst)
            {
                if(InputData.HighRes == null)
                {
                    DialogBox.Show("Missing high resolution image.",
                        "Open or drop a high resolution image for the tables or uncheck the option to fuse images.",
                        "Ratio", DialogBoxIcon.Stop);
                    return;
                }

                // Numerator
                if(InputData.HighRes.PixelWidth < numWidth ||
                    InputData.HighRes.PixelHeight < numHeight)
                {
                    DialogBox.Show("Invalid image size.",
                        "One or both of the dimensions of the high resolution numerator image is (are) smaller than the data tables.",
                        "Ratio", DialogBoxIcon.Stop);
                    return;
                }

                // Denominator -- Probably redundant since table dimensions were already checked
                if (InputData.HighRes.PixelWidth < denWidth ||
                    InputData.HighRes.PixelHeight < denHeight)
                {
                    DialogBox.Show("Invalid image size.",
                        "One or both of the dimensions of the high resolution denominator image is (are) smaller than the data tables.",
                        "Ratio", DialogBoxIcon.Stop);
                    return;
                }
            }

            // Set up background worker
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = false;
            bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
            bw.ProgressChanged += Bw_ProgressChanged;
            bw.DoWork += Bw_DoWork;

            _pw = new ProgressWindow("Calculating ratio tables. Please wait...", "Ratio");
            _pw.Show();

            Data2D highRes = null;
            if (InputData.FuseImagesFirst)
            {
                highRes = ImageHelper.ConvertToData2D(InputData.HighRes, Data2DConverionType.Grayscale);
            }

            // args
            // [0]: Data2D[]        Numerator tables
            // [1]: Data2D[]        Denominator tables
            // [2]: string          Output base name
            // [3]: bool            Do cross
            // [4]: bool            Multiply by factor
            // [5]: double          Factor
            // [6]: bool            Remove original tables
            // [7]: bool            Fuse images first
            // [8]: FusionType      Fusion method
            // [9]: Data2D          High res image
            object[] args = new object[]
            {
                InputData.NumeratorTables.ToList(),
                InputData.DenominatorTables.ToList(),
                InputData.OutputBaseName,
                InputData.DoCrossRatio,
                InputData.MultiplyByFactor,
                InputData.MultiplyFactor,
                InputData.RemoveOriginalTables,
                InputData.FuseImagesFirst,
                InputData.FusionType,
                highRes
            };
            bw.RunWorkerAsync(args);
        }

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            object[] args = (object[])e.Argument;

            // args
            // [0]: Data2D[]        Numerator tables
            // [1]: Data2D[]        Denominator tables
            // [2]: string          Output base name
            // [3]: bool            Do cross
            // [4]: bool            Multiply by factor
            // [5]: double          Factor
            // [6]: bool            Remove original tables
            // [7]: bool            Fuse images first
            // [8]: FusionType      Fusion method
            // [9]: Data2D          High res image
            List<Data2D> numeratorTables = (List<Data2D>)args[0];
            List<Data2D> denominatorTables = (List<Data2D>)args[1];
            string baseName = (string)args[2];            
            bool doCross = (bool)args[3];
            bool multiplyByFactor = (bool)args[4];
            double multiplyFactor = (double)args[5];
            bool removeOriginal = (bool)args[6];
            bool fusionFirst = (bool)args[7];
            FusionType fusionType = (FusionType)args[8];
            Data2D highRes = (Data2D)args[9];

            int totalSteps = 0;
            int pos = 0;
            if (fusionFirst)
            {
                totalSteps += numeratorTables.Count + denominatorTables.Count;
            }
            if (doCross)
            {
                totalSteps += numeratorTables.Count;
            }
            else
            {
                totalSteps += numeratorTables.Count + 1;
            }

            if (fusionFirst)
            {
                int numTables = numeratorTables.Count;
                for (int i = 0; i < numTables; i++)
                {
                    Fusion fusionNum;

                    switch (fusionType)
                    {
                        case FusionType.HSL:
                            fusionNum = new HSLFusion((float[,])highRes, (float[,])numeratorTables[i], Color.FromArgb(255, 0, 0, 255));
                            break;
                        //case FusionType.WeightedAverage:
                        //    fusionNum = new WeightedAverageFusion((float[,])numHighRes, (float[,])numeratorTables[i], Color.FromArgb(255, 0, 0, 255));
                        //    break;
                        case FusionType.HSLSmooth:
                            fusionNum = new HSLSmoothFusion((float[,])highRes, (float[,])numeratorTables[i], Color.FromArgb(255, 0, 0, 255));
                            break;
                        case FusionType.Adaptive:
                            fusionNum = new AdaptiveIHSFusion((float[,])highRes, (float[,])numeratorTables[i], Color.FromArgb(255, 0, 0, 255));
                            break;
                        case FusionType.HSLShift:
                            fusionNum = new HSLShiftFusion((float[,])highRes, (float[,])numeratorTables[i], Color.FromArgb(255, 0, 0, 255));
                            ((HSLShiftFusion)fusionNum).WindowSize = 11;
                            break;
                        default:
                            fusionNum = new HSLFusion((float[,])highRes, (float[,])numeratorTables[i], Color.FromArgb(255, 0, 0, 255));
                            return;
                    }

                    Data3D fusedNum3D = fusionNum.DoFusion();
                    Data2D fusedNum = new Data2D(fusedNum3D.Width, fusedNum3D.Height);
                    for (int x = 0; x < fusedNum3D.Width; x++)
                    {
                        for (int y = 0; y < fusedNum3D.Height; y++)
                        {
                            // BGRA
                            float[] pixel = fusedNum3D[x, y];

                            // Check each pixel. Original data is encoded as blue
                            if(pixel[0] > 0)
                            {
                                // If pixel is gray though, that means there is no
                                // signal present from the low res image, so output
                                // value should be zero
                                if (pixel[0] == pixel[1] && pixel[1] == pixel[2])
                                    continue;

                                // Data encoded as blue for this procedure
                                fusedNum[x, y] = pixel[0];
                            }
                        }
                    }

                    // Replace original table with the fused data
                    numeratorTables[i] = fusedNum;

                    // Update progress
                    pos++;
                    if (bw != null)
                        bw.ReportProgress(pos * 100 / totalSteps);

                    Fusion fusionDen;

                    switch (fusionType)
                    {
                        case FusionType.HSL:
                            fusionDen = new HSLFusion((float[,])highRes, (float[,])denominatorTables[i], Color.FromArgb(255, 0, 0, 255));
                            break;
                        //case FusionType.WeightedAverage:
                        //    fusionDen = new WeightedAverageFusion((float[,])denHighRes, (float[,])denominatorTables[i], Color.FromArgb(255, 0, 0, 255));
                        //    break;
                        case FusionType.HSLSmooth:
                            fusionDen = new HSLSmoothFusion((float[,])highRes, (float[,])denominatorTables[i], Color.FromArgb(255, 0, 0, 255));
                            break;
                        case FusionType.Adaptive:
                            fusionDen = new AdaptiveIHSFusion((float[,])highRes, (float[,])denominatorTables[i], Color.FromArgb(255, 0, 0, 255));
                            break;
                        case FusionType.HSLShift:
                            fusionDen = new HSLShiftFusion((float[,])highRes, (float[,])denominatorTables[i], Color.FromArgb(255, 0, 0, 255));
                            ((HSLShiftFusion)fusionNum).WindowSize = 11;
                            break;
                        default:
                            fusionDen = new HSLFusion((float[,])highRes, (float[,])denominatorTables[i], Color.FromArgb(255, 0, 0, 255));
                            return;
                    }

                    Data3D fusedDen3D = fusionDen.DoFusion();
                    Data2D fusedDen = new Data2D(fusedDen3D.Width, fusedDen3D.Height);
                    for (int x = 0; x < fusedDen3D.Width; x++)
                    {
                        for (int y = 0; y < fusedDen3D.Height; y++)
                        {
                            // BGRA
                            float[] pixel = fusedDen3D[x, y];

                            // Check each pixel. Original data is encoded as blue
                            if (pixel[0] > 0)
                            {
                                // If pixel is gray though, that means there is no
                                // signal present from the low res image, so output
                                // value should be zero
                                if (pixel[0] == pixel[1] && pixel[1] == pixel[2])
                                    continue;

                                // Data encoded as blue for this procedure
                                fusedDen[x, y] = pixel[0];
                            }
                        }
                    }

                    // Replace original table with the fused data
                    denominatorTables[i] = fusedDen;

                    // Update progress
                    pos++;
                    if (bw != null)
                        bw.ReportProgress(pos * 100 / totalSteps);
                }
            }            

            List<Data2D> ratioTables = new List<Data2D>();
            int outWidth = numeratorTables[0].Width;
            int outHeight = numeratorTables[0].Height;

            if (doCross)
            {
                int toRatio = numeratorTables.Count;

                for (int i = 0; i < toRatio; i++)
                {
                    Data2D ratio = numeratorTables[i] / denominatorTables[i];
                    ratio.DataName = $"{baseName}_{i}";
                    ratioTables.Add(ratio);

                    pos++;

                    if (bw != null)
                        bw.ReportProgress(pos * 100 / totalSteps);
                }
            }
            else
            {
                int toSum = numeratorTables.Count;

                Data2D summedNum = new Data2D(outWidth, outHeight);
                Data2D summedDen = new Data2D(outWidth, outHeight);

                for (int i = 0; i < toSum; i++)
                {
                    summedNum += numeratorTables[i];
                    summedDen += denominatorTables[i];

                    pos++;

                    if (bw != null)
                        bw.ReportProgress(pos * 100 / totalSteps);
                }

                Data2D ratio = summedNum / summedDen;
                ratio.DataName = baseName;
                ratioTables.Add(ratio);
                pos++;

                bw.ReportProgress(pos * 100 / totalSteps);
            }

            // Remove NaN from resulting data sets and 
            // multiply by factor if specified
            foreach (Data2D d in ratioTables)
            {
                for (int x = 0; x < d.Width; x++)
                {
                    for (int y = 0; y < d.Height; y++)
                    {
                        if (float.IsNaN(d[x, y]) ||  float.IsInfinity(d[x,y]))
                            d[x, y] = 0;

                        if (multiplyByFactor)
                            d[x, y] *= (float)multiplyFactor;
                    }
                }

                d.Refresh();
            }

            // results
            // [0]: List<Data2D>    Ratio tables
            // [1]: bool            Remove original tables
            // [2]: List<Data2D>    Original numerator tables
            // [3]: List<Data2D>    Original denominator tables
            object[] results = new object[]
            {
                ratioTables,
                removeOriginal,
                numeratorTables,
                denominatorTables
            };
            e.Result = results;
        }
        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (_pw != null)
                _pw.UpdateProgress(e.ProgressPercentage);
        }
        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // See if an exception was thrown on the background thread
            if(e.Error != null)
            {
                DialogBox.Show("Could not perform the ratio caluclations.", e.Error.Message, "Ratio", DialogBoxIcon.Stop);
                return;
            }
            // results
            // [0]: List<Data2D>    Ratio tables
            // [1]: bool            Remove original tables
            // [2]: List<Data2D>    Original numerator tables
            // [3]: List<Data2D>    Original denominator tables
            object[] result = (object[])e.Result;

            List<Data2D> ratioTables = (List<Data2D>)result[0];
            bool removeOriginal = (bool)result[1];
            List<Data2D> originalNumerator = (List<Data2D>)result[2];
            List<Data2D> originalDenominator = (List<Data2D>)result[3];

            _pw.ProgressFinished("Ratio complete");

            foreach(Data2D d in ratioTables)
            {
                InputData.AvailableTables.Add(d);
            }

            if (removeOriginal)
            {
                foreach(Data2D d in originalNumerator)
                {
                    if (InputData.AvailableTables.Contains(d))
                    {
                        InputData.AvailableTables.Remove(d);
                    }
                }
                foreach(Data2D d in originalDenominator)
                {
                    if (InputData.AvailableTables.Contains(d))
                    {
                        InputData.AvailableTables.Remove(d);
                    }
                }
            }
            
            _pw.Close();
            _pw = null;
        }
    }

    public class RatioTabViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Data2D> _availabileTables;
        ObservableCollection<Data2D> _numeratorTables;
        ObservableCollection<Data2D> _denominatorTables;
        string _outputBaseName;
        bool _removeOriginalTables;
        bool _doCrossRatio;
        bool _multiplyByFactor;
        double _multiplyFactor;
        bool _fuseImagesFirst;
        FusionType _fusionType;
        BitmapSource _numeratorHighRes;

        public ObservableCollection<Data2D> AvailableTables
        {
            get { return _availabileTables; }
            set
            {
                if(_availabileTables != value)
                {
                    _availabileTables = value;
                    NotifyPropertyChanged("AvailableTables");
                }
            }
        }
        public ObservableCollection<Data2D> NumeratorTables
        {
            get { return _numeratorTables; }
            set
            {
                if(_numeratorTables != value)
                {
                    _numeratorTables = value;
                    NotifyPropertyChanged("NumeratorTables");
                }
            }
        }
        public ObservableCollection<Data2D> DenominatorTables
        {
            get { return _denominatorTables; }
            set
            {
                if(_denominatorTables!= value)
                {
                    _denominatorTables = value;
                    NotifyPropertyChanged("DenominatorTables");
                }
            }
        }

        public string OutputBaseName
        {
            get { return _outputBaseName; }
            set
            {
                if(_outputBaseName!= value)
                {
                    _outputBaseName = value;
                    NotifyPropertyChanged("OutputBaseName");
                }
            }
        }
        public bool RemoveOriginalTables
        {
            get { return _removeOriginalTables; }
            set
            {
                if(_removeOriginalTables != value)
                {
                    _removeOriginalTables = value;
                    NotifyPropertyChanged("RemoveOriginalTables");
                }
            }
        }
        public bool DoCrossRatio
        {
            get { return _doCrossRatio; }
            set
            {
                if(_doCrossRatio!= value)
                {
                    _doCrossRatio = value;
                    NotifyPropertyChanged("DoCrossRatio");
                }
            }
        }
        public bool MultiplyByFactor
        {
            get { return _multiplyByFactor; }
            set
            {
                if(_multiplyByFactor!= value)
                {
                    _multiplyByFactor = value;
                    NotifyPropertyChanged("MultiplyByFactor");
                }
            }
        }
        public double MultiplyFactor
        {
            get { return _multiplyFactor; }
            set
            {
                if(_multiplyFactor != value)
                {
                    _multiplyFactor = value;
                    NotifyPropertyChanged("MultiplyFactor");
                }
            }
        }
        public bool FuseImagesFirst
        {
            get { return _fuseImagesFirst; }
            set
            {
                if(_fuseImagesFirst != value)
                {
                    _fuseImagesFirst = value;
                    NotifyPropertyChanged("FuseImagesFirst");
                }
            }
        }
        public FusionType FusionType
        {
            get { return _fusionType; }
            set
            {
                if(_fusionType!= value)
                {
                    _fusionType = value;
                    NotifyPropertyChanged("FusionType");
                }
            }
        }
        public BitmapSource HighRes
        {
            get { return _numeratorHighRes; }
            set
            {
                if(_numeratorHighRes != value)
                {
                    _numeratorHighRes = value;
                    NotifyPropertyChanged("HighRes");
                }
            }
        }

        public RatioTabViewModel()
        {
            AvailableTables = new ObservableCollection<Data2D>();
            NumeratorTables = new ObservableCollection<Data2D>();
            DenominatorTables = new ObservableCollection<Data2D>();

            MultiplyFactor = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
