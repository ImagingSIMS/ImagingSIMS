using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using ImagingSIMS.Data;

namespace ImagingSIMS.ImageRegistration
{
    public class DataTransformWrapper : IDisposable
    {
        const char _delim = ',';

        Process _transformProcess;

        string _dataFilePath;
        Data2D _originalData;

        public bool TransformSucceeded { get; private set; }

        public DataTransformWrapper()
        {

        }

        public void InitializeDataTransform(TransformParameters parameters, Data2D data, bool isMovingImage)
        {
            _originalData = data;
            _dataFilePath = generateTransformDataFile(parameters, data, isMovingImage);
        }

        public async Task TransformAsync()
        {
            await Task.Run(() => transform());
        }
        private void transform()
        {
            _transformProcess = new Process();
            _transformProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = @"Plugins\ITKImageRegistration.exe",
                Arguments = _dataFilePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            Trace.WriteLine("Starting process: " + _transformProcess.StartInfo.FileName);
            _transformProcess.Start();
            Trace.WriteLine("Process started at " + _transformProcess.StartTime);

            while (!_transformProcess.StandardOutput.EndOfStream)
            {
                string line = _transformProcess.StandardOutput.ReadLine();
                if (!String.IsNullOrEmpty(line))
                {
                    Trace.WriteLine(line);
                }
            }
            while(!_transformProcess.StandardError.EndOfStream)
            {
                string error = _transformProcess.StandardError.ReadLine();
                if (!String.IsNullOrEmpty(error))
                {
                    Trace.WriteLine(error);
                }
            }

            int exitCode = _transformProcess.ExitCode;
            if (exitCode == 0)
            {
                TransformSucceeded = true;
            }
            else
            {
                Trace.WriteLine("Process exited with code: " + exitCode);
            }
        }

        public Data2D FinalizeDataTransform()
        {
            if (!File.Exists(_dataFilePath))
            {
                throw new FileNotFoundException("Cannot find the data file.");
            }

            Data2D data = new Data2D(_originalData.Width, _originalData.Height);
            data.DataName = _originalData.DataName + " - Transformed";

            using (var fileStream = new FileStream(_dataFilePath, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fileStream))
                {
                    int transformType = br.ReadInt32();
                    int isMovingImage = br.ReadInt32();
                    float translationX = br.ReadSingle();
                    float translationY = br.ReadSingle();
                    float angle = br.ReadSingle();
                    float rotationCenterX = br.ReadSingle();
                    float rotationCenterY = br.ReadSingle();
                    float scale = br.ReadSingle();
                    float dataWidth = br.ReadInt32();
                    float dataHeight = br.ReadInt32();

                    for (int x = 0; x < data.Width; x++)
                    {
                        for (int y = 0; y < data.Height; y++)
                        {
                            data[x, y] = br.ReadSingle();
                        }
                    }
                }
            }

            File.Delete(_dataFilePath);

            return data;
        }

        public static Data2D LoadFinalDataFile(string path)
        {
            Data2D data;

            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                using (BinaryReader br = new BinaryReader(fileStream))
                {
                    int transformType = br.ReadInt32();
                    int isMovingImage = br.ReadInt32();
                    float translationX = br.ReadSingle();
                    float translationY = br.ReadSingle();
                    float angle = br.ReadSingle();
                    float rotationCenterX = br.ReadSingle();
                    float rotationCenterY = br.ReadSingle();
                    float scale = br.ReadSingle();
                    int dataWidth = br.ReadInt32();
                    int dataHeight = br.ReadInt32();

                    data = new Data2D(dataWidth, dataHeight);

                    for (int x = 0; x < data.Width; x++)
                    {
                        for (int y = 0; y < data.Height; y++)
                        {
                            data[x, y] = br.ReadSingle();
                        }
                    }
                }
            }

            return data;
        }
        
        private string generateTransformDataFile(TransformParameters parameters, Data2D data, bool isMovingImage)
        {
            // Array is to be array with the following layout:
            // [0]:     (int)TransformType
            // [1]:     (int)Moving (1) or fixed (0) image
            // [2]:     (float)Translation X
            // [3]:     (float)Translation Y
            // [4]:     (float)Angle
            // [5]:     (float)Rotation center X
            // [6]:     (float)Rotation center Y
            // [7]:     (float)Scale
            // [8]:     (int)Data width
            // [9]:     (int)Data height
            // [10-n]:  (float)Data

            string currentTime = DateTime.Now.ToFileTimeUtc().ToString();
            string filePath = AppDataFileHelper.GetFilePath(currentTime, ".tfm");

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                using (BinaryWriter bw = new BinaryWriter(fileStream))
                {
                    // Write parameters
                    bw.Write((int)parameters.TransformType);
                    bw.Write(isMovingImage ? 1 : 0);
                    bw.Write((float)parameters.TranslationX);
                    bw.Write((float)parameters.TranslationY);
                    bw.Write((float)parameters.Angle);
                    bw.Write((float)parameters.RotationCenterX);
                    bw.Write((float)parameters.RotationCenterY);
                    bw.Write((float)parameters.Scale);
                    bw.Write(data.Width);
                    bw.Write(data.Height);

                    for (int x = 0; x < data.Width; x++)
                    {
                        for (int y = 0; y < data.Height; y++)
                        {
                            bw.Write(data[x, y]);
                        }
                    }
                }
            }

            return filePath;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~DataTransformWrapper()
        {
            Dispose(false);
        }
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (File.Exists(_dataFilePath))
                {
                    File.Delete(_dataFilePath);
                }
            }
        }
    }

    public class TransformParameters : INotifyPropertyChanged
    {
        ImageRegistrationTypes _transformType;
        double _translationX;
        double _translationY;
        double _scale;
        double _angle;
        double _rotationCenterX;
        double _rotationCenterY;

        public ImageRegistrationTypes TransformType
        {
            get { return _transformType; }
            set
            {
                if (_transformType != value)
                {
                    _transformType = value;
                    NotifyPropertyChanged("TransformType");
                }
            }
        }
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public TransformParameters GetInverted
        {
            get
            {
                TransformParameters t = new TransformParameters()
                {
                    TranslationX = this.TranslationX,
                    TranslationY = this.TranslationY,
                    Angle = this.Angle,
                    Scale = this.Scale,
                    TransformType = this.TransformType
                };

                t.Invert();

                return t;
            }
        }
        public void Invert()
        {
            TranslationX *= -1;
            TranslationY *= -1;
            Angle *= -1;

            if (Scale != 0)
            {
                Scale = 1 / Scale;
            }
        }
    }

    public enum TransformTypes
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
}
