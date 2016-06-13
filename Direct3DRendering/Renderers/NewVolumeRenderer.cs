using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Direct3DRendering.Controls;
using SharpDX;
using SharpDX.Direct3D11;

using Buffer = SharpDX.Direct3D11.Buffer;

namespace ImagingSIMS.Direct3DRendering.Renderers
{
    public class NewVolumeRenderer : Renderer
    {
        Buffer[] _volumeBuffers;

        public NewVolumeRenderer(RenderWindow Window) 
            : base(Window)
        {
        }

        protected override void InitializeShaders()
        {
            throw new NotImplementedException();
        }
    }
}
