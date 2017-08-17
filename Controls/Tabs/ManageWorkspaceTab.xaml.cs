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
                    var selectedTables = listViewData.SelectedItems.Cast<Data2D>().ToList();

                    for (int i = 0; i < selectedTables.Count; i++)
                    {
                        var table = selectedTables[i];
                        int currentIndex = Workspace.Data.IndexOf(table);
                        int newIndex = currentIndex - 1;
                        if (newIndex < 0) break;

                        Workspace.Data.Remove(table);
                        Workspace.Data.Insert(newIndex, table);
                    }

                    foreach (var table in selectedTables)
                    {
                        listViewData.SelectedItems.Add(table);
                    }
                    break;
                case ListViewTarget.Components:
                    break;
                case ListViewTarget.ImageSeries:
                    break;
                case ListViewTarget.Spectra:
                    break;
                case ListViewTarget.Volumes:
                    break;
                case ListViewTarget.SEM:
                    break;
            }
        }
        private void MoveDown(ListViewTarget target)
        {
            switch (target)
            {
                case ListViewTarget.Data:
                    var selectedTables = listViewData.SelectedItems.Cast<Data2D>().ToList();

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
                    break;
                case ListViewTarget.ImageSeries:
                    break;
                case ListViewTarget.Spectra:
                    break;
                case ListViewTarget.Volumes:
                    break;
                case ListViewTarget.SEM:
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
                    break;
                case ListViewTarget.ImageSeries:
                    break;
                case ListViewTarget.Spectra:
                    break;
                case ListViewTarget.Volumes:
                    break;
                case ListViewTarget.SEM:
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

            return ListViewTarget.Data;
        }
        private ListViewTarget TargetFromListView(ListView listView)
        {
            if (listView == listViewData)
                return ListViewTarget.Data;

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
