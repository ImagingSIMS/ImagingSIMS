using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Data.Imaging;

namespace ImagingSIMS.Controls
{
    public interface IAvailableImageSeries
    {
        /// <summary>
        /// Gets the selected image series in the control that implements the interface.
        /// </summary>
        /// <returns>A list of the selected image series.</returns>
        List<DisplaySeries> GetSelectedImageSeries();

        /// <summary>
        /// Gets the available image series in the control that implements the interface.
        /// </summary>
        /// <returns>A list of the available image series.</returns>
        List<DisplaySeries> GetAvailableImageSeries();
    }
}
