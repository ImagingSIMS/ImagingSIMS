using System;

using SharpDX;
using SharpDX.Windows;

namespace ImagingSIMS.Direct3DRendering.Cameras
{
    public class D3DArcBall
    {
        RenderControl _renderControl;

        Matrix _rotation;
        Matrix _translation;
        Matrix _translationDelta;

        Vector2 _center;
        float _radius;
        float _radiusTranslation;

        Quaternion _quatDown;
        Quaternion _quatNow;

        bool _isDrag;
        //Point _mousePrevious;

        Vector3 _down;
        Vector3 _current;

        public float TranslationRadius
        {
            get { return _radiusTranslation; }
            set { _radiusTranslation = value; }
        }
        public Matrix RotationMatrix
        {
            get { return Matrix.RotationQuaternion(_quatNow); }
        }
        public Matrix TranslationMatrix
        {
            get { return _translation; }
        }
        public Matrix TranslationMatrixDelta
        {
            get { return _translationDelta; }
        }
        public Quaternion QuaternionNow
        {
            get { return _quatNow; }
            set { _quatNow = value; }
        }

        public D3DArcBall(RenderControl RenderControl)
        {
            Reset();

            _down = Vector3.Zero;
            _current = Vector3.Zero;

            _renderControl = RenderControl;
        }

        private void Reset()
        {
            _quatDown = Quaternion.Identity;
            _quatNow = Quaternion.Identity;
            _rotation = Matrix.Identity;
            _translation = Matrix.Identity;
            _translationDelta = Matrix.Identity;

            _radiusTranslation = 1.0f;
            _radius = 1.0f;
        }

        private Vector3 ScreenToVector(Point ScreenPoint)
        {
            float x = (float)ScreenPoint.X / (_radius * (float)_renderControl.Width / 2f);
            float y = (float)ScreenPoint.Y / (_radius * (float)_renderControl.Height / 2f);

            float z = 0f;
            float mag = (x * x) + (y * y);

            if (mag > 1.0f)
            {
                float scale = 1.0f / (float)Math.Sqrt(mag);
                x *= scale;
                y *= scale;
            }
            else z = (float)Math.Sqrt(1.0f - mag);

            return new Vector3(x, y, z);
        }
        private Vector3 ScreenToVector(float nX, float nY)
        {
            float x = nX / (_radius * (float)_renderControl.Width / 2f);
            float y = nY / (_radius * (float)_renderControl.Height / 2f);

            float z = 0f;
            float mag = (x * x) + (y * y);

            if (mag > 1.0f)
            {
                float scale = 1.0f / (float)Math.Sqrt(mag);
                x *= scale;
                y *= scale;
            }
            else z = (float)Math.Sqrt(1.0f - mag);

            return new Vector3(x, y, z);
        }
        private Quaternion QuatFromBallPoints(Vector3 From, Vector3 To)
        {
            float dot = Vector3.Dot(From, To);
            Vector3 part = Vector3.Cross(From, To);

            return new Quaternion(part, dot);
        }

        public void OnBegin(int nX, int nY)
        {
            _isDrag = true;
            _quatDown = _quatNow;
            _down = ScreenToVector((float)nX, (float)nY);
        }
        public void OnMove(int nX, int nY)
        {
            if (!_isDrag) return;
            _current = ScreenToVector(nX, nY);
            _quatNow = _quatDown * QuatFromBallPoints(_down, _current);
        }
        public void OnEnd()
        {
            _isDrag = false;
        }
    }

    //public class D3DBaseCamera
    //{
    //    protected RenderControl _renderControl;

    //    protected Matrix _view;
    //    protected Matrix _proj;

    //    protected Point _lastMousePosition;
    //    protected Vector2 _mouseDelta;
    //    protected float _framesToSmoothMouseData;

    //    protected Vector3 _defaultEye;
    //    protected Vector3 _defaultLookAt;
    //    protected Vector3 _eye;
    //    protected Vector3 _lookAt;
    //    protected float _yawAngle;
    //    protected float _pitchAngle;

    //    protected float _fov;
    //    protected float _aspect;
    //    protected float _nearPlane;
    //    protected float _farPlane;

    //    protected float _rotationScaler;
    //    protected float _moveScaler;

    //    protected bool _invertPitch;
    //    protected bool _enablePositionMovement;
    //    protected bool _enableYAxisMovement;

