using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for ControlPointSelection.xaml
    /// </summary>
    public partial class ControlPointSelection : UserControl
    {
        bool _isDragging;

        public static readonly DependencyProperty ControlPointProperty = DependencyProperty.Register("ControlPoint",
            typeof(ControlPointViewModel), typeof(ControlPointSelection));

        public static readonly RoutedEvent ControlPointDraggingEvent = EventManager.RegisterRoutedEvent("ControlPointDragging",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ControlPointSelection));
        public static readonly RoutedEvent ControlPointRemovedEvent = EventManager.RegisterRoutedEvent("ControlPointRemoved",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ControlPointSelection));

        public ControlPointViewModel ControlPoint
        {
            get { return (ControlPointViewModel)GetValue(ControlPointProperty); }
            set { SetValue(ControlPointProperty, value); }
        }
        public event RoutedEventHandler ControlPointDragging
        {
            add { AddHandler(ControlPointDraggingEvent, value); }
            remove { RemoveHandler(ControlPointDraggingEvent, value); }
        }
        public event RoutedEventHandler ControlPointRemoved
        {
            add { AddHandler(ControlPointRemovedEvent, value); }
            remove { RemoveHandler(ControlPointDraggingEvent, value); }
        }

        public ControlPointSelection()
        {
            InitializeComponent();
        }

        public void SetVisualCoordinate(double visualWidth,
            double visualHeight, int matrixWidth, int matrixHeight)
        {
            ControlPoint.VisualX = (visualWidth * ControlPoint.CoordX / matrixWidth) - (ActualWidth / 2);
            ControlPoint.VisualY = (visualHeight * ControlPoint.CoordY / matrixHeight) - (ActualHeight / 2);
        }
        public void SetMatrixCoordinate(double visualWidth,
            double visualHeight, int matrixWidth, int matrixHeight)
        {
            ControlPoint.CoordX = (ControlPoint.VisualX + (ActualWidth / 2)) * matrixWidth / visualWidth;
            ControlPoint.CoordY = (ControlPoint.VisualY + (ActualHeight / 2)) * matrixHeight / visualHeight;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                _isDragging = true;
                e.Handled = true;
            }
            else if(e.ChangedButton == MouseButton.Right)
            {
                RaiseEvent(new RoutedEventArgs(ControlPointRemovedEvent, this));
                e.Handled = true;
            }
        }
        private void Border_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                RaiseEvent(new RoutedEventArgs(ControlPointDraggingEvent, this));
            }
        }
        private void Border_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                _isDragging = false;
                e.Handled = true;
            }
        }
    }

    public class ControlPointViewModel : INotifyPropertyChanged
    {
        int _id;
        double _coordX;
        double _coordY;
        double _visualX;
        double _visualY;
        Color _color;

        public int Id
        {
            get { return _id; }
            set
            {
                if(_id != value)
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
                if(_coordX != value)
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
                if(_coordY != value)
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
                if(_visualX != value)
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
                if(_visualY != value)
                {
                    _visualY = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Color Color
        {
            get { return _color; }
            set
            {
                if(_color != value)
                {
                    _color = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ControlPointViewModel(int id)
        {
            Id = id;
        }
        public ControlPointViewModel(int id, double coordX, double coordY)
        {
            Id = id;
            CoordX = coordX;
            CoordY = coordY;
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
                && c.VisualY == VisualY && c.Color == Color;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class ControlPoint
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        public override bool Equals(object obj)
        {
            var c = obj as ControlPoint;
            if (c == null) return false;

            return c.Id == Id && c.X == X && c.Y == Y;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
