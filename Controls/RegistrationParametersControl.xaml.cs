using System;
using System.Collections.Generic;
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

using ImagingSIMS.ImageRegistration;

namespace ImagingSIMS.Controls
{
    /// <summary>
    /// Interaction logic for RegistrationParametersControl.xaml
    /// </summary>
    public partial class RegistrationParametersControl : UserControl
    {
        public static readonly DependencyProperty RegistrationParametersProperty = DependencyProperty.Register("RegistrationParameters",
            typeof(RegistrationParametersViewItem), typeof(RegistrationParametersControl));

        public RegistrationParametersViewItem RegistrationParameters
        {
            get { return (RegistrationParametersViewItem)GetValue(RegistrationParametersProperty); }
            set { SetValue(RegistrationParametersProperty, value); }
        }

        public RegistrationParametersControl()
        {
            RegistrationParameters = new RegistrationParametersViewItem();

            InitializeComponent();
        }
    }

    public class RegistrationParametersViewItem : INotifyPropertyChanged
    {
        ImageRegistrationTypes _regType;
        int _maxIterations;
        double _angle;
        double _scale;
        bool _multiModal;
        int _numberBins;
        int _numberSamples;
        double _maxStepLength;
        double _minStepLength;
        bool _useAllPixels;
        double _relaxationFactor;
        bool _pointPased;
        double _gradientTolerance;
        double _valueTolerance;
        double _epsilonFunction;
        double _translationX;
        double _translationY;
        bool _denoiseImages;
        DenoiseMethodTypes _denoiseMethod;
        bool _useCenterForRotation;

        public ImageRegistrationTypes RegType
        {
            get { return _regType; }
            set
            {
                if (_regType != value)
                {
                    _regType = value;
                    this.NotifyPropertyChanged("RegType");
                }
            }
        }
        public int MaxIterations
        {
            get { return _maxIterations; }
            set
            {
                if (_maxIterations != value)
                {
                    _maxIterations = value;
                    NotifyPropertyChanged("MaxIterations");
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
        public bool MultiModal
        {
            get { return _multiModal; }
            set
            {
                if (_multiModal != value)
                {
                    _multiModal = value;
                    NotifyPropertyChanged("MultiModal");
                }
            }
        }
        public double MaxStepLength
        {
            get { return _maxStepLength; }
            set
            {
                if (_maxStepLength != value)
                {
                    _maxStepLength = value;
                    NotifyPropertyChanged("MaxStepLength");
                }
            }
        }
        public double MinStepLength
        {
            get { return _minStepLength; }
            set
            {
                if (_minStepLength != value)
                {
                    _minStepLength = value;
                    NotifyPropertyChanged("MinStepLength");
                }
            }
        }
        public int NumberBins
        {
            get { return _numberBins; }
            set
            {
                if (_numberBins != value)
                {
                    _numberBins = value;
                    NotifyPropertyChanged("NumberBins");
                }
            }
        }
        public int NumberSamples
        {
            get { return _numberSamples; }
            set
            {
                if (_numberSamples != value)
                {
                    _numberSamples = value;
                    NotifyPropertyChanged("NumberSamples");
                }
            }
        }
        public bool UseAllPixels
        {
            get { return _useAllPixels; }
            set
            {
                if (_useAllPixels != value)
                {
                    _useAllPixels = value;
                    NotifyPropertyChanged("UseAllPixels");
                }
            }
        }
        public double RelaxationFactor
        {
            get { return _relaxationFactor; }
            set
            {
                if (_relaxationFactor != value)
                {
                    _relaxationFactor = value;
                    NotifyPropertyChanged("RelaxationFactor");
                }
            }
        }
        public bool PointBased
        {
            get { return _pointPased; }
            set
            {
                if (_pointPased != value)
                {
                    _pointPased = value;
                    this.NotifyPropertyChanged("PointBased");
                }
            }
        }
        public double GradientTolerance
        {
            get { return _gradientTolerance; }
            set
            {
                if (_gradientTolerance != value)
                {
                    _gradientTolerance = value;
                    this.NotifyPropertyChanged("GradientTolerance");
                }
            }
        }
        public double ValueTolerance
        {
            get { return _valueTolerance; }
            set
            {
                if (_valueTolerance != value)
                {
                    _valueTolerance = value;
                    this.NotifyPropertyChanged("ValueTolerance");
                }
            }
        }
        public double EpsilonFunction
        {
            get { return _epsilonFunction; }
            set
            {
                if (_epsilonFunction != value)
                {
                    _epsilonFunction = value;
                    this.NotifyPropertyChanged("EpsilonFunction");
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
                    this.NotifyPropertyChanged("TranslationX");
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
                    this.NotifyPropertyChanged("TranslationY");
                }
            }
        }
        public bool DenoiseImages
        {
            get { return _denoiseImages; }
            set
            {
                if (_denoiseImages != value)
                {
                    _denoiseImages = value;
                    NotifyPropertyChanged("DenoiseImages");
                }
            }
        }
        public DenoiseMethodTypes DenoiseMethod
        {
            get { return _denoiseMethod; }
            set
            {
                if (_denoiseMethod != value)
                {
                    _denoiseMethod = value;
                    NotifyPropertyChanged("DenoiseMethod");
                }
            }
        }
        public bool UseCenterForRotation
        {
            get { return _useCenterForRotation; }
            set
            {
                if (_useCenterForRotation != value)
                {
                    _useCenterForRotation = value;
                    this.NotifyPropertyChanged("UseCenterForRotation");
                }
            }
        }

        public RegistrationParametersViewItem()
        {
            RegType = ImageRegistrationTypes.Translation;
            MaxIterations = 200;
            Angle = 0.0d;
            Scale = 1.0d;
            NumberBins = 24;
            NumberSamples = 10000;
            MaxStepLength = 0.1d;
            MinStepLength = 0.01d;
            RelaxationFactor = 0.5d;
            GradientTolerance = 1e-5;
            ValueTolerance = 1e-5;
            EpsilonFunction = 1e-6;
            TranslationX = 0.0d;
            TranslationY = 0.0d;
            DenoiseImages = true;
            DenoiseMethod = DenoiseMethodTypes.CurvatureFlow;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public RegistrationParameters ToRegistrationParameters()
        {
            RegistrationParameters parameters = new RegistrationParameters()
            {
                RegType = RegType,
                MaxIterations = MaxIterations,
                Angle = (Math.PI / 180d) * Angle,
                Scale = Scale,
                MultiModal = MultiModal,
                MaxStepLength = MaxStepLength,
                MinStepLength = MinStepLength,
                NumberBins = NumberBins,
                NumberSamples = NumberSamples,
                UseAllPixels = UseAllPixels,
                RelaxationFactor = RelaxationFactor,
                PointBased = PointBased,
                GradientTolerance = GradientTolerance,
                ValueTolerance = ValueTolerance,
                EpsilonFunction = EpsilonFunction,
                TranslationX = TranslationX,
                TranslationY = TranslationY,
                DenoiseImages = DenoiseImages,
                DenoiseMethod = DenoiseMethod,
                UseCenterForRotation = UseCenterForRotation
            };

            return parameters;
        }
    }
}
