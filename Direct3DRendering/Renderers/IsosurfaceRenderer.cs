using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ImagingSIMS.Direct3DRendering.Controls;
using ImagingSIMS.Direct3DRendering.DrawingObjects;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using BoundingBox = ImagingSIMS.Direct3DRendering.SceneObjects.BoundingBox;
using ImagingSIMS.Direct3DRendering.SceneObjects;
using System.Linq;
using System.Windows;

namespace ImagingSIMS.Direct3DRendering.Renderers
{
    public class IsosurfaceRenderer : Renderer
    {
        VertexShader _vsIsosurface;
        InputLayout _ilIsosurface;
        PixelShader _psIsosurface;
        GeometryShader _gsIsosurface;

        Buffer _vertexBufferTriangles;

        IsosurfaceParams _isosurfaceParams;
        Buffer _isosurfaceParamBuffer;

        string _isosurfaceEffectPath;

        int _numVertices;

        public IsosurfaceRenderer(RenderWindow Window)
            : base(Window)
        {
            _renderType = RenderType.Isosurface;
        }

        public RenderParams RenderParams
        {
            get { return _renderParams; }
            set { _renderParams = value; }
        }
        public IsosurfaceParams IsosurfaceParams
        {
            get { return _isosurfaceParams; }
            set { _isosurfaceParams = value; }
        }

        #region IDisposable
        ~IsosurfaceRenderer()
        {
            Dispose(false);
        }
        private bool _disposed = false;
        new public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected override void Dispose(bool Disposing)
        {
            if (!_disposed)
            {
                if (Disposing)
                {

                }
                Disposer.RemoveAndDispose(ref _vsIsosurface);
                Disposer.RemoveAndDispose(ref _ilIsosurface);
                Disposer.RemoveAndDispose(ref _psIsosurface);

                Disposer.RemoveAndDispose(ref _vertexBufferTriangles);

                Disposer.RemoveAndDispose(ref _isosurfaceParamBuffer);

                _disposed = true;
            }
            base.Dispose(Disposing);
        }
        #endregion

        public override void InitializeRenderer()
        {
            _isosurfaceEffectPath = @"Shaders\Isosurface";

            base.InitializeRenderer();

            _isosurfaceParams = IsosurfaceParams.Empty;           
            _isosurfaceParamBuffer = new Buffer(_device, Marshal.SizeOf(typeof(IsosurfaceParams)), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }
        protected override void InitializeShaders()
        {
            var vsByteCode = ShaderBytecode.FromFile(_isosurfaceEffectPath + ".vso");
            _vsIsosurface = new VertexShader(_device, vsByteCode);

            _ilIsosurface = new InputLayout(_device, ShaderSignature.GetInputSignature(vsByteCode), new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("NORMAL", 0, Format.R32G32B32A32_Float, 16, 0)
            });

            var psByteCode = ShaderBytecode.FromFile(_isosurfaceEffectPath + ".pso");
            _psIsosurface = new PixelShader(_device, psByteCode);

            var gsByteCode = ShaderBytecode.FromFile(_isosurfaceEffectPath + ".gso");
            _gsIsosurface = new GeometryShader(_device, gsByteCode);
        }
        protected override void InitializeStates()
        {
            base.InitializeStates();    
        }

