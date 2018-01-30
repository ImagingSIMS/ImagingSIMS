using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using ImagingSIMS.Data.Imaging;

namespace UnitTesting
{
    [TestClass]
    public class GPUShaders
    {
        [TestMethod]
        public void VerifyShadersCompile()
        {
            var device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            Assert.IsNotNull(device);

            var factory = new ImageShaderFactory(device);
            Assert.IsNotNull(factory);

            foreach (ColorScaleTypes colorScale in Enum.GetValues(typeof(ColorScaleTypes)))
            {
                var shader = factory.GetShader(colorScale);
                Assert.IsNotNull(shader);
            }
        }
    }
}
