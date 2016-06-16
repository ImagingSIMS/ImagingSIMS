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
    public class BoundingBox : IDisposable
    {
        Device _device;

        Buffer _vertexBuffer;
        Buffer _indexBuffer;

        VertexShader _vsBox;
        InputLayout _ilBox;
        PixelShader _psBox;

        Buffer _bufferParams;

        string _effectPath;

        public static event OnSetBoundingBoxVerticesEventHandler OnSetBoundingBoxVertices;

        public BoundingBox(Device Device, Vector4[] Vertices)
        {
            _effectPath = @"Shaders\BoundingBox";

            _device = Device;
            CreateBoundingBox(Vertices);
            LoadShaders();
        }

        private void CreateBoundingBox(Vector4[] Vertices)
        {
            Vector4[] vertices = new Vector4[16];

            for (int i = 0; i < 8; i++)
            {
                vertices[i * 2] = TranslateBoxVector(Vertices[i]);
                vertices[(i * 2) + 1] = new Vector4(0.0f, 0.8f, 0.2f, 1.0f);
            }

            _vertexBuffer = Buffer.Create(_device, BindFlags.VertexBuffer, vertices);

            short[] indices = new short[24]
            {
                0, 1,
                2, 3,
                0, 2,
                1, 3,

                4, 5,
                6, 7,
                4, 6,
                5, 7,

                0, 4,
                1, 5,
                2, 6,
                3, 7
            };

            _indexBuffer = Buffer.Create(_device, BindFlags.IndexBuffer, indices);

            // Pass non-translated original vertices used to create bounding box
            OnSetBoundingBoxVertices?.Invoke(this, new SetBoundingBoxVerticesEventArgs(Vertices));
        }

        private Vector4 TranslateBoxVector(Vector4 Original)
        {
            float x = Original.X;
            float y = Original.Y;
            float z = Original.Z;

            x *= 1.01f;
            y *= 1.01f;
            z *= 1.01f;

            return new Vector4(x, y, z, 1.0f);
        }

        private void LoadShaders()
        {
            _bufferParams = new Buffer(_device, Marshal.SizeOf(typeof(Matrix)), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            var vsByteCode = ShaderBytecode.FromFile(_effectPath + ".vso");
            _vsBox = new VertexShader(_device, vsByteCode);

            _ilBox = new InputLayout(_device, ShaderSignature.GetInputOutputSignature(vsByteCode), new[]
            {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
            });

            var psByteCode = ShaderBytecode.FromFile(_effectPath + ".pso");
            _psBox = new PixelShader(_device, psByteCode);
        }

        public void Update(Matrix WorldProjView)
        {
            var context = _device.ImmediateContext;

            context.UpdateSubresource(ref WorldProjView, _bufferParams, 0);
        }

        public void Draw()
        {
            var context = _device.ImmediateContext;

            context.InputAssembler.InputLayout = _ilBox;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertexBuffer, Marshal.SizeOf(typeof(Vector4)) * 2, 0));
            context.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

            context.VertexShader.Set(_vsBox);
            context.VertexShader.SetConstantBuffer(0, _bufferParams);
            context.PixelShader.Set(_psBox);

            context.DrawIndexed(24, 0, 0);
        }

        #region IDisposable
        ~BoundingBox()
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
                Disposer.RemoveAndDispose(ref _vsBox);
                Disposer.RemoveAndDispose(ref _ilBox);
                Disposer.RemoveAndDispose(ref _psBox);
                Disposer.RemoveAndDispose(ref _bufferParams);

                _disposed = true;
            }
        }
        #endregion
    }

    public delegate void OnSetBoundingBoxVerticesEventHandler(object sender, SetBoundingBoxVerticesEventArgs e);
    public class SetBoundingBoxVerticesEventArgs : EventArgs
    {
        public Vector4 Minima;
        public Vector4 Maximia;

        public SetBoundingBoxVerticesEventArgs(Vector4[] vertices)
            : base()
        {
            float minX = vertices.Select(v => v.X).Min();
            float maxX = vertices.Select(v => v.X).Max();

            float minY = vertices.Select(v => v.Y).Min();
            float maxY = vertices.Select(v => v.Y).Max();

            float minZ = vertices.Select(v => v.Z).Min();
            float maxZ = vertices.Select(v => v.Z).Max();

            Minima = new Vector4(minX, minY, minZ, 1.0f);
            Maximia = new Vector4(maxX, maxY, maxZ, 1.0f);
        }
        public SetBoundingBoxVerticesEventArgs(Vector4 minima, Vector4 maxima)
            : base()
        {
            Minima = minima;
            Maximia = maxima;
        }
    }
}
