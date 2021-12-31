using System;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Windows;
using ImagingSIMS.Direct3DRendering.ViewModels;
using BoundingBox = ImagingSIMS.Direct3DRendering.SceneObjects.BoundingBox;
using ImagingSIMS.Direct3DRendering.Controls;
using ImagingSIMS.Direct3DRendering.Cameras;
using ImagingSIMS.Direct3DRendering.SceneObjects;
using System.Windows.Media.Imaging;

namespace ImagingSIMS.Direct3DRendering.Renderers
{
    public abstract class Renderer : IDisposable
    {
        protected bool _initializeRendererIsRunning;
        public bool IsBusy
        {
            get { return _initializeRendererIsRunning; }
        }

        protected RenderType _renderType = RenderType.NotSpecified;

        protected FeatureLevel _featureLevel;
        protected RenderWindow _dataContextRenderWindow;
        protected RenderControl _parent;
        protected Device _device;

        protected SwapChain _swapChain;

        protected OrbitCamera _orbitCamera;

        protected Texture2D _backBuffer;
        protected Texture2D _depthBuffer;
        protected RenderTargetView _renderView;
        protected DepthStencilView _depthView;

        protected Axes _axes;
        protected BoundingBox _boundingBox;
        protected CoordinateBox _coordinateBox;

        protected RenderParams _renderParams;
        protected LightingParams _lightingParams;
        protected ModelParams _modelParams;
        protected Buffer _bufferRenderParams;
        protected Buffer _bufferLightingParams;
        protected Buffer _bufferModelParams;

        protected bool _needsResize;
        protected bool _dataLoaded;
        protected const float _clipDistance = 1.0f;

        protected Plane _nearClippingPlane;
        protected Plane _farClippingPlane;

        protected RasterizerState _rasterizerStateCullNone;
        protected RasterizerState _rasterizerStateCullFront;
        protected RasterizerState _rasterizerStateCullBack;

        protected RenderModelDescription _renderModelDescription;

        protected RenderingViewModel RenderingViewModel
        {
            get { return _dataContextRenderWindow.RenderWindowView; }
        }
        protected bool ShowAxes
        {
            get
            {
                if (_dataContextRenderWindow != null)
                {
                    return _dataContextRenderWindow.RenderWindowView.ShowAxes;
                }
                return false;
            }
        }
        protected bool ShowBoundingBox
        {
            get
            {
                if (_dataContextRenderWindow != null)
                {
                    return _dataContextRenderWindow.RenderWindowView.ShowBoundingBox;
                }
                return false;
            }
        }
        protected bool ShowCoordinateBox
        {
            get
            {
                if (_dataContextRenderWindow != null)
                {
                    return _dataContextRenderWindow.RenderWindowView.ShowCoordinateBox;
                }
                return false;
            }
        }
        protected float CoordinateBoxTransparency
        {
            get
            {
                if (_dataContextRenderWindow != null)
                {
                    return _dataContextRenderWindow.RenderWindowView.CoordinateBoxTransparency;
                }
                return 0;
            }
        }
        protected Color BackColor
        {
            get
            {
                if (_dataContextRenderWindow != null)
                {
                    return _dataContextRenderWindow.RenderWindowView.BackColor;
                }
                return Color.Black;
            }
        }
        protected Vector4 MinClipCoords
        {
            get
            {
                if(_dataContextRenderWindow != null)
                {
                    return new Vector4()
                    {
                        X = _dataContextRenderWindow.RenderWindowView.BoundingBoxLowerX,
                        Y = _dataContextRenderWindow.RenderWindowView.BoundingBoxLowerY,
                        Z = _dataContextRenderWindow.RenderWindowView.BoundingBoxLowerZ,
                        W = 1.0f
                    };
                }
                return Vector4.Zero;
            }
        }
        protected Vector4 MaxClipCoords
        {
            get
            {
                if (_dataContextRenderWindow != null)
                {
                    return new Vector4()
                    {
                        X = _dataContextRenderWindow.RenderWindowView.BoundingBoxUpperX,
                        Y = _dataContextRenderWindow.RenderWindowView.BoundingBoxUpperY,
                        Z = _dataContextRenderWindow.RenderWindowView.BoundingBoxUpperZ,
                        W = 1.0f
                    };
                }
                return Vector4.Zero;
            }
        }
        protected bool EnableDepthBuffering
        {
            get { return _dataContextRenderWindow.RenderWindowView.EnableDepthBuffering; }
        }

        public RenderType RenderType
        {
            get { return _renderType; }
        }

