using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls.ViewModels
{
    //public class Data2DDisplayViewModel : INotifyPropertyChanged
    //{
    //    Data2D _dataSource;
    //    ImageSource _displayImageSource;
    //    double _saturation;
    //    double _initialSaturation;
    //    ColorScaleTypes _colorScale;
    //    Color _solidColorScale;
    //    double _imageTransformedWidth;
    //    double _imageTransformedHeight;

    //    Point? _lastCenterPositionOnTarget;
    //    double _scale;

    //    public Data2D DataSource
    //    {
    //        get { return _dataSource; }
    //        set
    //        {
    //            if (_dataSource != value)
    //            {
    //                _dataSource = value;
    //                NotifyPropertyChanged("DataSource");
    //                Redraw();
    //            }
    //        }
    //    }
    //    public ImageSource DisplayImageSource
    //    {
    //        get { return _displayImageSource; }
    //        set
    //        {
    //            if (_displayImageSource != value)
    //            {
    //                _displayImageSource = value;
    //                NotifyPropertyChanged("DisplayImageSource");
    //            }
    //        }
    //    }
    //    public double Saturation
    //    {
    //        get { return _saturation; }
    //        set
    //        {
    //            if (_saturation != value)
    //            {
    //                _saturation = value;
    //                NotifyPropertyChanged("Saturation");
    //                Redraw();
    //            }
    //        }
    //    }
    //    public double InitialSaturation
    //    {
    //        get { return _initialSaturation; }
    //        set
    //        {
    //            if (_initialSaturation != value)
    //            {
    //                _initialSaturation = value;
    //                NotifyPropertyChanged("InitialSaturation");
    //            }
    //        }
    //    }
    //    public ColorScaleTypes ColorScale
    //    {
    //        get { return _colorScale; }
    //        set
    //        {
    //            if (_colorScale != value)
    //            {
    //                _colorScale = value;
    //                NotifyPropertyChanged("ColorScale");
    //                Redraw();
    //            }
    //        }
    //    }
    //    public Color SolidColorScale
    //    {
    //        get { return _solidColorScale; }
    //        set
    //        {
    //            if (_solidColorScale != value)
    //            {
    //                _solidColorScale = value;
    //                NotifyPropertyChanged("SolidColorScale");
    //                Redraw();
    //            }
    //        }
    //    }
    //    public double ImageTransformedWidth
    //    {
    //        get { return _imageTransformedWidth; }
    //        set
    //        {
    //            if (_imageTransformedWidth != value)
    //            {
    //                _imageTransformedWidth = value;
    //                NotifyPropertyChanged("ImageTransformedWidth");
    //            }
    //        }
    //    }
    //    public double ImageTransformedHeight
    //    {
    //        get { return _imageTransformedHeight; }
    //        set
    //        {
    //            if (_imageTransformedHeight != value)
    //            {
    //                _imageTransformedHeight = value;
    //                NotifyPropertyChanged("ImageTransformedHeight");
    //            }
    //        }
    //    }

    //    public Point? LastCenterPositionOnTarget
    //    {
    //        get { return _lastCenterPositionOnTarget; }
    //        set
    //        {
    //            if(_lastCenterPositionOnTarget != value)
    //            {
    //                _lastCenterPositionOnTarget = value;
    //                NotifyPropertyChanged("LastCenterPositionOnTarget");
    //            }
    //        }
    //    }
    //    public double Scale
    //    {
    //        get { return _scale; }
    //        set
    //        {
    //            if(_scale != value)
    //            {
    //                _scale = value;
    //                NotifyPropertyChanged("Scale");
    //            }
    //        }
    //    }

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    private void NotifyPropertyChanged(string propertyName)
    //    {
    //        if (PropertyChanged != null)
    //        {
    //            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    //        }
    //    }

    //    private Data2DDisplayViewModel()
    //    {

    //    }
    //    public Data2DDisplayViewModel(Data2D DataSource, ColorScaleTypes ColorScale)
    //    {
    //        this.ImageTransformedHeight = 225d;
    //        this.ImageTransformedWidth = 225d;

    //        this.SolidColorScale = Color.FromArgb(255, 255, 255, 255);

    //        this.DataSource = DataSource;
    //        this.ColorScale = ColorScale;
    //        this.Saturation = (int)DataSource.Maximum;
    //        this.InitialSaturation = this.Saturation;

    //        Scale = 1;
    //    }
    //    public Data2DDisplayViewModel(Data2D DataSource, Color SolidColorScale)
    //    {
    //        this.ImageTransformedHeight = 225d;
    //        this.ImageTransformedWidth = 225d;

    //        this.ColorScale = ColorScaleTypes.Solid;
    //        this.SolidColorScale = SolidColorScale;

    //        this.DataSource = DataSource;
    //        this.Saturation = (int)DataSource.Maximum;
    //        this.InitialSaturation = this.Saturation;

    //        Scale = 1;
    //    }

    //    public async Task SetData2DDisplayItemAsync(Data2D dataSource, ColorScaleTypes colorScale)
    //    {
    //        await Task.Run(() => setData2DDisplayItem(dataSource, colorScale));
    //    }
    //    public async Task SetData2DDisplayItemAsync(Data2D dataSource, Color solidColor)
    //    {
    //        await Task.Run(() => setData2DDisplayItem(dataSource, solidColor));
    //    }

    //    private void setData2DDisplayItem(Data2D dataSource, ColorScaleTypes colorScale)
    //    {
    //        this.ImageTransformedHeight = 225d;
    //        this.ImageTransformedWidth = 225d;

    //        this.SolidColorScale = Color.FromArgb(255, 255, 255, 255);

    //        this.DataSource = dataSource;
    //        this.ColorScale = colorScale;
    //        this.Saturation = (int)DataSource.Maximum;
    //        this.InitialSaturation = this.Saturation;

    //        Scale = 1;
    //    }
    //    private void setData2DDisplayItem(Data2D dataSource, Color solidColorScale)
    //    {
    //        this.ImageTransformedHeight = 225d;
    //        this.ImageTransformedWidth = 225d;

    //        this.ColorScale = ColorScaleTypes.Solid;
    //        this.SolidColorScale = solidColorScale;

    //        this.DataSource = dataSource;
    //        this.Saturation = (int)dataSource.Maximum;
    //        this.InitialSaturation = this.Saturation;

    //        Scale = 1;
    //    }

    //    private void Redraw()
    //    {
    //        if (DataSource == null) return;
    //        if (ColorScale == ColorScaleTypes.Solid)
    //        {
    //            BitmapSource bs = ImageHelper.CreateSolidColorImage(DataSource, SolidColorScale, (float)Saturation);
    //            bs.Freeze();
    //            DisplayImageSource = bs;
    //        }
    //        else
    //        {
    //            BitmapSource bs = ImageHelper.CreateColorScaleImage(DataSource, ColorScale, (float)Saturation);
    //            bs.Freeze();
    //            DisplayImageSource = bs;
    //        }
    //    }

    //    public static Data2DDisplayViewModel Empty
    //    {
    //        get
    //        {
    //            return new Data2DDisplayViewModel()
    //            {
    //                ImageTransformedWidth = 255,
    //                ImageTransformedHeight = 255,
    //                SolidColorScale = Color.FromArgb(255, 255, 255, 255)
    //            };
    //        }
    //    }
    //}
}
