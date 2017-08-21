using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImagingSIMS.Common;
using ImagingSIMS.Common.Dialogs;
using ImagingSIMS.Controls.ViewModels;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;
using ImagingSIMS.Data.Rendering;
using ImagingSIMS.Data.Spectra;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Interaction logic for ReorderWorkspaceTab.xaml
    /// </summary>
    public partial class ManageWorkspaceTab : UserControl
    {
        ListViewTarget _lastFocusedTarget = ListViewTarget.Data;

        public static readonly DependencyProperty WorkspaceProperty = DependencyProperty.Register("Workspace",
            typeof(Workspace), typeof(ManageWorkspaceTab));

        public Workspace Workspace
        {
            get { return (Workspace)GetValue(WorkspaceProperty); }
            set { SetValue(WorkspaceProperty, value); }
        }

        public ManageWorkspaceTab()
        {
            Workspace = new Workspace();

            InitializeComponent();
        }
        public ManageWorkspaceTab(Workspace currentWorkspace)
        {
            Workspace = currentWorkspace;

            InitializeComponent();
        }

        private void buttonUp_Click(object sender, RoutedEventArgs e)
        {
            var target = TargetFromButton(sender as Button);
            MoveUp(target);
        }
        private void buttonDown_Click(object sender, RoutedEventArgs e)
        {
            var target = TargetFromButton(sender as Button);
            MoveDown(target);
        }
        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            var target = TargetFromButton(sender as Button);
            Delete(target);
        }

        private void grid_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (e.Key == Key.Up)
                MoveUp(_lastFocusedTarget);
            else if (e.Key == Key.Down)
                MoveDown(_lastFocusedTarget);
            else if (e.Key == Key.Delete)
                Delete(_lastFocusedTarget);

            else e.Handled = false;
        }

        private void MoveUp(ListViewTarget target)
        {            
            switch (target)
            {
                case ListViewTarget.Data:
                    var selectedData = listViewData.SelectedItems.Cast<Data2D>()
                        .OrderBy(i => listViewData.Items.IndexOf(i)).ToList();
                    

                    for (int i = 0; i < selectedData.Count; i++)
                    {
                        var table = selectedData[i];
                        int currentIndex = Workspace.Data.IndexOf(table);
                        int newIndex = currentIndex - 1;
                        if (newIndex < 0) break;

                        Workspace.Data.Remove(table);
                        Workspace.Data.Insert(newIndex, table);
                    }

                    foreach (var table in selectedData)
                    {
                        listViewData.SelectedItems.Add(table);
                    }
                    break;
                case ListViewTarget.Components:
                    var selectedComponents = listViewComponents.SelectedItems.Cast<ImageComponent>()
                        .OrderBy(i => listViewComponents.Items.IndexOf(i)).ToList();

                    for (int i = 0; i < selectedComponents.Count; i++)
                    {
                        var component = selectedComponents[i];
                        int currentIndex = Workspace.Components.IndexOf(component);
                        int newIndex = currentIndex - 1;
                        if (newIndex < 0) break;

                        Workspace.Components.Remove(component);
                        Workspace.Components.Insert(newIndex, component);
                    }

                    foreach (var component in selectedComponents)
                    {
                        listViewComponents.SelectedItems.Add(component);
                    }
                    break;
                case ListViewTarget.ImageSeries:
                    var selectedSeries = listViewImageSeries.SelectedItems.Cast<DisplaySeries>()
                        .OrderBy(i => listViewImageSeries.Items.IndexOf(i)).ToList();

                    for (int i = 0; i < selectedSeries.Count; i++)
                    {
                        var series = selectedSeries[i];
                        int currentIndex = Workspace.ImageSeries.IndexOf(series);
                        int newIndex = currentIndex - 1;
                        if (newIndex < 0) break;

                        Workspace.ImageSeries.Remove(series);
                        Workspace.ImageSeries.Insert(newIndex, series);
                    }

                    foreach (var series in selectedSeries)
                    {
                        listViewImageSeries.SelectedItems.Add(series);
                    }
                    break;
                case ListViewTarget.Spectra:
                    var selectedSpectra = listViewSpectra.SelectedItems.Cast<Spectrum>()
                        .OrderBy(i => listViewSpectra.Items.IndexOf(i)).ToList();

                    for (int i = 0; i < selectedSpectra.Count; i++)
                    {
                        var spectrum = selectedSpectra[i];
                        int currentIndex = Workspace.Spectra.IndexOf(spectrum);
                        int newIndex = currentIndex - 1;
                        if (newIndex < 0) break;

                        Workspace.Spectra.Remove(spectrum);
                        Workspace.Spectra.Insert(newIndex, spectrum);
                    }

                    foreach (var spectrum in selectedSpectra)
                    {
                        listViewSpectra.SelectedItems.Add(spectrum);
                    }
                    break;
                case ListViewTarget.Volumes:
                    var selectedVolumes = listViewVolumes.SelectedItems.Cast<Volume>()
                        .OrderBy(i => listViewVolumes.Items.IndexOf(i)).ToList();

                    for (int i = 0; i < selectedVolumes.Count; i++)
                    {
                        var volume = selectedVolumes[i];
                        int currentIndex = Workspace.Volumes.IndexOf(volume);
                        int newIndex = currentIndex - 1;
                        if (newIndex < 0) break;

                        Workspace.Volumes.Remove(volume);
                        Workspace.Volumes.Insert(newIndex, volume);
                    }

                    foreach (var volume in selectedVolumes)
                    {
                        listViewVolumes.SelectedItems.Add(volume);
                    }
                    break;
                case ListViewTarget.SEM:
                    var selectedSEMs = listViewSEM.SelectedItems.Cast<SEM>()
                        .OrderBy(i => listViewSEM.Items.IndexOf(i)).ToList();

                    for (int i = 0; i < selectedSEMs.Count; i++)
                    {
                        var sem = selectedSEMs[i];
                        int currentIndex = Workspace.SEMs.IndexOf(sem);
                        int newIndex = currentIndex - 1;
                        if (newIndex < 0) break;

                        Workspace.SEMs.Remove(sem);
                        Workspace.SEMs.Insert(newIndex, sem);
                    }

                    foreach (var sem in selectedSEMs)
                    {
                        listViewSEM.SelectedItems.Add(sem);
                    }
                    break;
            }
        }
        private void MoveDown(ListViewTarget target)
        {
            switch (target)
            {
                case ListViewTarget.Data:
                    var selectedTables = listViewData.SelectedItems.Cast<Data2D>()
                        .OrderBy(i => listViewData.Items.IndexOf(i)).ToList();

                    for (int i = selectedTables.Count - 1; i >= 0; i--)
                    {
                        var table = selectedTables[i];
                        int currentIndex = Workspace.Data.IndexOf(table);
                        int newIndex = currentIndex + 1;
                        if (newIndex >= Workspace.Data.Count) break;

                        Workspace.Data.Remove(table);
                        Workspace.Data.Insert(newIndex, table);
                    }

                    foreach (var table in selectedTables)
                    {
                        listViewData.SelectedItems.Add(table);
                    }
                    break;
                case ListViewTarget.Components:
                    var selectedComponents = listViewComponents.SelectedItems.Cast<ImageComponent>()
                        .OrderBy(i => listViewComponents.Items.IndexOf(i)).ToList();

                    for (int i = selectedComponents.Count - 1; i >= 0; i--)
                    {
                        var component = selectedComponents[i];
                        int currentIndex = Workspace.Components.IndexOf(component);
                        int newIndex = currentIndex + 1;
                        if (newIndex >= Workspace.Components.Count) break;

                        Workspace.Components.Remove(component);
                        Workspace.Components.Insert(newIndex, component);
                    }

                    foreach (var component in selectedComponents)
                    {
                        listViewComponents.SelectedItems.Add(component);
                    }
                    break;
                case ListViewTarget.ImageSeries:
                    var selectedSeries = listViewImageSeries.SelectedItems.Cast<DisplaySeries>()
                        .OrderBy(i => listViewImageSeries.Items.IndexOf(i)).ToList();

                    for (int i = selectedSeries.Count - 1; i >= 0; i--)
                    {
                        var series = selectedSeries[i];
                        int currentIndex = Workspace.ImageSeries.IndexOf(series);
                        int newIndex = currentIndex + 1;
                        if (newIndex >= Workspace.ImageSeries.Count) break;

                        Workspace.ImageSeries.Remove(series);
                        Workspace.ImageSeries.Insert(newIndex, series);
                    }

                    foreach (var series in selectedSeries)
                    {
                        listViewImageSeries.SelectedItems.Add(series);
                    }
                    break;
                case ListViewTarget.Spectra:
                    var selectedSpectra = listViewSpectra.SelectedItems.Cast<Spectrum>()
                        .OrderBy(i => listViewSpectra.Items.IndexOf(i)).ToList();

                    for (int i = selectedSpectra.Count - 1; i >= 0; i--)
                    {
                        var spectrum = selectedSpectra[i];
                        int currentIndex = Workspace.Spectra.IndexOf(spectrum);
                        int newIndex = currentIndex + 1;
                        if (newIndex >= Workspace.Spectra.Count) break;

                        Workspace.Spectra.Remove(spectrum);
                        Workspace.Spectra.Insert(newIndex, spectrum);
                    }

                    foreach (var table in selectedSpectra)
                    {
                        listViewSpectra.SelectedItems.Add(table);
                    }
                    break;
                case ListViewTarget.Volumes:
                    var selectedVolumes = listViewVolumes.SelectedItems.Cast<Volume>()
                        .OrderBy(i => listViewVolumes.Items.IndexOf(i)).ToList();

                    for (int i = selectedVolumes.Count - 1; i >= 0; i--)
                    {
                        var volume = selectedVolumes[i];
                        int currentIndex = Workspace.Volumes.IndexOf(volume);
                        int newIndex = currentIndex + 1;
                        if (newIndex >= Workspace.Volumes.Count) break;

                        Workspace.Volumes.Remove(volume);
                        Workspace.Volumes.Insert(newIndex, volume);
                    }

                    foreach (var table in selectedVolumes)
                    {
                        listViewVolumes.SelectedItems.Add(table);
                    }
                    break;
                case ListViewTarget.SEM:
                    var selectedSEMs = listViewSEM.SelectedItems.Cast<SEM>()
                        .OrderBy(i => listViewSEM.Items.IndexOf(i)).ToList();

                    for (int i = selectedSEMs.Count - 1; i >= 0; i--)
                    {
                        var sem = selectedSEMs[i];
                        int currentIndex = Workspace.SEMs.IndexOf(sem);
                        int newIndex = currentIndex + 1;
                        if (newIndex >= Workspace.SEMs.Count) break;

                        Workspace.SEMs.Remove(sem);
                        Workspace.SEMs.Insert(newIndex, sem);
                    }

                    foreach (var table in selectedSEMs)
                    {
                        listViewSEM.SelectedItems.Add(table);
                    }
                    break;
            }

        }
        private void Delete(ListViewTarget target)
        {
            var result = DialogBox.Show("Delete the selected items?", $"Click Ok to remove the selected {EnumEx.Get(target)} items.", "Delete", DialogIcon.Warning);
            if (result != true) return;

            switch (target)
            {
                case ListViewTarget.Data:
                    var selectedTables = listViewData.SelectedItems.Cast<Data2D>().ToList();

                    foreach (var table in selectedTables)
                    {
                        Workspace.Data.Remove(table);
                    }
                    break;
                case ListViewTarget.Components:
                    var selectedComponents = listViewComponents.SelectedItems.Cast<ImageComponent>().ToList();

                    foreach (var component in selectedComponents)
                    {
                        Workspace.Components.Remove(component);
                    }
                    break;
                case ListViewTarget.ImageSeries:
                    var selectedSeries = listViewImageSeries.SelectedItems.Cast<DisplaySeries>().ToList();

                    foreach (var series in selectedSeries)
                    {
                        Workspace.ImageSeries.Remove(series);
                    }
                    break;
                case ListViewTarget.Spectra:
                    var selectedSpectra = listViewSpectra.SelectedItems.Cast<Spectrum>().ToList();

                    foreach (var spectrum in selectedSpectra)
                    {
                        Workspace.Spectra.Remove(spectrum);
                    }
                    break;
                case ListViewTarget.Volumes:
                    var selectedVolumes = listViewVolumes.SelectedItems.Cast < Volume>().ToList();

                    foreach (var volume in selectedVolumes)
                    {
                        Workspace.Volumes.Remove(volume);
                    }
                    break;
                case ListViewTarget.SEM:
                    var selectedSEMs = listViewSEM.SelectedItems.Cast<SEM>().ToList();

                    foreach (var sem in selectedSEMs)
                    {
                        Workspace.SEMs.Remove(sem);
                    }
                    break;
            }
        }

        private void listView_GotFocus(object sender, RoutedEventArgs e)
        {
            _lastFocusedTarget = TargetFromListView(sender as ListView);
        }

        private ListViewTarget TargetFromButton(Button button)
        {
            if (button == buttonDataUp || button == buttonDataDown || button == buttonDataDelete)
                return ListViewTarget.Data;

            if (button == buttonComponentsUp || button == buttonComponentsDown || button == buttonComponentsDelete)
                return ListViewTarget.Components;

            if (button == buttonImageSeriesUp || button == buttonImageSeriesDown || button == buttonImageSeriesDelete)
                return ListViewTarget.ImageSeries;

            if (button == buttonSpectraUp || button == buttonSpectraDown || button == buttonSpectraDelete)
                return ListViewTarget.Spectra;

            if (button == buttonVolumesUp || button == buttonVolumesDown || button == buttonVolumesDelete)
                return ListViewTarget.Volumes;

            if (button == buttonSEMUp || button == buttonSEMDown || button == buttonSEMDelete)
                return ListViewTarget.SEM;

            return ListViewTarget.Data;
        }
        private ListViewTarget TargetFromListView(ListView listView)
        {
            if (listView == listViewData)
                return ListViewTarget.Data;

            if (listView == listViewComponents)
                return ListViewTarget.Components;

            if (listView == listViewImageSeries)
                return ListViewTarget.ImageSeries;

            if (listView == listViewSpectra)
                return ListViewTarget.Spectra;

            if (listView == listViewVolumes)
                return ListViewTarget.Volumes;

            if (listView == listViewSEM)
                return ListViewTarget.SEM;

            return ListViewTarget.Data;
        }
    }

    internal enum ListViewTarget
    {
        Data,
        Spectra,
        Volumes,
        Components,
        [Description("Image Series")]
        ImageSeries,
        SEM,
    }
}
