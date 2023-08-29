using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using ImagingSIMS.Common.Math;
using ImagingSIMS.Common.Registry;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.Data.Rendering;
using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.Data
{
    public interface IWorkspaceData
    {

    }
    public class Workspace : ISObject
    {
        //Registry
        RegSettings _registry;

        //Workspace title
        string _workspaceName;

        //IsDirty
        bool _isDirty;
        bool _hasContents;

        //Collections of data
        ObservableCollection<Data2D> _data;
        ObservableCollection<Spectrum> _spectra;
        ObservableCollection<ImageComponent> _components;
        ObservableCollection<Volume> _volumes;
        ObservableCollection<DisplaySeries> _imageSeries;
        ObservableCollection<SEM> _sems;

        //Misc parameters
        double _spectraMassStart;
        double _spectraMassEnd;
        string _spectraCustomRange;
        double _spectraBinWidth;
        double _volumePixelSize;
        double _volumePixelDepth;
        double _volumeZSpacing;

        int _sampleXDimension;
        int _sampleYDimension;
        int _sampleZDimension;
        int _sampleCenterX1;
        int _sampleCenterY1;
        int _sampleCenterZ1;
        int _sampleCenterX2;
        int _sampleCenterY2;
        int _sampleCenterZ2;
        int _sampleRadius1;
        int _sampleRadius2;

        int _imagingZCorrectThresh;
        int _imagingSlicePixel;
        int _volumePixelThreshold;
        int _isosurfaceThreshold;
        int _isosurfaceIsoValue;
        int _isosurfaceSmoothWindowSize;

        bool _sampleDoMultiple;
        bool _imagingTotalIon;
        bool _imagingSqrtEnhance;
        bool _renderingShowAxes;
        bool _omitDataNumbering;
        bool _isosurfaceDoSmooth;

        Color _renderingBackColor;

        public RegSettings Registry
        {
            get { return _registry; }
        }
        public string WorkspaceName
        {
            get { return _workspaceName; }
            set
            {
                if (_workspaceName != value)
                {
                    _workspaceName = value;
                    NotifyPropertyChanged("WorkspaceName");
                }
            }
        }
        public bool IsDirty
        {
            get 
            {
                if (_isNew || _isSaved || _isLoaded) return false;

                return _isDirty; 
            }
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    NotifyPropertyChanged("IsDirty");
                }
            }
        }
        public bool HasContents
        {
            get { return _hasContents; }
            set
            {
                if (_hasContents != value)
                {
                    _hasContents = value;
                    NotifyPropertyChanged("HasContents");
                }
            }
        }

        public ObservableCollection<Data2D> Data
        {
            get { return _data; }
            set
            {
                if (_data != value)
                {
                    _data = value;
                    NotifyPropertyChanged("Data");
                }
            }
        }
        public ObservableCollection<Spectrum> Spectra
        {
            get { return _spectra; }
            set
            {
                if (_spectra != value)
                {
                    _spectra = value;
                    NotifyPropertyChanged("Spectra");
                }
            }
        }
        public ObservableCollection<ImageComponent> Components
        {
            get { return _components; }
            set
            {
                if (_components != value)
                {
                    _components = value;
                    NotifyPropertyChanged("Components");
                }
            }
        }
        public ObservableCollection<Volume> Volumes
        {
            get { return _volumes; }
            set
            {
                if (_volumes != value)
                {
                    _volumes = value;
                    NotifyPropertyChanged("Volumes");
                }
            }
        }
        public ObservableCollection<DisplaySeries> ImageSeries
        {
            get { return _imageSeries; }
            set
            {
                if (_imageSeries != value)
                {
                    _imageSeries = value;
                    NotifyPropertyChanged("ImageSeries");
                }
            }
        }
        public ObservableCollection<SEM> SEMs
        {
            get { return _sems; }
            set
            {
                if (_sems != value)
                {
                    _sems = value;
                    NotifyPropertyChanged("SEMs");
                }
            }
        }
        
        public double SpectraMassStart
        {
            get { return _spectraMassStart; }
            set
            {
                if (_spectraMassStart != value)
                {
                    _spectraMassStart = value;
                    NotifyPropertyChanged("SpectraMassStart");
                }
            }
        }
        public double SpectraMassEnd
        {
            get { return _spectraMassEnd; }
            set
            {
                if (_spectraMassEnd != value)
                {
                    _spectraMassEnd = value;
                    NotifyPropertyChanged("SpectraMassEnd");
                }
            }
        }
        public string SpectraCustomRange
        {
            get { return _spectraCustomRange; }
            set
            {
                if(_spectraCustomRange!= value)
                {
                    _spectraCustomRange = value;
                    NotifyPropertyChanged("SpectraCustomRange");
                }
            }
        }
        public double SpectraBinWidth
        {
            get { return _spectraBinWidth; }
            set
            {
                if (_spectraBinWidth != value)
                {
                    _spectraBinWidth = value;
                    NotifyPropertyChanged("SpectraBinWidth");
                }
            }
        }
        public double VolumePixelSize
        {
            get { return _volumePixelSize; }
            set
            {
                if (_volumePixelSize != value)
                {
                    _volumePixelSize = value;
                    NotifyPropertyChanged("VolumePixelSize");
                }
            }
        }
        public double VolumePixelDepth
        {
            get { return _volumePixelDepth; }
            set
            {
                if (_volumePixelDepth != value)
                {
                    _volumePixelDepth = value;
                    NotifyPropertyChanged("VolumePixelDepth");
                }
            }
        }
        public double VolumeZSpacing
        {
            get { return _volumeZSpacing; }
            set
            {
                if (_volumeZSpacing != value)
                {
                    _volumeZSpacing = value;
                    NotifyPropertyChanged("VolumeZSpacing");
                }
            }
        }

        public int SampleXDimension
        {
            get { return _sampleXDimension; }
            set
            {
                if (_sampleXDimension != value)
                {
                    _sampleXDimension = value;
                    NotifyPropertyChanged("SampleXDimension");
                }
            }
        }
        public int SampleYDimension
        {
            get { return _sampleYDimension; }
            set
            {
                if (_sampleYDimension != value)
                {
                    _sampleYDimension = value;
                    NotifyPropertyChanged("SampleYDimension");
                }
            }
        }
        public int SampleZDimension
        {
            get { return _sampleZDimension; }
            set
            {
                if (_sampleZDimension != value)
                {
                    _sampleZDimension = value;
                    NotifyPropertyChanged("SampleZDimension");
                }
            }
        }
        public int SampleCenterX1
        {
            get { return _sampleCenterX1; }
            set
            {
                if (_sampleCenterX1 != value)
                {
                    _sampleCenterX1 = value;
                    NotifyPropertyChanged("SampleCenterX1");
                }
            }
        }
        public int SampleCenterY1
        {
            get { return _sampleCenterY1; }
            set
            {
                if (_sampleCenterY1 != value)
                {
                    _sampleCenterY1 = value;
                    NotifyPropertyChanged("SampleCenterY1");
                }
            }
        }
        public int SampleCenterZ1
        {
            get { return _sampleCenterZ1; }
            set
            {
                if (_sampleCenterZ1 != value)
                {
                    _sampleCenterZ1 = value;
                    NotifyPropertyChanged("SampleCenterZ1");
                }
            }
        }
        public int SampleCenterX2
        {
            get { return _sampleCenterX2; }
            set
            {
                if (_sampleCenterX2 != value)
                {
                    _sampleCenterX2 = value;
                    NotifyPropertyChanged("SampleCenterX2");
                }
            }
        }
        public int SampleCenterY2
        {
            get { return _sampleCenterY2; }
            set
            {
                if (_sampleCenterY2 != value)
                {
                    _sampleCenterY2 = value;
                    NotifyPropertyChanged("SampleCenterY2");
                }
            }
        }
        public int SampleCenterZ2
        {
            get { return _sampleCenterZ2; }
            set
            {
                if (_sampleCenterZ2 != value)
                {
                    _sampleCenterZ2 = value;
                    NotifyPropertyChanged("SampleCenterZ2");
                }
            }
        }
        public int SampleRadius1
        {
            get { return _sampleRadius1; }
            set
            {
                if (_sampleRadius1 != value)
                {
                    _sampleRadius1 = value;
                    NotifyPropertyChanged("SampleRadius1");
                }
            }
        }
        public int SampleRadius2
        {
            get { return _sampleRadius2; }
            set
            {
                if (_sampleRadius2 != value)
                {
                    _sampleRadius2 = value;
                    NotifyPropertyChanged("SampleRadius2");
                }
            }
        }

        public int ImagingZCorrectThresh
        {
            get { return _imagingZCorrectThresh; }
            set
            {
                if (_imagingZCorrectThresh != value)
                {
                    _imagingZCorrectThresh = value;
                    NotifyPropertyChanged("ImagingZCorrectThresh");
                }
            }
        }
        public int ImagingSlicePixel
        {
            get { return _imagingSlicePixel; }
            set
            {
                if (_imagingSlicePixel != value)
                {
                    _imagingSlicePixel = value;
                    NotifyPropertyChanged("ImagingSlicePixel");
                }
            }
        }
        public int VolumePixelThreshold
        {
            get { return _volumePixelThreshold; }
            set
            {
                if (_volumePixelThreshold != value)
                {
                    _volumePixelThreshold = value;
                    NotifyPropertyChanged("VolumePixelThreshold");
                }
            }
        }
        public int IsosurfaceThreshold
        {
            get { return _isosurfaceThreshold; }
            set
            {
                if (_isosurfaceThreshold != value)
                {
                    _isosurfaceThreshold = value;
                    NotifyPropertyChanged("IsosurfaceThreshold");
                }
            }
        }
        public int IsosurfaceIsoValue
        {
            get { return _isosurfaceIsoValue; }
            set
            {
                if (_isosurfaceIsoValue != value)
                {
                    _isosurfaceIsoValue = value;
                    NotifyPropertyChanged("IsosurfaceIsoValue");
                }
            }
        }
        public int IsosurfaceSmoothWindowSize
        {
            get { return _isosurfaceSmoothWindowSize; }
            set
            {
                if(_isosurfaceSmoothWindowSize != value)
                {
                    _isosurfaceSmoothWindowSize = value;
                    NotifyPropertyChanged("IsosurfaceSmoothWindowSize");
                }
            }
        }

        public bool SampleDoMultiple
        {
            get { return _sampleDoMultiple; }
            set
            {
                if (_sampleDoMultiple != value)
                {
                    _sampleDoMultiple = value;
                    NotifyPropertyChanged("SampleDoMultiple");
                }
            }
        }
        public bool ImagingTotalIon
        {
            get { return _imagingTotalIon; }
            set
            {
                if (_imagingTotalIon != value)
                {
                    _imagingTotalIon = value;
                    NotifyPropertyChanged("ImagingTotalIon");
                }
            }
        }
        public bool ImagingSqrtEnhance
        {
            get { return _imagingSqrtEnhance; }
            set
            {
                if (_imagingSqrtEnhance != value)
                {
                    _imagingSqrtEnhance = value;
                    NotifyPropertyChanged("ImagingSqrtEnhance");
                }
            }
        }
        public bool RenderingShowAxes
        {
            get { return _renderingShowAxes; }
            set
            {
                if (_renderingShowAxes != value)
                {
                    _renderingShowAxes = value;
                    NotifyPropertyChanged("RenderingShowAxes");
                }
            }
        }
        public bool OmitDataNumbering
        {
            get { return _omitDataNumbering; }
            set
            {
                if(_omitDataNumbering != value)
                {
                    _omitDataNumbering = value;
                    NotifyPropertyChanged("OmitDataNumbering");
                }
            }
        }
        public bool IsosurfaceDoSmooth
        {
            get { return _isosurfaceDoSmooth; }
            set
            {
                if(_isosurfaceDoSmooth != value)
                {
                    _isosurfaceDoSmooth = value;
                    NotifyPropertyChanged("IsosurfaceDoSmooth");
                }
            }
        }

        public Color RenderingBackColor
        {
            get { return _renderingBackColor; }
            set
            {
                if (_renderingBackColor != value)
                {
                    _renderingBackColor = value;
                    NotifyPropertyChanged("RenderingBackColor");
                }
            }
        }

        public Workspace()
        {
            setupNewWorkspace();

            _registry = new RegSettings();
            _registry.ReadSettings();
        }
        public Workspace(string FilePath)
        {
            setupNewWorkspace();

            Load(FilePath);
        }
        public Workspace(string FilePath, BackgroundWorker bw)
        {
            setupNewWorkspace();

            Load(FilePath, bw);
        }
        private void setupNewWorkspace()
        {
            _data = new ObservableCollection<Data2D>();
            _spectra = new ObservableCollection<Spectrum>();
            _components = new ObservableCollection<ImageComponent>();
            _volumes = new ObservableCollection<Volume>();
            _imageSeries = new ObservableCollection<DisplaySeries>();
            _sems = new ObservableCollection<SEM>();

            _data.CollectionChanged += CollectionChanged;
            _spectra.CollectionChanged += CollectionChanged;
            _components.CollectionChanged += CollectionChanged;
            _volumes.CollectionChanged += CollectionChanged;
            _imageSeries.CollectionChanged += CollectionChanged;
            _sems.CollectionChanged += CollectionChanged;

            SetDefaultValues();

            _registry = new RegSettings();
            _registry.ReadSettings();

            _isNew = true;
        }

        public bool InitializeRegistry()
        {
            _registry = new RegSettings();
            return _registry.ReadSettings();
        }

        private void SetDefaultValues()
        {
            WorkspaceName = "New Workspace";
            SpectraMassStart = 0.00d;
            SpectraMassEnd = 1000.00d;
            SpectraCustomRange = String.Empty;

            ImagingSqrtEnhance = false;
            ImagingZCorrectThresh = 15;
            ImagingSlicePixel = 0;
            VolumePixelThreshold = 20;
            VolumePixelSize = 1.0f;
            VolumePixelDepth = 1.0f;
            VolumeZSpacing = 0.0f;
            IsosurfaceThreshold = 20;
            IsosurfaceIsoValue = 30;
            RenderingBackColor = Color.FromArgb(255, 0, 0, 0);
            RenderingShowAxes = true;
            SampleDoMultiple = false;
            SampleXDimension = 128;
            SampleYDimension = 128;
            SampleZDimension = 128;
            SampleCenterX1 = 32;
            SampleCenterY1 = 32;
            SampleCenterZ1 = 32;
            SampleCenterX2 = 0;
            SampleCenterY2 = 0;
            SampleCenterZ2 = 0;
            SampleRadius1 = 16;
            SampleRadius2 = 16;
            OmitDataNumbering = false;
            IsosurfaceDoSmooth = true;
            IsosurfaceSmoothWindowSize = 3;
        }

        public void Load(string FilePath)
        {
            using (Stream stream = File.OpenRead(FilePath))
            {
                BinaryReader br = new BinaryReader(stream);

                WorkspaceName = br.ReadString();

                double totalSteps = br.ReadDouble();

                int dataCount = br.ReadInt32();
                for (int i = 0; i < dataCount; i++)
                {
                    string dataName = br.ReadString();
                    int width = br.ReadInt32();
                    int height = br.ReadInt32();

                    Data2D d = new Data2D(width, height);

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (br.ReadBoolean())
                            {
                                d[x, y] = br.ReadSingle();
                            }
                        }
                    }

                    d.DataName = dataName;
                    d.UniqueID = br.ReadInt32();

                    Data.Add(d);
                }

                int compCount = br.ReadInt32();
                for (int i = 0; i < compCount; i++)
                {
                    string compName = br.ReadString();

                    byte a = br.ReadByte();
                    byte r = br.ReadByte();
                    byte g = br.ReadByte();
                    byte b = br.ReadByte();

                    ImageComponent c = new ImageComponent(compName, Color.FromArgb(a, r, g, b));

                    int dataLength = br.ReadInt32();
                    for (int j = 0; j < dataLength; j++)
                    {
                        if (br.ReadBoolean())
                        {
                            int uniqueId = br.ReadInt32();
                            foreach (Data2D d in Data)
                            {
                                if (d.UniqueID == uniqueId)
                                {
                                    c.AddData(d);
                                    break;
                                }
                            }
                            int width = br.ReadInt32();
                            int height = br.ReadInt32();
                        }
                        else
                        {
                            int width = br.ReadInt32();
                            int height = br.ReadInt32();

                            Data2D d = new Data2D(width, height);
                            for (int x = 0; x < width; x++)
                            {
                                for (int y = 0; y < height; y++)
                                {
                                    if (br.ReadBoolean())
                                    {
                                        d[x, y] = br.ReadSingle();
                                    }
                                }
                            }
                            c.AddData(d);
                        }
                    }
                    Components.Add(c);
                }

                int isoCount = br.ReadInt32();
                for (int i = 0; i < isoCount; i++)
                {
                    int dummy = br.ReadInt32();
                    if (dummy != -1) throw new ArgumentException("File position is off.");

                    int width = br.ReadInt32();
                    int height = br.ReadInt32();
                    int depth = br.ReadInt32();

                    string isoName = br.ReadString();

                    byte a = br.ReadByte();
                    byte r = br.ReadByte();
                    byte g = br.ReadByte();
                    byte b = br.ReadByte();

                    int threshold = br.ReadInt32();
                    int isoValue = br.ReadInt32();

                    Data3D d = new Data3D(width, height, depth);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int z = 0; z < depth; z++)
                            {
                                if (br.ReadBoolean())
                                {
                                    d[x, y, z] = br.ReadSingle();
                                }
                            }
                        }
                    }

                    //Removed this since isosurfaces are no longer used. However, to maintain
                    //compatibility with previous versions of .wks files, the procedure is left
                    //in place.

                    //Isosurfaces.Add(new Isosurface(isoName, Color.FromArgb(a, r, g, b),
                    //    d, isoValue, threshold));
                }

                int volCount = br.ReadInt32();
                for (int i = 0; i < volCount; i++)
                {
                    string volName = br.ReadString();

                    int width = br.ReadInt32();
                    int height = br.ReadInt32();
                    int depth = br.ReadInt32();

                    byte a = br.ReadByte();
                    byte r = br.ReadByte();
                    byte g = br.ReadByte();
                    byte b = br.ReadByte();

                    Data3D d = new Data3D(width, height, depth);
                    for (int z = 0; z < depth; z++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                if (br.ReadBoolean())
                                {
                                    d[x, y, z] = br.ReadSingle();
                                }
                            }
                        }
                    }

                    Volumes.Add(new Volume(d, Color.FromArgb(a, r, g, b), volName));
                }

                SpectraMassStart = br.ReadDouble();
                SpectraMassEnd = br.ReadDouble();
                VolumePixelSize = br.ReadDouble();
                VolumePixelDepth = br.ReadDouble();
                VolumeZSpacing = br.ReadDouble();

                SampleXDimension = br.ReadInt32();
                SampleYDimension = br.ReadInt32();
                SampleZDimension = br.ReadInt32();
                SampleCenterX1 = br.ReadInt32();
                SampleCenterY1 = br.ReadInt32();
                SampleCenterZ1 = br.ReadInt32();
                SampleCenterX2 = br.ReadInt32();
                SampleCenterY2 = br.ReadInt32();
                SampleCenterZ2 = br.ReadInt32();
                SampleRadius1 = br.ReadInt32();
                SampleRadius2 = br.ReadInt32();

                ImagingZCorrectThresh = br.ReadInt32();
                ImagingSlicePixel = br.ReadInt32();
                VolumePixelThreshold = br.ReadInt32();
                IsosurfaceThreshold = br.ReadInt32();
                IsosurfaceIsoValue = br.ReadInt32();

                SampleDoMultiple = br.ReadBoolean();
                ImagingTotalIon = br.ReadBoolean();
                ImagingSqrtEnhance = br.ReadBoolean();
                RenderingShowAxes = br.ReadBoolean();

                RenderingBackColor = Color.FromArgb(br.ReadByte(), br.ReadByte(), 
                    br.ReadByte(), br.ReadByte());
            }

            _isLoaded = true;
        }
        public void Load(string FilePath, BackgroundWorker bw)
        {
            using (Stream stream = File.OpenRead(FilePath))
            {
                BinaryReader br = new BinaryReader(stream);

                WorkspaceName = br.ReadString();

                double counter = 0;
                double totalSteps = br.ReadDouble();

                int dataCount = br.ReadInt32();
                for (int i = 0; i < dataCount; i++)
                {
                    string dataName = br.ReadString();
                    int width = br.ReadInt32();
                    int height = br.ReadInt32();

                    Data2D d = new Data2D(width, height);

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (br.ReadBoolean())
                            {
                                d[x, y] = br.ReadSingle();
                            }
                        }
                        counter += height;
                        bw.ReportProgress(Percentage.GetPercent(counter, totalSteps));
                    }

                    d.DataName = dataName;
                    d.UniqueID = br.ReadInt32();

                    Data.Add(d);
                }

                int compCount = br.ReadInt32();
                for (int i = 0; i < compCount; i++)
                {
                    string compName = br.ReadString();

                    byte a = br.ReadByte();
                    byte r = br.ReadByte();
                    byte g = br.ReadByte();
                    byte b = br.ReadByte();

                    ImageComponent c = new ImageComponent(compName, Color.FromArgb(a, r, g, b));

                    int dataLength = br.ReadInt32();
                    for (int j = 0; j < dataLength; j++)
                    {
                        if (br.ReadBoolean())
                        {
                            int uniqueId = br.ReadInt32();
                            foreach (Data2D d in Data)
                            {
                                if (d.UniqueID == uniqueId)
                                {
                                    c.AddData(d);
                                    break;
                                }
                            }
                            int width = br.ReadInt32();
                            int height = br.ReadInt32();

                            counter += (width * height);
                            bw.ReportProgress(Percentage.GetPercent(counter, totalSteps));
                        }
                        else
                        {
                            int width = br.ReadInt32();
                            int height = br.ReadInt32();

                            Data2D d = new Data2D(width, height);
                            for (int x = 0; x < width; x++)
                            {
                                for (int y = 0; y < height; y++)
                                {
                                    if (br.ReadBoolean())
                                    {
                                        d[x, y] = br.ReadSingle();
                                    }
                                }
                                counter += height;
                                bw.ReportProgress(Percentage.GetPercent(counter, totalSteps));
                            }
                            c.AddData(d);
                        }
                    }
                    Components.Add(c);
                }

                int isoCount = br.ReadInt32();
                for (int i = 0; i < isoCount; i++)
                {
                    int dummy = br.ReadInt32();
                    if (dummy != -1) throw new ArgumentException("File position is off.");

                    int width = br.ReadInt32();
                    int height = br.ReadInt32();
                    int depth = br.ReadInt32();

                    string isoName = br.ReadString();

                    byte a = br.ReadByte();
                    byte r = br.ReadByte();
                    byte g = br.ReadByte();
                    byte b = br.ReadByte();

                    int threshold = br.ReadInt32();
                    int isoValue = br.ReadInt32();

                    Data3D d = new Data3D(width, height, depth);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int z = 0; z < depth; z++)
                            {
                                if (br.ReadBoolean())
                                {
                                    d[x, y, z] = br.ReadSingle();
                                }
                            }
                            counter += depth;
                            bw.ReportProgress(Percentage.GetPercent(counter, totalSteps));
                        }
                    }

                    //Removed this since isosurfaces are no longer used. However, to maintain
                    //compatibility with previous versions of .wks files, the procedure is left
                    //in place. bw.Write(0) remains in the Save functions to maintain file position.

                    //Isosurfaces.Add(new Isosurface(isoName, Color.FromArgb(a, r, g, b),
                    //    d, isoValue, threshold));
                }

                int volCount = br.ReadInt32();
                for (int i = 0; i < volCount; i++)
                {
                    string volName = br.ReadString();

                    int width = br.ReadInt32();
                    int height = br.ReadInt32();
                    int depth = br.ReadInt32();

                    byte a = br.ReadByte();
                    byte r = br.ReadByte();
                    byte g = br.ReadByte();
                    byte b = br.ReadByte();

                    Data3D d = new Data3D(width, height, depth);
                    for (int z = 0; z < depth; z++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            for (int y = 0; y < height; y++)
                            {
                                if (br.ReadBoolean())
                                {
                                    d[x, y, z] = br.ReadSingle();
                                }
                            }
                            counter += height;
                            bw.ReportProgress(Percentage.GetPercent(counter, totalSteps));
                        }
                    }

                    Volumes.Add(new Volume(d, Color.FromArgb(a, r, g, b), volName));
                }

                SpectraMassStart = br.ReadDouble();
                SpectraMassEnd = br.ReadDouble();
                VolumePixelSize = br.ReadDouble();
                VolumePixelDepth = br.ReadDouble();
                VolumeZSpacing = br.ReadDouble();

                counter += 5;
                bw.ReportProgress(Percentage.GetPercent(counter, totalSteps));

                SampleXDimension = br.ReadInt32();
                SampleYDimension = br.ReadInt32();
                SampleZDimension = br.ReadInt32();
                SampleCenterX1 = br.ReadInt32();
                SampleCenterY1 = br.ReadInt32();
                SampleCenterZ1 = br.ReadInt32();
                SampleCenterX2 = br.ReadInt32();
                SampleCenterY2 = br.ReadInt32();
                SampleCenterZ2 = br.ReadInt32();
                SampleRadius1 = br.ReadInt32();
                SampleRadius2 = br.ReadInt32();

                counter += 11;
                bw.ReportProgress(Percentage.GetPercent(counter, totalSteps));

                ImagingZCorrectThresh = br.ReadInt32();
                ImagingSlicePixel = br.ReadInt32();
                VolumePixelThreshold = br.ReadInt32();
                IsosurfaceThreshold = br.ReadInt32();
                IsosurfaceIsoValue = br.ReadInt32();

                counter += 5;
                bw.ReportProgress(Percentage.GetPercent(counter, totalSteps));

                SampleDoMultiple = br.ReadBoolean();
                ImagingTotalIon = br.ReadBoolean();
                ImagingSqrtEnhance = br.ReadBoolean();
                RenderingShowAxes = br.ReadBoolean();

                counter += 4;
                bw.ReportProgress(Percentage.GetPercent(counter, totalSteps));

                RenderingBackColor = Color.FromArgb(br.ReadByte(), br.ReadByte(),
                    br.ReadByte(), br.ReadByte());

                bw.ReportProgress(100);
            }

            _isLoaded = true;
        }
        public async void LoadAsync(string FilePath)
        {
            await Task.Run(() => Load(FilePath));
        }
        public void Save(string FilePath)
        {
            if (WorkspaceName == null || WorkspaceName == "" || WorkspaceName == "New Workspace") 
                WorkspaceName = Path.GetFileNameWithoutExtension(FilePath);

            double totalSteps = 0;
            foreach (Data2D d in Data)
            {
                totalSteps += (d.Width * d.Height);
            }
            foreach (ImageComponent c in Components)
            {
                totalSteps += (c.Width * c.Height);
            }
            foreach (Volume v in Volumes)
            {
                totalSteps += (v.Width * v.Height * v.Depth);
            }
            totalSteps += 30;

            using (Stream stream = File.OpenWrite(FilePath))
            {
                BinaryWriter bw = new BinaryWriter(stream);

                bw.Write(WorkspaceName);
                bw.Write(totalSteps);

                GenerateUniqueIDs();

                bw.Write(Data.Count);
                for (int i = 0; i < Data.Count; i++)
                {
                    Data2D d = Data[i];
                    bw.Write(d.DataName);
                    bw.Write(d.Width);
                    bw.Write(d.Height);

                    for (int x = 0; x < d.Width; x++)
                    {
                        for (int y = 0; y < d.Height; y++)
                        {
                            if (d[x, y] > 0)
                            {
                                bw.Write(true);
                                bw.Write(d[x, y]);
                            }
                            else bw.Write(false);
                        }
                    }

                    bw.Write(d.UniqueID);
                }

                bw.Write(Components.Count);
                for (int i = 0; i < Components.Count; i++)
                {
                    ImageComponent c = Components[i];

                    bw.Write(c.ComponentName);

                    bw.Write(c.PixelColor.A);
                    bw.Write(c.PixelColor.R);
                    bw.Write(c.PixelColor.G);
                    bw.Write(c.PixelColor.B);

                    bw.Write(c.Data.Length);
                    for (int j = 0; j < c.Data.Length; j++)
                    {
                        Data2D d = c.Data[j];
                        if(Data.Contains(d))
                        {
                            bw.Write(true);
                            bw.Write(d.UniqueID);

                            bw.Write(d.Width);
                            bw.Write(d.Height);
                        }
                        else
                        {
                            bw.Write(false);

                            bw.Write(d.Width);
                            bw.Write(d.Height);

                            for (int x = 0; x < d.Width; x++)
                            {
                                for (int y = 0; y < d.Height; y++)
                                {
                                    if (d[x, y] > 0)
                                    {
                                        bw.Write(true);
                                        bw.Write(d[x, y]);
                                    }
                                    else bw.Write(false);
                                }
                            }
                        }
                    }
                }

                //Removed this since isosurfaces are no longer used. However, to maintain
                //compatibility with previous versions of .wks files, the procedure is left
                //in place and bw.Write(0) in to maintain file position.
                bw.Write(0);
                //bw.Write(Isosurfaces.Count);
                //for (int i = 0; i < Isosurfaces.Count; i++)
                //{
                //    Isosurface iso = Isosurfaces[i];

                //    bw.Write(-1);

                //    bw.Write(iso.RenderArea.x);
                //    bw.Write(iso.RenderArea.y);
                //    bw.Write(iso.RenderArea.z);

                //    bw.Write(iso.IsosurfaceName);
                //    bw.Write(iso.SurfaceColor.A);
                //    bw.Write(iso.SurfaceColor.R);
                //    bw.Write(iso.SurfaceColor.G);
                //    bw.Write(iso.SurfaceColor.B);

                //    bw.Write(iso.Threshold);
                //    bw.Write(iso.IsoValue);

                //    for (int x = 0; x < iso.RenderArea.x; x++)
                //    {
                //        for (int y = 0; y < iso.RenderArea.y; y++)
                //        {
                //            for (int z = 0; z < iso.RenderArea.z; z++)
                //            {
                //                float value = iso[x, y, z];
                //                if (value == 0) bw.Write(false);
                //                else
                //                {
                //                    bw.Write(true);
                //                    bw.Write(value);
                //                }
                //            }
                //        }
                //    }
                //}

                bw.Write(Volumes.Count);
                for (int i = 0; i < Volumes.Count; i++)
                {
                    Volume v = Volumes[i];

                    bw.Write(v.VolumeName);

                    bw.Write(v.Width);
                    bw.Write(v.Height);
                    bw.Write(v.Depth);

                    bw.Write(v.DataColor.A);
                    bw.Write(v.DataColor.R);
                    bw.Write(v.DataColor.G);
                    bw.Write(v.DataColor.B);

                    for (int z = 0; z < v.Depth; z++)
                    {
                        for (int x = 0; x < v.Width; x++)
                        {
                            for (int y = 0; y < v.Height; y++)
                            {

                                if (v[x, y, z] == 0) bw.Write(false);
                                else
                                {
                                    bw.Write(true);
                                    bw.Write(v[x, y, z]);
                                }
                            }
                        }
                    }
                }

                bw.Write(SpectraMassStart);
                bw.Write(SpectraMassEnd);
                bw.Write(VolumePixelSize);
                bw.Write(VolumePixelDepth);
                bw.Write(VolumeZSpacing);

                bw.Write(SampleXDimension);
                bw.Write(SampleYDimension);
                bw.Write(SampleZDimension);
                bw.Write(SampleCenterX1);
                bw.Write(SampleCenterY1);
                bw.Write(SampleCenterZ1);
                bw.Write(SampleCenterX2);
                bw.Write(SampleCenterY2);
                bw.Write(SampleCenterZ2);
                bw.Write(SampleRadius1);
                bw.Write(SampleRadius2);

                bw.Write(ImagingZCorrectThresh);
                bw.Write(ImagingSlicePixel);
                bw.Write(VolumePixelThreshold);
                bw.Write(IsosurfaceThreshold);
                bw.Write(IsosurfaceIsoValue);

                bw.Write(SampleDoMultiple);
                bw.Write(ImagingTotalIon);
                bw.Write(ImagingSqrtEnhance);
                bw.Write(RenderingShowAxes);

                bw.Write(RenderingBackColor.A);
                bw.Write(RenderingBackColor.R);
                bw.Write(RenderingBackColor.G);
                bw.Write(RenderingBackColor.B);
            }

            _isSaved = true;
        }
        public void Save(string FilePath, BackgroundWorker worker)
        {
            if (WorkspaceName == null || WorkspaceName == "" || WorkspaceName == "New Workspace")
                WorkspaceName = Path.GetFileNameWithoutExtension(FilePath);

            double counter = 0;
            double totalSteps = 0;
            foreach (Data2D d in Data)
            {
                totalSteps += (d.Width * d.Height);
            }
            foreach (ImageComponent c in Components)
            {
                totalSteps += (c.Width * c.Height);
            }
            foreach (Volume v in Volumes)
            {
                totalSteps += (v.Width * v.Height * v.Depth);
            }
            totalSteps += 30;

            using (Stream stream = File.OpenWrite(FilePath))
            {
                BinaryWriter bw = new BinaryWriter(stream);

                bw.Write(WorkspaceName);
                bw.Write(totalSteps);

                GenerateUniqueIDs();

                bw.Write(Data.Count);
                for (int i = 0; i < Data.Count; i++)
                {
                    Data2D d = Data[i];
                    bw.Write(d.DataName);
                    bw.Write(d.Width);
                    bw.Write(d.Height);

                    for (int x = 0; x < d.Width; x++)
                    {
                        for (int y = 0; y < d.Height; y++)
                        {
                            if (d[x, y] > 0)
                            {
                                bw.Write(true);
                                bw.Write(d[x, y]);
                            }
                            else bw.Write(false);
                        }
                        counter += d.Height;
                        worker.ReportProgress(Percentage.GetPercent(counter, totalSteps));
                    }

                    bw.Write(d.UniqueID);
                }

                bw.Write(Components.Count);
                for (int i = 0; i < Components.Count; i++)
                {
                    ImageComponent c = Components[i];

                    bw.Write(c.ComponentName);

                    bw.Write(c.PixelColor.A);
                    bw.Write(c.PixelColor.R);
                    bw.Write(c.PixelColor.G);
                    bw.Write(c.PixelColor.B);

                    bw.Write(c.Data.Length);
                    for (int j = 0; j < c.Data.Length; j++)
                    {
                        Data2D d = c.Data[j];
                        if (Data.Contains(d))
                        {
                            bw.Write(true);
                            bw.Write(d.UniqueID);

                            bw.Write(d.Width);
                            bw.Write(d.Height);

                            counter += (d.Width * d.Height);
                            worker.ReportProgress(Percentage.GetPercent(counter, totalSteps));
                        }
                        else
                        {
                            bw.Write(false);

                            bw.Write(d.Width);
                            bw.Write(d.Height);

                            for (int x = 0; x < d.Width; x++)
                            {
                                for (int y = 0; y < d.Height; y++)
                                {
                                    if (d[x, y] > 0)
                                    {
                                        bw.Write(true);
                                        bw.Write(d[x, y]);
                                    }
                                    else bw.Write(false);
                                }
                                counter += d.Height;
                                worker.ReportProgress(Percentage.GetPercent(counter, totalSteps));
                            }
                        }
                    }
                }

                //Removed this since isosurfaces are no longer used. However, to maintain
                //compatibility with previous versions of .wks files, the procedure is left
                //in place and bw.Write(0) inserted to maintain file position.
                bw.Write(0);
                //bw.Write(Isosurfaces.Count);
                //for (int i = 0; i < Isosurfaces.Count; i++)
                //{
                //    Isosurface iso = Isosurfaces[i];

                //    bw.Write(-1);

                //    bw.Write(iso.RenderArea.x);
                //    bw.Write(iso.RenderArea.y);
                //    bw.Write(iso.RenderArea.z);

                //    bw.Write(iso.IsosurfaceName);
                //    bw.Write(iso.SurfaceColor.A);
                //    bw.Write(iso.SurfaceColor.R);
                //    bw.Write(iso.SurfaceColor.G);
                //    bw.Write(iso.SurfaceColor.B);

                //    bw.Write(iso.Threshold);
                //    bw.Write(iso.IsoValue);

                //    for (int x = 0; x < iso.RenderArea.x; x++)
                //    {
                //        for (int y = 0; y < iso.RenderArea.y; y++)
                //        {
                //            for (int z = 0; z < iso.RenderArea.z; z++)
                //            {
                //                float value = iso[x, y, z];
                //                if (value == 0) bw.Write(false);
                //                else
                //                {
                //                    bw.Write(true);
                //                    bw.Write(value);
                //                }
                //            }
                //            counter += iso.RenderArea.z;
                //            worker.ReportProgress(Percentage.GetPercent(counter, totalSteps));
                //        }
                //    }
                //}

                bw.Write(Volumes.Count);
                for (int i = 0; i < Volumes.Count; i++)
                {
                    Volume v = Volumes[i];

                    bw.Write(v.VolumeName);

                    bw.Write(v.Width);
                    bw.Write(v.Height);
                    bw.Write(v.Depth);

                    bw.Write(v.DataColor.A);
                    bw.Write(v.DataColor.R);
                    bw.Write(v.DataColor.G);
                    bw.Write(v.DataColor.B);

                    for (int z = 0; z < v.Depth; z++)
                    {
                        for (int x = 0; x < v.Width; x++)
                        {
                            for (int y = 0; y < v.Height; y++)
                            {
                                if (v[x, y, z] == 0) bw.Write(false);
                                else
                                {
                                    bw.Write(true);
                                    bw.Write(v[x, y, z]);
                                }
                            }
                            counter += v.Height;
                            worker.ReportProgress(Percentage.GetPercent(counter, totalSteps));
                        }
                    }
                }

                bw.Write(SpectraMassStart);
                bw.Write(SpectraMassEnd);
                bw.Write(VolumePixelSize);
                bw.Write(VolumePixelDepth);
                bw.Write(VolumeZSpacing);

                counter += 5;
                worker.ReportProgress(Percentage.GetPercent(counter, totalSteps));

                bw.Write(SampleXDimension);
                bw.Write(SampleYDimension);
                bw.Write(SampleZDimension);
                bw.Write(SampleCenterX1);
                bw.Write(SampleCenterY1);
                bw.Write(SampleCenterZ1);
                bw.Write(SampleCenterX2);
                bw.Write(SampleCenterY2);
                bw.Write(SampleCenterZ2);
                bw.Write(SampleRadius1);
                bw.Write(SampleRadius2);

                counter += 11;
                worker.ReportProgress(Percentage.GetPercent(counter, totalSteps));

                bw.Write(ImagingZCorrectThresh);
                bw.Write(ImagingSlicePixel);
                bw.Write(VolumePixelThreshold);
                bw.Write(IsosurfaceThreshold);
                bw.Write(IsosurfaceIsoValue);

                counter += 5;
                worker.ReportProgress(Percentage.GetPercent(counter, totalSteps));

                bw.Write(SampleDoMultiple);
                bw.Write(ImagingTotalIon);
                bw.Write(ImagingSqrtEnhance);
                bw.Write(RenderingShowAxes);

                counter += 4;
                worker.ReportProgress(Percentage.GetPercent(counter, totalSteps));

                bw.Write(RenderingBackColor.A);
                bw.Write(RenderingBackColor.R);
                bw.Write(RenderingBackColor.G);
                bw.Write(RenderingBackColor.B);

                worker.ReportProgress(100);
            }

            _isSaved = true;
        }
        public void Merge(Workspace WorkspaceToMerge)
        {
            foreach (Data2D d in WorkspaceToMerge.Data)
            {
                Data.Add(d);
            }
            foreach (Spectrum s in WorkspaceToMerge.Spectra)
            {
                Spectra.Add(s);
            }
            foreach (ImageComponent c in WorkspaceToMerge.Components)
            {
                Components.Add(c);
            }
            foreach (Volume v in WorkspaceToMerge.Volumes)
            {
                Volumes.Add(v);
            }
            foreach (DisplaySeries i in WorkspaceToMerge.ImageSeries)
            {
                ImageSeries.Add(i);
            }
            foreach (SEM s in WorkspaceToMerge.SEMs)
            {
                SEMs.Add(s);
            }
        }

        private void GenerateUniqueIDs()
        {
            Random r = new Random();
            List<int> ids = new List<int>();

            for (int i = 0; i < Data.Count; i++)
            {
                int id = 0;
                bool proceed = false;
                while (!proceed)
                {
                    id = r.Next(1000, Int32.MaxValue);
                    bool contains = false;
                    for (int j = 0; j < Data.Count; j++)
                    {
                        if (i == j) continue;
                        if (Data[j].UniqueID == id) contains = true;
                    }
                    if (!contains) proceed = true;
                }
                Data[i].UniqueID = id;
            }
        }

        void CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateIsDirty();
            UpdateHasContents();
        }
        public override void NotifyPropertyChanged(string PropertyName)
        {
            base.NotifyPropertyChanged(PropertyName);

            UpdateIsDirty();
            UpdateHasContents();
        }

        #region IsDirty
        bool _isNew;
        bool _isSaved;
        bool _isLoaded;
        private void UpdateIsDirty()
        {
            if (_isNew) _isNew = false;
            if (_isSaved) _isSaved = false;
            if (_isLoaded) _isLoaded = false;

            IsDirty = Data.Count > 0 || Spectra.Count > 0 || Components.Count > 0 ||
                Volumes.Count > 0 || ImageSeries.Count > 0 ||
                SEMs.Count > 0;
        }
        private void UpdateHasContents()
        {
            HasContents = Data.Count > 0 || Spectra.Count > 0 || Components.Count > 0 ||
                Volumes.Count > 0 || ImageSeries.Count > 0 ||
                SEMs.Count > 0;
        }
        #endregion

        public static Workspace SampleWorkspace()
        {
            Workspace w = new Workspace();

            Random r = new Random();

            for (int i = 0; i < 15; i++)
            {
                Data2D d = new Data2D(256, 256);
                d.DataName = string.Format("Sample table {0}", i + 1);

                for (int x = 0; x < d.Width; x++)
                {
                    for (int y = 0; y < d.Height; y++)
                    {
                        d[x, y] = r.Next(0, 255);
                    }
                }

                w.Data.Add(d);
            }

            for (int i = 0; i < 3; i++)
            {
                ImageComponent c = new ImageComponent(string.Format("Sample comp {0}", i + 1),
                    Color.FromArgb(255, (byte)r.Next(0, 255), (byte)r.Next(0, 255), (byte)r.Next(0, 255)));
                for (int z = 0; z < 5; z++)
                {
                    int index = (i * 3) + z;
                    c.AddData(w.Data[index]);
                }

                w.Components.Add(c);
            }

            for (int i = 0; i < 3; i++)
            {
                ImageComponent c = w.Components[i];
                System.Windows.Media.Imaging.BitmapSource[] bitmapSources = ImageGenerator.Instance.Create(new ImageComponent[] { c }, new ImagingParameters()
                {
                    NormalizationMethod = NormalizationMethod.Single,
                    SqrtEnhance = false,
                    TotalIon = false
                });

                DisplayImage[] images = new DisplayImage[bitmapSources.Length];
                for (int j = 0; j < bitmapSources.Length; j++)
                {
                    images[j] = new DisplayImage();
                    images[j].Source = bitmapSources[j];

                    string title = "";
                    title += "Image_" + j.ToString();
                    images[j].Title = title;
                }

                DisplaySeries series = new DisplaySeries(images);
                series.SeriesName = "Sample series " + (i + 1);
                w.ImageSeries.Add(series);
            }

            for (int i = 0; i < 3; i++)
            {
                ImageComponent c = w.Components[i];
                Volume v = new Volume(c.Data, c.PixelColor);
                v.VolumeName = "Sample volume " + (i + 1);
                w.Volumes.Add(v);
            }

            return w;
        }
    }
    internal class WorkspaceFile
    {
        
    }
    internal class WorkspaceHeader
    {
        const int _headerLength = 5000;

        byte[] _buffer;

        internal int HeaderLength { get { return _headerLength; } }
        internal Version FileVersion;

    }

    internal static class WorkspaceVersion
    {
        // Workspace versions:
        // 3.6.0.0: Original version 3
        // 3.7.0.0: First version to contain header. File is prefixed with random
        //          string with 32 random characters to distinguish from old
        //          version of workspace files

        internal const string _filePrefix = "hBI4@^eDz#tquuMRvW2KUDUrNr04H8vL";

        internal static Version GetVersion(BinaryReader reader)
        {
            string prefix = reader.ReadString();

            // If less than version 3.7.0.0 this will be name of workspace
            // otherwise this will be random string prefix
            if (prefix == _filePrefix)
            {
                int major = reader.ReadInt32();
                int minor = reader.ReadInt32();
                int build = reader.ReadInt32();
                int revis = reader.ReadInt32();

                return new Version(major, minor, build, revis);
            }
            else
            {
                // Reset position to beginning since routine for earlier version will not
                // be looking for a header
                reader.BaseStream.Position = 0;
                return new Version(3, 6);
            }
        }
    }

    public class WorkspaceNew : ISObject
    {
        string _name;
        bool _isDirty;
        bool _hasContents;

        WorkspaceParameters _parameters;

        ObservableCollection<Data2D> _data;
        ObservableCollection<Spectrum> _spectra;
        ObservableCollection<Volume> _volumes;

        public string WorkspaceName
        {
            get { return _name; }
            set { _name = value; }
        }
        public bool IsDirty
        {
            get { return _isDirty; }
            set { _isDirty = value; }
        }
        public bool HasContents
        {
            get { return _hasContents; }
            set { _hasContents = value; }
        }

        public WorkspaceParameters Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public ObservableCollection<Data2D> Data
        {
            get { return _data; }
            set { _data = value; }
        }
        public ObservableCollection<Spectrum> Spectra
        {
            get { return _spectra; }
            set { _spectra = value; }
        }
        public ObservableCollection<Volume> Volumes
        {
            get { return _volumes; }
            set { _volumes = value; }
        }

        public WorkspaceNew()
        {
            WorkspaceName = String.Empty;
            Parameters = new WorkspaceParameters();
        }
    }

    public class WorkspaceParameters : ISObject
    {
        // Data tab
        int _dataExpandWindowSize;
        int _dataSampleDataSizeX1;
        int _dataSampleDataSizeY1;
        int _dataSampleDataSizeZ1;
        int _dataSampleDataCenterX1;
        int _dataSampleDataCenterY1;
        int _dataSampleDataCenterZ1;
        int _dataSampleDataRadius1;
        int _dataSampleDataSizeX2;
        int _dataSampleDataSizeY2;
        int _dataSampleDataSizeZ2;
        int _dataSampleDataCenterX2;
        int _dataSampleDataCenterY2;
        int _dataSampleDataCenterZ2;
        int _dataSampleDataRadius2;

        public int DataExpandWindowSize
        {
            get { return _dataExpandWindowSize; }
            set
            {
                if (_dataExpandWindowSize != value)
                {
                    _dataExpandWindowSize = value;
                    NotifyPropertyChanged("DataExpandWindowSize");
                }
            }
        }
        public int DataSampleDataSizeX1
        {
            get { return _dataSampleDataSizeX1; }
            set
            {
                if (_dataSampleDataSizeX1 != value)
                {
                    _dataSampleDataSizeX1 = value;
                    NotifyPropertyChanged("DataSampleDataSizeX1");
                }
            }
        }
        public int DataSampleDataSizeY1
        {
            get { return _dataSampleDataSizeY1; }
            set
            {
                if (_dataSampleDataSizeY1 != value)
                {
                    _dataSampleDataSizeY1 = value;
                    NotifyPropertyChanged("DataSampleDataSizeY1");
                }
            }
        }
        public int DataSampleDataSizeZ1
        {
            get { return _dataSampleDataSizeZ1; }
            set
            {
                if (_dataSampleDataSizeZ1 != value)
                {
                    _dataSampleDataSizeZ1 = value;
                    NotifyPropertyChanged("DataSampleDataSizeZ1");
                }
            }
        }
        public int DataSampleDataCenterX1
        {
            get { return _dataSampleDataCenterX1; }
            set
            {
                if (_dataSampleDataCenterX1 != value)
                {
                    _dataSampleDataCenterX1 = value;
                    NotifyPropertyChanged("DataSampleDataCenterX1");
                }
            }
        }
        public int DataSampleDataCenterY1
        {
            get { return _dataSampleDataCenterY1; }
            set
            {
                if (_dataSampleDataCenterY1 != value)
                {
                    _dataSampleDataCenterY1 = value;
                    NotifyPropertyChanged("DataSampleDataCenterY1");
                }
            }
        }
        public int DataSampleDataCenterZ1
        {
            get { return _dataSampleDataCenterZ1; }
            set
            {
                if (_dataSampleDataCenterZ1 != value)
                {
                    _dataSampleDataCenterZ1 = value;
                    NotifyPropertyChanged("DataSampleDataCenterZ1");
                }
            }
        }
        public int DataSampleDataRadius1
        {
            get { return _dataSampleDataRadius1; }
            set
            {
                if (_dataSampleDataRadius1 != value)
                {
                    _dataSampleDataRadius1 = value;
                    NotifyPropertyChanged("DataSampleDataRadius1");
                }
            }
        }
        public int DataSampleDataSizeX2
        {
            get { return _dataSampleDataSizeX2; }
            set
            {
                if (_dataSampleDataSizeX2 != value)
                {
                    _dataSampleDataSizeX2 = value;
                    NotifyPropertyChanged("DataSampleDataSizeX2");
                }
            }
        }
        public int DataSampleDataSizeY2
        {
            get { return _dataSampleDataSizeY2; }
            set
            {
                if (_dataSampleDataSizeY2 != value)
                {
                    _dataSampleDataSizeY2 = value;
                    NotifyPropertyChanged("DataSampleDataSizeY2");
                }
            }
        }
        public int DataSampleDataSizeZ2
        {
            get { return _dataSampleDataSizeZ2; }
            set
            {
                if (_dataSampleDataSizeZ2 != value)
                {
                    _dataSampleDataSizeZ2 = value;
                    NotifyPropertyChanged("DataSampleDataSizeZ2");
                }
            }
        }
        public int DataSampleDataCenterX2
        {
            get { return _dataSampleDataCenterX2; }
            set
            {
                if (_dataSampleDataCenterX2 != value)
                {
                    _dataSampleDataCenterX2 = value;
                    NotifyPropertyChanged("DataSampleDataCenterX2");
                }
            }
        }
        public int DataSampleDataCenterY2
        {
            get { return _dataSampleDataCenterY2; }
            set
            {
                if (_dataSampleDataCenterY2 != value)
                {
                    _dataSampleDataCenterY2 = value;
                    NotifyPropertyChanged("DataSampleDataCenterY2");
                }
            }
        }
        public int DataSampleDataCenterZ2
        {
            get { return _dataSampleDataCenterZ2; }
            set
            {
                if (_dataSampleDataCenterZ2 != value)
                {
                    _dataSampleDataCenterZ2 = value;
                    NotifyPropertyChanged("DataSampleDataCenterZ2");
                }
            }
        }
        public int DataSampleDataRadius2
        {
            get { return _dataSampleDataRadius2; }
            set
            {
                if (_dataSampleDataRadius2 != value)
                {
                    _dataSampleDataRadius2 = value;
                    NotifyPropertyChanged("DataSampleDataRadius2");
                }
            }
        }

        // Imaging tab
        NormalizationMethod _imagingNormalizationMethod;
        bool _imagingIncludeTotalIon;
        bool _imagingSquareRootEnhance;
        int _imagingSlicingPixel;

        public NormalizationMethod ImagingNormalizationMethod
        {
            get { return _imagingNormalizationMethod; }
            set
            {
                if (_imagingNormalizationMethod != value)
                {
                    _imagingNormalizationMethod = value;
                    NotifyPropertyChanged("ImagingNormalizationMethod");
                }
            }
        }
        public bool ImagingIncludeTotalIon
        {
            get { return _imagingIncludeTotalIon; }
            set
            {
                if (_imagingIncludeTotalIon != value)
                {
                    _imagingIncludeTotalIon = value;
                    NotifyPropertyChanged("ImagingIncludeTotalIon");
                }
            }
        }
        public bool ImagingSquareRootEnhance
        {
            get { return _imagingSquareRootEnhance; }
            set
            {
                if (_imagingSquareRootEnhance != value)
                {
                    _imagingSquareRootEnhance = value;
                    NotifyPropertyChanged("ImagingSquareRootEnhance");
                }
            }
        }
        public int ImagingSlicingPixel
        {
            get { return _imagingSlicingPixel; }
            set
            {
                if (_imagingSlicingPixel != value)
                {
                    _imagingSlicingPixel = value;
                    NotifyPropertyChanged("ImagingSlicingPixel");
                }
            }
        }

        // Spectrum tab
        double _spectrumStartMass;
        double _spectrumEndMass;
        string _spectrumCustomRange;
        int _spectrumBinSize;
        bool _omitDataNumbering;

        public double SpectrumStartMass
        {
            get { return _spectrumStartMass; }
            set
            {
                if (_spectrumStartMass != value)
                {
                    _spectrumStartMass = value;
                    NotifyPropertyChanged("SpectrumStartMass");
                }
            }
        }
        public double SpectrumEndMass
        {
            get { return _spectrumEndMass; }
            set
            {
                if (_spectrumEndMass != value)
                {
                    _spectrumEndMass = value;
                    NotifyPropertyChanged("SpectrumEndMass");
                }
            }
        }
        public string SpectrumCustomRange
        {
            get { return _spectrumCustomRange; }
            set
            {
                if(_spectrumCustomRange != value)
                {
                    _spectrumCustomRange = value;
                    NotifyPropertyChanged("SpectrumCustomRange");
                }
            }
        }
        public int SpectrumBinSize
        {
            get { return _spectrumBinSize; }
            set
            {
                if (_spectrumBinSize != value)
                {
                    _spectrumBinSize = value;
                    NotifyPropertyChanged("SpectrumBinSize");
                }
            }
        }
        public bool OmitDataNumbering
        {
            get { return _omitDataNumbering; }
            set
            {
                if(_omitDataNumbering != value)
                {
                    _omitDataNumbering = value;
                    NotifyPropertyChanged("OmitDataNumbering");
                }
            }
        }

        // Rendering tab
        bool _renderingDoZCorrection;
        int _renderingZCorrectionThreshold;

        public bool RenderingDoZCorrection
        {
            get { return _renderingDoZCorrection; }
            set
            {
                if (_renderingDoZCorrection != value)
                {
                    _renderingDoZCorrection = value;
                    NotifyPropertyChanged("RenderingDoZCorrection");
                }
            }
        }
        public int RenderingZCorrectionThreshold
        {
            get { return _renderingZCorrectionThreshold; }
            set
            {
                if (_renderingZCorrectionThreshold != value)
                {
                    _renderingZCorrectionThreshold = value;
                    NotifyPropertyChanged("RenderingZCorrectionThreshold");
                }
            }
        }

        public WorkspaceParameters()
        {
            setDefaults();
        }

        private void setDefaults()
        {
            DataExpandWindowSize = 5;
            DataSampleDataSizeX1 = 128;
            DataSampleDataSizeY1 = 128;
            DataSampleDataSizeZ1 = 128;
            DataSampleDataCenterX1 = 64;
            DataSampleDataCenterY1 = 64;
            DataSampleDataCenterZ1 = 64;
            DataSampleDataRadius1 = 32;
            DataSampleDataSizeX2 = 128;
            DataSampleDataSizeY2 = 128;
            DataSampleDataSizeZ2 = 128;
            DataSampleDataCenterX2 = 32;
            DataSampleDataCenterY2 = 32;
            DataSampleDataCenterZ2 = 32;
            DataSampleDataRadius2 = 32;

            ImagingNormalizationMethod = NormalizationMethod.Both;
            ImagingIncludeTotalIon = false;
            ImagingSquareRootEnhance = false;
            ImagingSlicingPixel = 0;

            SpectrumStartMass = 0;
            SpectrumEndMass = 1000;
            SpectrumCustomRange = String.Empty;
            SpectrumBinSize = 5;
            OmitDataNumbering = false;

            RenderingDoZCorrection = false;
            RenderingZCorrectionThreshold = 25;
        }
    }
}