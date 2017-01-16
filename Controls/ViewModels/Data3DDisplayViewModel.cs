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
using ImagingSIMS.Common.Controls;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.Data.Rendering;

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
        NotifiableColor _solidColorScale;
        double _imageTransformedWidth;
        double _imageTransformedHeight;
        int _layerStart;
        int _layerEnd;
        double _threshold;
        double _initalThreshold;

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
                    redraw();
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
                    redraw();
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
                    redraw();
                }
            }
        }
        public NotifiableColor SolidColorScale
        {
            get { return _solidColorScale; }
            set
            {
                if (_solidColorScale != value)
                {
                    if(_solidColorScale != null)
                    {
                        _solidColorScale.ColorChanged -= SolidColorScale_PropertyChanged;
                    }
                    _solidColorScale = value;
                    NotifyPropertyChanged("SolidColorScale");
                    _solidColorScale.ColorChanged += SolidColorScale_PropertyChanged;
                    redraw();
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
                    redraw();
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
                    redraw();
                }
            }
        }
        public double Threshold
        {
            get { return _threshold; }
            set
            {
                if (_threshold != value)
                {
                    _threshold = value;
                    NotifyPropertyChanged("Threshold");
                    redraw();
                }
            }
        }
        public double InitialThreshold
        {
            get { return _initalThreshold; }
            set
            {
                if(_initalThreshold != value)
                {
                    _initalThreshold = value;
                    NotifyPropertyChanged("InitialThreshold");
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
            SolidColorScale = NotifiableColor.White;
        }
        public Data3DDisplayViewModel(Data3D dataSource, ColorScaleTypes colorScale)
        {
            ImageTransformedWidth = 225;
            ImageTransformedHeight = 225;

            SolidColorScale = NotifiableColor.White;

            DataSource = dataSource;
            ColorScale = colorScale;

            LayerStart = 1;
            LayerEnd = dataSource.Depth;

            Scale = 1;
        }
        public Data3DDisplayViewModel(Data3D dataSource, NotifiableColor solidColorScale)
        {
            ImageTransformedWidth = 225;
            ImageTransformedHeight = 225;

            SolidColorScale = solidColorScale;

            DataSource = dataSource;
            ColorScale = ColorScaleTypes.Solid;

            LayerStart = 1;
            LayerEnd = dataSource.Depth;

            Scale = 1;
        }

        private void SolidColorScale_PropertyChanged(object sender, NotifiableColorChangedEventArgs e)
        {
            if(e.OldColor != e.NewColor)
            {
                redraw();
            }            
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

            DataSource = dataSource;
            ColorScale = colorScale;

            LayerStart = 1;
            LayerEnd = dataSource.Depth;

            Scale = 1;
        }
        private void setData3DDisplayItem(Data3D dataSource, NotifiableColor solidColorScale)
        {
            ImageTransformedWidth = 225;
            ImageTransformedHeight = 225;

            SolidColorScale = solidColorScale;

            DataSource = dataSource;
            ColorScale = ColorScaleTypes.Solid;

            LayerStart = 1;
            LayerEnd = dataSource.Depth;

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
            int lowerLayer = lower;
            int upperLayer = upper;

            if(lowerLayer >= 0)
            {
                if (lowerLayer < DataSource.Depth)
                    LayerStart = lowerLayer;
                else LayerStart = DataSource.Depth;
            }

            if(upperLayer >= 0)
            {
                if (upperLayer < DataSource.Depth)
                    LayerEnd = upperLayer;
                else LayerEnd = DataSource.Depth;
            }

            if (lowerLayer > upperLayer)
                LayerEnd = LayerStart;
        }

        int _previousStartLayer = -1;
        int _previousEndLayer = -1;
        private void redraw()
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
                Threshold = ViewableDataSource.Minimum;
                InitialThreshold = Threshold;
            }

            if (ColorScale == ColorScaleTypes.Solid)
            {
                BitmapSource bs = ImageHelper.CreateSolidColorImage(ViewableDataSource, SolidColorScale, (float)Saturation, (float)Threshold);
                bs.Freeze();
                DisplayImageSource = bs;
            }
            else
            {
                BitmapSource bs = ImageHelper.CreateColorScaleImage(ViewableDataSource, ColorScale, (float)Saturation, (float)Threshold);
                bs.Freeze();
                DisplayImageSource = bs;
            }
        }
        public void Redraw()
        {
            if (DataSource == null) return;

            int startLayer = LayerStart - 1;
            int endLayer = LayerEnd - 1;

            if (startLayer < 0) return;
            if (endLayer >= DataSource.Depth) return;

            ViewableDataSource = DataSource.FromLayers(startLayer, endLayer);

            _previousEndLayer = endLayer;
            _previousStartLayer = startLayer;

            Saturation = ViewableDataSource.Maximum;
            InitialSaturation = Saturation;
            Threshold = ViewableDataSource.Minimum;
            InitialThreshold = Threshold;

            if (ColorScale == ColorScaleTypes.Solid)
            {
                BitmapSource bs = ImageHelper.CreateSolidColorImage(ViewableDataSource, SolidColorScale, (float)Saturation, (float)Threshold);
                bs.Freeze();
                DisplayImageSource = bs;
            }
            else
            {
                BitmapSource bs = ImageHelper.CreateColorScaleImage(ViewableDataSource, ColorScale, (float)Saturation, (float)Threshold);
                bs.Freeze();
                DisplayImageSource = bs;
            }
        }
        
        public void ZeroSelectedRows()
        {
            if (DataSource == null) return;

            int startLayer = LayerStart - 1;
            int endLayer = LayerEnd - 1;

            if (startLayer < 0) return;
            if (endLayer >= DataSource.Depth) return;

            for (int z = startLayer; z <= endLayer; z++)
            {
                for (int x = 0; x < DataSource.Width; x++)
                {
                    for (int y = 0; y < DataSource.Height; y++)
                    {
                        DataSource[x, y, z] = 0;
                    }
                }
            }

            ViewableDataSource = DataSource.FromLayers(startLayer, endLayer);
            Saturation = ViewableDataSource.Maximum;
            InitialSaturation = Saturation;
            Threshold = ViewableDataSource.Minimum;
            InitialThreshold = Threshold;

            if (ColorScale == ColorScaleTypes.Solid)
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

        public Volume GetSelectedVolume(int layerBinSize = 1)
        {
            Color c;
            if (ColorScale == ColorScaleTypes.Solid)
                c = SolidColorScale;
            else c = ColorScales.FromScale(1, 1, ColorScale);

            // Convert back to zero-based
            Data3D d = DataSource.Slice(LayerStart - 1, LayerEnd - 1, layerBinSize);

            return new Volume(d, c, d.DataName);
        }
        public async Task<Volume> GetSelectedVolumeAsync(int layerBinSize = 1)
        {
            Color c;
            if (ColorScale == ColorScaleTypes.Solid)
                c = SolidColorScale;
            else c = ColorScales.FromScale(1, 1, ColorScale);

            // Convert back to zero-based
            Data3D d = await DataSource.SliceAsync(LayerStart - 1, LayerEnd - 1, layerBinSize);

            return new Volume(d, c, d.DataName);
        }
        public Data2D SumSelectedLayers()
        {
            // Convert back to zero-based
            return DataSource.FromLayers(LayerStart - 1, LayerEnd - 1);
        }
        public async Task<Data2D> SumSelectedLayersAsync()
        {
            // Convert back to zero-based
            return await DataSource.FromLayersAsync(LayerStart - 1, LayerEnd - 1);
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

        protected static Data2D ThresholdViewData(Data2D data, double threshold)
        {
            var thresholded = new Data2D(data.Width, data.Height);

            for (int x = 0; x < data.Width; x++)
            {
                for (int y = 0; y < data.Height; y++)
                {
                    thresholded[x, y] = data[x, y] >= threshold ? data[x, y] : 0;
                }
            }

            return thresholded;
        }
    }
}
