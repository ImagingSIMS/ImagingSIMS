using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.Controls
{
    public interface IAvailableSpectra
    {
        /// <summary>
        /// Gets the selected spectra in the control that implements the interface.
        /// </summary>
        /// <returns></returns>
        List<Spectrum> GetSelectedSpectra();
        /// <summary>
        /// Gets the selected spectra in the control that implements the interface.
        /// </summary>
        /// <returns></returns>
        List<Spectrum> GetAvailableSpectra();
        /// <summary>
        /// Removes the specified spectra from the control that implements the interface.
        /// </summary>
        /// <param name="spectraToRemove">Spectra to remove</param>
        void RemoveSpectra(IEnumerable<Spectrum> spectraToRemove);
        /// <summary>
        /// Removes the specified spectra from the control that implements the interface.
        /// </summary>
        /// <param name="spectraToRemove">Spectra to remove</param>
        void RemoveSpectra(Spectrum[] spectraToRemove);
        /// <summary>
        /// Adds the specifid spectrum to the control that implements the interface
        /// </summary>
        /// <param name="spectrumToAdd">Spectrum to add</param>
        void AddSpectrum(Spectrum spectrumToAdd);
        /// <summary>
        /// Adds the specifid spectra to the control that implements the interface
        /// </summary>
        /// <param name="spectrumToAdd">Spectra to add</param>
        void AddSpectra(IEnumerable<Spectrum> spectraToAdd);
        /// <summary>
        /// Adds the specifid spectra to the control that implements the interface
        /// </summary>
        /// <param name="spectrumToAdd">Spectra to add</param>
        void AddSpectra(Spectrum[] spectraToAdd);
    }
}
