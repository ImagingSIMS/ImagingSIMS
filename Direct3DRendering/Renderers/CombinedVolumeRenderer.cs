using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Direct3DRendering.Controls;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace ImagingSIMS.Direct3DRendering.Renderers
{
    public class CombinedVolumeRenderer : Renderer
    {
        string _effectPath;

        bool[,,] _clippedVoxels;

        public CombinedVolumeRenderer(RenderWindow Window)
            : base(Window)
        {
            _renderType = RenderType.CombinedVolume;
        }

        #region IDisposable
        ~CombinedVolumeRenderer()
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
                // TODO: Add calls to Disposer

                _disposed = true;
            }
            base.Dispose(Disposing);
        }
        #endregion

        public override void InitializeRenderer()
        {
            _effectPath = @"Shaders\CombinedVolume";

            base.InitializeRenderer();
        }
        protected override void InitializeShaders()
        {
            var effectByteCode = ShaderBytecode.CompileFromFile(_effectPath, "fx_5_0", ShaderFlags.None, EffectFlags.None);
            
        }

        RenderParams _previousParams = new RenderParams();
        public override void Update(bool targetYAxisOrbiting)
        {
            if(_clippedVoxels ==  null || 
                _previousParams.MinClipCoords != _renderParams.MinClipCoords || 
                _previousParams.MaxClipCoords != _renderParams.MaxClipCoords)
            {
                //_clippedVoxels = new bool[000]
            }
            
            base.Update(targetYAxisOrbiting);
        }
    }
}
