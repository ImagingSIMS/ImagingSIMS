using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ImagingSIMS.Controls.ViewModels;

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for ControlPointSelection.xaml
    /// </summary>
    public partial class ControlPointSelection : UserControl
    {
        public static double TargetWidth { get; set; }
        public static double TargetHeight { get; set; }

        bool _isDragging;

        static ControlPointSelection()
        {
            TargetWidth = 40;
            TargetHeight = 40;
        }

        public static readonly DependencyProperty ControlPointProperty = DependencyProperty.Register("ControlPoint",
            typeof(ControlPointViewModel), typeof(ControlPointSelection));
        public static readonly DependencyProperty SelectionColorProperty = DependencyProperty.Register("SelectionColor",
            typeof(Color), typeof(ControlPointSelection));

        public static readonly RoutedEvent ControlPointDraggingEvent = EventManager.RegisterRoutedEvent("ControlPointDragging",
            RoutingStrategy.Bubble, typeof(ControlPointDragRoutedEventHandler), typeof(ControlPointSelection));
        public static readonly RoutedEvent ControlPointRemovedEvent = EventManager.RegisterRoutedEvent("ControlPointRemoved",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ControlPointSelection));

        public ControlPointViewModel ControlPoint
        {
            get { return (ControlPointViewModel)GetValue(ControlPointProperty); }
            set { SetValue(ControlPointProperty, value); }
        }
        public Color SelectionColor
        {
            get { return (Color)GetValue(SelectionColorProperty); }
            set { SetValue(SelectionColorProperty, value); }
        }
        public event ControlPointDragRoutedEventHandler ControlPointDragging
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
                RaiseEvent(new ControlPointDragRoutedEventArgs(e, ControlPointDraggingEvent, this));
                e.Handled = true;
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

    public delegate void ControlPointDragRoutedEventHandler(object sender, ControlPointDragRoutedEventArgs e);
    public class ControlPointDragRoutedEventArgs : RoutedEventArgs
    {
        public MouseEventArgs DragArgs { get; set; }

        public ControlPointDragRoutedEventArgs(MouseEventArgs dragArgs) : base() { DragArgs = dragArgs; }
        public ControlPointDragRoutedEventArgs(MouseEventArgs dragArgs, RoutedEvent routedEvent) : base(routedEvent) { DragArgs = dragArgs; }
        public ControlPointDragRoutedEventArgs(MouseEventArgs dragArgs, RoutedEvent routedEvent, object source) : base(routedEvent, source) { DragArgs = dragArgs; }
    }
}
