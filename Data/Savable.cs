using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSIMS.Data
{
    public interface ISavable
    {
        /// <summary>
        /// Converts the object into a serialized version for saving as part of a stream. 
        /// The return array is prefixed with the int size of the entire array, not including 
        /// the prefix.
        /// </summary>
        /// <returns>Serialized version of the object as a size-prefixed byte array.</returns>
        byte[] ToByteArray();

        /// <summary>
        /// Populates the object with data from a serialized version.
        /// </summary>
        /// <param name="array">Non-size-prefixed serialized byte array.</param>
        void FromByteArray(byte[] array);
    }
}
