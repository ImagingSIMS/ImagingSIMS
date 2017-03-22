using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ImagingSIMS.Direct3DRendering.DrawingObjects;
using ImagingSIMS.Common.Controls;
using SharpDX;

namespace ImagingSIMS.Direct3DRendering.ViewModels
{
    public class RenderingViewModel : INotifyPropertyChanged
    {
        // Renderer
        bool _isRenderLoaded;
        RenderType _renderType;
        double _fps;
        int _numTrianglesDrawn;

        public bool IsRenderLoaded
        {
            get { return _isRenderLoaded; }
            set
            {
                if (_isRenderLoaded != value)
                {
                    _isRenderLoaded = value;
                    NotifyPropertyChanged("IsRenderLoaded");
                }
            }
        }
        public RenderType RenderType
        {
            get { return _renderType; }
            set
            {
                if (_renderType != value)
                {
                    _renderType = value;
                    NotifyPropertyChanged("RenderType");
                }
            }
        }
        public double FPS
        {
            get { return _fps; }
            set
            {
                if (_fps != value)
                {
                    _fps = value;
                    NotifyPropertyChanged("FPS");
                }
            }
        }
        public int NumTrianglesDrawn
        {
            get { return _numTrianglesDrawn; }
            set
            {
                if (_numTrianglesDrawn != value)
                {
                    _numTrianglesDrawn = value;
                    NotifyPropertyChanged("NumTrianglesDrawn");
                }
            }
        }

        // Recording
        bool _isRecording;
        bool _getSnapshot;

        public bool IsRecording
        {
            get { return _isRecording; }
            set
            {
                if (_isRecording != value)
                {
                    _isRecording = value;
                    NotifyPropertyChanged("IsRecording");
                }
            }
        }
        public bool GetSnapshot
        {
            get { return _getSnapshot; }
            set
            {
                if (_getSnapshot != value)
                {
                    _getSnapshot = value;
                    NotifyPropertyChanged("GetSnapshot");
                }
            }
        }

        // Rendering
        bool _showAxes;
        bool _showBoundingBox;
        bool _showCoordinateBox;
        float _coordinateBoxTransparency;
        Color _backColor;
        Color[] _volumeColors;
        bool _targetYAxisOrbiting;
        float _heightMapHeight;
        bool _enableDepthBufering;

        public bool ShowAxes
        {
            get { return _showAxes; }
            set
            {
                if (_showAxes != value)
                {
                    _showAxes = value;
                    NotifyPropertyChanged("ShowAxes");
                }
            }
        }
        public bool ShowBoundingBox
        {
            get { return _showBoundingBox; }
            set
            {
                if (_showBoundingBox != value)
                {
                    _showBoundingBox = value;
                    NotifyPropertyChanged("ShowBoundingBox");
                }
            }
        }
        public bool ShowCoordinateBox
        {
            get { return _showCoordinateBox; }
            set
            {
                if (_showCoordinateBox != value)
                {
                    _showCoordinateBox = value;
                    NotifyPropertyChanged("ShowCoordinateBox");
                }
            }
        }
        public float CoordinateBoxTransparency
        {
            get { return _coordinateBoxTransparency; }
            set
            {
                if (_coordinateBoxTransparency != value)
                {
                    _coordinateBoxTransparency = value;
                    NotifyPropertyChanged("CoordinateBoxTransparency");
                }
            }
        }
        public Color BackColor
        {
            get { return _backColor; }
            set
            {
                if (_backColor != value)
                {
                    _backColor = value;
                    NotifyPropertyChanged("BackColor");
                }
            }
        }
        public Color[] VolumeColors
        {
            get { return _volumeColors; }
            set
            {
                if (_volumeColors != value)
                {
                    _volumeColors = value;
                    NotifyPropertyChanged("VolumeColors");
                }
            }
        }
        public bool TargetYAxisOrbiting
        {
            get { return _targetYAxisOrbiting; }
            set
            {
                if (_targetYAxisOrbiting != value)
                {
                    _targetYAxisOrbiting = value;
                    NotifyPropertyChanged("TargetYAxisOrbiting");
                }
            }
        }
        public float HeightMapHeight
        {
            get { return _heightMapHeight; }
            set
            {
                if (_heightMapHeight != value)
                {
                    _heightMapHeight = value;
                    NotifyPropertyChanged("HeightMapHeight");
                }
            }
        }
        public bool EnableDepthBuffering
        {
            get { return _enableDepthBufering; }
            set
            {
                if(_enableDepthBufering != value)
                {
                    _enableDepthBufering = value;
                    NotifyPropertyChanged("EnableDepthBuffering");
                }
            }
        }

