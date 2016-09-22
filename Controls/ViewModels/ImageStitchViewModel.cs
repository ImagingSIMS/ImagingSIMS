using System;
using System.ComponentModel;
using ImagingSIMS.Data;

namespace ImagingSIMS.Controls.ViewModels
{
    public class ImageStitchItemViewModel : INotifyPropertyChanged
    {
        int _indexX;
        int _indexY;
        int _offsetX;
        int _offsetY;
        Data3D _dataItem;

        public int IndexX
        {
            get { return _indexX; }
            set
            {
                if (_indexX != value)
                {
                    _indexX = value;
                    notifyPropertyChanged("IndexX");
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
                    notifyPropertyChanged("IndexY");
                }
            }
        }
        public int OffsetX
        {
            get { return _offsetX; }
            set
            {
                if (_offsetX != value)
                {
                    _offsetX = value;
                    notifyPropertyChanged("OffsetX");
                    notifyOffsetChanged(true, false);
                }
            }
        }
        public int OffsetY
        {
            get { return _offsetY; }
            set
            {
                if (_offsetY != value)
                {
                    _offsetY = value;
                    notifyPropertyChanged("OffsetY");
                    notifyOffsetChanged(false, true);
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
                    notifyPropertyChanged("DataItem");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event OffsetChangedEventHandler OffsetChanged;

        public ImageStitchItemViewModel()
        {

        }
        public ImageStitchItemViewModel(int x, int y)
        {
            IndexX = x;
            IndexY = y;
        }

        private void notifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void notifyOffsetChanged(bool isChangedX, bool isChangedY)
        {
            OffsetChanged?.Invoke(this, new OffsetChangedEventArgs(isChangedX, isChangedY));
        }
    }

    public class OffsetChangedEventArgs : EventArgs
    {
        public bool IsChangedX { get; set; }
        public bool IsChangedY { get; set; }

        public OffsetChangedEventArgs(bool isChangedX, bool isChangedY)
            : base()
        {
            IsChangedX = isChangedX;
            IsChangedY = isChangedY;
        }
    }
    public delegate void OffsetChangedEventHandler(object sender, OffsetChangedEventArgs e);
}