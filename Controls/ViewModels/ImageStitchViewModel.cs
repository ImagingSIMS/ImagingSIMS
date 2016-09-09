using System.ComponentModel;
using ImagingSIMS.Data;

namespace ImagingSIMS.Controls.ViewModels
{
    public class ImageStitchItemViewModel : INotifyPropertyChanged
    {
        int _indexX;
        int _indexY;
        Data3D _dataItem;

        public int IndexX
        {
            get { return _indexX; }
            set
            {
                if (_indexX != value)
                {
                    _indexX = value;
                    NotifyPropertyChanged("IndexX");
                }
            }
        }
        public int IndexY
        {
            get { return _indexY; }
            set
            {
                if (_indexY != value)
                {
                    _indexY = value;
                    NotifyPropertyChanged("IndexY");
                }
            }
        }
        public Data3D DataItem
        {
            get { return _dataItem; }
            set
            {
                if (_dataItem != value)
                {
                    _dataItem = value;
                    NotifyPropertyChanged("DataItem");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageStitchItemViewModel()
        {

        }
        public ImageStitchItemViewModel(int x, int y)
        {
            IndexX = x;
            IndexY = y;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
