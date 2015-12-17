using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using ImagingSIMS.Data;
using System.Windows.Data;

namespace ImagingSIMS.ImageRegistration
{
    public class Registration
    {
        const string _fixedInputImageFile = "inputFixedImage";
        const string _movingInputImageFile = "inputMovingImage";
        const string _fixedOutputImageFile = "outputFixedImage";
        const string _movingOutputImageFile = "outputMovingImage";
        const string _fixedPointSetFile = "fixedPointList";
        const string _movingPointSetFile = "movingPointsList";

        RegistrationProgressTextBox _progressUpdater;
        Process _registrationProcess;
        string _outputMovingImagePath;
        string _outputFixedImagePath;
        bool _cancelPending;

        ImageRegistrationTypes _regType;

        public bool RegistrationSucceeded { get; private set; }
        public BitmapSource RegisteredMovingImage { get; private set; }
        public BitmapSource RegisteredFixedImage { get; private set; }
        public static string TransferFolderPath
        {
            get { return AppDataFileHelper.GetFolderPath(); }
        }
        public RegistrationResult RegistrationResults { get; private set; }

        public static string FixedPointSetLocation
        {
            get { return AppDataFileHelper.GetFilePath(_fixedPointSetFile, "pts"); }
        }
        public static string MovingPointSetLocation
        {
            get { return AppDataFileHelper.GetFilePath(_movingPointSetFile, "pts"); }
        }
        
        public Registration()
        {
            RegistrationProgressWindow window = new RegistrationProgressWindow();
            _progressUpdater = window.TextBox;
            window.Show();

            _registrationProcess = new Process();
        }
        public Registration(RegistrationProgressTextBox ProgressUpdater)
        {
            _progressUpdater = ProgressUpdater;

            _registrationProcess = new Process();
        }

        public void InitializeRegistration(ImageRegistrationTypes RegType, BitmapSource InputFixedImage,
            BitmapSource InputMovingImage, ObservableCollection<Point> FixedPoints, ObservableCollection<Point> MovingPoints, RegistrationParameters Parameters)
        {
            string inputFixedImage = AppDataFileHelper.SaveImageToAppData(InputFixedImage, _fixedInputImageFile);
            string inputMovingImage = AppDataFileHelper.SaveImageToAppData(InputMovingImage, _movingInputImageFile);
            _outputFixedImagePath = AppDataFileHelper.GetFilePath(_fixedOutputImageFile);
            _outputMovingImagePath = AppDataFileHelper.GetFilePath(_movingOutputImageFile);
            string fixedPointList = AppDataFileHelper.SavePointsToAppData(FixedPoints, _fixedPointSetFile);
            string movingPointList = AppDataFileHelper.SavePointsToAppData(MovingPoints, _movingPointSetFile);
            string fixedGrayDenoisedPath = AppDataFileHelper.GetFilePath("inputFixedGrayImage");
            string movingGrayDenoisedPath = AppDataFileHelper.GetFilePath("inputMovingGrayImage");

            _registrationProcess.StartInfo = ImageRegistrationHelper.GetStartInfo(
                ImageRegistrationHelper.GenerateArguments(RegType, inputFixedImage, inputMovingImage, 
                _outputFixedImagePath, _outputMovingImagePath, fixedPointList, movingPointList, 
                fixedGrayDenoisedPath, movingGrayDenoisedPath, Parameters));

            _regType = RegType;
        }

        public async Task RegisterAsync()
        {
            await Task.Run(() => register());
        }
        public void LoadRegisteredImages()
        {
            RegisteredMovingImage = AppDataFileHelper.LoadImageFromAppData(_outputMovingImagePath);
            RegisteredFixedImage = AppDataFileHelper.LoadImageFromAppData(_outputFixedImagePath);
        }

        public void RequestCancel()
        {
            _cancelPending = true;
        }

