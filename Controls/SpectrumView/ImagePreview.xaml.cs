using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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

using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.Controls.SpectrumView
{
    /// <summary>
    /// Interaction logic for ImagePreview.xaml
    /// </summary>
    public partial class ImagePreview : UserControl
    {
        Spectrum spectrum;
        int _sizeX;
        int _sizeY;

        public static readonly DependencyProperty LivePreviewProperty = DependencyProperty.Register("LivePreview",
            typeof(bool), typeof(ImagePreview));
        public static readonly DependencyProperty BrushSizeProperty = DependencyProperty.Register("BrushSize",
            typeof(int), typeof(ImagePreview));

        public bool LivePreview
        {
            get { return (bool)GetValue(LivePreviewProperty); }
            set { SetValue(LivePreviewProperty, value); }
        }

        public ImagePreview()
        {
            InitializeComponent();
            SizeChanged += ImagePreview_SizeChanged;
        }

        void ImagePreview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }
        }        

        public void SetData(Spectrum Spectrum)
        {
            spectrum = Spectrum;
            _sizeX = spectrum.SizeX;
            _sizeY = spectrum.SizeY;
        }

        public async Task CreateImageAsync(double StartMass, double EndMass)
        {
            Data2D d = await Task<Data2D>.Run(() =>
                {
                    float max = 0;
                    return spectrum.FromMassRange(new MassRangePair(StartMass, EndMass), out max);
                });

            imageDisplay.Dispatcher.Invoke(() =>
                {
                    imageDisplay.Source = ImageHelper.CreateColorScaleImage(d, ColorScaleTypes.ThermalWarm);
                });
        }

        #region Highlighting
        bool _isHighlighting;
        bool _isErasing;
        private void imageDisplay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isHighlighting = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                _isErasing = true;
            }
        }

        private void imageDisplay_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isHighlighting = false;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                _isErasing = false;
            }
        }

        private void imageDisplay_MouseMove(object sender, MouseEventArgs e)
        {

        }
        #endregion
    }
}
