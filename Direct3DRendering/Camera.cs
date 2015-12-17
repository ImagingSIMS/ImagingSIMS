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

namespace Direct3DRendering
{
    public class OrbitCamera
    {
        Device _device;
        RenderControl _renderControl;
        IntPtr _windowHandle;

        Keyboard _keyboard;
        Mouse _mouse;

        Vector3 _target;
        Vector3 _up;
        Vector3 _eye;
        Vector3 _defaultEye;
        Vector3 _defaultUp;

        bool _inputAcquired;

        public float Radius
        {
            get
            {
                Vector3 v = _eye - _target;
                return v.Length();
            }
        }

        public Vector3 Position
        {
            get { return _eye; }
        }
        public Vector3 Direction
        {
            get
            {
                Vector3 direction = _target - _eye;
                direction.Normalize();
                return direction;
            }
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

        public OrbitCamera(Device Device, RenderControl RenderControl, IntPtr windowHandle)
        {
            _device = Device;
            _renderControl = RenderControl;
            _windowHandle = windowHandle;

            _target = Vector3.Zero;
        }

        public void SetInitialConditions(Vector3 EyePoint, Vector3 Up)
        {
            _eye = _defaultEye = EyePoint;
            _up = _defaultUp = Up;
        }

        private void acquireInput()
        {
            var directInput = new DirectInput();

            _keyboard = new Keyboard(directInput);
            _keyboard.SetCooperativeLevel(_windowHandle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);
            _keyboard.Acquire();

            _mouse = new Mouse(directInput);
            _mouse.SetCooperativeLevel(_windowHandle, CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);
            _mouse.Acquire();

            _inputAcquired = true;
        }

        const int _updateCounterLimit = 5;
        int _inputUpdateCounter;
        bool _recentlyPressed;

        const double _rotateSpeed = 0.5d;
        const double _zoomSpeed = 0.01d;

        public void UpdateCamera(bool targetYAxisOrbiting)
        {
            if (!_inputAcquired)
            {
                try
                {
                    acquireInput();
                }
                catch (Exception)
                {
                    return;
                }
            }

            KeyboardState keyboardState;
            MouseState mouseState;
            try
            {
                keyboardState = _keyboard.GetCurrentState();
                mouseState = _mouse.GetCurrentState();
            }
            catch (Exception)
            {
                _inputAcquired = false;
                return;
            }

            double dx = 0;
            double dy = 0;
            double zoom = 0;

           
            if (keyboardState.IsPressed(Key.W))
            {
                dy = -_rotateSpeed;
            }
            if (keyboardState.IsPressed(Key.S))
            {
                dy = _rotateSpeed;
            }
            if (keyboardState.IsPressed(Key.A))
            {
                dx = -_rotateSpeed;
            }
            if (keyboardState.IsPressed(Key.D))
            {
                dx = _rotateSpeed;
            }
            if (keyboardState.IsPressed(Key.PageUp))
            {
                zoom = _zoomSpeed;
            }
            if (keyboardState.IsPressed(Key.PageDown))
            {
                zoom = -_zoomSpeed;
            }

            if (keyboardState.IsPressed(Key.Space))
            {
                if (!_recentlyPressed)
                {
                    ResetCamera();
                    _recentlyPressed = true;
                }
            }
            if (keyboardState.IsPressed(Key.B))
            {
                if (!_recentlyPressed)
                {
                    ReverseCamera();
                    _recentlyPressed = true;
                }
            }

            // Check for left button pressed
            if (mouseState.Buttons[0] == true)
            {
                dx = mouseState.X * _rotateSpeed;
                dy = mouseState.Y * _rotateSpeed;
            }

            if (mouseState.Z != 0)
            {
                zoom = mouseState.Z * _zoomSpeed / 20f;
            }

            if (dx != 0 || dy != 0)
            {
                rotateCamera(dx, dy, targetYAxisOrbiting);
            }

            //System.Diagnostics.Trace.WriteLineIf((dx != 0 || dy != 0 || zoom != 0), 
            //    string.Format("dX: {0} dY: {1} dZ: {2}", dx, dy, zoom));
            if (zoom != 0)
            {
                zoomCamera((float)zoom);
            }

            if (_recentlyPressed)
            {
                _inputUpdateCounter++;
                if (_inputUpdateCounter >= _updateCounterLimit)
                {
                    _recentlyPressed = false;
                    _inputUpdateCounter = 0;
                }
            }
        }
        public void ResetCamera()
        {
            resetCamera();
        }
        public void ReverseCamera()
        {
            reverseCamera();
        }

        Quaternion rot;
        Quaternion ortn;
        private void rotateCamera(double dx, double dy, bool TargetYAxisOrbiting)
        {
            var worldProjView = WorldProjView;

            float heading = D3DXToRadian((float)dx * 0.5f);
            float pitch = D3DXToRadian(-(float)dy * 0.5f);

            if (TargetYAxisOrbiting)
            {
                if (heading != 0.0f)
                {
                    rot = Quaternion.RotationAxis(Up, heading);
                    ortn = Quaternion.Multiply(rot, ortn);
                }
                if (pitch != 0.0f)
                {
                    Vector3 worldX = Vector3.Cross(Up, Direction);
                    rot = Quaternion.RotationAxis(worldX, pitch);
                    ortn = Quaternion.Multiply(ortn, rot);
                }
            }
            else
            {
                float roll = D3DXToRadian(0.0f);

                rot = Quaternion.RotationYawPitchRoll(heading, pitch, roll);
                ortn = Quaternion.Multiply(ortn, rot);
            }

            Matrix matRot = Matrix.RotationQuaternion(rot);

            _eye = Vector3.TransformCoordinate(_eye, matRot);
            _up = Vector3.TransformCoordinate(_up, matRot);
        }
        private void resetCamera()
        {
            _eye = _defaultEye;
            _up = _defaultUp;
        }
        private void reverseCamera()
        {
            _eye *= -1;
        }
        private void zoomCamera(float delta)
        {
            float length = Radius;
            float zoom = delta * 1.5f / length;

            float ratio = 1f - zoom;

            _eye *= ratio;
        }

        public Vector3 GetClipPoint(float ClipDistance)
        {
            Vector3 traverseVector = Direction * ClipDistance;
            return Position + traverseVector;
        }

        private Matrix GetProjectionMatrix()
        {
            if (_device == null) return Matrix.Identity;
            ViewportF viewport = _device.ImmediateContext.Rasterizer.GetViewports()[0];
            return Matrix.PerspectiveFovLH((float)(Math.PI / 4f), viewport.Width / viewport.Height, 0.5f, 500.0f);
        }
        private Matrix GetViewMatrix()
        {
            Vector3 sum = (_eye + Direction);

            return Matrix.LookAtLH(_eye, sum, _up);
        }

        private Vector3 ScreenToVector(float nX, float nY)
        {
            float x = nX / (Radius * (float)_renderControl.Width / 2f);
            float y = nY / (Radius * (float)_renderControl.Height / 2f);

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

        private float D3DXToRadian(float degree)
        {
            return (float)(degree * Math.PI / 180.0f);
        }
    }
}