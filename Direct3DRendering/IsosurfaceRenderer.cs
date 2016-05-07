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
        Buffer _indexBufferTriangles;

        RenderParams _renderParams;
        IsosurfaceParams _isosurfaceParams;
        Buffer _renderParamBuffer;
        Buffer _isosurfaceParamBuffer;

        string _isosurfaceEffectPath;

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
                Disposer.RemoveAndDispose(ref _indexBufferTriangles);

                Disposer.RemoveAndDispose(ref _renderParamBuffer);
                Disposer.RemoveAndDispose(ref _isourfaceParamBuffer);

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
            _renderParamBuffer = new Buffer(_device, Marshal.SizeOf(typeof(RenderParams)), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }
        protected override void InitializeShaders()
        {
            DeviceContext context = _device.ImmediateContext;

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

            int numIsosurfaces = Isosurfaces.Count;

            _isosurfaceParams = IsosurfaceParams.Empty;
            _isosurfaceParamBuffer = new Buffer(_device, Marshal.SizeOf(typeof(IsosurfaceParams)), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            const float maxSize = 2.0f;
            float sizeX = maxSize;
            float sizeY = maxSize;
            float sizeZ = ((float)depth / (float)width) * maxSize;
            //sizeZ = sizeX;

            float startX = -sizeX / 2f;
            float startY = -sizeY / 2f;
            float startZ = -sizeZ / 2f;

            float maxValue = (float)Math.Max(sizeX, sizeY);
            maxValue = (float)Math.Max(maxValue, sizeZ);

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

            int pos = 0;
            foreach (RenderIsosurface iso in Isosurfaces)
            {
                foreach (TriangleSurface tri in iso.Triangles)
                {
                    foreach (Vector4 point in tri.Points)
                    {
                        isosurfaceVertices[pos++] = point;
                        isosurfaceVertices[pos++] = tri.Normal;
                    }                    
                }                
            }

            _vertexBufferTriangles = Buffer.Create(_device, BindFlags.VertexBuffer, isosurfaceVertices);

            _dataLoaded = true;
        }
        protected override bool Resize()
        {
            try
            {
                bool baseResized = base.Resize();
                if (!baseResized) return false;

                ShaderResourceViewDescription descSRV = new ShaderResourceViewDescription()
                {
                    Format = Format.R32G32B32A32_Float,
                    Dimension = ShaderResourceViewDimension.Texture2D
                };
                RenderTargetViewDescription descRTV = new RenderTargetViewDescription()
                {
                    Format = Format.R32G32B32A32_Float,
                    Dimension = RenderTargetViewDimension.Texture2D
                };

                _texPositions = new Texture2D[2];
                _srvPositions = new ShaderResourceView[2];
                _rtvPositions = new RenderTargetView[2];

                Texture2DDescription descTex = new Texture2DDescription()
                {
                    ArraySize = 1,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    Usage = ResourceUsage.Default,
                    Format = Format.R32G32B32A32_Float,
                    Width = _parent.ClientSize.Width,
                    Height = _parent.ClientSize.Height,
                    MipLevels = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    CpuAccessFlags = CpuAccessFlags.None
                };
                for (int i = 0; i < 2; i++)
                {
                    Texture2D tex = new Texture2D(_device, descTex);

                    _texPositions[i] = tex;
                    _srvPositions[i] = new ShaderResourceView(_device, tex);
                    _rtvPositions[i] = new RenderTargetView(_device, tex);
                }

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

            if (_dataLoaded)
            {
                _renderParams.InvWindowSize = new Vector2(1f / _parent.ClientSize.Width, 1f / _parent.ClientSize.Height);
                _renderParams.WorldProjView = worldProjView;
                _renderParams.Brightness = Brightness;
                context.UpdateSubresource(ref _renderParams, _renderParamBuffer);

                _volumeParams.VolumeScale = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                for (int i = 0; i < _volumeParams.NumVolumes; i++)
                {
                    Vector4 current = _volumeParams.GetColor(i);
                    _volumeParams.UpdateColor(i,
                        new Vector4(current.X, current.Y, current.Z, _dataContextRenderWindow.RenderWindowView.VolumeAlphas[i]));
                }
                context.UpdateSubresource(ref _volumeParams, _volumeParamBuffer);

                for (int i = 0; i < _volumeParams.NumVolumes; i++)
                {
                    //_isosurfaceParams.UpdateValue(i, _dataContextRenderWindow.IsosurfaceValues[i]);
                }
                context.UpdateSubresource(ref _isosurfaceParams, _isourfaceParamBuffer);

                context.InputAssembler.InputLayout = _ilPosition;
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBufferCube, Marshal.SizeOf(typeof(Vector4)), 0));
                context.InputAssembler.SetIndexBuffer(_indexBufferCube, Format.R16_UInt, 0);
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                context.VertexShader.Set(_vsPosition);
                context.VertexShader.SetConstantBuffer(0, _renderParamBuffer);
                context.VertexShader.SetConstantBuffer(1, _volumeParamBuffer);
                context.PixelShader.Set(_psPosition);

                context.Rasterizer.State = _rasterizerCullFront;
                context.OutputMerger.SetRenderTargets(null, _rtvPositions[1]);
                context.ClearRenderTargetView(_rtvPositions[1], Color.Black);
                context.DrawIndexed(36, 0, 0);

                context.Rasterizer.State = _rasterizerCullBack;
                context.OutputMerger.SetRenderTargets(null, _rtvPositions[0]);
                context.ClearRenderTargetView(_rtvPositions[0], Color.Black);
                context.DrawIndexed(36, 0, 0);

                context.VertexShader.Set(_vsIsosurface);
                context.VertexShader.SetConstantBuffer(0, _renderParamBuffer);
                context.PixelShader.Set(_psIsosurface);

                context.PixelShader.SetConstantBuffer(0, _renderParamBuffer);
                context.PixelShader.SetConstantBuffer(1, _volumeParamBuffer);
                context.InputAssembler.InputLayout = _ilIsosurface;

                context.OutputMerger.SetRenderTargets(_depthView, _renderView);

                //Set all volumes
                context.PixelShader.SetShaderResources(0, 1, _srvPositions[0]);
                context.PixelShader.SetShaderResources(1, 1, _srvPositions[1]);

                int startIndex = 2;
                for (int i = 0; i < _volumeParams.NumVolumes; i++)
                {
                    context.PixelShader.SetShaderResources(startIndex + i, 1, _srvVolumes[i]);
                }

                context.PixelShader.SetSamplers(0, 1, _samplerLinear);

                context.DrawIndexed(36, 0, 0);

                ShaderResourceView[] nullRV = new ShaderResourceView[3] { null, null, null };
                context.PixelShader.SetShaderResources(0, 3, nullRV);
            }

            CompleteDraw();
        }        
    }
}
