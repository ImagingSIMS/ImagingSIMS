using System.ComponentModel;

namespace ImagingSIMS.Data.Spectra
{
    public enum SpectrumType
    {
        J105,

        [Description("Bio-ToF")]
        BioToF,

        Generic,

        None,

        [Description("CAMECA 1280")]
        Cameca1280,

        [Description("CAMECA NanoSIMS")]
        CamecaNanoSIMS
    }
}