        public void CreateVolumeVertices(float zScaling)
        {
            Disposer.RemoveAndDispose(ref _coordinateBox);
            Disposer.RemoveAndDispose(ref _boundingBox);

            _renderModelDescription.ModelSize.X = _renderModelDescription.ModelSize.Y = 2.0f;
            _renderModelDescription.ModelSize.Z = ((float)_renderModelDescription.DataSize.Z / (float)_renderModelDescription.DataSize.X * zScaling * 20f);

            _renderModelDescription.ModelEnd.X = _renderModelDescription.ModelStart.X + _renderModelDescription.ModelSize.X;
            _renderModelDescription.ModelEnd.Y = _renderModelDescription.ModelStart.Y + _renderModelDescription.ModelSize.Y;
            _renderModelDescription.ModelEnd.Z = _renderModelDescription.ModelStart.Z + _renderModelDescription.ModelSize.Z;

            Vector4[] vertices = new Vector4[8]
            {
                new Vector4(_renderModelDescription.ModelStart.X, _renderModelDescription.ModelStart.Y, _renderModelDescription.ModelStart.Z, 1.0f),
                new Vector4(_renderModelDescription.ModelStart.X, _renderModelDescription.ModelStart.Y, _renderModelDescription.ModelEnd.Z, 1.0f),
                new Vector4(_renderModelDescription.ModelStart.X, _renderModelDescription.ModelEnd.Y, _renderModelDescription.ModelStart.Z, 1.0f),
                new Vector4(_renderModelDescription.ModelStart.X, _renderModelDescription.ModelEnd.Y, _renderModelDescription.ModelEnd.Z, 1.0f),
                new Vector4(_renderModelDescription.ModelEnd.X, _renderModelDescription.ModelStart.Y, _renderModelDescription.ModelStart.Z, 1.0f),
                new Vector4(_renderModelDescription.ModelEnd.X, _renderModelDescription.ModelStart.Y, _renderModelDescription.ModelEnd.Z, 1.0f),
                new Vector4(_renderModelDescription.ModelEnd.X, _renderModelDescription.ModelEnd.Y, _renderModelDescription.ModelStart.Z, 1.0f),
                new Vector4(_renderModelDescription.ModelEnd.X, _renderModelDescription.ModelEnd.Y, _renderModelDescription.ModelEnd.Z, 1.0f)
            };

            _boundingBox = new BoundingBox(_device, vertices);
            _coordinateBox = new CoordinateBox(_device, vertices);
        }

        public void SetData(List<RenderIsosurface> Isosurfaces)
        {
            //Create new or replace Texture3D so can be changed on fly

            _dataLoaded = false;

            Disposer.RemoveAndDispose(ref _boundingBox);
            Disposer.RemoveAndDispose(ref _coordinateBox);

            if (Isosurfaces.GroupBy(i => i.Width).Count() != 1 ||
                Isosurfaces.GroupBy(i => i.Height).Count() != 1 ||
                Isosurfaces.GroupBy(i => i.Depth).Count() != 1 )
                throw new ArgumentException("Not all volumes are the same dimensions.");

            int width = Isosurfaces[0].Width;
            int height = Isosurfaces[0].Height;
            int depth = Isosurfaces[0].Depth;

            _renderModelDescription.DataSize.X = Isosurfaces[0].Width;
            _renderModelDescription.DataSize.Y = Isosurfaces[0].Height;
            _renderModelDescription.DataSize.Z = Isosurfaces[0].Depth;

            Application.Current.Dispatcher.Invoke(() =>
            {
                RenderingViewModel.DataWidth = _renderModelDescription.DataSize.X;
                RenderingViewModel.DataHeight = _renderModelDescription.DataSize.Y;
                RenderingViewModel.DataDepth = _renderModelDescription.DataSize.Z;

                RenderingViewModel.RenderPlaneMinX = RenderingViewModel.RenderPlaneMinY = RenderingViewModel.RenderPlaneMinZ = 0;
                RenderingViewModel.RenderPlaneMaxX = _renderModelDescription.DataSize.X;
                RenderingViewModel.RenderPlaneMaxY = _renderModelDescription.DataSize.Y;
                RenderingViewModel.RenderPlaneMaxZ = _renderModelDescription.DataSize.Z;
            });

            CreateVolumeVertices(1.0f);

            const float maxSize = 2.0f;

            float maxDimension = 0;
            // x dimension is largest
            if(width >= Math.Max(height, depth))
            {
                maxDimension = width;
            }
            // y dimension is largest
            else if(height >= depth)
            {
                maxDimension = height;
            }
            // z dimension is largest
            else
            {
                maxDimension = depth;
            }

            float boxSizeX = width * maxSize / maxDimension;
            float boxSizeY = height * maxSize / maxDimension;
            float boxSizeZ = depth * maxSize / maxDimension;

            float boxStartX = -boxSizeX / 2f;
            float boxStartY = -boxSizeY / 2f;
            float boxStartZ = -boxSizeZ / 2f;

            float dataSizeX = width;
            float dataSizeY = height;
            float dataSizeZ = depth;

            float dataStartX = 0;
            float dataStartY = 0;
            float dataStartZ = 0;

            short[] boundingIndices = new short[36]
            {
                0, 1, 2,
                2, 1, 3,

                0, 4, 1,
                1, 4, 5,

                0, 2, 4,
                4, 2, 6,

                1, 5, 3,
                3, 5, 7,

                2, 3, 6,
                6, 3, 7,

                5, 4, 7,
                7, 4, 6,
            };

            int numTriangles = Isosurfaces.Sum(i => i.Triangles.Length);

            // Number triangles * 3 points per triangle * 2 vectors for each point (location and normal)
            Vector4[] isosurfaceVertices = new Vector4[numTriangles * 3 * 2];

            int pos = 0;
            foreach (RenderIsosurface iso in Isosurfaces)
            {
                foreach (TriangleSurface tri in iso.Triangles)
                {
                    foreach (Vector4 point in tri.Vertices)
                    {
                        isosurfaceVertices[pos++] = new Vector4()
                        {
                            X = (((point.X + dataStartX) * boxSizeX) / dataSizeX) + boxStartX,
                            Y = (((point.Y + dataStartY) * boxSizeY) / dataSizeY) + boxStartY,
                            Z = (((point.Z + dataStartZ) * boxSizeZ) / dataSizeZ) + boxStartZ,
                            W = point.W
                        };
                        // Encode the SurfaceId as the w of the normal vector
                        isosurfaceVertices[pos++] = new Vector4(tri.Normal, tri.SurfaceId);
                    }
                }
            }

            _vertexBufferTriangles = Buffer.Create(_device, BindFlags.VertexBuffer, isosurfaceVertices);

            _isosurfaceParams.NumberIsosurfaces = Isosurfaces.Count;

            _numVertices = isosurfaceVertices.Length / 2;

            _dataLoaded = true;
        }
        
