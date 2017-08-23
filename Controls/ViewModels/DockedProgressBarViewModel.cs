using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSIMS.Controls.ViewModels
{
    public class DockedProgressBarViewModel : INotifyPropertyChanged
    {
        bool _isProgressRunning;
        int _progress;
        string _message;

        public bool IsProgressRunning
        {
            get { return _isProgressRunning; }
            set
            {
                if (_isProgressRunning != value)
                {
                    _isProgressRunning = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int Progress
        {
            get { return _progress; }
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                if(_message != value)
                {
                    _message = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DockedProgressBarViewModel()
        {
            Message = string.Empty;
        }
    }
}
