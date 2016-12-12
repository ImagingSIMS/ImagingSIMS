using System;
using ImagingSIMS.Data.Colors;
using ImagingSIMS.Data.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTesting
{
    [TestClass]
    public class ColorSpaces
    {
        [TestMethod]
        public void VerifyRGBtoIHS()
        {
            for (int r = 0; r < 255; r++)
            {
                for (int g = 0; g < 255; g++)
                {
                    for (int b = 0; b < 255; b++)
                    {
                        var rgb = new RGB(r, g, b);

                        var ihs = ColorConversion.RGBtoIHS(rgb.Normalize());
                        var rgbBack = ColorConversion.IHStoRGB(ihs);

                        Assert.IsTrue(rgb.IsClose(rgbBack));
                    }
                }
            }
        }

        [TestMethod]
        public void VerifyRGBtoHSL()
        {
            for (int r = 0; r < 255; r++)
            {
                for (int g = 0; g < 255; g++)
                {
                    for (int b = 0; b < 255; b++)
                    {
                        var rgb = new RGB(r, g, b);

                        var hsl = ColorConversion.RGBtoHSL(rgb.Normalize());
                        var rgbBack = ColorConversion.HSLtoRGB(hsl);

                        Assert.IsTrue(rgb.IsClose(rgbBack, 2));
                    }
                }
            }
        }
    }
}