    //    protected bool _clipToBoundary;
    //    protected Vector3 _minBoundary;
    //    protected Vector3 _maxBoundary;

    //    public bool InvertPitch
    //    {
    //        get { return _invertPitch; }
    //        set { _invertPitch = value; }
    //    }
    //    public bool EnableYAxisMovement
    //    {
    //        get { return _enableYAxisMovement; }
    //        set { _enableYAxisMovement = value; }
    //    }
    //    public bool EnablePositionMovement
    //    {
    //        get { return _enablePositionMovement; }
    //        set { _enablePositionMovement = value; }
    //    }

    //    public int FramesToSmoothMouseData
    //    {
    //        set
    //        {
    //            if (value > 0) _framesToSmoothMouseData = value;
    //        }
    //    }

    //    public Matrix ViewMatrix
    //    {
    //        get { return _view; }
    //    }
    //    public Matrix ProjectionMatrix
    //    {
    //        get { return _proj; }
    //    }
    //    public Vector3 Eye
    //    {
    //        get { return _eye; }
    //    }
    //    public Vector3 LookAt
    //    {
    //        get { return _lookAt; }
    //    }

    //    public bool ClipToBoundary
    //    {
    //        get { return _clipToBoundary; }
    //        set { _clipToBoundary = value; }
    //    }
    //    public float NearClip
    //    {
    //        get { return _nearPlane; }
    //        set { _nearPlane = value; }
    //    }
    //    public float FarClip
    //    {
    //        get { return _farPlane; }
    //        set { _farPlane = value; }
    //    }

    //    public float RotataionScaler
    //    {
    //        get { return _rotationScaler; }
    //        set { _rotationScaler = value; }
    //    }
    //    public float MoveScaler
    //    {
    //        get { return _moveScaler; }
    //        set { _moveScaler = value; }
    //    }

    //    public D3DBaseCamera(RenderControl RenderControl)
    //    {
    //        _renderControl = RenderControl;

    //        _eye = Vector3.Zero;
    //        _lookAt = Vector3.Zero;

    //        SetViewParams(_eye, _lookAt);
    //        SetProjParams(MathUtil.PiOverFour, 1.0f, 1.0f, 1000.0f);

    //        _lastMousePosition = new Point();

    //        _yawAngle = 0.0f;
    //        _pitchAngle = 0.0f;

    //        _rotationScaler = 0.01f;
    //        _moveScaler = 5.0f;

    //        _invertPitch = false;
    //        _enableYAxisMovement = true;
    //        _enablePositionMovement = true;

    //        _mouseDelta = Vector2.Zero;
    //        _framesToSmoothMouseData = 2.0f;

    //        _clipToBoundary = false;
    //        _minBoundary = new Vector3(-1.0f);
    //        _maxBoundary = new Vector3(1.0f);
    //    }

    //    public virtual void SetViewParams(Vector3 EyePt, Vector3 LookAtPt)
    //    {
    //        _defaultEye = _eye = EyePt;
    //        _defaultLookAt = _lookAt = LookAtPt;

    //        Vector3 up = new Vector3(0, 1, 0);
    //        _view = Matrix.LookAtLH(EyePt, LookAtPt, up);

    //        Matrix invView = Matrix.Invert(_view);

    //        Vector3 zBasis = new Vector3(invView.M31, invView.M32, invView.M33);

    //        _yawAngle = (float)Math.Atan2(zBasis.X, zBasis.Z);
    //        float len = (float)Math.Sqrt((zBasis.Z * zBasis.Z) + (zBasis.X * zBasis.X));
    //        _pitchAngle = -(float)Math.Atan2(zBasis.Y, len);
    //    }
    //    public virtual void SetProjParams(float FOV, float Aspect, float NearPlane, float FarPlane)
    //    {
    //        _fov = FOV;
    //        _aspect = Aspect;
    //        _nearPlane = NearPlane;
    //        _farPlane = FarPlane;

    //        _proj = Matrix.PerspectiveFovLH(_fov, _aspect, _nearPlane, _farPlane);
    //    }
    //    public virtual void FrameMove(float ElapsedTime)
    //    {

    //    }
    //    public virtual void Reset()
    //    {
    //        SetViewParams(_defaultEye, _defaultLookAt);
    //    }
    //}
    //public class D3DOrbitCamera : D3DBaseCamera
    //{
    //    protected D3DArcBall _worldArcBall;
    //    protected D3DArcBall _viewArcBall;
    //    protected Vector3 _modelCenter;
    //    protected Matrix _modelLastRot;
    //    protected Matrix _modelRot;
    //    protected Matrix _world;