        // Camera
        Vector3 _cameraDirection;
        Vector3 _cameraPosition;
        Vector3 _cameraUp;

        public Vector3 CameraDirection
        {
            get { return _cameraDirection; }
            set
            {
                if (_cameraDirection != value)
                {
                    _cameraDirection = value;
                    NotifyPropertyChanged("CameraDirection");
                }
            }
        }
        public Vector3 CameraPosition
        {
            get { return _cameraPosition; }
            set
            {
                if (_cameraPosition != value)
                {
                    _cameraPosition = value;
                    NotifyPropertyChanged("CameraPosition");
                }
            }
        }
        public Vector3 CameraUp
        {
            get { return _cameraUp; }
            set
            {
                if (_cameraUp != value)
                {
                    _cameraUp = value;
                    NotifyPropertyChanged("CameraUp");
                }
            }
        }

        // Bounding Box
        float _boundingBoxMinX;
        float _boundingBoxMaxX;
        float _boundingBoxLowerX;
        float _boundingBoxUpperX;
        float _boundingBoxMinY;
        float _boundingBoxMaxY;
        float _boundingBoxLowerY;
        float _boundingBoxUpperY;
        float _boundingBoxMinZ;
        float _boundingBoxMaxZ;
        float _boundingBoxLowerZ;
        float _boundingBoxUpperZ;

        public float BoundingBoxMinX
        {
            get { return _boundingBoxMinX; }
            set
            {
                if (_boundingBoxMinX != value)
                {
                    _boundingBoxMinX = value;
                    NotifyPropertyChanged("BoundingBoxMinX");
                }
            }
        }
        public float BoundingBoxMaxX
        {
            get { return _boundingBoxMaxX; }
            set
            {
                if (_boundingBoxMaxX != value)
                {
                    _boundingBoxMaxX = value;
                    NotifyPropertyChanged("BoundingBoxMaxX");
                }
            }
        }
        public float BoundingBoxLowerX
        {
            get { return _boundingBoxLowerX; }
            set
            {
                if (_boundingBoxLowerX != value)
                {
                    _boundingBoxLowerX = value;
                    NotifyPropertyChanged("BoundingBoxLowerX");
                }
            }
        }
        public float BoundingBoxUpperX
        {
            get { return _boundingBoxUpperX; }
            set
            {
                if (_boundingBoxUpperX != value)
                {
                    _boundingBoxUpperX = value;
                    NotifyPropertyChanged("BoundingBoxUpperX");
                }
            }
        }
        public float BoundingBoxMinY
        {
            get { return _boundingBoxMinY; }
            set
            {
                if (_boundingBoxMinY != value)
                {
                    _boundingBoxMinY = value;
                    NotifyPropertyChanged("BoundingBoxMinY");
                }
            }
        }
        public float BoundingBoxMaxY
        {
            get { return _boundingBoxMaxY; }
            set
            {
                if (_boundingBoxMaxY != value)
                {
                    _boundingBoxMaxY = value;
                    NotifyPropertyChanged("BoundingBoxMaxY");
                }
            }
        }
        public float BoundingBoxLowerY
        {
            get { return _boundingBoxLowerY; }
            set
            {
                if (_boundingBoxLowerY != value)
                {
                    _boundingBoxLowerY = value;
                    NotifyPropertyChanged("BoundingBoxLowerY");
                }
            }
        }
        public float BoundingBoxUpperY
        {
            get { return _boundingBoxUpperY; }
            set
            {
                if (_boundingBoxUpperY != value)
                {
                    _boundingBoxUpperY = value;
                    NotifyPropertyChanged("BoundingBoxUpperY");
                }
            }
        }
        public float BoundingBoxMinZ
        {
            get { return _boundingBoxMinZ; }
            set
            {
                if (_boundingBoxMinZ != value)
                {
                    _boundingBoxMinZ = value;
                    NotifyPropertyChanged("BoundingBoxMinZ");
                }
            }
        }
        public float BoundingBoxMaxZ
        {
            get { return _boundingBoxMaxZ; }
            set
            {
                if (_boundingBoxMaxZ != value)
                {
                    _boundingBoxMaxZ = value;
                    NotifyPropertyChanged("BoundingBoxMaxZ");
                }
            }
        }
        public float BoundingBoxLowerZ
        {
            get { return _boundingBoxLowerZ; }
            set
            {
                if (_boundingBoxLowerZ != value)
                {
                    _boundingBoxLowerZ = value;
                    NotifyPropertyChanged("BoundingBoxLowerZ");
                }
            }
        }
        public float BoundingBoxUpperZ
        {
            get { return _boundingBoxUpperZ; }
            set
            {
                if (_boundingBoxUpperZ != value)
                {
                    _boundingBoxUpperZ = value;
                    NotifyPropertyChanged("BoundingBoxUpperZ");
                }
            }
        }

