using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Direct3DRendering
{
    public class IsosurfaceRenderer : Renderer
    {
        VertexShader _vsIsosurface;
        InputLayout _ilIsosurface;
        PixelShader _psIsosurface;

        Buffer _vertexBufferTriangles;

        RenderParams _renderParams;
        IsosurfaceParams _isosurfaceParams;
        Buffer _renderParamBuffer;
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

                Disposer.RemoveAndDispose(ref _renderParamBuffer);
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

            _renderParams = new RenderParams()
            {
                WorldProjView = Matrix.Identity
            };
            _isosurfaceParams = IsosurfaceParams.Empty;

            _renderParamBuffer = new Buffer(_device, Marshal.SizeOf(typeof(RenderParams)), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);            
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
        }
        protected override void InitializeStates()
        {
            base.InitializeStates();

            RasterizerStateDescription descRasterizer = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                IsDepthClipEnabled = true
            };
            RasterizerState rasterizer = new RasterizerState(_device, descRasterizer);

            var context = _device.ImmediateContext;
            context.Rasterizer.State = rasterizer;            
        }

        public void SetData(List<RenderIsosurface> Isosurfaces)
        {
            //Create new or replace Texture3D so can be changed on fly

            _dataLoaded = false;

            Disposer.RemoveAndDispose(ref _boundingBox);
            Disposer.RemoveAndDispose(ref _coordinateBox);
            int width = -1;
            int height = -1;
            int depth = -1;
            foreach (RenderIsosurface isosurface in Isosurfaces)
            {
                if (width == -1)
                {
                    width = isosurface.Width;
                }
                if (height == -1)
                {
                    height = isosurface.Height;
                }
                if (depth == -1)
                {
                    depth = isosurface.Depth;
                }

                if (isosurface.Width != width || isosurface.Height != height || isosurface.Depth != depth)
                {
                    throw new ArgumentException("Not all volumes are the same dimensions.");
                }
            }

            const float maxSize = 2.0f;
            float sizeX = maxSize;
            float sizeY = maxSize;
            float sizeZ = ((float)depth / (float)width) * maxSize;
            //sizeZ = sizeX;

            float startX = -sizeX / 2f;
            float startY = -sizeY / 2f;
            float startZ = -sizeZ / 2f;

            float maxValue = Math.Max(sizeX, sizeY);
            maxValue = Math.Max(maxValue, sizeZ);

            float ratioX = sizeX / (2 * maxValue);
            float ratioY = sizeY / (2 * maxValue);
            float ratioZ = sizeZ / (2 * maxValue);

            float sStartX = 0.5f - ratioX;
            float sStartY = 0.5f - ratioY;
            float sStartZ = 0.5f - ratioZ;

            float sEndX = 0.5f + ratioX;
            float sEndY = 0.5f + ratioY;
            float sEndZ = 0.5f + ratioZ;

            Vector4[] boundingVertices = new Vector4[8]
            {
                new Vector4(startX, startY, startZ, 1.0f),
                new Vector4(startX, startY, startZ + sizeZ, 1.0f),
                new Vector4(startX, startY + sizeY, startZ, 1.0f),
                new Vector4(startX, startY + sizeY, startZ + sizeZ, 1.0f),
                new Vector4(startX + sizeX, startY, startZ, 1.0f),
                new Vector4(startX + sizeX, startY, startZ + sizeZ, 1.0f),
                new Vector4(startX + sizeX, startY + sizeY, startZ, 1.0f),
                new Vector4(startX + sizeX, startY + sizeY, startZ + sizeZ, 1.0f)
            };

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

            int numTriangles = 0;
            foreach (RenderIsosurface iso in Isosurfaces)
            {
                numTriangles += iso.Triangles.Length;
            }

            _boundingBox = new BoundingBox(_device, boundingVertices);
            _coordinateBox = new CoordinateBox(_device, boundingVertices);

            // Number triangles * 3 points per triangle * 2 vectors for each point (location and normal)
            Vector4[] isosurfaceVertices = new Vector4[numTriangles * 3 * 2];
            //Vector4[] isosurfaceVertices = new Vector4[numTriangles * 3];

            int pos = 0;
            foreach (RenderIsosurface iso in Isosurfaces)
            {
                foreach (TriangleSurface tri in iso.Triangles)
                {
                    foreach (Vector4 point in tri.Vertices)
                    {
                        isosurfaceVertices[pos++] = new Vector4()
                        {
                            X = (point.X * sizeX / width) + startX,
                            Y = (point.Y * sizeY / height) + startY,
                            Z = (point.Z * sizeZ / depth) + startZ,
                            W = point.W
                        };
                        // Encode the SurfaceId as the w of the normal vector
                        isosurfaceVertices[pos++] = new Vector4(tri.Normal, tri.SurfaceId);
                    }
                }
            }

            //Vector4[] isosurfaceVertices = new Vector4[4 * 3 * 2]
            //{
            //    // 0
            //    new Vector4(-0.5f, -0.5f, 1, 1), new Vector4(0, 0, 0, 0),
            //    new Vector4(0, 0.5f, 1, 1), new Vector4(0, 0, 0, 0),
            //    new Vector4(0.5f, -0.5f, 1, 1), new Vector4(0, 0, 0 ,0),
            //    // 1
            //    new Vector4(-0.5f, -0.5f, 0.66f, 1), new Vector4(0, 0, 0, 1),
            //    new Vector4(0, 0.5f, 0.66f, 1), new Vector4(0, 0, 0, 1),
            //    new Vector4(0.5f, -0.5f, 0.66f, 1), new Vector4(0, 0, 0, 1),
            //    // 2
            //    new Vector4(-0.5f, -0.5f, -0.33f, 1), new Vector4(0, 0, 0, 2),
            //    new Vector4(0, 0.5f, -0.33f, 1), new Vector4(0, 0, 0, 2),
            //    new Vector4(0.5f, -0.5f, -0.33f, 1), new Vector4(0, 0, 0, 2),
            //    //3
            //    new Vector4(-0.5f, -0.5f, -1, 1), new Vector4(0, 0, 0, 3),
            //    new Vector4(0, 0.5f, -1, 1), new Vector4(0, 0, 0, 3),
            //    new Vector4(0.5f, -0.5f, -1, 1), new Vector4(0, 0, 0, 3)
            //};

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
        public override void Draw()
        {
            base.Draw();

            if (_device == null) return;

            var context = _device.ImmediateContext;

            Matrix worldProjView = _orbitCamera.WorldProjView;
            worldProjView.Transpose();
            _renderParams.WorldProjView = worldProjView;
            _renderParams.Brightness = Brightness;

            if (_dataLoaded)
            {
                // Update and set RenderParams and IsosurfaceParams
                context.UpdateSubresource(ref _renderParams, _renderParamBuffer);

                for (int i = 0; i < _isosurfaceParams.NumberIsosurfaces; i++)
                {
                    _isosurfaceParams.UpdateColor(i, _dataContextRenderWindow.RenderWindowView.VolumeColors[i].ToVector4());
                }
                context.UpdateSubresource(ref _isosurfaceParams, _isosurfaceParamBuffer);

                context.InputAssembler.InputLayout = _ilIsosurface;
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBufferTriangles, Utilities.SizeOf<Vector4>() * 2, 0));
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                context.VertexShader.Set(_vsIsosurface);
                context.VertexShader.SetConstantBuffer(0, _renderParamBuffer);
                context.VertexShader.SetConstantBuffer(2, _isosurfaceParamBuffer);
                context.PixelShader.Set(_psIsosurface);
                context.PixelShader.SetConstantBuffer(0, _renderParamBuffer);

                //context.OutputMerger.SetRenderTargets(_depthView, _renderView);
                context.OutputMerger.SetRenderTargets(_renderView);

                context.Draw(_numVertices, 0);

                _dataContextRenderWindow.RenderWindowView.NumTrianglesDrawn = _numVertices / 3;  
            }

            CompleteDraw();
        }        
    }
}