    //    protected bool _attachCameraToModel;
    //    protected bool _limitPitch;
    //    protected float _radius;
    //    protected float _defaultRadius;
    //    protected float _minRadius;
    //    protected float _maxRadius;
    //    protected bool _dragSinceLastUpdate;

    //    protected Matrix _cameraRotLast;

    //    public bool AttachCameraToModel
    //    {
    //        get { return _attachCameraToModel; }
    //        set { _attachCameraToModel = value; }
    //    }
    //    public float DefaultRadius
    //    {
    //        get { return _defaultRadius; }
    //        set { _defaultRadius = value; }
    //    }
    //    public float Radius
    //    {
    //        get { return _radius; }
    //        set { _radius = value; }
    //    }
    //    public float MinRadius
    //    {
    //        get { return _minRadius; }
    //        set { _minRadius = value; }
    //    }
    //    public float MaxRadius
    //    {
    //        get { return _maxRadius; }
    //        set { _maxRadius = value; }
    //    }
    //    public Vector3 ModelCenter
    //    {
    //        get { return _modelCenter; }
    //        set { _modelCenter = value; }
    //    }
    //    public bool LimitPitch
    //    {
    //        get { return _limitPitch; }
    //        set { _limitPitch = value; }
    //    }
    //    public Quaternion ViewQuaternion
    //    {
    //        get { return _viewArcBall.QuaternionNow; }
    //        set { _viewArcBall.QuaternionNow = value; }
    //    }
    //    public Quaternion WorldQuaternion
    //    {
    //        get { return _worldArcBall.QuaternionNow; }
    //        set { _worldArcBall.QuaternionNow = value; }
    //    }
    //    public Matrix WorldMatrix
    //    {
    //        get { return _world; }
    //        set { _world = value; }
    //    }
        
    //    public D3DOrbitCamera(RenderControl RenderControl)
    //        : base(RenderControl)
    //    {
    //        _world = Matrix.Identity;
    //        _modelRot = Matrix.Identity;
    //        _modelLastRot = Matrix.Identity;
    //        _cameraRotLast = Matrix.Identity;

    //        _modelCenter = Vector3.Zero;

    //        _radius = 5.0f;
    //        _defaultRadius = 5.0f;
    //        _minRadius = 1.0f;
    //        _maxRadius = float.MaxValue;

    //        _limitPitch = false;
    //        _attachCameraToModel = false;
    //    }

    //    public override void SetViewParams(Vector3 EyePt, Vector3 LookAtPt)
    //    {
    //        base.SetViewParams(EyePt, LookAtPt);

    //        Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);

    //        Matrix rotation = Matrix.LookAtLH(_eye, _lookAt, up);
    //        Quaternion quat = Quaternion.RotationMatrix(rotation);

    //        _viewArcBall.QuaternionNow = quat;

    //        Vector3 eyeToPoint = Vector3.Subtract(_lookAt, _eye);
    //        _radius = eyeToPoint.Length();
    //    }
    //    public virtual void FrameMove(float ElapsedTime)
    //    {

    //    }

    //    public void BeginDrag(Point MousePoint)
    //    {
    //        _worldArcBall.OnBegin(MousePoint.X, MousePoint.Y);
    //        _viewArcBall.OnBegin(MousePoint.X, MousePoint.Y);
    //    }
    //    public void DoDrag(Point MousePoint)
    //    {
    //        _worldArcBall.OnMove(MousePoint.X, MousePoint.Y);
    //        _viewArcBall.OnMove(MousePoint.X, MousePoint.Y);
    //    }
    //    public void StopDrag(Point MousePoint)
    //    {
    //        _worldArcBall.OnEnd();
    //        _viewArcBall.OnEnd();
    //    }

    //    public void Update(int Clicks)
    //    {
    //        Matrix cameraRot = Matrix.Invert(_viewArcBall.RotationMatrix);

    //        Vector3 localUp = Vector3.Up;
    //        Vector3 localAhead = Vector3.ForwardLH;
    //        Vector3 worldUp = Vector3.TransformCoordinate(localUp, cameraRot);
    //        Vector3 worldAhead = Vector3.TransformCoordinate(localAhead, cameraRot);

            
    //    }
    //}
}