        // Lighting
        bool _enableAmbientLighting;
        bool _enablePointLighting;
        bool _enableSpecularLighting;
        Color _ambientLightColor;
        float _ambientLightIntensity;
        PointLightSource[] _pointLights;

        public bool EnableAmbientLighting
        {
            get { return _enableAmbientLighting; }
            set
            {
                if(_enableAmbientLighting != value)
                {
                    _enableAmbientLighting = value;
                    NotifyPropertyChanged("EnableAmbientLighting");
                }
            }
        }
        public bool EnablePointLighting
        {
            get { return _enablePointLighting; }
            set
            {
                if (_enablePointLighting != value)
                {
                    _enablePointLighting = value;
                    NotifyPropertyChanged("EnablePointLighting");
                }
            }
        }
        public bool EnableSpecularLighting
        {
            get { return _enableSpecularLighting; }
            set
            {
                if (_enableSpecularLighting != value)
                {
                    _enableSpecularLighting = value;
                    NotifyPropertyChanged("EnableSpecularLighting");
                }
            }
        }
        public Color AmbientLightColor
        {
            get { return _ambientLightColor; }
            set
            {
                if(_ambientLightColor != value)
                {
                    _ambientLightColor = value;
                    NotifyPropertyChanged("AmbientLightColor");
                }
            }
        }
        public float AmbientLightIntensity
        {
            get { return _ambientLightIntensity;}
            set
            {
                if(_ambientLightIntensity != value)
                {
                    _ambientLightIntensity = value;
                    NotifyPropertyChanged("AmbientLightIntensity");
                }
            }
        }
        public PointLightSource[] PointLights
        {
            get { return _pointLights; }
            set
            {
                if(_pointLights != value)
                {
                    _pointLights = value;
                    NotifyPropertyChanged("PointLights");
                }
            }
        }

