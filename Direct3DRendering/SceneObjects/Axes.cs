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
using SharpDX.Windows;

using Device = SharpDX.Direct3D11.Device;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace ImagingSIMS.Direct3DRendering.SceneObjects
{
    public class Axes : IDisposable
    {
        Device _device;
        FeatureLevel _featureLevel;

        string _effectPath;

        VertexShader _axesVertexShader;
        PixelShader _axesPixelShader;
        InputLayout _axesInputLayout;

        Buffer _axesVertexBuffer;

        Buffer _axesParamsBuffer;

        public Axes(Device Device)
        {
            _device = Device;

            _featureLevel = Device.FeatureLevel;
            _effectPath = @"Shaders\Axes";
            
            CreateVertices();
            LoadShaders();
        }

        private void CreateVertices()
        {
            Vector4[] vertices = new Vector4[12]
            {
                new Vector4(-100.0f,0.0f,0.0f,1.0f), new Vector4(1.0f,0.0f,0.0f,1.0f),
                new Vector4(100.0f,0.0f,0.0f,1.0f), new Vector4(1.0f,0.0f,0.0f,1.0f),

                new Vector4(0.0f,-100.0f,0.0f,1.0f), new Vector4(0.0f,1.0f,0.0f,1.0f),
                new Vector4(0.0f,100.0f,0.0f,1.0f), new Vector4(0.0f,1.0f,0.0f,1.0f),

                new Vector4(0.0f,0.0f,-100.0f,1.0f), new Vector4(0.0f,0.0f,1.0f,1.0f),
                new Vector4(0.0f,0.0f,100.0f,1.0f), new Vector4(0.0f,0.0f,1.0f,1.0f),
            };

            _axesVertexBuffer = Buffer.Create(_device, BindFlags.VertexBuffer, vertices);
        }
        private void LoadShaders()
        {
            _axesParamsBuffer = new Buffer(_device, Marshal.SizeOf(typeof(Matrix)), ResourceUsage.Default,
                BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            var vsByteCode = ShaderBytecode.FromFile(_effectPath + ".vso");
            _axesVertexShader = new VertexShader(_device, vsByteCode);

            _axesInputLayout = new InputLayout(_device, ShaderSignature.GetInputSignature(vsByteCode), new[]
                {
                    new InputElement("POSITION",0,Format.R32G32B32A32_Float,0,0),
                    new InputElement("COLOR",0,Format.R32G32B32A32_Float,16,0)
                });

            var psByteCode = ShaderBytecode.FromFile(_effectPath + ".pso");
            _axesPixelShader = new PixelShader(_device, psByteCode);
        }

        public void Update(Matrix WorldProjView)
        {
            var context = _device.ImmediateContext;

            context.UpdateSubresource(ref WorldProjView, _axesParamsBuffer, 0);
        }
        public void Draw()
        {
            var context = _device.ImmediateContext;

            context.InputAssembler.InputLayout = _axesInputLayout;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_axesVertexBuffer, Utilities.SizeOf<Vector4>() * 2, 0));
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.LineList;

            context.VertexShader.Set(_axesVertexShader);
            context.VertexShader.SetConstantBuffer(0, _axesParamsBuffer);
            context.PixelShader.Set(_axesPixelShader);

            context.Draw(6, 0);

        }
               #region IDisposable
        ~Axes()
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
                Disposer.RemoveAndDispose(ref _axesVertexShader);
                Disposer.RemoveAndDispose(ref _axesPixelShader);
                Disposer.RemoveAndDispose(ref _axesInputLayout);
                Disposer.RemoveAndDispose(ref _axesVertexBuffer);
                Disposer.RemoveAndDispose(ref _axesParamsBuffer);

                _disposed = true;
            }
        }
        #endregion
    }
}