        private void register()
        {
            _registrationProcess.Start();

            while (!_registrationProcess.StandardOutput.EndOfStream)
            {
                if (_cancelPending)
                {
                    _registrationProcess.Kill();
                    _cancelPending = false;
                    updateTextBox("\nRegsitration cancelled by user.");
                    return;
                }

                string line = _registrationProcess.StandardOutput.ReadLine();
                if (!line.StartsWith("Ý")) updateTextBox(line);

                if (line.StartsWith("Registration results:"))
                {
                    try
                    {
                        processResults(line);
                    }
                    catch (Exception ex)
                    {
                        updateTextBox("\nCould not parse registration results. Check progress window for raw results. Reason:\n\t\t" + ex.Message);
                    }
                }
            }

            int exitCode = _registrationProcess.ExitCode;
            updateTextBox(string.Format("\nProcess has exited with code ({0})", exitCode));

            if (exitCode != 0)
            {
                while (!_registrationProcess.StandardError.EndOfStream)
                {
                    string line = "\n" + _registrationProcess.StandardError.ReadLine();
                    updateTextBox(line);
                }
            }

            _registrationProcess.Dispose();
            setIsCompleteProperty();

            if (exitCode == 0)
            {
                RegistrationSucceeded = true;                 
            }
        }

        private void updateTextBox(string line)
        {
            if (!String.IsNullOrEmpty(line))
            {
                _progressUpdater.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        _progressUpdater.UpdateTextBox(line + "\n");
                    }
                ));
            }
        }
        private void setIsCompleteProperty()
        {
            _progressUpdater.Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    _progressUpdater.IsRegistrationComplete = true;
                    _progressUpdater.textBox.ScrollToEnd();
                }
                ));
        }
        private void processResults(string results)
        {
            string sub = results.Substring("Registration results: ".Length);
            string[] splits = sub.Split(';');

            int numResults = splits.Length;
            double[] values = new double[numResults];

            for (int i = 0; i < numResults; i++)
            {
                double.TryParse(splits[i], out values[i]);
            }

            StringBuilder sb = new StringBuilder();

            if (_regType == ImageRegistrationTypes.Translation)
            {
                //Results layout:
                //values[0] (double)TranslationX
                //values[1] (double)TranslationY
                //values[2] (double)Iterations performed
                //values[3] (double)Best metric value
               
                sb.AppendLine();
                sb.AppendFormat("Translation: ({0}, {1})\n", values[0], values[1]);
                sb.AppendFormat("Number iterations performed: {0}\n", values[2]);

                RegistrationResults = new RegistrationResult()
                {
                    TranslationX = values[0],
                    TranslationY = values[1],
                    Iterations = (int)values[2]
                };
            }
            if (_regType == ImageRegistrationTypes.CenterRigid2D)
            {
                //Results layout:
                //values[0] (double)TranslationX
                //values[1] (double)TranslationY
                //values[2] (double)Roation (radians)
                //values[3] (double)RotationCenterX
                //values[4] (double)RotationCenterY
                //values[5] (double)Iterations performed
                //values[6] (double)Best metric value

                sb.AppendLine();
                sb.AppendFormat("Rotation: {0} (radians) {1} (degrees)\n", values[2], values[2] * 180d / Math.PI);
                sb.AppendFormat("Rotation center: ({0}, {1})\n", values[3], values[4]);
                sb.AppendFormat("Translation: ({0}, {1})\n", values[0], values[1]);
                sb.AppendFormat("Number iterations performed: {0}\n", values[5]);

                RegistrationResults = new RegistrationResult()
                {
                    TranslationX = values[0],
                    TranslationY = values[1],
                    Angle = values[2] * 180d/Math.PI,
                    RotationCenterX = values[3],
                    RotationCenterY = values[4],
                    Iterations = (int)values[5]
                };
            }

            if (_regType == ImageRegistrationTypes.CenterSimilarity)
            {
                //Results layout:
                //values[0] (double)TranslationX
                //values[1] (double)TranslationY
                //values[2] (double)Roation (radians)
                //values[3] (double)RotationCenterX
                //values[4] (double)RotationCenterY
                //values[5] (double)FinalScale
                //values[6] (double)Iterations performed
                //values[7] (double)Best metric value

                sb.AppendLine();
                sb.AppendFormat("Rotation: {0} (radians) {1} (degrees)\n", values[2], values[2] * 180d / Math.PI);
                sb.AppendFormat("Rotation center: ({0}, {1})\n", values[3], values[4]);
                sb.AppendFormat("Translation: ({0}, {1})\n", values[0], values[1]);
                sb.AppendFormat("Scale: {0}\n", values[5]);
                sb.AppendFormat("Number iterations performed: {0}\n", values[6]);

                RegistrationResults = new RegistrationResult()
                {
                    TranslationX = values[0],
                    TranslationY = values[1],
                    Angle = values[2] * 180d / Math.PI,
                    RotationCenterX = values[3],
                    RotationCenterY = values[4],
                    Scale = values[5],
                    Iterations = (int)values[6]
                };
            } 

            if(_regType == ImageRegistrationTypes.Affine)
            {
                //Results layout:
                //values[0] (double)TranslationX
                //values[1] (double)TranslationY
                //values[2] (double)Roation (radians)
                //values[3] (double)RotationCenterX
                //values[4] (double)RotationCenterY
                //values[5] (double)FinalScale1
                //values[6] (double)FinalScale2
                //values[7] (double)Iterations performed
                //values[8] (double)Best metric value

                sb.AppendLine();
                sb.AppendFormat("Rotation: {0} (radians) {1} (degrees)\n", values[2], values[2] * 180d / Math.PI);
                sb.AppendFormat("Rotation center: ({0}, {1})\n", values[3], values[4]);
                sb.AppendFormat("Translation: ({0}, {1})\n", values[0], values[1]);
                sb.AppendFormat("Scale 1: {0} Scale 2: {1}\n", values[5], values[6]);
                sb.AppendFormat("Number iterations performed: {0}\n", values[7]);

                RegistrationResults = new RegistrationResult()
                {
                    TranslationX = values[0],
                    TranslationY = values[1],
                    Angle = values[2] * 180d / Math.PI,
                    RotationCenterX = values[3],
                    RotationCenterY = values[4],
                    Scale = values[5],
                    Iterations = (int)values[7]
                };
            }

            updateTextBox(sb.ToString());
        }
    }

    internal static class AppDataFileHelper
    {
        internal static string GetFilePath(string FileName)
        {
            return GetFilePath(FileName, "bmp");
        }
        internal static string GetFilePath(string FileName, string Extension)
        {
            string extension = Extension;
            if(!Extension.StartsWith("."))
            {
                extension = extension.Insert(0, ".");
            }
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"ImagingSIMS\plugins\imageregistration\transfer", FileName + extension);
        }
        internal static string GetFolderPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"ImagingSIMS\plugins\imageregistration\transfer\");
        }
        internal static string SaveImageToAppData(BitmapSource image, string fileName)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            string path = AppDataFileHelper.GetFilePath(fileName);

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"ImagingSIMS\plugins\imageregistration\transfer");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using (var fileStream = new System.IO.FileStream(path, FileMode.Create))
            {
                encoder.Save(fileStream);
            }

            return path;
        }
        internal static BitmapSource LoadImageFromAppData(string fileName)
        {
            BitmapImage src = new BitmapImage();

            src.BeginInit();
            src.UriSource = new Uri(fileName, UriKind.Absolute);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            src.EndInit();

            return src;
        }
        internal static string SavePointsToAppData(ObservableCollection<Point> points, string fileName)
        {
            string path = AppDataFileHelper.GetFilePath(fileName, "pts");

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"ImagingSIMS\plugins\imageregistration\transfer");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            PointSet.PointSetToFile(points, path);

            return path;
        }
    }
    internal static class ImageRegistrationHelper
    { 
        internal static ProcessStartInfo GetStartInfo(string arguments)
        {
            return new ProcessStartInfo()
            {
                FileName = @"Plugins\ITKImageRegistration.exe",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
        }

        internal static string GenerateArguments(ImageRegistrationTypes RegType, string inputFixedImage,
            string inputMovingImage, string outputFixedImage, string outputMovingImage, 
            string fixedPointList, string movingPointList, string fixedDenoisedPath, 
            string movingDenoisedPath, RegistrationParameters parameters)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append((int)RegType + " ");

            sb.Append(inputFixedImage + " ");
            sb.Append(inputMovingImage + " ");
            sb.Append(outputFixedImage + " ");
            sb.Append(outputMovingImage + " ");
            sb.Append(fixedPointList + " ");
            sb.Append(movingPointList + " ");
            sb.Append(fixedDenoisedPath + " ");
            sb.Append(movingDenoisedPath + " ");

            sb.Append(parameters.MaxIterations + " ");

            sb.Append(parameters.Angle + " ");
            sb.Append(parameters.Scale + " ");

            sb.Append((parameters.MultiModal ? 1 : 0) + " ");

            sb.Append(parameters.NumberBins + " ");
            sb.Append(parameters.NumberSamples + " ");
            sb.Append(parameters.MaxStepLength + " ");
            sb.Append(parameters.MinStepLength + " ");
            sb.Append((parameters.UseAllPixels ? 1 : 0) + " ");
            sb.Append(parameters.RelaxationFactor + " ");
            sb.Append((parameters.PointBased ? 1 : 0) + " ");
            sb.Append(parameters.GradientTolerance + " ");
            sb.Append(parameters.ValueTolerance + " ");
            sb.Append(parameters.EpsilonFunction + " ");
            sb.Append(parameters.TranslationX + " ");
            sb.Append(parameters.TranslationY + " ");

            sb.Append(parameters.ROIFixedLeft + " ");
            sb.Append(parameters.ROIFixedTop + " ");
            sb.Append(parameters.ROIFixedRight + " ");
            sb.Append(parameters.ROIFixedBottom + " ");
            sb.Append(parameters.ROIMovingLeft + " ");
            sb.Append(parameters.ROIMovingTop + " ");
            sb.Append(parameters.ROIMovingRight + " ");
            sb.Append(parameters.ROIMovingBottom + " ");

            sb.Append((parameters.DenoiseImages ? 1 : 0) + " ");
            sb.Append((int)parameters.DenoiseMethod + " ");
            sb.Append(parameters.UseCenterForRotation ? 1 : 0);

            return sb.ToString();
        }
    }

    public enum ImageRegistrationTypes
    {
    
        [Description("No Registration")]
        [RegistrationDescription("Doesn't actually do anything.")]
        NoRegistration = 0,

        [RegistrationDescription("Performs a 2D transform involving translation.")]
        Translation = 1,

        [Description("Centered Rigid 2D")]
        [RegistrationDescription("Performs a 2D transform involving translation and rotation.")]
        CenterRigid2D = 2,

        [Description("Center Similarity")]
        [RegistrationDescription("Performs a 2D transform involving translation, rotation, and scaling.")]
        CenterSimilarity = 3,

        [RegistrationDescription("Performs a 2D transform involving translation, rotation, scaling, and skewing.")]
        Affine = 4,
    }

    public enum DenoiseMethodTypes
    {
        [Description("No Denoise")]
        NoDenoise = 0,

        [Description("Curvature Flow")]
        CurvatureFlow = 1,

        [Description("Discrete Gaussian")]
        DiscreteGaussian = 2,

        [Description("Gradient Anisotropic Diffusion")]
        GradientAnisotropicDiffusion = 3,
    }

    public sealed class RegistrationDescriptionAttribute : Attribute
    {
        private readonly string value;
        public RegistrationDescriptionAttribute(string value)
        {
            this.value = value;
        }

        public string Value
        {
            get { return this.value; }
        }
    }
    public sealed class RegistrationDescriptionConverter : IValueConverter
    {
        private string GetEnumDescription(Enum enumObj)
        {
            System.Reflection.FieldInfo fieldInfo = enumObj.GetType().GetField(enumObj.ToString());

            if (fieldInfo == null) return enumObj.ToString();

            object[] attribArray = fieldInfo.GetCustomAttributes(false);

            if (attribArray == null || attribArray.Length == 0)
            {
                return enumObj.ToString();
            }
            else
            {
                foreach (object attrib in attribArray)
                {
                    RegistrationDescriptionAttribute desc = attrib as RegistrationDescriptionAttribute;
                    if (desc == null) continue;

                    return desc.Value;
                }

                return enumObj.ToString();
            }
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Enum myEnum = (Enum)value;
            string description = GetEnumDescription(myEnum);
            return description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public struct RegistrationParameters
    {
        public ImageRegistrationTypes RegType;
        public int MaxIterations;
        public double Angle;
        public double Scale;
        public bool MultiModal;
        public double MaxStepLength;
        public double MinStepLength;
        public int NumberBins;
        public int NumberSamples;
        public bool UseAllPixels;
        public double RelaxationFactor;
        public bool PointBased;
        public double GradientTolerance;
        public double ValueTolerance;
        public double EpsilonFunction;
        public double TranslationX;
        public double TranslationY;
        public double ROIFixedLeft;
        public double ROIFixedTop;
        public double ROIFixedRight;
        public double ROIFixedBottom;
        public double ROIMovingLeft;
        public double ROIMovingTop;
        public double ROIMovingRight;
        public double ROIMovingBottom;
        public bool DenoiseImages;
        public DenoiseMethodTypes DenoiseMethod;
        public bool UseCenterForRotation;

        public void SetROIFixed(double left, double top, double right, double bottom)
        {
            ROIFixedLeft = left;
            ROIFixedTop = top;
            ROIFixedRight = right;
            ROIFixedBottom = bottom;
        }
        public void SetROIMoving(double left, double top, double right, double bottom)
        {
            ROIMovingLeft = left;
            ROIMovingTop = top;
            ROIMovingRight = right;
            ROIMovingBottom = bottom;
        }
    }

    public class RegistrationResult : INotifyPropertyChanged
    {
        double _translationX;
        double _translationY;
        double _scale;
        double _scale2;
        double _rotationCenterX;
        double _rotationCenterY;
        double _angle;
        int _iterations;
        double _metric;

        public double TranslationX
        {
            get { return _translationX; }
            set
            {
                if (_translationX != value)
                {
                    _translationX = value;
                    NotifyPropertyChanged("TranslationX");
                }
            }
        }
        public double TranslationY
        {
            get { return _translationY; }
            set
            {
                if (_translationY != value)
                {
                    _translationY = value;
                    NotifyPropertyChanged("TranslationY");
                }
            }
        }
        public double Scale
        {
            get { return _scale; }
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    NotifyPropertyChanged("Scale");
                }
            }
        }
        public double Scale2
        {
            get { return _scale2; }
            set
            {
                if (_scale2 != value)
                {
                    _scale2 = value;
                    NotifyPropertyChanged("Scale2");
                }
            }
        }
        public double RotationCenterX
        {
            get { return _rotationCenterX; }
            set
            {
                if (_rotationCenterX != value)
                {
                    _rotationCenterX = value;
                    NotifyPropertyChanged("RotationCenterX");
                }
            }
        }
        public double RotationCenterY
        {
            get { return _rotationCenterY; }
            set
            {
                if (_rotationCenterY != value)
                {
                    _rotationCenterY = value;
                    NotifyPropertyChanged("RotationCenterY");
                }
            }
        }
        public double Angle
        {
            get { return _angle; }
            set
            {
                if (_angle != value)
                {
                    _angle = value;
                    NotifyPropertyChanged("Angle");
                }
            }
        }
        public int Iterations
        {
            get { return _iterations; }
            set
            {
                if (_iterations != value)
                {
                    _iterations = value;
                    NotifyPropertyChanged("Iterations");
                }
            }
        }
        public double Metric
        {
            get { return _metric; }
            set
            {
                if (_metric != value)
                {
                    _metric = value;
                    NotifyPropertyChanged("Metric");
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
    }

    //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    //unsafe internal struct UnsafeRegistrationParameters
    //{
    //    ImageRegistrationTypes RegType;
    //    char* FixedImagePath;
    //    char* MovingImagePath;
    //    char* OutputFixedImagePath;
    //    char* OutputMovingImagePath;
    //    char* FixedPointListPath;
    //    char* MovingPointListPath;
    //    char* DenoisedFixedPath;
    //    char* DenoisedMovingPath;
    //    int MaximumIterations;
    //    double Angle;
    //    double Scale;
    //    int NumberBins;
    //    int NumberSamples;
    //    double MaxStepLength;
    //    double MinStepLength;
    //    int MultiModal;
    //    int UseAllPixels;
    //    double RelaxationFactor;
    //    int PointBased;
    //    double GradientTolerance;
    //    double ValueTolerance;
    //    double EpsilonFunction;
    //    double TranslationX;
    //    double TranslationY;
    //    double ROIFixedLeft;
    //    double ROIFixedTop;
    //    double ROIFixedRight;
    //    double ROIFixedBottom;
    //    double ROIMovingLeft;
    //    double ROIMovingTop;
    //    double ROIMovingRight;
    //    double ROIMovingBottom;
    //    int DenoiseImages;
    //    DenoiseMethodTypes DenoiseMethod;

    //    public UnsafeRegistrationParameters(ImageRegistrationTypes regType, string inputFixedImagePath, string inputMovingImagePath, 
    //            string outputFixedImagePath, string outputMovingImagePath, string fixedPointListPath, string movingPointListPath, 
    //            string fixedGrayDenoisedPath, string movingGrayDenoisedPath, RegistrationParameters Parameters)
    //    {
    //        fixed(char* p_fixedImagePath = inputFixedImagePath)
    //        {
    //            FixedImagePath = p_fixedImagePath;
    //        }
    //        fixed (char* p_movingImagePath = inputMovingImagePath)
    //        {
    //            MovingImagePath = p_movingImagePath;
    //        }
    //        fixed (char* p_outputFixedImagePath = outputFixedImagePath)
    //        {
    //            OutputFixedImagePath = p_outputFixedImagePath;
    //        }
    //        fixed (char* p_outputMovingImagePath = outputMovingImagePath)
    //        {
    //            OutputMovingImagePath = p_outputMovingImagePath;
    //        }
    //        fixed (char* p_fixedPointList = fixedPointListPath)
    //        {
    //            FixedPointListPath = p_fixedPointList;
    //        }
    //        fixed (char* p_movingPointList = movingPointListPath)
    //        {
    //            MovingPointListPath = p_movingPointList;
    //        }
    //        fixed (char* p_denoisedFixedPath = fixedGrayDenoisedPath)
    //        {
    //            DenoisedFixedPath = p_denoisedFixedPath;
    //        }
    //        fixed (char* p_denoisedMovingPath = movingGrayDenoisedPath)
    //        {
    //            DenoisedMovingPath = p_denoisedMovingPath;
    //        }    

    //        RegType = regType;

    //        MaximumIterations = Parameters.MaxIterations;

    //        Angle = Parameters.Angle;
    //        Scale = Parameters.Scale;

    //        NumberBins = Parameters.NumberBins;
    //        NumberSamples = Parameters.NumberSamples;
    //        MaxStepLength = Parameters.MaxStepLength;
    //        MinStepLength = Parameters.MinStepLength;

    //        MultiModal = Parameters.MultiModal ? 1 : 0;
    //        UseAllPixels = Parameters.UseAllPixels ? 1 : 0;

    //        RelaxationFactor = Parameters.RelaxationFactor;

    //        PointBased = Parameters.PointBased ? 1 : 0;

    //        GradientTolerance = Parameters.GradientTolerance;
    //        ValueTolerance = Parameters.ValueTolerance;
    //        EpsilonFunction = Parameters.EpsilonFunction;

    //        TranslationX = Parameters.TranslationX;
    //        TranslationY = Parameters.TranslationY;

    //        ROIFixedLeft = Parameters.ROIFixedLeft;
    //        ROIFixedTop = Parameters.ROIFixedTop;
    //        ROIFixedRight = Parameters.ROIFixedRight;
    //        ROIFixedBottom = Parameters.ROIFixedBottom;
    //        ROIMovingLeft = Parameters.ROIMovingLeft;
    //        ROIMovingTop = Parameters.ROIMovingTop;
    //        ROIMovingRight = Parameters.ROIMovingRight;
    //        ROIMovingBottom = Parameters.ROIMovingBottom;

    //        DenoiseImages = Parameters.DenoiseImages ? 1 : 0;
    //        DenoiseMethod = Parameters.DenoiseMethod;
    //    }
    //}
}