        public bool NeedsResize
        {
            get { return _needsResize; }
            set { _needsResize = value; }
        }
        public OrbitCamera Camera
        {
            get { return _orbitCamera; }
        }

        public bool DataLoaded
        {
            get { return _dataLoaded; }
        }

        public Device Device
        {
            get { return _device; }
        }
        public Texture2D BackBuffer
        {
            get { return _backBuffer; }
        }
        public byte[] GetCurrentCapture(out int Width, out int Height)
        {
            Width = 0;
            Height = 0;

            if (NeedsResize) return null;

            var texCopy = new Texture2D(_device, new Texture2DDescription()
            {
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                Format = _backBuffer.Description.Format,
                OptionFlags = ResourceOptionFlags.None,
                ArraySize = _backBuffer.Description.ArraySize,
                Height = _backBuffer.Description.Height,
                Width = _backBuffer.Description.Width,
                MipLevels = 1,
                SampleDescription = _backBuffer.Description.SampleDescription
            });
            _device.ImmediateContext.CopyResource(_backBuffer, texCopy);

            try
            {         
                var mapSource = _device.ImmediateContext.MapSubresource(texCopy, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);

                int rowPitch = mapSource.RowPitch;
                int slicePitch = mapSource.SlicePitch;

                Width = rowPitch / 4;
                Height = slicePitch / rowPitch;

                int bufferSize = Width * Height * 4;

                byte[] buffer = new byte[bufferSize];
                Marshal.Copy(mapSource.DataPointer, buffer, 0, bufferSize);                          

                return buffer;
            }
            catch(SharpDXException SDXex)
            {
                throw SDXex;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                _device.ImmediateContext.UnmapSubresource(texCopy, 0);
                Disposer.RemoveAndDispose(ref texCopy);
            }
        }

        public Renderer(RenderWindow Window)
        {
            _renderModelDescription = new RenderModelDescription();

            _dataContextRenderWindow = Window;
            _parent = Window.RenderControl;

            BoundingBox.OnSetBoundingBoxVertices += BoundingBox_OnSetBoundingBoxVertices;

            _dataContextRenderWindow.WindowActivatedChanged += _dataContextRenderWindow_WindowActivatedChanged;
        }

