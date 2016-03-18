using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls.ViewModels
{
    public class Data3DDisplayViewModel : INotifyPropertyChanged
    {
        Data3D _dataSource;
        Data2D _viewableDataSource;
        ImageSource _displayImageSource;
        double _saturation;
        double _initialSaturation;
        ColorScaleTypes _colorScale;
        Color _solidColorScale;
        double _imageTransformedWidth;
        double _imageTransformedHeight;
        int _layerStart;
        int _layerEnd;

        Point? _lastCenterPositionOnTarget;
        double _scale;

        public Data3D DataSource
        {
            get { return _dataSource; }
            set
            {
                if(_dataSource != value)
                {
                    _dataSource = value;
                    NotifyPropertyChanged("DataSource");
                    Redraw();
                }
            }
        }
        public Data2D ViewableDataSource
        {
            get { return _viewableDataSource; }
            set
            {
                if(_viewableDataSource != value)
                {
                    _viewableDataSource = value;
                    NotifyPropertyChanged("ViewableDataSource");
                }
            }
        }
        public ImageSource DisplayImageSource
        {
            get { return _displayImageSource; }
            set
            {
                if (_displayImageSource != value)
                {
                    _displayImageSource = value;
                    NotifyPropertyChanged("DisplayImageSource");
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
                    NotifyPropertyChanged("Saturation");
                    Redraw();
                }
            }
        }
        public double InitialSaturation
        {
            get { return _initialSaturation; }
            set
            {
                if (_initialSaturation != value)
                {
                    _initialSaturation = value;
                    NotifyPropertyChanged("InitialSaturation");
                }
            }
        }
        public ColorScaleTypes ColorScale
        {
            get { return _colorScale; }
            set
            {
                if (_colorScale != value)
                {
                    _colorScale = value;
                    NotifyPropertyChanged("ColorScale");
                    Redraw();
                }
            }
        }
        public Color SolidColorScale
        {
            get { return _solidColorScale; }
            set
            {
                if (_solidColorScale != value)
                {
                    _solidColorScale = value;
                    NotifyPropertyChanged("SolidColorScale");
                    Redraw();
                }
            }
        }
        public double ImageTransformedWidth
        {
            get { return _imageTransformedWidth; }
            set
            {
                if (_imageTransformedWidth != value)
                {
                    _imageTransformedWidth = value;
                    NotifyPropertyChanged("ImageTransformedWidth");
                }
            }
        }
        public double ImageTransformedHeight
        {
            get { return _imageTransformedHeight; }
            set
            {
                if (_imageTransformedHeight != value)
                {
                    _imageTransformedHeight = value;
                    NotifyPropertyChanged("ImageTransformedHeight");
                }
            }
        }
        public int LayerStart
        {
            get { return _layerStart; }
            set
            {
                if (_layerStart != value)
                {
                    _layerStart = value;
                    if(_layerStart > _layerEnd)
                    {
                        LayerEnd = LayerStart;
                    }
                    NotifyPropertyChanged("LayerStart");
                    Redraw();
                }
            }
        }
        public int LayerEnd
        {
            get { return _layerEnd; }
            set
            {
                if (_layerEnd != value)
                {
                    _layerEnd = value;
                    if(_layerEnd < _layerStart)
                    {
                        LayerStart = _layerEnd;
                    }
                    NotifyPropertyChanged("LayerEnd");
                    Redraw();
                }
            }
        }

        public Point? LastCenterPositionOnTarget
        {
            get { return _lastCenterPositionOnTarget; }
            set
            {
                if (_lastCenterPositionOnTarget != value)
                {
                    _lastCenterPositionOnTarget = value;
                    NotifyPropertyChanged("LastCenterPositionOnTarget");
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

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private Data3DDisplayViewModel()
        {

        }
        public Data3DDisplayViewModel(Data3D dataSource, ColorScaleTypes colorScale)
        {
            ImageTransformedWidth = 225;
            ImageTransformedHeight = 225;

            SolidColorScale = Color.FromArgb(255, 255, 255, 255);

            DataSource = dataSource;
            ColorScale = colorScale;
            Saturation = (int)DataSource.SingluarMaximum;
            InitialSaturation = Saturation;

            Scale = 1;
        }
        public Data3DDisplayViewModel(Data3D dataSource, Color solidColorScale)
        {
            ImageTransformedWidth = 225;
            ImageTransformedHeight = 225;

            SolidColorScale = solidColorScale;

            DataSource = dataSource;
            ColorScale = ColorScaleTypes.Solid;
            Saturation = (int)DataSource.SingluarMaximum;
            InitialSaturation = Saturation;

            Scale = 1;
        }

        public async Task SetData3DDisplayItemAsync(Data3D dataSource, ColorScaleTypes colorScale)
        {
            await Task.Run(() => setData3DDisplayItem(dataSource, colorScale));
        }
        public async Task SetData3DDisplayItemAsync(Data3D dataSource, Color solidColorScale)
        {
            await Task.Run(() => setData3DDisplayItem(dataSource, solidColorScale));
        }

        private void setData3DDisplayItem(Data3D dataSource, ColorScaleTypes colorScale)
        {
            ImageTransformedWidth = 225;
            ImageTransformedHeight = 225;

            SolidColorScale = Color.FromArgb(255, 255, 255, 255);

            DataSource = dataSource;
            ColorScale = colorScale;
            Saturation = (int)DataSource.SingluarMaximum;
            InitialSaturation = Saturation;

            LayerStart = 1;
            LayerEnd = 1;

            Scale = 1;
        }
        private void setData3DDisplayItem(Data3D dataSource, Color solidColorScale)
        {
            ImageTransformedWidth = 225;
            ImageTransformedHeight = 225;

            SolidColorScale = solidColorScale;

            DataSource = dataSource;
            ColorScale = ColorScaleTypes.Solid;
            Saturation = (int)DataSource.SingluarMaximum;
            InitialSaturation = Saturation;

            LayerStart = 1;
            LayerEnd = 1;

            Scale = 1;
        }

        public void IncrementLayers()
        {
            int newLowerLayer = LayerStart + 1;
            int newUpperLayer = LayerEnd + 1;

            if(newUpperLayer <= DataSource.Depth)
            {
                LayerStart = newLowerLayer;
                LayerEnd = newUpperLayer;
            }
        }
        public void DecrementLayers()
        {
            int newLowerLayer = LayerStart - 1;
            int newUpperLayer = LayerEnd - 1;

            if(newLowerLayer > 0)
            {
                LayerStart = newLowerLayer;
                LayerEnd = newUpperLayer;
            }
        }
        public void SetLayers(int lower, int upper)
        {
            int lowerLayer = lower - 1;
            int upperLayer = upper - 1;

            if (lowerLayer >= 0 && lowerLayer < DataSource.Depth)
                LayerStart = lowerLayer;

            if (upperLayer >= 0 && upperLayer < DataSource.Depth)
                LayerEnd = upperLayer;

            if (lowerLayer > upperLayer)
                upperLayer = lowerLayer;
        }

        int _previousStartLayer = -1;
        int _previousEndLayer = -1;
        private void Redraw()
        {
            if (DataSource == null) return;

            int startLayer = LayerStart - 1;
            int endLayer = LayerEnd - 1;

            if (startLayer < 0) return;
            if (endLayer >= DataSource.Depth) return;

            // Only resample the data source if the range changes
            if(startLayer != _previousStartLayer || endLayer != _previousEndLayer)
            {
                ViewableDataSource = DataSource.FromLayers(startLayer, endLayer);

                _previousEndLayer = endLayer;
                _previousStartLayer = startLayer;

                Saturation = ViewableDataSource.Maximum;
                InitialSaturation = Saturation;
            }
            

            if(ColorScale == ColorScaleTypes.Solid)
            {
                BitmapSource bs = ImageHelper.CreateSolidColorImage(ViewableDataSource, SolidColorScale, (float)Saturation);
                bs.Freeze();
                DisplayImageSource = bs;
            }
            else
            {
                BitmapSource bs = ImageHelper.CreateColorScaleImage(ViewableDataSource, ColorScale, (float)Saturation);
                bs.Freeze();
                DisplayImageSource = bs;
            }
        }

        public static Data3DDisplayViewModel Empty
        {
            get
            {
                return new Data3DDisplayViewModel()
                {
                    ImageTransformedWidth = 255,
                    ImageTransformedHeight = 255,
                    SolidColorScale = Color.FromArgb(255, 255, 255, 255),
                    LayerStart = 1,
                    LayerEnd = 1,
                };
            }
        }
    }
}
