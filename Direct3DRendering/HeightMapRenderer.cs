using System;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace Direct3DRendering
{
    public class HeightMapRenderer : Renderer
    {
        string _hmEffectPath;

        VertexShader _hmVertexShader;
        PixelShader _hmPixelShader;
        InputLayout _hmInputLayout;

        float _hmWidth;
        float _hmHeight;
        float _hmDepth;
        float _startX;
        float _startY;
        float _startZ;
        Vector3 _renderSize;

        int _vertexCount;

        Buffer _hmVertexBuffer;

        RenderParams _renderParams;
        HeightMapParams _heightMapParams;
        Buffer _hmRenderParamsBuffer;
        Buffer _hmHeightMapParamsBuffer;

        Color[,] _colorData;
        float[,] _heightData;

        public HeightMapRenderer(RenderWindow Window)
            :base(Window)
        {
            _renderType = RenderType.HeightMap;
        }

        #region IDisposable
        ~HeightMapRenderer()
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
                Disposer.RemoveAndDispose(ref _hmVertexBuffer);
                Disposer.RemoveAndDispose(ref _hmPixelShader);
                Disposer.RemoveAndDispose(ref _hmInputLayout);
                Disposer.RemoveAndDispose(ref _hmVertexBuffer);
                Disposer.RemoveAndDispose(ref _hmRenderParamsBuffer);
                Disposer.RemoveAndDispose(ref _hmHeightMapParamsBuffer);

                _disposed = true;
            }
            base.Dispose(Disposing);
        }
        #endregion

        public override void InitializeRenderer()
        {
            _hmEffectPath = @"Shaders\HeightMap";

            base.InitializeRenderer();

            _renderParams = new RenderParams()
            {
                WorldProjView = Matrix.Identity
            };
            _heightMapParams = new HeightMapParams();

            _hmRenderParamsBuffer = new Buffer(_device, Marshal.SizeOf(typeof(RenderParams)), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            _hmHeightMapParamsBuffer = new Buffer(_device, Marshal.SizeOf(typeof(HeightMapParams)), ResourceUsage.Default,
               BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
        }

        protected override void InitializeShaders()
        {
            var vsByteCode = ShaderBytecode.FromFile(_hmEffectPath + ".vso");
            _hmVertexShader = new VertexShader(_device, vsByteCode);

            _hmInputLayout = new InputLayout(_device, ShaderSignature.GetInputSignature(vsByteCode), new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
            });

            var psByteCode = ShaderBytecode.FromFile(_hmEffectPath + ".pso");
            _hmPixelShader = new PixelShader(_device, psByteCode);
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

        public void SetData(Color[,] ColorData, float[,] HeightData, Vector3 RenderSize)
        {
            _hmWidth = ColorData.GetLength(0);
            _hmHeight = ColorData.GetLength(1);

            if (_hmWidth != HeightData.GetLength(0) ||
                _hmHeight != HeightData.GetLength(1))
                throw new ArgumentException("Height map does not match dimensions of color data.");

            _colorData = ColorData;
            _heightData = HeightData;
            _renderSize = RenderSize;

            _dataLoaded = true;
        }

        private void updateVertices()
        {
            float height = _dataContextRenderWindow.RenderWindowView.HeightMapHeight;

            // If height parameter hasn't changed since last draw, no need to recalculate the vertices
            if (height == _renderSize.Z)
                return;

            _renderSize = new Vector3(_renderSize.X, _renderSize.Y, height);

            Disposer.RemoveAndDispose(ref _hmVertexBuffer);
            Disposer.RemoveAndDispose(ref _boundingBox);
            Disposer.RemoveAndDispose(ref _coordinateBox);

            float minZ = 0;
            float maxZ = 0;

            for (int x = 0; x < _hmWidth; x++)
            {
                for (int y = 0; y < _hmHeight; y++)
                {
                    float value = _heightData[x, y];
                    if (value < minZ) minZ = value;
                    if (value > maxZ) maxZ = value;
                }
            }

            _hmDepth = minZ + maxZ;

            _startZ = -_renderSize.Z / 2f;
            _startX = -_renderSize.X / 2f;
            _startY = -_renderSize.Y / 2f;

            Vector4[] vertices = new Vector4[((int)_hmWidth - 1) * ((int)_hmHeight - 1) * 6 * 2];

            int vPos = 0;
            for (int x = 0; x < _hmWidth - 1; x++)
            {
                for (int y = 0; y < _hmHeight - 1; y++)
                {
                    //Triangle #1
                    vertices[vPos + 0] = Transform(new Vector4(
                        x, y, _heightData[x, y], 1.0f));
                    Color c1 = _colorData[x, y];
                    vertices[vPos + 1] = ColorToVector(c1);

                    vertices[vPos + 2] = Transform(new Vector4(
                        x, y + 1, _heightData[x, y + 1], 1.0f));
                    Color c2 = _colorData[x, y + 1];
                    vertices[vPos + 3] = ColorToVector(c2);

                    vertices[vPos + 4] = Transform(new Vector4(
                        x + 1, y, _heightData[x + 1, y], 1.0f));
                    Color c3 = _colorData[x + 1, y];
                    vertices[vPos + 5] = ColorToVector(c3);

                    //Triangle #2
                    vertices[vPos + 6] = Transform(new Vector4(
                        x, y + 1, _heightData[x, y + 1], 1.0f));
                    vertices[vPos + 7] = ColorToVector(c2);

                    vertices[vPos + 8] = Transform(new Vector4(
                        x + 1, y + 1, _heightData[x + 1, y + 1], 1.0f));
                    Color c4 = _colorData[x + 1, y + 1];
                    vertices[vPos + 9] = ColorToVector(c4);

                    vertices[vPos + 10] = Transform(new Vector4(
                        x + 1, y, _heightData[x + 1, y], 1.0f));
                    vertices[vPos + 11] = ColorToVector(c3);

                    vPos += 12;
                }
            }

            _vertexCount = vertices.Length / 2;

            _hmVertexBuffer = Buffer.Create(_device, BindFlags.VertexBuffer, vertices);

            Vector4[] bboxVertices = new Vector4[8] 
            {
                Transform(new Vector4(0, 0, 0, 1.0f)),
                Transform(new Vector4(0, 0, _hmDepth, 1.0f)),
                Transform(new Vector4(0, _hmHeight, 0, 1.0f)),
                Transform(new Vector4(0, _hmHeight, _hmDepth, 1.0f)),
                Transform(new Vector4(_hmWidth, 0, 0, 1.0f)),
                Transform(new Vector4(_hmWidth, 0, _hmDepth, 1.0f)),
                Transform(new Vector4(_hmWidth, _hmHeight, 0, 1.0f)),
                Transform(new Vector4(_hmWidth, _hmHeight, _hmDepth, 1.0f))
            };

            _boundingBox = new BoundingBox(_device, bboxVertices);
            _coordinateBox = new CoordinateBox(_device, bboxVertices);
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
                updateVertices();

                context.UpdateSubresource(ref _renderParams, _hmRenderParamsBuffer);
                context.UpdateSubresource(ref _heightMapParams, _hmHeightMapParamsBuffer);

                context.InputAssembler.InputLayout = _hmInputLayout;
                context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_hmVertexBuffer, Utilities.SizeOf<Vector4>() * 2, 0));
                context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

                context.VertexShader.Set(_hmVertexShader);
                context.VertexShader.SetConstantBuffer(0, _hmRenderParamsBuffer);
                context.VertexShader.SetConstantBuffer(1, _hmHeightMapParamsBuffer);
                context.PixelShader.Set(_hmPixelShader);
                context.PixelShader.SetConstantBuffer(0, _hmRenderParamsBuffer);

                context.OutputMerger.SetRenderTargets(_depthView, _renderView);

                context.Draw(_vertexCount, 0);

                _dataContextRenderWindow.RenderWindowView.NumTrianglesDrawn = _vertexCount / 3;
            }

            CompleteDraw();
        }

        private Vector4 Transform(Vector4 Original)
        {
            float x = _startX + (Original.X * _renderSize.X / _hmWidth);
            float y = _startY + (Original.Y * _renderSize.Y / _hmHeight);
            float z = _startZ + (Original.Z * _renderSize.Z / _hmDepth);

            return new Vector4(x, y, z, 1.0f);
        }
        private Vector4 ColorToVector(Color Original)
        {
            return new Vector4(Original.R / 255f, Original.G / 255f, Original.B / 255f, Original.A / 255f);
        }
    }

    //public class HeightMapRenderer : Renderer
    //{
    //    string _hmEffectPath;

    //    VertexShader _hmVertexShader;
    //    PixelShader _hmPixelShader;
    //    InputLayout _hmInputLayout;

    //    float _hmWidth;
    //    float _hmHeight;
    //    float _hmDepth;
    //    float _startX;
    //    float _startY;
    //    float _startZ;
    //    Vector3 _renderSize;

    //    int _vertexCount;

    //    Buffer _hmVertexBuffer;

    //    RenderParams _renderParams;
    //    HeightMapParams _heightMapParams;
    //    Buffer _hmRenderParamsBuffer;
    //    Buffer _hmHeightMapParamsBuffer;

    //    public HeightMapRenderer(RenderWindow Window)
    //        : base(Window)
    //    {
    //        _renderType = RenderType.HeightMap;
    //    }

    //    #region IDisposable
    //    ~HeightMapRenderer()
    //    {
    //        Dispose(false);
    //    }
    //    private bool _disposed = false;
    //    public void Dispose()
    //    {
    //        Dispose(true);
    //        GC.SuppressFinalize(this);
    //    }
    //    protected override void Dispose(bool Disposing)
    //    {
    //        if (!_disposed)
    //        {
    //            if (Disposing)
    //            {

    //            }
    //            Disposer.RemoveAndDispose(ref _hmVertexBuffer);
    //            Disposer.RemoveAndDispose(ref _hmPixelShader);
    //            Disposer.RemoveAndDispose(ref _hmInputLayout);
    //            Disposer.RemoveAndDispose(ref _hmVertexBuffer);
    //            Disposer.RemoveAndDispose(ref _hmRenderParamsBuffer);
    //            Disposer.RemoveAndDispose(ref _hmHeightMapParamsBuffer);

    //            _disposed = true;
    //        }
    //        base.Dispose(Disposing);
    //    }
    //    #endregion

    //    public override void InitializeRenderer()
    //    {
    //        _hmEffectPath = @"Shaders\HeightMap";

    //        base.InitializeRenderer();

    //        _renderParams = new RenderParams()
    //        {
    //            WorldProjView = Matrix.Identity
    //        };
    //        _heightMapParams = new HeightMapParams();

    //        _hmRenderParamsBuffer = new Buffer(_device, Marshal.SizeOf(typeof(RenderParams)), ResourceUsage.Default,
    //            BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
    //        _hmHeightMapParamsBuffer = new Buffer(_device, Marshal.SizeOf(typeof(HeightMapParams)), ResourceUsage.Default,
    //           BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
    //    }

    //    protected override void InitializeShaders()
    //    {
    //        var vsByteCode = ShaderBytecode.FromFile(_hmEffectPath + ".vso");
    //        _hmVertexShader = new VertexShader(_device, vsByteCode);

    //        _hmInputLayout = new InputLayout(_device, ShaderSignature.GetInputSignature(vsByteCode), new[]
    //        {
    //            new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
    //            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
    //        });

    //        var psByteCode = ShaderBytecode.FromFile(_hmEffectPath + ".pso");
    //        _hmPixelShader = new PixelShader(_device, psByteCode);
    //    }
    //    protected override void InitializeStates()
    //    {
    //        base.InitializeStates();

    //        RasterizerStateDescription descRasterizer = new RasterizerStateDescription()
    //        {
    //            FillMode = FillMode.Solid,
    //            CullMode = CullMode.None,
    //            IsDepthClipEnabled = true
    //        };
    //        RasterizerState rasterizer = new RasterizerState(_device, descRasterizer);

    //        var context = _device.ImmediateContext;
    //        context.Rasterizer.State = rasterizer;
    //    }

    //    public void SetData(Color[,] ColorData, float[,] HeightData, Vector3 RenderSize)
    //    {
    //        _hmWidth = ColorData.GetLength(0);
    //        _hmHeight = ColorData.GetLength(1);

    //        if (_hmWidth != HeightData.GetLength(0) ||
    //            _hmHeight != HeightData.GetLength(1))
    //            throw new ArgumentException("Height map does not match dimensions of color data.");

    //        Disposer.RemoveAndDispose(ref _hmVertexBuffer);
    //        Disposer.RemoveAndDispose(ref _boundingBox);
    //        Disposer.RemoveAndDispose(ref _coordinateBox);

    //        float minZ = 0;
    //        float maxZ = 0;

    //        for (int x = 0; x < _hmWidth; x++)
    //        {
    //            for (int y = 0; y < _hmHeight; y++)
    //            {
    //                float value = HeightData[x, y];
    //                if (value < minZ) minZ = value;
    //                if (value > maxZ) maxZ = value;
    //            }
    //        }

    //        _hmDepth = minZ + maxZ;

    //        _startZ = -RenderSize.Z / 2f;
    //        _startX = -RenderSize.X / 2f;
    //        _startY = -RenderSize.Y / 2f;
    //        _renderSize = RenderSize;

    //        Vector4[] vertices = new Vector4[((int)_hmWidth - 1) * ((int)_hmHeight - 1) * 6 * 2];

    //        int vPos = 0;
    //        for (int x = 0; x < _hmWidth - 1; x++)
    //        {
    //            for (int y = 0; y < _hmHeight - 1; y++)
    //            {
    //                //Triangle #1
    //                vertices[vPos + 0] = Transform(new Vector4(
    //                    x, y, HeightData[x, y], 1.0f));
    //                Color c1 = ColorData[x, y];
    //                vertices[vPos + 1] = ColorToVector(c1);

    //                vertices[vPos + 2] = Transform(new Vector4(
    //                    x, y + 1, HeightData[x, y + 1], 1.0f));
    //                Color c2 = ColorData[x, y + 1];
    //                vertices[vPos + 3] = ColorToVector(c2);

    //                vertices[vPos + 4] = Transform(new Vector4(
    //                    x + 1, y, HeightData[x + 1, y], 1.0f));
    //                Color c3 = ColorData[x + 1, y];
    //                vertices[vPos + 5] = ColorToVector(c3);

    //                //Triangle #2
    //                vertices[vPos + 6] = Transform(new Vector4(
    //                    x, y + 1, HeightData[x, y + 1], 1.0f));
    //                vertices[vPos + 7] = ColorToVector(c2);

    //                vertices[vPos + 8] = Transform(new Vector4(
    //                    x + 1, y + 1, HeightData[x + 1, y + 1], 1.0f));
    //                Color c4 = ColorData[x + 1, y + 1];
    //                vertices[vPos + 9] = ColorToVector(c4);

    //                vertices[vPos + 10] = Transform(new Vector4(
    //                    x + 1, y, HeightData[x + 1, y], 1.0f));
    //                vertices[vPos + 11] = ColorToVector(c3);

    //                vPos += 12;
    //            }
    //        }

    //        _vertexCount = vertices.Length / 2;

    //        _hmVertexBuffer = Buffer.Create(_device, BindFlags.VertexBuffer, vertices);

    //        Vector4[] bboxVertices = new Vector4[8] 
    //        {
    //            Transform(new Vector4(0, 0, 0, 1.0f)),
    //            Transform(new Vector4(0, 0, _hmDepth, 1.0f)),
    //            Transform(new Vector4(0, _hmHeight, 0, 1.0f)),
    //            Transform(new Vector4(0, _hmHeight, _hmDepth, 1.0f)),
    //            Transform(new Vector4(_hmWidth, 0, 0, 1.0f)),
    //            Transform(new Vector4(_hmWidth, 0, _hmDepth, 1.0f)),
    //            Transform(new Vector4(_hmWidth, _hmHeight, 0, 1.0f)),
    //            Transform(new Vector4(_hmWidth, _hmHeight, _hmDepth, 1.0f))
    //        };

    //        _boundingBox = new BoundingBox(_device, bboxVertices);
    //        _coordinateBox = new CoordinateBox(_device, bboxVertices);

    //        _dataLoaded = true;
    //    }

    //    protected override bool Resize()
    //    {
    //        try
    //        {
    //            bool baseResized = base.Resize();
    //            if (!baseResized) return false;

    //            return true;
    //        }
    //        catch (Exception)
    //        {
    //            return false;
    //        }
    //    }

    //    public override void Draw()
    //    {
    //        base.Draw();

    //        if (_device == null) return;

    //        var context = _device.ImmediateContext;

    //        Matrix worldProjView = _orbitCamera.WorldProjView;
    //        worldProjView.Transpose();
    //        _renderParams.WorldProjView = worldProjView;
    //        _renderParams.Brightness = Brightness;

    //        if (_dataLoaded)
    //        {
    //            context.UpdateSubresource(ref _renderParams, _hmRenderParamsBuffer);
    //            context.UpdateSubresource(ref _heightMapParams, _hmHeightMapParamsBuffer);

    //            context.InputAssembler.InputLayout = _hmInputLayout;
    //            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_hmVertexBuffer, Utilities.SizeOf<Vector4>() * 2, 0));
    //            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

    //            context.VertexShader.Set(_hmVertexShader);
    //            context.VertexShader.SetConstantBuffer(0, _hmRenderParamsBuffer);
    //            context.VertexShader.SetConstantBuffer(1, _hmHeightMapParamsBuffer);
    //            context.PixelShader.Set(_hmPixelShader);
    //            context.PixelShader.SetConstantBuffer(0, _hmRenderParamsBuffer);

    //            context.OutputMerger.SetRenderTargets(_depthView, _renderView);

    //            context.Draw(_vertexCount, 0);
    //        }

    //        CompleteDraw();
    //    }

    //    private Vector4 Transform(Vector4 Original)
    //    {
    //        float x = _startX + (Original.X * _renderSize.X / _hmWidth);
    //        float y = _startY + (Original.Y * _renderSize.Y / _hmHeight);
    //        float z = _startZ + (Original.Z * _renderSize.Z / _hmDepth);

    //        return new Vector4(x, y, z, 1.0f);
    //    }
    //    private Vector4 ColorToVector(Color Original)
    //    {
    //        return new Vector4(Original.R / 255f, Original.G / 255f, Original.B / 255f, Original.A / 255f);
    //    }
    //}
}
