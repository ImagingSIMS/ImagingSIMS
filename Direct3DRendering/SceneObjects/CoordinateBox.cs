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

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ImagingSIMS.Direct3DRendering.SceneObjects
{
    public class CoordinateBox
    {
        Device _device;

        Buffer _vertexBuffer;
        Buffer _indexBuffer;

        VertexShader _vsCoordinateBox;
        InputLayout _ilCoordinateBox;
        PixelShader _psCoordinateBox;

        CoordinateBoxRenderParams _renderParams;
        Buffer _bufferParams;

        RasterizerState _rasteizerState;

        string _effectPath;

        public CoordinateBox(Device Device, Vector4[] Vertices)
        {
            _effectPath = @"Shaders\CoordinateBox";

            _renderParams = CoordinateBoxRenderParams.Empty;

            _device = Device;
            CreateCoordinateBox(Vertices);
            LoadShaders();
        }

        private void CreateCoordinateBox(Vector4[] Vertices)
        {
            Vector4[] vertices = new Vector4[16];

            float minX = 0; 
            float minY = 0; 
            float minZ = 0; 
            float maxX = 0; 
            float maxY = 0; 
            float maxZ = 0;

            foreach (Vector4 v in Vertices)
            {
                if (v.X < minX) minX = v.X;
                if (v.X > maxX) maxX = v.X;
                if (v.Y < minY) minY = v.Y;
                if (v.Y > maxY) maxY = v.Y;
                if (v.Z < minZ) minZ = v.Z;
                if (v.Z > maxZ) maxZ = v.Z;
            }

            for (int i = 0; i < 8; i++)
            {
                vertices[i * 2] = TranslateBoxVector(Vertices[i]);

                Vector4 color = new Vector4(1.0f);

                color.X = Vertices[i].X == minX ? 0.0f : 1.0f;
                color.Y = Vertices[i].Y == minY ? 0.0f : 1.0f;
                color.Z = Vertices[i].Z == minZ ? 0.0f : 1.0f;
                
                vertices[(i * 2) + 1] = color;
            }

            _vertexBuffer = Buffer.Create(_device, BindFlags.VertexBuffer, vertices);

            short[] indices = new short[36]
            {
                0,1,2, 3,2,1,
                0,4,1, 5,1,4,
                1,5,3, 7,3,5,
                3,2,7, 6,7,2,
                0,4,2, 6,2,4,
                4,5,7, 6,4,7
            };

            _indexBuffer = Buffer.Create(_device, BindFlags.IndexBuffer, indices);

            RasterizerStateDescription desc = new RasterizerStateDescription()
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                IsDepthClipEnabled = true
            };
            _rasteizerState = new RasterizerState(_device, desc);
        }
        private Vector4 TranslateBoxVector(Vector4 Original)
        {
            float x = Original.X;
            float y = Original.Y;
            float z = Original.Z;

            x *= 1.005f;
            y *= 1.005f;
            z *= 1.005f;

            return new Vector4(x, y, z, 1.0f);
        }

        private void LoadShaders()
        {
            _bufferParams = new Buffer(_device, Marshal.SizeOf(typeof(CoordinateBoxRenderParams)), ResourceUsage.Default,
               BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            var vsByteCode = ShaderBytecode.FromFile(_effectPath + ".vso");
            _vsCoordinateBox = new VertexShader(_device, vsByteCode);

            _ilCoordinateBox = new InputLayout(_device, ShaderSignature.GetInputOutputSignature(vsByteCode), new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
            });

            var psByteCode = ShaderBytecode.FromFile(_effectPath + ".pso");
            _psCoordinateBox = new PixelShader(_device, psByteCode);
        }

        public void Update(Matrix WorldProjView, float Transparency)
        {
            var context = _device.ImmediateContext;

            _renderParams.WorldProjView = WorldProjView;
            _renderParams.Transparency = Transparency;

            context.UpdateSubresource(ref _renderParams, _bufferParams, 0);
        }

        public void Draw()
        {
            var context = _device.ImmediateContext;

            context.InputAssembler.InputLayout = _ilCoordinateBox;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, Marshal.SizeOf(typeof(Vector4)) * 2, 0));
            context.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            context.Rasterizer.State = _rasteizerState;

            context.VertexShader.Set(_vsCoordinateBox);
            context.VertexShader.SetConstantBuffer(0, _bufferParams);
            context.PixelShader.Set(_psCoordinateBox);
            context.PixelShader.SetConstantBuffer(0, _bufferParams);

            context.DrawIndexed(36, 0, 0);
        }

        #region IDisposable
        ~CoordinateBox()
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

                Disposer.RemoveAndDispose(ref _vertexBuffer);
                Disposer.RemoveAndDispose(ref _indexBuffer);
                Disposer.RemoveAndDispose(ref _vsCoordinateBox);
                Disposer.RemoveAndDispose(ref _ilCoordinateBox);
                Disposer.RemoveAndDispose(ref _psCoordinateBox);
                Disposer.RemoveAndDispose(ref _bufferParams);

                _disposed = true;
            }
        }
        #endregion
    }

    [StructLayout(LayoutKind.Explicit, Size=80)]
    public struct CoordinateBoxRenderParams
    {
        [FieldOffset(0)]
        public Matrix WorldProjView;

        [FieldOffset(64)]
        public float Transparency;

        [FieldOffset(68)]
        private float padding1;

        [FieldOffset(72)]
        private float padding2;

        [FieldOffset(76)]
        private float padding3;

        public static CoordinateBoxRenderParams Empty
        {
            get
            {
                return new CoordinateBoxRenderParams()
                {
                    WorldProjView = Matrix.Identity,
                    Transparency = 0,
                    padding1 = 0,
                    padding2 = 0,
                    padding3 = 0
                };
            }
        }
    }
}
