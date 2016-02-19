using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ImagingSIMS.Data
{   

    public class ISObject : INotifyPropertyChanged
    {        
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged(string PropertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
    }

    public abstract class Data : ISObject
    {
        protected string _name = "";
    }

    namespace Spectra
    {
        public enum FileType
        {
            CamecaAPM,
            J105, BioToF, QStar, CSV
        }
    }

}
