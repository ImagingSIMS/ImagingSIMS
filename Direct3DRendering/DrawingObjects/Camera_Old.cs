using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SharpDX;
using SharpDX.DirectInput;
using SharpDX.Windows;

using Device = SharpDX.Direct3D11.Device;

namespace ImagingSIMS.Direct3DRendering.Cameras
{
    internal class FirstPersonCamera : IDisposable
    {
        bool _isInputInitialized;
        IntPtr _windowHandle;
        Device _device;
        double _cameraSpeed;
        double _cameraRotateSpeed;

        Vector3 _defaultPosition;
        Vector3 _defaultDirection;
        Vector3 _defaultUp;

        Vector3 _position;
        Vector3 _direction;
        Vector3 _up;

        Matrix _view;
        Matrix _projection;

        bool _isPaused;

        DirectInput _directInput;

        Keyboard _inputKeyboard;
        Mouse _inputMouse;

        KeyboardState _currentKeyState;
        KeyboardState _prevKeyState;
        MouseState _currentMouseState;
        MouseState _prevMouseState;

        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                if (_isPaused != value)
                {
                    _isPaused = value;
                }
            }
        }
        public Vector3 Position
        {
            get { return _position; }
        }
        public Vector3 Direction
        {
            get { return _direction; }
        }
        public Vector3 Up
        {
            get { return _up; }
        }

        public Matrix ViewMatrix { get { return _view; } }
        public Matrix ProjectionMatrix { get { return _projection; } }
        public Matrix WorldProjView
        {
            get
            {
                var viewProj = Matrix.Multiply(_view, GetProjectionMatrix());
                Matrix worldProjView = viewProj * Matrix.Identity;
                worldProjView.Transpose();
                return worldProjView;
            }
        }

        public FirstPersonCamera(IntPtr WindowHandle)
        {
            _windowHandle = WindowHandle;

            _defaultPosition = new Vector3();
            _defaultDirection = new Vector3();
            _defaultUp = new Vector3();

            _position = new Vector3();
            _direction = new Vector3();
            _up = new Vector3();

            _cameraSpeed = 0.0020d;
            _cameraRotateSpeed = 1500d;

            _isPaused = false;
        }

        ~FirstPersonCamera()
        {
            Dispose(false);
        }
        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool Disposing)
        {
            if (!_disposed)
            {
                if (Disposing)
                {
                    _inputKeyboard.Dispose();
                    _inputMouse.Dispose();
                    _directInput.Dispose();
                }

                _disposed = true;
            }
        }

        public void SetInitialConditions(Vector3 Position, Vector3 Target, Vector3 Up)
        {
            _defaultPosition = Position;
            _defaultDirection = Target - Position;
            _defaultDirection.Normalize();
            _defaultUp = Up;

            _position = Position;
            _direction = Target - Position;
            _direction.Normalize();
            _up = Up;

            // _projection = GetProjectionMatrix();
            _view = GetViewMatrix();
        }

        public void InitializeCamera(Device Device)
        {
            _device = Device;

            InitializeDirectInput();
        }
        public void ResizeProjectionMatrix()
        {
            _projection = GetProjectionMatrix();
        }

        private Matrix GetProjectionMatrix()
        {
            if (_device == null) return Matrix.Identity;
            ViewportF viewport = _device.ImmediateContext.Rasterizer.GetViewports<Viewport>()[0];
            return Matrix.PerspectiveFovLH((float)(Math.PI / 4f), viewport.Width / viewport.Height, 0.5f, 500.0f);
        }
        private Matrix GetViewMatrix()
        {
            Vector3 direction = _direction;
            direction.Normalize();

            Vector3 sum = (_position + direction);

            return Matrix.LookAtLH(_position, sum, _up);
        }

        private void InitializeDirectInput()
        {
            _directInput = new DirectInput();

            _inputKeyboard = new Keyboard(_directInput);
            _inputMouse = new Mouse(_directInput);

            _inputKeyboard.SetCooperativeLevel(_windowHandle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);
            _inputKeyboard.Acquire();

            _inputMouse.SetCooperativeLevel(_windowHandle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);
            _inputMouse.Acquire();

            _currentKeyState = _inputKeyboard.GetCurrentState();
            _currentMouseState = _inputMouse.GetCurrentState();

            _isInputInitialized = true;
        }

        public void UpdateCamera()
        {
            if (_isPaused) return;
            if (!_isInputInitialized)
            {
                try
                {
                    InitializeDirectInput();
                }
                catch (SharpDXException)
                {
                    return;
                }
            }

            _prevKeyState = _currentKeyState;
            _prevMouseState = _currentMouseState;

            try
            {
                _currentKeyState = _inputKeyboard.GetCurrentState();
                _currentMouseState = _inputMouse.GetCurrentState();
            }
            catch (SharpDXException)
            {
                _isInputInitialized = false;
                return;
            }

            if (_currentKeyState.PressedKeys.Count > 0)
            {
                //Move
                if (_currentKeyState.IsPressed(Key.W))
                {
                    MoveCamera(MoveDirection.Forward);
                }
                if (_currentKeyState.IsPressed(Key.A))
                {
                    MoveCamera(MoveDirection.Left);
                }
                if (_currentKeyState.IsPressed(Key.S))
                {
                    MoveCamera(MoveDirection.Backward);
                }
                if (_currentKeyState.IsPressed(Key.D))
                {
                    MoveCamera(MoveDirection.Right);
                }

                //Rotate
                if (_currentKeyState.IsPressed(Key.Left) && !_currentKeyState.IsPressed(Key.LeftShift))
                {
                    RotateCamera(RotateDirection.Left);
                }
                if (_currentKeyState.IsPressed(Key.Right) && !_currentKeyState.IsPressed(Key.LeftShift))
                {
                    RotateCamera(RotateDirection.Right);
                }
                if (_currentKeyState.IsPressed(Key.Up))
                {
                    RotateCamera(RotateDirection.Up);
                }
                if (_currentKeyState.IsPressed(Key.Down))
                {
                    RotateCamera(RotateDirection.Down);
                }
                if (_currentKeyState.IsPressed(Key.Left) && _currentKeyState.IsPressed(Key.LeftShift))
                {
                    RotateCamera(RotateDirection.CounterClockwise);
                }
                if (_currentKeyState.IsPressed(Key.Right) && _currentKeyState.IsPressed(Key.LeftShift))
                {
                    RotateCamera(RotateDirection.Clockwise);
                }

                //Misc
                if (_currentKeyState.IsPressed(Key.Space))
                {
                    ResetCamera();
                }
                if (_currentKeyState.IsPressed(Key.R))
                {
                    ReverseCamera();
                }
            }

            //Mouse
            if (IsLeftMouseDown(_currentMouseState) && IsLeftMouseDown(_prevMouseState))
            {
                int deltaX = _currentMouseState.X - _prevMouseState.X;
                int deltaY = _currentMouseState.Y - _prevMouseState.Y;


            }
        }
        private void DoMouseMovement(int DeltaX, int DeltaY)
        {
            float angleX = (float)DeltaX * .01f;
            float angleY = (float)DeltaY * .01f;
        }
        public void MoveCamera(MoveDirection Direction)
        {
            if (_isPaused) return;

            Vector3 cross;
            switch (Direction)
            {
                case MoveDirection.Left:
                    cross = Vector3.Cross(_up, _direction);
                    _position = _position - (cross * (float)_cameraSpeed);
                    break;
                case MoveDirection.Right:
                    cross = Vector3.Cross(_up, _direction);
                    _position = _position + (cross * (float)_cameraSpeed);
                    break;
                case MoveDirection.Forward:
                    _position = _position + (_direction * (float)_cameraSpeed);
                    break;
                case MoveDirection.Backward:
                    _position = _position - (_direction * (float)_cameraSpeed);
                    break;
            }

            _view = GetViewMatrix();
        }
        public void RotateCamera(RotateDirection Direction)
        {
            if (_isPaused) return;

            Matrix pitchMatrix;
            Matrix yawMatrix;
            Matrix rollMatrix;

            Vector3 cross;

            float angle = (float)(Math.PI / (4 * _cameraRotateSpeed));

            switch (Direction)
            {
                case RotateDirection.Down:
                    cross = Vector3.Cross(_up, _direction);
                    pitchMatrix = Matrix.RotationAxis(cross, angle);
                    _direction = Vector3.TransformCoordinate(_direction, pitchMatrix);
                    _up = Vector3.TransformCoordinate(_up, pitchMatrix);
                    break;
                case RotateDirection.Up:
                    cross = Vector3.Cross(_up, _direction);
                    pitchMatrix = Matrix.RotationAxis(cross, -angle);
                    _direction = Vector3.TransformCoordinate(_direction, pitchMatrix);
                    _up = Vector3.TransformCoordinate(_up, pitchMatrix);
                    break;
                case RotateDirection.Left:
                    yawMatrix = Matrix.RotationAxis(_up, -angle);
                    _direction = Vector3.TransformCoordinate(_direction, yawMatrix);
                    break;
                case RotateDirection.Right:
                    yawMatrix = Matrix.RotationAxis(_up, +angle);
                    _direction = Vector3.TransformCoordinate(_direction, yawMatrix);
                    break;
                case RotateDirection.Clockwise:
                    rollMatrix = Matrix.RotationAxis(_direction, -angle);
                    _up = Vector3.TransformCoordinate(_up, rollMatrix);
                    break;
                case RotateDirection.CounterClockwise:
                    rollMatrix = Matrix.RotationAxis(_direction, +angle);
                    _up = Vector3.TransformCoordinate(_up, rollMatrix);
                    break;
            }

            _view = GetViewMatrix();
        }
        public void ReverseCamera()
        {
            if (_isPaused) return;

            _direction = _direction * -1;

            _view = GetViewMatrix();
        }
        public void ResetCamera()
        {
            if (_isPaused) return;

            _position = _defaultPosition;
            _direction = _defaultDirection;
            _up = _defaultUp;

            _view = GetViewMatrix();
        }

        private bool IsLeftMouseDown(MouseState mouseState)
        {
            return mouseState.Buttons[0];
        }
        private bool IsRightMouseDown(MouseState mouseState)
        {
            return mouseState.Buttons[2];
        }
    }

    internal class Camera
    {
        Device _device;

        double _cameraSpeed;
        double _cameraRotateSpeed;

        Vector3 _defaultPosition;
        Vector3 _defaultDirection;
        Vector3 _defaultUp;
        Vector3 _defaultTarget;

        Vector3 _position;
        Vector3 _direction;
        Vector3 _up;
        Vector3 _target;

        public Vector3 Position
        {
            get { return _position; }
        }
        public Vector3 Direction
        {
            get { return _direction; }
        }
        public Vector3 Up
        {
            get { return _up; }
        }

        public Matrix WorldProjView
        {
            get
            {
                var viewProj = Matrix.Multiply(GetViewMatrix(), GetProjectionMatrix());
                Matrix worldProjView = viewProj * Matrix.Identity;
                worldProjView.Transpose();
                return worldProjView;
            }
        }

        public Camera(Device Device)
        {
            _device = Device;

            _cameraRotateSpeed = 0.01;
            _cameraSpeed = 0.05;

            _rotationMatrix = new Matrix();
        }
        public void SetInitialConditions(Vector3 Position, Vector3 Target, Vector3 Up)
        {
            _defaultPosition = Position;
            _defaultTarget = Target;
            _defaultDirection = Target - Position;
            _defaultDirection.Normalize();
            _defaultUp = Up;

            _position = Position;
            _target = Target;
            _direction = Target - Position;
            _direction.Normalize();
            _up = Up;
        }
        public void SetTarget(Vector3 Target)
        {
            _target = Target;
            _direction = Target - _position;
            _direction.Normalize();
        }
        public void SetPosition(Vector3 Position)
        {
            _position = Position;
        }

        private Matrix GetProjectionMatrix()
        {
            if (_device == null) return Matrix.Identity;
            ViewportF viewport = _device.ImmediateContext.Rasterizer.GetViewports<Viewport>()[0];
            return Matrix.PerspectiveFovLH((float)(Math.PI / 4f), viewport.Width / viewport.Height, 0.5f, 500.0f);
        }
        private Matrix GetViewMatrix()
        {
            Vector3 direction = _direction;
            direction.Normalize();

            Vector3 sum = (_position + direction);

            return Matrix.LookAtLH(_position, sum, _up);
        }

        Matrix _rotationMatrix;
        public void RotateCamera(Point Previous, Point New)
        {
            ViewportF viewport = _device.ImmediateContext.Rasterizer.GetViewports<Viewport>()[0];
            double displayWidth = viewport.Width;
            double displayHeight = viewport.Height;

            double dX = New.X - Previous.X;
            double dY = New.Y - Previous.Y;

            double angleX = (dX / displayWidth) * 360d * _cameraRotateSpeed;
            double angleY = -(dY / displayHeight) * 360d * _cameraRotateSpeed;

            float fDelatX = (Previous.X - New.X) * 0.9f / (float)displayWidth;
            float fDeltaY = (Previous.Y - New.Y) * 0.9f / (float)displayHeight;

            Matrix m_mTranslationDelta = Matrix.Translation(0.0f, 0.0f, 5.0f * fDeltaY);

            Vector3 initPos = -Position;
            _rotationMatrix = Matrix.RotationYawPitchRoll((float)angleX, (float)angleY, 0.0f);
            Vector4 t_newPos = Vector3.Transform(initPos, _rotationMatrix);
            Vector3 newPos = new Vector3(t_newPos.X, t_newPos.Y, t_newPos.Z);

            Quaternion quat = Quaternion.Identity;

            _position = -newPos;

            _direction = _target - _position;
            _direction.Normalize();
        }
        public void ResetCamera()
        {
            _direction = _defaultDirection;
            _position = _defaultPosition;
            _up = _defaultUp;
        }
        public void ReverseCamera()
        {
            _position *= -1;
            _direction = _target - _position;
            _direction.Normalize();
        }
        public void ZoomCamera(int Delta)
        {
            float length = (float)GetDistance(_position);
            float delta = Delta * (float)_cameraSpeed / 30f;
            float zoom = delta / length;

            float ratio = 1f - zoom;

            _position *= ratio;
            _direction = _target - _position;
            _direction.Normalize();
        }

        private double GetDistance(Vector3 Position)
        {
            double x = Position.X - _target.X;
            double y = Position.Y - _target.Y;
            double z = Position.Z - _target.Z;

            return Math.Sqrt((x * x) + (y * y) + (z * z));
        }
    }

    internal class _OrbitCamera
    {
        Device _device;
        RenderControl _renderControl;

        Matrix _world;
        Matrix _view;
        Matrix _proj;

        Vector3 _initialPosition;
        Vector3 _initalUp;

        Vector3 _position;
        Vector3 _up;

        D3DArcBall _worldArcBall;
        D3DArcBall _viewArcBall;

        Matrix _modelRot;
        Matrix _modelLastRot;
        Matrix _viewLastRot;

        private float Radius
        {
            get
            {
                return _position.Length();
            }
        }
        public Matrix WorldViewProj
        {
            get
            {
                Matrix worldProjView = Matrix.Multiply(_world, _proj);
                return Matrix.Multiply(worldProjView, _view);
            }
        }


        public _OrbitCamera(Device Device, RenderControl RenderControl)
        {
            _device = Device;
            _renderControl = RenderControl;

            _worldArcBall = new D3DArcBall(_renderControl);
            _viewArcBall = new D3DArcBall(_renderControl);

            _world = _view = _proj = Matrix.Identity;
            _modelRot = _modelLastRot = _viewLastRot = Matrix.Identity;
        }

        public void Initialize(Vector3 DefaultPosition, Vector3 DefaultUp)
        {
            _initialPosition = _position = DefaultPosition;
            _initalUp = _up = DefaultUp;
        }
        public void Reset()
        {
            _position = _initialPosition;
            _up = _initalUp;
        }

        public void RotateCamera(Point Previous, Point New)
        {
            Matrix cameraRot = Matrix.Invert(_viewArcBall.RotationMatrix);

            Vector3 localUp = Vector3.Up;
            Vector3 localAhead = Vector3.ForwardLH;
            Vector3 worldUp = Vector3.TransformCoordinate(localUp, cameraRot);
            Vector3 worldAhead = Vector3.TransformCoordinate(localAhead, cameraRot);

            _position = Vector3.Zero - worldAhead * Radius;
            _view = Matrix.LookAtLH(_position, Vector3.Zero, worldUp);

            Matrix invView = Matrix.Invert(_view);
            invView.M41 = invView.M42 = invView.M43 = 0;

            Matrix modelLastRotInv = Matrix.Invert(_modelLastRot);

            Matrix modelRot = _worldArcBall.RotationMatrix;
            _modelRot = _view * modelLastRotInv * modelRot * invView;


        }

        public void RotateCamera(double dx, double dy)
        {
            double mouseAngle = 0;
            if (dx != 0 && dy != 0)
            {
                mouseAngle = Math.Asin(Math.Abs(dy) / Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2)));

                if (dx < 0 && dy > 0) mouseAngle += Math.PI / 2;
                else if (dx < 0 && dy < 0) mouseAngle += Math.PI;
                else if (dx > 0 && dy < 0) mouseAngle += Math.PI * 1.5;
            }
            else if (dx == 0 && dy != 0) mouseAngle = Math.Sign(dy) > 0 ? Math.PI / 2 : Math.PI * 1.5;
            else if (dx != 0 && dy == 0) mouseAngle = Math.Sign(dx) > 0 ? 0 : Math.PI;

            double axisAngle = mouseAngle + Math.PI / 2;

            Vector3 axis = new Vector3((float)Math.Cos(axisAngle) * 4, (float)Math.Sin(axisAngle) * 4, 0);

            double rotation = 0.01 * Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            rotation *= 0.01d;

            Quaternion quatRot = Quaternion.RotationAxis(axis, (float)(rotation * 180 / Math.PI));
            Matrix matRot = Matrix.RotationQuaternion(quatRot);

            _position = Vector3.TransformCoordinate(_position, matRot);
            _up = Vector3.TransformCoordinate(_up, matRot);
        }
        public void ReverseCamera()
        {
            _position *= -1;
        }
        public void ZoomCamera(int Delta)
        {
            float length = Radius;
            float delta = Delta * 0.05f / 30f;
            float zoom = delta / length;

            float ratio = 1f - zoom;

            Vector3 oldPosition = _position;

            _position *= ratio;
            if (Radius < 0.5f)
            {
                _position = oldPosition;
            }
        }

        private Point WindowsToSharpDx(System.Windows.Point Point)
        {
            return new Point((int)Point.X, (int)Point.Y);
        }
        private System.Windows.Point SharpDXToWindow(SharpDX.Point Point)
        {
            return new System.Windows.Point(Point.X, Point.Y);
        }
    }
}
