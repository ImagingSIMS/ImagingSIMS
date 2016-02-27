using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ImagingSIMS.Data;

namespace ImagingSIMS.Controls
{
    public interface IAvailableTables
    {
        /// <summary>
        /// Gets the selected data tables in the control that implements the interface.
        /// </summary>
        /// <returns>A list of the selected tables.</returns>
        List<Data2D> GetSelectedTables();
        /// <summary>
        /// Gets all of the data tables available in the control that implements the interface.
        /// </summary>
        /// <returns>A list of all available tables.</returns>
        List<Data2D> GetAvailableTables();

        /// <summary>
        /// Removes the specified tables from the control that implements the interface.
        /// </summary>
        /// <param name="tablesToRemove">Tables to remove from the collection.</param>
        void RemoveTables(List<Data2D> tablesToRemove);

        /// <summary>
        /// Removes the specified tables from the control that implements the interface.
        /// </summary>
        /// <param name="tablesToRemove">Tables to remove from the collection.</param>
        void RemoveTables(Data2D[] tablesToRemove);

        /// <summary>
        /// Adds the specified tables to the control that implements the interface.
        /// </summary>
        /// <param name="tablesToAdd">Tables to add.</param>
        void AddTables(List<Data2D> tablesToAdd);

        /// <summary>
        /// Adds the specified tables to the control that implements the interface.
        /// </summary>
        /// <param name="tablesToAdd">Tables to add.</param>
        void AddTables(Data2D[] tablesToAdd);

        /// <summary>
        /// Replaces the specified table.
        /// </summary>
        /// <param name="tableToReplace">Table to remove.</param>
        /// <param name="newTable">Table to insert.</param>
        void ReplaceTable(Data2D tableToReplace, Data2D newTable);
    }

    public static class AvailableTablesHost
    {
        public static IAvailableTables AvailableTablesSource { get; set; }
    }
}
