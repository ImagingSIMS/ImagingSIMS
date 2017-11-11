using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ImagingSIMS.Data.Spectra
{
    public interface ICamecaSpectrum
    {
        Data2D FromSpecies(CamecaSpecies species, BackgroundWorker bw = null);
        Task<Data2D> FromSpeciesAsync(CamecaSpecies species);
        Data2D FromSpecies(CamecaSpecies species, out float max, BackgroundWorker bw = null);
        ICollection<Data2D> FromSpecies(CamecaSpecies species, string tableBaseName, bool omitNumbering, BackgroundWorker bw = null);
        Task<ICollection<Data2D>> FromSpeciesAsync(CamecaSpecies species, string tableBaseName, bool omitNumbering);
        Data2D FromSpecies(CamecaSpecies species, int layer, string tableBaseName, bool omitNumbering, BackgroundWorker bw = null);
        Task<Data2D> FromSpeciesAsync(CamecaSpecies species, int layer, string tableBaseName, bool omitNumbering);
        Data3D FromSpecies(CamecaSpecies species, string tableName, BackgroundWorker bw = null);
        Task<Data3D> FromSpeciesAsync(CamecaSpecies species, string tableName);
    }
}
