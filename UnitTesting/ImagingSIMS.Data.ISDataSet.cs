using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTesting
{
    [TestClass]
    public class Data2DTesting
    {
        [TestMethod]
        public void ValidateUpscaleDimensions()
        {
            var original = GetSampleData2D(256, 256);
            var upscaledBilinear = original.Upscale(500, 500, true);

            Assert.IsTrue(upscaledBilinear.Width == 500 && upscaledBilinear.Height == 500);

            var upscaledBicubic = original.Upscale(500, 500, false);

            Assert.IsTrue(upscaledBicubic.Width == 500 && upscaledBicubic.Height == 500);
        }

        [TestMethod]
        public void ValdiateDownscaleDimensions()
        {
            var original = GetSampleData2D(512, 512);
            var downscaled = original.Downscale(300, 300);

            Assert.IsTrue(downscaled.Width == 300 && downscaled.Height == 300);
        }

        private Data2D GetSampleData2D(int sizeX, int sizeY)
        {
            Data2D sample = new Data2D(sizeX, sizeY) { DataName = "Sample" };
            for (int x = 0; x < sample.Width; x++)
            {
                for (int y = 0; y < sample.Height; y++)
                {
                    sample[x, y] = x + y;
                }
            }

            return sample;
        }
    }
}
