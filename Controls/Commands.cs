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

    public static class DataMathCommands
    {
        private static RoutedUICommand _performOperation = new RoutedUICommand("Perform Math Operation", "PerformOperation", typeof(DataMathCommands));
        private static RoutedUICommand _addResultToWorkspace = new RoutedUICommand("Add Result To Workspace", "AddResultToWorkspace", typeof(DataMathCommands));
        private static RoutedUICommand _assignVariable = new RoutedUICommand("Assign Variable", "AssignVariable", typeof(DataMathCommands));
        private static RoutedUICommand _clearVariable = new RoutedUICommand("Clear Variable", "ClearVariable", typeof(DataMathCommands));
        private static RoutedUICommand _clearHistory = new RoutedUICommand("Clear History", "ClearHistory", typeof(DataMathCommands));

        public static RoutedUICommand PerformOperation { get { return _performOperation; } }
        public static RoutedUICommand AddResultToWorkspace { get { return _addResultToWorkspace; } }
        public static RoutedUICommand AssignVariable { get { return _assignVariable; } }
        public static RoutedUICommand ClearVariable { get { return _clearVariable; } }
        public static RoutedUICommand ClearHistory { get { return _clearHistory; } }
    }

    public static class RegistrationCommands
    {
        private static RoutedUICommand _clearSelection = new RoutedUICommand("Clear Selection", "ClearSelection", typeof(RegistrationCommands));
        private static RoutedUICommand _resetSaturation = new RoutedUICommand("Clear Selection", "ClearSelection", typeof(RegistrationCommands));
        private static RoutedUICommand _register = new RoutedUICommand("Register", "Register", typeof(RegistrationCommands));
        private static RoutedUICommand _cancelRegistration = new RoutedUICommand("Cancel Registration", "CancelRegistration", typeof(RegistrationCommands));
        private static RoutedUICommand _undoRegistration = new RoutedUICommand("Undo Registration", "UndoRegistration", typeof(RegistrationCommands));
        private static RoutedUICommand _addTable = new RoutedUICommand("Add Table", "AddTable", typeof(RegistrationCommands));
        private static RoutedUICommand _applyTransformToSelected = new RoutedUICommand("Apply transform to selected data", "ApplyTransformToSelected", typeof(RegistrationCommands));

        public static RoutedUICommand ClearSelection
        {
            get { return _clearSelection; }
        }
        public static RoutedUICommand ResetSaturation
        {
            get { return _resetSaturation; }
        }
        public static RoutedUICommand Register
        {
            get { return _register; }
        }
        public static RoutedUICommand CancelRegistration
        {
            get { return _cancelRegistration; }
        }
        public static RoutedUICommand UndoRegistration
        {
            get { return _undoRegistration; }
        }
        public static RoutedUICommand AddTable
        {
            get { return _addTable; }
        }
        public static RoutedUICommand ApplyTransformToSelected { get { return _applyTransformToSelected; } }

    }

    public static class FusionCommands
    {
        private static RoutedUICommand _fuse = new RoutedUICommand("Fuse", "Fuse", typeof(FusionCommands));

        public static RoutedUICommand Fuse
        {
            get { return _fuse; }
        }
    }
}
