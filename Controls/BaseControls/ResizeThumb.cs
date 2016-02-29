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
    public class ResizeThumb : Thumb
    {
        public static readonly RoutedEvent ThumbResizedEvent = EventManager.RegisterRoutedEvent("ThumbResized",
            RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ResizeThumb));
        public event RoutedEventHandler ThumbMoved
        {
            add { AddHandler(ThumbResizedEvent, value); }
            remove { RemoveHandler(ThumbResizedEvent, value); }
        }

        public Control designerItem
        {
            get
            {
                Control item = this.DataContext as Control;
                return item;
            }
        }

        public ResizeThumb()
        {
            DragDelta += ResizeThumb_DragDelta;
        }

        void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (designerItem == null) return;

            double deltaVertical, deltaHorizontal;
            double newHeight, newWidth;
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Bottom:
                    deltaVertical = -e.VerticalChange;
                    newHeight = designerItem.ActualHeight - deltaVertical;
                    if (newHeight < 10) break;
                    if (newHeight >= 10 && newHeight < 450 - Canvas.GetTop(designerItem)) designerItem.Height = newHeight;
                    break;
                case VerticalAlignment.Top:
                    deltaVertical = e.VerticalChange;
                    if (designerItem.Height - deltaVertical < 10) break;
                    double newLoc = Canvas.GetTop(designerItem) + deltaVertical;
                    if (newLoc >= 0 && newLoc < 450)
                    {
                        Canvas.SetTop(designerItem, newLoc);
                        designerItem.Height -= deltaVertical;
                    }
                    break;
                default:
                    break;
            }
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    deltaHorizontal = e.HorizontalChange;
                    if (designerItem.Width - deltaHorizontal < 10) break;
                    double newLoc = Canvas.GetLeft(designerItem) + deltaHorizontal;
                    if (newLoc >= 0 && newLoc < 450)
                    {
                        Canvas.SetLeft(designerItem, newLoc);
                        designerItem.Width -= deltaHorizontal;
                    }
                    break;
                case HorizontalAlignment.Right:
                    deltaHorizontal = -e.HorizontalChange;
                    newWidth = designerItem.ActualWidth - deltaHorizontal;
                    if (newWidth < 10) break;
                    if (newWidth >= 10 && newWidth < 450 - Canvas.GetLeft(designerItem)) designerItem.Width = newWidth;
                    break;
                default:
                    break;
            }

            RaiseEvent(new RoutedEventArgs(ThumbResizedEvent, this));
            e.Handled = true;
        }
    }
}
