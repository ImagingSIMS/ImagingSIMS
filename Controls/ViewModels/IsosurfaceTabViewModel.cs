using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Data.Rendering;

namespace ImagingSIMS.Controls.ViewModels
{
    public class IsosurfaceTabViewModel : INotifyPropertyChanged
    {
        Volume _datVolume;
        int _isoValue;
        string _name;


        public void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
