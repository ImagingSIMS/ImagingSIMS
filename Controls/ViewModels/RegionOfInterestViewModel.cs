using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ImagingSIMS.Controls.ViewModels
{
    public class RegionOfInterestViewModel : INotifyPropertyChanged
    {
        Point _dragStart;

        double _left;
        double _top;
        double _height;
        double _width;

        double _visualLeft;
        double _visualTop;
        double _visualHeight;
        double _visualWidth;

        bool _hasRegionOfInterest;

        public double Left
        {
            get { return _left; }
            set
            {
                if (_left != value)
                {
                    _left = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Top
        {
            get { return _top; }
            set
            {
                if (_top != value)
                {
                    _top = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    NotifyPropertyChanged();
                }
            }

        }

        public double VisualLeft
        {
            get { return _visualLeft; }
            set
            {
                if (_visualLeft != value)
                {
                    _visualLeft = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double VisualTop
        {
            get { return _visualTop; }
            set
            {
                if (_visualTop != value)
                {
                    _visualTop = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double VisualWidth
        {
            get { return _visualWidth; }
            set
            {
                if (_visualWidth != value)
                {
                    _visualWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public double VisualHeight
        {
            get { return _visualHeight; }
            set
            {
                if (_visualHeight != value)
                {
                    _visualHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool HasRegionOfInterest
        {
            get { return _hasRegionOfInterest; }
            set
            {
                if (_hasRegionOfInterest != value)
                {
                    _hasRegionOfInterest = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Rect Rectangle
        {
            get { return new Rect(Left, Top, Width, Height); }
        }

        public RegionOfInterestViewModel()
        {
            _dragStart = new Point();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Reset(Point startPoint)
        {
            VisualLeft = startPoint.X;
            VisualTop = startPoint.Y;
            VisualWidth = 0;
            VisualHeight = 0;

            _dragStart = startPoint;

            HasRegionOfInterest = true;
        }
        public void DragTo(Point endPoint)
        {
            if (_dragStart == null) return;

            VisualLeft = Math.Min(_dragStart.X, endPoint.X);
            VisualTop = Math.Min(_dragStart.Y, endPoint.Y);

            VisualWidth = Math.Abs(_dragStart.X - endPoint.X);
            VisualHeight = Math.Abs(_dragStart.Y - endPoint.Y);

            HasRegionOfInterest = true;
        }

        public void SetVisualCoordinate(double visualElementWidth,
            double visualElementHeight, int matrixWidth, int matrixHeight)
        {
            VisualLeft = visualElementWidth * Left / matrixWidth;
            VisualTop = visualElementHeight * Top / matrixHeight;
            VisualWidth = visualElementWidth * Width / matrixWidth;
            VisualHeight = visualElementHeight * Height / matrixHeight;
        }
        public void SetMatrixCoordinate(double visualElementWidth,
            double visualElementHeight, int matrixWidth, int matrixHeight)
        {
            Left = VisualLeft * matrixWidth / visualElementWidth;
            Top = VisualTop * matrixHeight / visualElementHeight;
            Width = VisualWidth * matrixWidth / visualElementWidth;
            Height = VisualHeight * matrixHeight / visualElementHeight;
        }

        public void ClearRegionOfInterest()
        {
            _dragStart = new Point();

            HasRegionOfInterest = false;

            VisualLeft = 0;
            VisualTop = 0;
            VisualWidth = 0;
            VisualHeight = 0;

            Left = 0;
            Top = 0;
            Width = 0;
            Height = 0;
        }
    }
}
