using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ImagingSIMS.Controls.BaseControls
{
    public class MoveThumb : Thumb
    {
        public static readonly RoutedEvent ThumbMovedEvent = EventManager.RegisterRoutedEvent("ThumbMoved",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MoveThumb));
        public event RoutedEventHandler ThumbMoved
        {
            add { AddHandler(ThumbMovedEvent, value); }
            remove { RemoveHandler(ThumbMovedEvent, value); }
        }

        double _leftPoint;
        double _topPoint;

        public double LeftPoint
        {
            get { return _leftPoint; }
        }
        public double TopPoint
        {
            get { return _topPoint; }
        }

        public Control designerItem
        {
            get
            {
                Control item = this.DataContext as Control;
                return item;
            }
        }

        public MoveThumb()
        {
            DragDelta += MoveThumb_DragDelta;
        }

        void MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (designerItem == null) return;

            double left = Canvas.GetLeft(designerItem);
            double top = Canvas.GetTop(designerItem);

            double newLeft = left + e.HorizontalChange;
            double newTop = top + e.VerticalChange;

            if (newLeft >= 0 && newLeft + designerItem.ActualWidth <= 450)
            {
                Canvas.SetLeft(designerItem, newLeft);
                _leftPoint = newLeft;
            }
            if (newTop >= 0 && newTop + designerItem.ActualHeight <= 450)
            {
                Canvas.SetTop(designerItem, newTop);
                _topPoint = newTop;
            }

            RaiseEvent(new RoutedEventArgs(MouseMoveEvent, this));
        }
    }
}
