using System.Windows;
using System.Windows.Controls;
using ImagingSIMS.Data;

namespace ImagingSIMS.Controls
{
    public class WorkspaceTab : UserControl
    {
        public static readonly DependencyProperty WorkspaceProperty = DependencyProperty.Register("Workspace",
            typeof(Workspace), typeof(WorkspaceTab));

        public Workspace Workspace
        {
            get { return (Workspace)GetValue(WorkspaceProperty); }
            set { SetValue(WorkspaceProperty, value); }
        }

        public WorkspaceTab()
        {

        }
        public WorkspaceTab(Workspace workspace)
        {
            Workspace = workspace;
        }
    }

    public class AvailableTablesTab : WorkspaceTab
    {
        ListViewDD _availableTablesControl;

        public AvailableTablesTab()
        {

        }
        public AvailableTablesTab(Workspace workspace, ListViewDD availableTables)
            :base(workspace)
        {
            _availableTablesControl = availableTables;
        }
    }
}
