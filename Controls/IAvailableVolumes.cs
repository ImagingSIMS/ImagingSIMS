using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Data.Rendering;

namespace ImagingSIMS.Controls
{
    public interface IAvailableVolumes
    {   
        /// <summary>
        /// Gets the selected volumes in the control that implements the interface.
        /// </summary>
        /// <returns></returns>
        List<Volume> GetSelectedVolumes();
        /// <summary>
        /// Gets all of the volumes available in the control that implements the interface.
        /// </summary>
        /// <returns></returns>
        List<Volume> GetAvailableVolumes();
        /// <summary>
        /// Removes the specified volumes from the control that implements the interface.
        /// </summary>
        /// <param name="volumesToRemove"></param>
        void RemoveVolumes(List<Volume> volumesToRemove);
        /// <summary>
        /// Removes the specified volumes from the control that implements the interface.
        /// </summary>
        /// <param name="volumesToRemove"></param>
        void RemoveVolumes(Volume[] volumesToRemove);
        /// <summary>
        /// Adds the specifed volumes to the control that implements the interface.
        /// </summary>
        /// <param name="volumesToAdd"></param>
        void AddVolumes(List<Volume> volumesToAdd);
        /// <summary>
        /// Adds the specifed volumes to the control that implements the interface.
        /// </summary>
        /// <param name="volumesToAdd"></param>
        void AddVolumes(Volume[] volumesToAdd);
        /// <summary>
        /// Replaces the specified volumes in the control that implements the interface.
        /// </summary>
        /// <param name="toReplace"></param>
        /// <param name="newvolume"></param>
        void ReplaceVolume(Volume toReplace, Volume newvolume);
    }
}
