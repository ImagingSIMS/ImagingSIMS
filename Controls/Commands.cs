using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ImagingSIMS.Controls
{
    public static class CommonCommands
    {
        private static RoutedUICommand _saveImageSeries = new RoutedUICommand("Save Image Series", "SaveImageSeries", typeof(CommonCommands));

        public static RoutedUICommand SaveImageSeries
        {
            get { return _saveImageSeries; }
        }
    }
    public static class DataDisplayTabCommands
    {
        private static RoutedUICommand _applyColorScale = new RoutedUICommand("Apply Color Scale", "ApplyColorScale", typeof(DataDisplayTabCommands));
        private static RoutedUICommand _saveItems = new RoutedUICommand("Save Display Items", "SaveItems", typeof(DataDisplayTabCommands));
        private static RoutedUICommand _resetSaturations = new RoutedUICommand("Reset Saturation", "ResetSaturation", typeof(DataDisplayTabCommands));
        private static RoutedUICommand _applyLayerRange = new RoutedUICommand("Apply Layer Range", "ApplyLayerRange", typeof(DataDisplayTabCommands));
        private static RoutedUICommand _getSelectedVolumes = new RoutedUICommand("Get Selected Volumes", "GetSelectedVolume", typeof(DataDisplayTabCommands));
        private static RoutedUICommand _getSummedLayers = new RoutedUICommand("Get Summed Layers", "GetSummedLayers", typeof(DataDisplayTabCommands));
        private static RoutedUICommand _exportToWorkspace = new RoutedUICommand("Export to Workspace", "ExportToWorkspace", typeof(DataDisplayTabCommands));
        private static RoutedUICommand _transformData = new RoutedUICommand("Transform Data", "TransformData", typeof(DataDisplayTabCommands));

        public static RoutedUICommand ApplyColorScale
        {
            get { return _applyColorScale; }
        }
        public static RoutedUICommand SaveItems
        {
            get { return _saveItems; }
        }
        public static RoutedUICommand ResetSaturations
        {
            get { return _resetSaturations; }
        }
        public static RoutedUICommand ApplyLayerRange
        {
            get { return _applyLayerRange; }
        }
        public static RoutedUICommand GetSelectedVolumes
        {
            get { return _getSelectedVolumes; }
        }
        public static RoutedUICommand GetSummedLayers
        {
            get { return _getSummedLayers; }
        }
        public static RoutedUICommand ExportToWorkspace
        {
            get { return _exportToWorkspace; }
        }
        public static RoutedUICommand TransformData
        {
            get { return _transformData; }
        }
    }

    public static class SpectrumCommands
    {
        private static RoutedUICommand _deadTimeCorrect = new RoutedUICommand("Dead Time Correct", "DeadTimeCorrect", typeof(SpectrumCommands));

        public static RoutedUICommand DeadTimeCorrect { get { return _deadTimeCorrect; } }
    }
}
