using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImagingSIMS.Controls.Tabs;

namespace ImagingSIMS.Controls.ViewModels
{
    public class FusionImageViewModel : INotifyPropertyChanged
    {
        BitmapSource _imageSource;
        FusionImageType _imageType;
        bool _isRegistered;

        public BitmapSource ImageSource
        {
            get { return _imageSource; }
            set
            {
                if (_imageSource != value)
                {
                    _imageSource = value;
                    NotifyPropertyChanged("ImageSource");
                    OnImageSourceChanged();
                }
            }
        }
        public FusionImageType ImageType
        {
            get { return _imageType; }
            set
            {
                if (_imageType != value)
                {
                    _imageType = value;
                    NotifyPropertyChanged("ImageType");
                }
            }
        }
        public bool IsRegistered
        {
            get { return _isRegistered; }
            set
            {
                if (_isRegistered != value)
                {
                    _isRegistered = value;
                    NotifyPropertyChanged("IsRegistered");
                }
            }
        }

        public FusionImageViewModel(FusionImageType Type)
        {
            this.ImageSource = null;
            this.ImageType = Type;
            this.IsRegistered = false;
        }
        public FusionImageViewModel(BitmapSource ImageSource, FusionImageType Type)
        {
            this.ImageSource = ImageSource;
            this.ImageType = Type;
            this.IsRegistered = false;
        }
        public FusionImageViewModel(BitmapSource ImageSource, FusionImageType Type, bool IsRegistered)
        {
            this.ImageSource = ImageSource;
            this.ImageType = Type;
            this.IsRegistered = IsRegistered;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public event EventHandler ImageSourceChanged;
        private void OnImageSourceChanged()
        {
            if (ImageSourceChanged != null)
            {
                ImageSourceChanged(this, EventArgs.Empty);
            }
        }

        public FusionImageViewModel Clone()
        {
            return new FusionImageViewModel(this.ImageSource.Clone(), this.ImageType, this.IsRegistered);
        }
    }
}
