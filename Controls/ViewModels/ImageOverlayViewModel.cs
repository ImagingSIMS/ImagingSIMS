using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;

namespace ImagingSIMS.Controls.ViewModels
{
    public class OverlaySettingsViewModel : INotifyPropertyChanged
    {
        Data2D _image;
        Color _color;
        double _saturation;
        double _threshold;

        public Data2D Image
        {
            get { return _image; }
            set
            {
                if (_image != value)
                {
                    _image = value;
                    NotifyPropertyChanged();
                    Saturation = Image.Maximum;
                    Threshold = 0;
                }
            }
        }
        public Color Color
        {
            get { return _color; }
            set
            {
                if ( _color != value)
                {
                    _color = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Saturation
        {
            get { return _saturation; }
            set
            {
                if (_saturation != value)
                {
                    _saturation = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Threshold
        {
            get { return _threshold; }
            set
            {
                if ( _threshold != value)
                {
                    _threshold = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand RemoveCommand { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler SettingsChanged;

        public OverlaySettingsViewModel()
        {
            Color = Color.FromRgb(255, 255, 255);
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            SettingsChanged?.Invoke(this, EventArgs.Empty);            
        }
    }
    public class ImageOverlayViewModel : INotifyPropertyChanged
    {
        Workspace _workspace;
        ObservableCollection<OverlaySettingsViewModel> _images;
        ImageSource _overlayImage;

        public Workspace Workspace
        {
            get { return _workspace; }
            set
            {
                if ( _workspace != value)
                {
                    _workspace = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public ObservableCollection<OverlaySettingsViewModel> Images
        {
            get { return _images; }
            set
            {
                if (_images != value)
                {
                    _images = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public ImageSource OverlayImage
        {
            get { return _overlayImage; }
            set
            {
                if (_overlayImage != value)
                {
                    _overlayImage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageOverlayViewModel()
        {
            Images = new ObservableCollection<OverlaySettingsViewModel>();
        }

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void AddImage()
        {
            var setting = new OverlaySettingsViewModel() { Color = Color.FromRgb(255, 255, 255) };
            setting.SettingsChanged += Setting_SettingsChanged;
            Images.Add(setting);
        }

        private void Setting_SettingsChanged(object sender, EventArgs e)
        {
            UpdateOverlay();
        }

        public void RemoveImage(OverlaySettingsViewModel setting)
        {
            setting.SettingsChanged -= Setting_SettingsChanged;
            Images.Remove(setting);

            UpdateOverlay();
        }

        private void UpdateOverlay()
        {
            var toOverlay = Images.Where(s => s.Image != null).ToList();

            var widths = toOverlay.Select(i => i.Image?.Width)
                .Where(w => w.HasValue).Distinct();
            var heights = toOverlay.Select(i => i.Image?.Height)
                .Where(h => h.HasValue).Distinct();

            if (widths.Count() > 1 || heights.Count() > 1) return;

            var images = Images.Where(i => i.Image != null).Select(i => i.Image).ToArray();
            var colors = Images.Where(i => i.Image != null).Select(i => i.Color).ToArray();
            var saturations = Images.Where(i => i.Image != null).Select(i => (float)(i.Saturation)).ToArray();
            var thresholds = Images.Where(i => i.Image != null).Select(i => (float)(i.Threshold)).ToArray();

            if (images.Count() == 0)
            {
                OverlayImage = null;
                return;
            }

            var overlay = Overlay.CreateOverlay(images, colors, saturations, thresholds);
            overlay.Freeze();
            OverlayImage = overlay;
        }
    }
}
