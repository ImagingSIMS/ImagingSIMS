using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Fusion;

namespace ImagingSIMS.Controls.ViewModels
{

    public class RatioTabViewModel : INotifyPropertyChanged
    {
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

        public ObservableCollection<Data2D> NumeratorTables
        {
            get { return _numeratorTables; }
            set
            {
                if (_numeratorTables != value)
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
                if (_denominatorTables != value)
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
                if (_outputBaseName != value)
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
                if (_removeOriginalTables != value)
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
                if (_doCrossRatio != value)
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
                if (_multiplyByFactor != value)
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
                if (_multiplyFactor != value)
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
                if (_fuseImagesFirst != value)
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
                if (_fusionType != value)
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
                if (_numeratorHighRes != value)
                {
                    _numeratorHighRes = value;
                    NotifyPropertyChanged("HighRes");
                }
            }
        }

        public RatioTabViewModel()
        {
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
