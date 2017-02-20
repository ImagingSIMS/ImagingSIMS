using System;

namespace ImagingSIMS.Data.Spectra
{
    public delegate void SpectrumLoadedEventHandler(object sender, SpectrumLoadedEvenArgs e);
    public class SpectrumLoadedEvenArgs : EventArgs
    {
        public SpectrumLoadedEvenArgs()
            : base()
        {
        }
    }
    public delegate void TablesGeneratedEventHandler(object sender, TablesGeneratedEventArgs e);
    public class TablesGeneratedEventArgs : EventArgs
    {
        public TablesGeneratedEventArgs()
            : base()
        {
        }
    }
}
