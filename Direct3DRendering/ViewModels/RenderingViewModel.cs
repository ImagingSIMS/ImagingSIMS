using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using ImagingSIMS.Common.Controls;
using SharpDX;

namespace Direct3DRendering.ViewModels
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
        NotifiableColor _backColor;
        NotifiableColor[] _volumeColors;
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
        public NotifiableColor BackColor
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
        public NotifiableColor[] VolumeColors
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
        bool _enableDirectionalLighting;
        bool _enableSpecularLighting;
        NotifiableColor _ambientLightColor;
        float _ambientLightIntensity;
        bool[] _directionalEnabled;
        Vector4[] _directionalDirection;
        NotifiableColor[] _directionalColor;
        float[] _directionalIntensity;

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
        public bool EnableDirectionalLighting
        {
            get { return _enableDirectionalLighting; }
            set
            {
                if (_enableDirectionalLighting != value)
                {
                    _enableDirectionalLighting = value;
                    NotifyPropertyChanged("EnableDirectionalLighting");
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
        public NotifiableColor AmbientLightColor
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
        public bool[] DirectionalEnabled
        {
            get { return _directionalEnabled; }
            set
            {
                if(_directionalEnabled != value)
                {
                    _directionalEnabled = value;
                    NotifyPropertyChanged("DirectionalEnabled");
                }
            }
        }
        public Vector4[] DirectionalDirection
        {
            get { return _directionalDirection; }
            set
            {
                if(_directionalDirection != value)
                {
                    _directionalDirection = value;
                    NotifyPropertyChanged("DirectionalEnabled");
                }
            }
        }
        public NotifiableColor[] DirectionalColor
        {
            get { return _directionalColor; }
            set
            {
                if(_directionalColor != value)
                {
                    _directionalColor = value;
                    NotifyPropertyChanged("DirectionalColor");
                }
            }
        }
        public float[] DirectionalIntensity
        {
            get { return _directionalIntensity; }
            set
            {
                if(_directionalIntensity != value)
                {
                    _directionalIntensity = value;
                }
            }
        }

        public RenderingViewModel()
        {
            EnableDepthBuffering = true;

            ShowCoordinateBox = false;
            CoordinateBoxTransparency = 1.0f;
            VolumeColors = new NotifiableColor[8]
            {
                NotifiableColor.Black,
                NotifiableColor.Black,
                NotifiableColor.Black,
                NotifiableColor.Black,
                NotifiableColor.Black,
                NotifiableColor.Black,
                NotifiableColor.Black,
                NotifiableColor.Black
            };

            HeightMapHeight = 1.0f;

            BackColor = new NotifiableColor()
            {
                A = 255,
                R = 0,
                G = 0,
                B = 0
            };

            IsRenderLoaded = false;

            EnableAmbientLighting = true;
            EnableDirectionalLighting = false;
            EnableSpecularLighting = false;
            AmbientLightColor = new NotifiableColor()
            {
                A = 255,
                R = 255,
                G = 255,
                B = 255
            };
            AmbientLightIntensity = 0.1f;

            DirectionalEnabled = new bool[]
            {
                //true, true, true, true,
                false, false, false, false,
                false, false, false, false
            };
            DirectionalDirection = new Vector4[]
            {
                new Vector4(-1f, -1f, 1f, 1f),
                new Vector4(1f, -1f, 1f, 1f),
                new Vector4(1f, 1f, 1f, 1f),
                new Vector4(-1f, 1f, 1f, 1f),
                new Vector4(-1f, -1f, -1f, 1f),
                new Vector4(1f, -1f, -1f, 1f),
                new Vector4(1f, 1f, -1f, 1f),
                new Vector4(-1f, 1f, -1f, 1f)
            };
            DirectionalColor = new NotifiableColor[]
            {
                NotifiableColor.White,
                NotifiableColor.White,
                NotifiableColor.White,
                NotifiableColor.White,
                NotifiableColor.White,
                NotifiableColor.White,
                NotifiableColor.White,
                NotifiableColor.White
            };
            DirectionalIntensity = new float[]
            {
                0.25f, 0.25f, 0.25f, 0.25f,
                0.25f, 0.25f, 0.25f, 0.25f
            };
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
            lightingParams.EnableDirectionalLighting = EnableDirectionalLighting ? 1.0f : 0.0f;
            lightingParams.EnableSpecularLighting = EnableSpecularLighting ? 1.0f : 0.0f;

            lightingParams.AmbientLightColor = AmbientLightColor.ToVector4();
            lightingParams.AmbientLightIntensity = AmbientLightIntensity;

            for (int i = 0; i < 8; i++)
            {
                lightingParams.UpdateDirectionalLight(i, 
                    DirectionalEnabled[i], DirectionalDirection[i], 
                    DirectionalColor[i], DirectionalIntensity[i]);
            }
        }
    } 
}