        protected override bool Resize()
        {
            try
            {
                bool baseResized = base.Resize();
                if (!baseResized) return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public override void Update(bool targetYAxisOrbiting)
        {
            base.Update(targetYAxisOrbiting);

            _renderParams.RenderPlanesMin = new Vector4(
                RenderingViewModel.RenderPlaneMinX / _renderModelDescription.DataSize.X,
                RenderingViewModel.RenderPlaneMinY / _renderModelDescription.DataSize.Y,
                RenderingViewModel.RenderPlaneMinZ / RenderingViewModel.ScalingZ / _renderModelDescription.DataSize.Z, 0);
            _renderParams.RenderPlanesMax = new Vector4(
                RenderingViewModel.RenderPlaneMaxX / _renderModelDescription.DataSize.X,
                RenderingViewModel.RenderPlaneMaxY / _renderModelDescription.DataSize.Y,
                RenderingViewModel.RenderPlaneMaxZ / RenderingViewModel.ScalingZ / _renderModelDescription.DataSize.Z, 0);
        }

        public override void Draw()
        {
            base.Draw();

            if (_device == null) return;

            var context = _device.ImmediateContext;

            if (_dataLoaded)
            {
                // Update and set IsosurfaceParams

                for (int i = 0; i < _isosurfaceParams.NumberIsosurfaces; i++)
                {
                    _isosurfaceParams.UpdateColor(i, _dataContextRenderWindow.RenderWindowView.VolumeColors[i].ToVector4());
                }
                _isosurfaceParams.IsosurfaceScale = new Vector4(1.0f, 1.0f, _dataContextRenderWindow.RenderWindowView.ScalingZ, 1.0f);
                context.UpdateSubresource(ref _isosurfaceParams, _isosurfaceParamBuffer);

                context.InputAssembler.InputLayout = _ilIsosurface;
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBufferTriangles, Utilities.SizeOf<Vector4>() * 2, 0));
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                context.VertexShader.Set(_vsIsosurface);
                context.VertexShader.SetConstantBuffer(0, _bufferRenderParams);
                context.VertexShader.SetConstantBuffer(1, _bufferLightingParams);
                context.VertexShader.SetConstantBuffer(3, _isosurfaceParamBuffer);

                context.GeometryShader.Set(_gsIsosurface);
                context.GeometryShader.SetConstantBuffer(0, _bufferRenderParams);

                context.PixelShader.Set(_psIsosurface);
                context.PixelShader.SetConstantBuffer(0, _bufferRenderParams);
                context.PixelShader.SetConstantBuffer(1, _bufferLightingParams);

                // TODO: Remove this
                context.PixelShader.SetConstantBuffer(3, _isosurfaceParamBuffer);

                if (EnableDepthBuffering)
                    context.OutputMerger.SetRenderTargets(_depthView, _renderView);
                else context.OutputMerger.SetRenderTargets(_renderView);

                context.Rasterizer.State = _rasterizerStateCullNone;
                context.Draw(_numVertices, 0);

                _dataContextRenderWindow.RenderWindowView.NumTrianglesDrawn = _numVertices / 3;

                // Clear geometry shader since axes/bounding box don't use it
                context.GeometryShader.Set(null);
            }

            CompleteDraw();
        }        
    }
}
