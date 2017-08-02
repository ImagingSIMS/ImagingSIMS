using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImagingSIMS.Controls.BaseControls;
using ImagingSIMS.Data.Fusion;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.ImageRegistration;

namespace ImagingSIMS.Controls.ViewModels
{
    public class FusionPointTabViewModel : INotifyPropertyChanged
    {
        ControlPointImageViewModel _movingImageViewModel;
        ControlPointImageViewModel _fixedImageViewModel;
        RegistrationParametersViewModel _itkRegistrationViewModel;

        PointRegistrationType _pointRegistrationType;
        BitmapSource _fusedImage;
        BitmapSource _registeredOverlay;
        bool _isRegistering;
        int _registrationProgress;
        RegistrationResult _registrationResults;
        Task _registrationTask;
        Task _fusionTask;
        int _shiftWindowSize;
        Data2DConverionType _panchromaticConversion;
        Color _panchrolmaticConversionSolidColor;
        FusionType _fusionType;
        string _analysisResults;

        public ControlPointImageViewModel MovingImageViewModel
        {
            get { return _movingImageViewModel; }
            set
            {
                if (_movingImageViewModel != value)
                {
                    _movingImageViewModel = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public ControlPointImageViewModel FixedImageViewModel
        {
            get { return _fixedImageViewModel; }
            set
            {
                if (_fixedImageViewModel != value)
                {
                    _fixedImageViewModel = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public RegistrationParametersViewModel ItkRegistrationViewModel
        {
            get { return _itkRegistrationViewModel; }
            set
            {
                if (_itkRegistrationViewModel != value)
                {
                    _itkRegistrationViewModel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public PointRegistrationType PointRegistrationType
        {
            get { return _pointRegistrationType; }
            set
            {
                if(_pointRegistrationType != value)
                {
                    _pointRegistrationType = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public BitmapSource FusedImage
        {
            get { return _fusedImage; }
            set
            {
                if (_fusedImage != value)
                {
                    _fusedImage = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public BitmapSource RegisteredOverlay
        {
            get { return _registeredOverlay; }
            set
            {
                if (_registeredOverlay != value)
                {
                    _registeredOverlay = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public bool IsRegistering
        {
            get { return _isRegistering; }
            set
            {
                if (_isRegistering != value)
                {
                    _isRegistering = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int RegistrationProgress
        {
            get { return _registrationProgress; }
            set
            {
                if (_registrationProgress != value)
                {
                    _registrationProgress = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public RegistrationResult RegistrationResults
        {
            get { return _registrationResults; }
            set
            {
                if (_registrationResults != value)
                {
                    _registrationResults = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Task RegistrationTask
        {
            get { return _registrationTask; }
            set
            {
                if (_registrationTask != value)
                {
                    _registrationTask = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Task FusionTask
        {
            get { return _fusionTask; }
            set
            {
                if (_fusionTask != value)
                {
                    _fusionTask = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int ShiftWindowSize
        {
            get { return _shiftWindowSize; }
            set
            {
                if (_shiftWindowSize != value)
                {
                    _shiftWindowSize = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Data2DConverionType PanchromaticConversion
        {
            get { return _panchromaticConversion; }
            set
            {
                if (_panchromaticConversion != value)
                {
                    _panchromaticConversion = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Color PanchromaticConversionSolidColor
        {
            get { return _panchrolmaticConversionSolidColor; }
            set
            {
                if (_panchrolmaticConversionSolidColor != value)
                {
                    _panchrolmaticConversionSolidColor = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public FusionType FusionType
        {
            get { return _fusionType; }
            set
            {
                if (_fusionType != value)
                {
                    _fusionType = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string AnalysisResults
        {
            get { return _analysisResults; }
            set
            {
                if (_analysisResults != value)
                {
                    _analysisResults = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public FusionPointTabViewModel()
        {
            MovingImageViewModel = new ControlPointImageViewModel() { ColorScale = ColorScaleTypes.ThermalCold };
            FixedImageViewModel = new ControlPointImageViewModel() { ColorScale = ColorScaleTypes.Gray };
            ItkRegistrationViewModel = new RegistrationParametersViewModel();

            PanchromaticConversion = Data2DConverionType.Grayscale;
            ShiftWindowSize = 11;
            FusionType = FusionType.HSL;
            PanchromaticConversionSolidColor = Color.FromArgb(255, 255, 255, 255);

            AnalysisResults = string.Empty;

            PointRegistrationType = PointRegistrationType.Projective;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