        private void BoundingBox_OnSetBoundingBoxVertices(object sender, SetBoundingBoxVerticesEventArgs e)
        {
            // Check to see if this call needs the dispatcher

            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    setInitialBoundingBoxVertices(e);
                });
            }
            else setInitialBoundingBoxVertices(e);
        }
        private void setInitialBoundingBoxVertices(SetBoundingBoxVerticesEventArgs e)
        {
            _dataContextRenderWindow.RenderWindowView.BoundingBoxMinX = e.Minima.X;
            _dataContextRenderWindow.RenderWindowView.BoundingBoxMaxX = e.Maximia.X;
            _dataContextRenderWindow.RenderWindowView.BoundingBoxLowerX = e.Minima.X;
            _dataContextRenderWindow.RenderWindowView.BoundingBoxUpperX = e.Maximia.X;

            _dataContextRenderWindow.RenderWindowView.BoundingBoxMinY = e.Minima.Y;
            _dataContextRenderWindow.RenderWindowView.BoundingBoxMaxY = e.Maximia.Y;
            _dataContextRenderWindow.RenderWindowView.BoundingBoxLowerY = e.Minima.Y;
            _dataContextRenderWindow.RenderWindowView.BoundingBoxUpperY = e.Maximia.Y;

            _dataContextRenderWindow.RenderWindowView.BoundingBoxMinZ = e.Minima.Z;
            _dataContextRenderWindow.RenderWindowView.BoundingBoxMaxZ = e.Maximia.Z;
            _dataContextRenderWindow.RenderWindowView.BoundingBoxLowerZ = e.Minima.Z;
            _dataContextRenderWindow.RenderWindowView.BoundingBoxUpperZ = e.Maximia.Z;
        }

        private void _dataContextRenderWindow_WindowActivatedChanged(object sender, WindowActivatedChangedEventArgs e)
        {
            if (_orbitCamera == null)
                return;

            if (e.IsWindowActivated)
            {
                _orbitCamera.AcquireInput();
            }
            else _orbitCamera.UnacquireInput();
        }
        public void EnsureInputAcquired()
        {            
            if (_orbitCamera == null) return;

            if (_orbitCamera.IsInputAcquired) return;

            _orbitCamera.AcquireInput();
        }

        public virtual void InitializeRenderer()
        {
            // Thread safe method to access window handle. This allows the function calling this
            // to be run asynchronously.
            IntPtr parentHandle = IntPtr.Zero;
            IntPtr windowHandle = IntPtr.Zero;
            _parent.Invoke(new System.Windows.Forms.MethodInvoker(delegate { parentHandle = _parent.Handle; }));
            _dataContextRenderWindow.Dispatcher.Invoke(
                () => windowHandle = new System.Windows.Interop.WindowInteropHelper(_dataContextRenderWindow).Handle);

            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(_parent.ClientSize.Width, _parent.ClientSize.Height,
                    new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = parentHandle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput,
            };

            DeviceCreationFlags dcFlags = DeviceCreationFlags.None;

#if DEBUG_DEVICE
            dcFlags = DeviceCreationFlags.Debug;
#endif
            Device.CreateWithSwapChain(DriverType.Hardware, dcFlags,
                desc, out _device, out _swapChain);

            var context = _device.ImmediateContext;
            var factory = _swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(parentHandle, WindowAssociationFlags.IgnoreAll);

            _featureLevel = _device.FeatureLevel;            

            _orbitCamera = new OrbitCamera(_device, _parent, windowHandle);

            // Set camera position so that (+x, +y, +z) of the volume is the top of the rendering
            _orbitCamera.SetInitialConditions(new Vector3(-3f, -3f, 2f), new Vector3(0.6f, 0.6f, 0.4f));

            _renderParams = new RenderParams()
            {
                WorldProjView = Matrix.Identity,
                CameraDirection = _orbitCamera.Direction,
                CameraUp = _orbitCamera.Up,
                CameraPositon = _orbitCamera.Position
            };
            _bufferRenderParams = new Buffer(_device, Marshal.SizeOf(typeof(RenderParams)), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            _lightingParams = new LightingParams()
            {

            };
            _bufferLightingParams = new Buffer(_device, Marshal.SizeOf(typeof(LightingParams)), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            _modelParams = new ModelParams()
            {
            };
            _bufferModelParams = new Buffer(_device, Marshal.SizeOf(typeof(ModelParams)), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            _needsResize = true;

            _axes = new Axes(_device);

            InitializeShaders();
            InitializeStates();
        }
        protected virtual void InitializeShaders()
        {

        }
        protected virtual void InitializeStates()
        {
            BlendStateDescription descBlend = new BlendStateDescription();
            descBlend = BlendStateDescription.Default();

            descBlend.RenderTarget[0].IsBlendEnabled = true;

            descBlend.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
            descBlend.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
            descBlend.RenderTarget[0].BlendOperation = BlendOperation.Add;

            descBlend.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
            descBlend.RenderTarget[0].DestinationAlphaBlend = BlendOption.InverseSourceAlpha;
            descBlend.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;

            descBlend.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

            BlendState state = new BlendState(_device, descBlend);
            _device.ImmediateContext.OutputMerger.SetBlendState(state);

            _rasterizerStateCullNone = new RasterizerState(_device, new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                IsDepthClipEnabled = true
            });
            _rasterizerStateCullBack = new RasterizerState(_device, new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Back,
                IsDepthClipEnabled = true
            });
            _rasterizerStateCullFront = new RasterizerState(_device, new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.Front,
                IsDepthClipEnabled = true
            });

            _device.ImmediateContext.Rasterizer.State = _rasterizerStateCullNone;
        }

        protected virtual bool Resize()
        {
            if (_device == null || _swapChain == null)
            {
                return false;
            }

            var context = _device.ImmediateContext;

            Disposer.RemoveAndDispose(ref _backBuffer);
            Disposer.RemoveAndDispose(ref _renderView);
            Disposer.RemoveAndDispose(ref _depthBuffer);
            Disposer.RemoveAndDispose(ref _depthView);

            SwapChainDescription desc = _swapChain.Description;
            _swapChain.ResizeBuffers(desc.BufferCount, _parent.ClientSize.Width,
                _parent.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

            _backBuffer = Texture2D.FromSwapChain<Texture2D>(_swapChain, 0);
            _renderView = new RenderTargetView(_device, _backBuffer);

            _depthBuffer = new Texture2D(_device, new Texture2DDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = _parent.ClientSize.Width,
                Height = _parent.ClientSize.Height,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });
            DepthStencilViewDescription descDepthStencil = new DepthStencilViewDescription()
            {
                Format = Format.D32_Float_S8X24_UInt,
                Dimension = DepthStencilViewDimension.Texture2DMultisampled
            };
            _depthView = new DepthStencilView(_device, _depthBuffer, descDepthStencil);

            //DepthStencilStateDescription descDepthStencilState = DepthStencilStateDescription.Default();
            DepthStencilStateDescription descDepthStencilState = new DepthStencilStateDescription()
            {
                IsDepthEnabled = true,
                DepthWriteMask = DepthWriteMask.All,
                DepthComparison = Comparison.Less,
                IsStencilEnabled = true,
                StencilReadMask = 0xff,
                StencilWriteMask = 0xff,
                FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                },
                BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                }
            };
            DepthStencilState stateDepthStencial = new DepthStencilState(_device, descDepthStencilState);
            context.OutputMerger.SetDepthStencilState(stateDepthStencial);

            context.Rasterizer.SetViewport(new Viewport(0, 0, _parent.ClientSize.Width, _parent.ClientSize.Height));
            context.OutputMerger.SetTargets(_depthView, _renderView);

            return true;
        }

        public virtual void Update(bool targetYAxisOrbiting)
        {
            if (_needsResize)
            {
                if (Resize()) _needsResize = false;
            }

            if (_dataContextRenderWindow.IsWindowActivated)
            {
                _orbitCamera.UpdateCamera(targetYAxisOrbiting);
            }            

            Matrix worldVewProj = _orbitCamera.WorldProjView;
            worldVewProj.Transpose();

            // Update render params
            _renderParams.WorldProjView = worldVewProj;

            _renderParams.CameraDirection = _orbitCamera.Direction;
            _renderParams.CameraPositon = _orbitCamera.Position;
            _renderParams.CameraUp = _orbitCamera.Up;

            _renderParams.InvWindowSize = new Vector2(1f / _parent.ClientSize.Width, 1f / _parent.ClientSize.Height);

            // Update lighting params
            RenderingViewModel.UpdateLightingParameters(ref _lightingParams);

            // Update model params
            _modelParams = (ModelParams)_renderModelDescription;

            var context = _device.ImmediateContext;
            context.UpdateSubresource(ref _renderParams, _bufferRenderParams);
            context.UpdateSubresource(ref _lightingParams, _bufferLightingParams);
            context.UpdateSubresource(ref _modelParams, _bufferModelParams);
        }
        public virtual void Draw()
        {
            if (_device == null) return;

            var context = _device.ImmediateContext;            

            // Reset render targets to inlcude depth and render
            context.OutputMerger.SetRenderTargets(_depthView, _renderView);
            context.ClearDepthStencilView(_depthView, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, _clipDistance, 0);
            context.ClearRenderTargetView(_renderView, BackColor);

            context.Rasterizer.State = _rasterizerStateCullNone;

            if (_axes != null && ShowAxes)
            {
                _axes.Update(_renderParams.WorldProjView);
                _axes.Draw();
            }
            if (_boundingBox != null && ShowBoundingBox)
            {
                _boundingBox.Update(_renderParams.WorldProjView);
                _boundingBox.Draw();
            }
        }

        protected void CompleteDraw()
        {
            if (_coordinateBox != null && ShowCoordinateBox)
            {
                var context = _device.ImmediateContext;

                context.Rasterizer.State = _rasterizerStateCullNone;

                // Reset render targets to inlcude depth and render
                context.OutputMerger.SetRenderTargets(_depthView, _renderView);

                _coordinateBox.Update(_renderParams.WorldProjView, CoordinateBoxTransparency);
                _coordinateBox.Draw();
            }

            _swapChain.Present(0, PresentFlags.None);
        }

        #region IDisposable
        ~Renderer()
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

                }
                Disposer.RemoveAndDispose(ref _orbitCamera);

                Disposer.RemoveAndDispose(ref _backBuffer);
                Disposer.RemoveAndDispose(ref _renderView);
                Disposer.RemoveAndDispose(ref _depthBuffer);
                Disposer.RemoveAndDispose(ref _depthView);

                Disposer.RemoveAndDispose(ref _device);
                Disposer.RemoveAndDispose(ref _swapChain);

                Disposer.RemoveAndDispose(ref _bufferRenderParams);
                Disposer.RemoveAndDispose(ref _bufferLightingParams);
                Disposer.RemoveAndDispose(ref _bufferModelParams);

                Disposer.RemoveAndDispose(ref _rasterizerStateCullNone);
                Disposer.RemoveAndDispose(ref _rasterizerStateCullBack);
                Disposer.RemoveAndDispose(ref _rasterizerStateCullFront);

                _disposed = true;
            }
        }
        #endregion
    }
}
