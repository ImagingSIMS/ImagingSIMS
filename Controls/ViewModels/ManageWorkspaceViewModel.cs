using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Data;

namespace ImagingSIMS.Controls.ViewModels
{
    public class ManageWorkspaceViewModel : INotifyPropertyChanged
    {
        ObservableCollection<Data2D> _workspaceData;

        public ObservableCollection<Data2D> WorkspaceData
        {
            get { return _workspaceData; }
            set
            {
                if(_workspaceData != value)
                {
                    _workspaceData = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ManageWorkspaceViewModel()
        {
            WorkspaceData = new ObservableCollection<Data2D>();
        }
        public ManageWorkspaceViewModel(IEnumerable<Data2D> currentData)
        {
            WorkspaceData = new ObservableCollection<Data2D>(currentData);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
