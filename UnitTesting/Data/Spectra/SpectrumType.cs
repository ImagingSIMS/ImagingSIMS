using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImagingSIMS.Data.Spectra;
using ImagingSIMS.Common.Registry;

namespace UnitTesting.Data.Spectra
{
    [TestClass]
    public class TestSpectrumType
    {
        [TestMethod]
        public void TestGetFilterForDefaultProgram()
        {
            var expectedBioTof = "Bio-ToF Spectra Files (.xyt, .dat)|*.xyt;*.dat";
            var expectedJ105 = "Ionoptika compressed V2 files (.zip, .IonoptikaIA2DspectrV2)|*.zip;*.IonoptikaIA2DspectrV2";
            var expectedCamecaNanoSIMS = "Cameca NanoSIMS Spectra Files (.im)|*.im";
            var expectedCameca1280 = "Cameca 1280 Spectra Files (.imp)|*.imp";

            Assert.AreEqual($"{expectedBioTof}|{expectedJ105}|{expectedCamecaNanoSIMS}|{expectedCameca1280}",
                SpectrumFileExtensions.GetFilterForDefaultProgram(DefaultProgram.BioToF));
            Assert.AreEqual($"{expectedJ105}|{expectedBioTof}|{expectedCamecaNanoSIMS}|{expectedCameca1280}",
                SpectrumFileExtensions.GetFilterForDefaultProgram(DefaultProgram.J105));
            Assert.AreEqual($"{expectedCamecaNanoSIMS}|{expectedCameca1280}|{expectedBioTof}|{expectedJ105}",
                SpectrumFileExtensions.GetFilterForDefaultProgram(DefaultProgram.CamecaNanoSIMS));
            Assert.AreEqual($"{expectedCameca1280}|{expectedCamecaNanoSIMS}|{expectedBioTof}|{expectedJ105}",
                SpectrumFileExtensions.GetFilterForDefaultProgram(DefaultProgram.Cameca1280));
            Assert.AreEqual(string.Empty, SpectrumFileExtensions.GetFilterForDefaultProgram(DefaultProgram.NotSpecified));
        }

        [TestMethod]
        public void TestGetTypeForFileExtension()
        {
            Assert.AreEqual(SpectrumType.BioToF, SpectrumFileExtensions.GetTypeForFileExtension("xyt"));
            Assert.AreEqual(SpectrumType.BioToF, SpectrumFileExtensions.GetTypeForFileExtension(".xyt"));
            Assert.AreEqual(SpectrumType.BioToF, SpectrumFileExtensions.GetTypeForFileExtension("dat"));
            Assert.AreEqual(SpectrumType.BioToF, SpectrumFileExtensions.GetTypeForFileExtension(".dat"));
            Assert.AreEqual(SpectrumType.J105, SpectrumFileExtensions.GetTypeForFileExtension("zip"));
            Assert.AreEqual(SpectrumType.J105, SpectrumFileExtensions.GetTypeForFileExtension(".zip"));
            Assert.AreEqual(SpectrumType.J105, SpectrumFileExtensions.GetTypeForFileExtension("IonoptikaIA2DspectrV2"));
            Assert.AreEqual(SpectrumType.J105, SpectrumFileExtensions.GetTypeForFileExtension(".IonoptikaIA2DspectrV2"));
            Assert.AreEqual(SpectrumType.Cameca1280, SpectrumFileExtensions.GetTypeForFileExtension("imp"));
            Assert.AreEqual(SpectrumType.Cameca1280, SpectrumFileExtensions.GetTypeForFileExtension(".imp"));
            Assert.AreEqual(SpectrumType.CamecaNanoSIMS, SpectrumFileExtensions.GetTypeForFileExtension("im"));
            Assert.AreEqual(SpectrumType.CamecaNanoSIMS, SpectrumFileExtensions.GetTypeForFileExtension(".im"));
            Assert.AreEqual(SpectrumType.None, SpectrumFileExtensions.GetTypeForFileExtension(""));
            Assert.AreEqual(SpectrumType.None, SpectrumFileExtensions.GetTypeForFileExtension("xytz"));
            Assert.AreEqual(SpectrumType.None, SpectrumFileExtensions.GetTypeForFileExtension(".xytz"));
        }
    }
}