        public RenderingViewModel()
        {
            EnableDepthBuffering = true;

            ShowCoordinateBox = false;
            CoordinateBoxTransparency = 1.0f;
            VolumeColors = new Color[8]
            {
                Color.Black,
                Color.Black,
                Color.Black,
                Color.Black,
                Color.Black,
                Color.Black,
                Color.Black,
                Color.Black,
            };

            HeightMapHeight = 1.0f;

            BackColor = Color.Black;

            IsRenderLoaded = false;

            EnableAmbientLighting = true;
            EnablePointLighting = false;
            EnableSpecularLighting = false;

            AmbientLightColor = Color.White;
            AmbientLightIntensity = 0f;

            EnablePointLighting = true;
            PointLights = new PointLightSource[8]
            {
                new PointLightSource(new Vector4(0f), 0f),
                new PointLightSource(new Vector4(0f), 0f),
                new PointLightSource(new Vector4(0f), 0f),
                new PointLightSource(new Vector4(0f), 0f),
                new PointLightSource(new Vector4(0f), 0f),
                new PointLightSource(new Vector4(0f), 0f),
                new PointLightSource(new Vector4(0f), 0f),
                new PointLightSource(new Vector4(0f), 0f)
            };

#if DEBUG || DEBUG_DEVICE
            ShowAxes = true;
            ShowBoundingBox = true;
#endif
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void UpdateLightingParameters(ref LightingParams lightingParams)
        {
            lightingParams.EnableAmbientLighting = EnableAmbientLighting ? 1.0f : 0.0f;
            lightingParams.EnablePointLighting = EnablePointLighting ? 1.0f : 0.0f;
            lightingParams.EnableSpecularLighting = EnableSpecularLighting ? 1.0f : 0.0f;

            lightingParams.AmbientLightColor = AmbientLightColor.ToVector4();
            lightingParams.AmbientLightIntensity = AmbientLightIntensity;

            for (int i = 0; i < 8; i++)
            {
                lightingParams.UpdatePointLight(i, PointLights[i]);
            }
        }

        public static RenderingViewModel DefaultVolumeParameters
        {
            get
            {
                return new RenderingViewModel()
                {
                    EnableAmbientLighting = true,
                    AmbientLightColor = Color.White,
                    AmbientLightIntensity = 1.0f,

                    EnablePointLighting = false
                };
            }
        }
        public static RenderingViewModel DefaultIsosurfaceParameters
        {
            get
            {
                return new RenderingViewModel()
                {
                    EnableAmbientLighting = true,
                    AmbientLightColor = Color.White,
                    AmbientLightIntensity = 0.5f,

                    EnablePointLighting = true,
                    PointLights = new PointLightSource[8] {
                        new PointLightSource(new Vector4(-5f, -5f, 5f, 1f), 0.25f, true),
                        new PointLightSource(new Vector4(5f, -5f, 5f, 5f), 0.25f, true),
                        new PointLightSource(new Vector4(5f, 5f, 5f, 5f), 0.25f, true),
                        new PointLightSource(new Vector4(-5f, 5f, 5f, 5f), 0.25f, true),
                        new PointLightSource(new Vector4(-5f, -5f, -5f, 1f), 0.25f, true),
                        new PointLightSource(new Vector4(5f, -5f, -5f, 5f), 0.25f, true),
                        new PointLightSource(new Vector4(5f, 5f, -5f, 5f), 0.25f, true),
                        new PointLightSource(new Vector4(-5f, 5f, -5f, 5f), 0.25f, true)
                    },
                    
                };
            }
        }
        public static RenderingViewModel DefaultHeightMapParameters
        {
            get
            {
                return new RenderingViewModel()
                {
                    EnableAmbientLighting =  true,
                    AmbientLightColor = Color.White,
                    AmbientLightIntensity = 0.5f,

                    EnablePointLighting = true,
                    PointLights = new PointLightSource[8] {
                        new PointLightSource(new Vector4(-5f, -5f, 5f, 1f), 0.25f, true),
                        new PointLightSource(new Vector4(5f, -5f, 5f, 5f), 0.25f, true),
                        new PointLightSource(new Vector4(5f, 5f, 5f, 5f), 0.25f, true),
                        new PointLightSource(new Vector4(-5f, 5f, 5f, 5f), 0.25f, true),
                        new PointLightSource(new Vector4(-5f, -5f, -5f, 1f), 0.25f, true),
                        new PointLightSource(new Vector4(5f, -5f, -5f, 5f), 0.25f, true),
                        new PointLightSource(new Vector4(5f, 5f, -5f, 5f), 0.25f, true),
                        new PointLightSource(new Vector4(-5f, 5f, -5f, 5f), 0.25f, true)
                    },
                };
            }
        }
    } 
}
