using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImagingSIMS.Controls.BaseControls;

namespace ImagingSIMS.Controls.ViewModels
{
    public class ControlPointViewModel : INotifyPropertyChanged
    {
        int _id;
        double _coordX;
        double _coordY;
        double _visualX;
        double _visualY;

        public int Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double CoordX
        {
            get { return _coordX; }
            set
            {
                if (_coordX != value)
                {
                    _coordX = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double CoordY
        {
            get { return _coordY; }
            set
            {
                if (_coordY != value)
                {
                    _coordY = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double VisualX
        {
            get { return _visualX; }
            set
            {
                if (_visualX != value)
                {
                    _visualX = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double VisualY
        {
            get { return _visualY; }
            set
            {
                if (_visualY != value)
                {
                    _visualY = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ControlPointViewModel(int id)
        {
            Id = id;
        }
        public ControlPointViewModel(ControlPoint controlPoint)
        {
            Id = controlPoint.Id;
            CoordX = controlPoint.X;
            CoordY = controlPoint.Y;
        }

        public ControlPoint ToControlPoint()
        {
            return new ControlPoint()
            {
                Id = Id,
                X = CoordX,
                Y = CoordY
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            var c = obj as ControlPointViewModel;
            if (c == null) return false;

            return c.Id == Id && c.CoordX == CoordX
                && c.CoordY == CoordY && c.VisualX == VisualX
                && c.VisualY == VisualY;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public void SetVisualCoordinate(double visualElementWidth,
            double visualElementHeight, int matrixWidth, int matrixHeight)
        {
            VisualX = (visualElementWidth * CoordX / matrixWidth) - (ControlPointSelection.TargetWidth / 2);
            VisualY = (visualElementHeight * CoordY / matrixHeight) - (ControlPointSelection.TargetHeight / 2);
        }
        public void SetMatrixCoordinate(double visualElementWidth,
            double visualElementHeight, int matrixWidth, int matrixHeight)
        {
            CoordX = (VisualX + (ControlPointSelection.TargetWidth / 2)) * matrixWidth / visualElementWidth;
            CoordY = (VisualY + (ControlPointSelection.TargetHeight / 2)) * matrixHeight / visualElementHeight;
        }
    }
}
