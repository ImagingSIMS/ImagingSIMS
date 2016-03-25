using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

using ImagingSIMS.Common;
using ImagingSIMS.Controls.ViewModels;
using ImagingSIMS.Data;
using ImagingSIMS.Data.Imaging;

using Microsoft.Win32;

namespace ImagingSIMS.Controls.BaseControls
{
    /// <summary>
    /// Interaction logic for Data2DDisplay.xaml
    /// </summary>
    //public partial class Data2DDisplay : UserControl
    //{
    //    public static readonly DependencyProperty DisplayItemProperty = DependencyProperty.Register("DisplayItem",
    //        typeof(Data2DDisplayViewModel), typeof(Data2DDisplay));

    //    public event RoutedEventHandler AnalyzeData
    //    {
    //        add { AddHandler(AnalyzeDataEvent, value); }
    //        remove { RemoveHandler(AnalyzeDataEvent, value); }
    //    }
    //    public static readonly RoutedEvent AnalyzeDataEvent = EventManager.RegisterRoutedEvent("AnalyzeData",
    //        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Data2DDisplay));

    //    public event RoutedEventHandler RemoveItemClick
    //    {
    //        add { AddHandler(RemoveItemClickEvent, value); }
    //        remove { RemoveHandler(RemoveItemClickEvent, value); }
    //    }
    //    public static readonly RoutedEvent RemoveItemClickEvent = EventManager.RegisterRoutedEvent("RemoveItemClick",
    //        RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Data2DDisplay));

    //    public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register("ImageWidth",
    //        typeof(double), typeof(Data2DDisplay));
    //    public static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register("ImageHeight",
    //        typeof(double), typeof(Data2DDisplay));

    //    public double ImageWidth
    //    {
    //        get { return (double)GetValue(ImageWidthProperty); }
    //        set { SetValue(ImageWidthProperty, value); }
    //    }
    //    public double ImageHeight
    //    {
    //        get { return (double)GetValue(ImageHeightProperty); }
    //        set { SetValue(ImageHeightProperty, value); }
    //    }

    //    public Data2DDisplayViewModel DisplayItem
    //    {
    //        get { return (Data2DDisplayViewModel)GetValue(DisplayItemProperty); }
    //        set { SetValue(DisplayItemProperty, value); }
    //    }

    //    public Data2DDisplay()
    //    {
    //        InitializeComponent();

    //        this.Loaded += Data2DDisplay_Loaded;
    //    }

    //    private void Data2DDisplay_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        DisplayItem.ImageTransformedWidth = ImageWidth * DisplayItem.Scale;
    //        DisplayItem.ImageTransformedHeight = ImageHeight * DisplayItem.Scale;
    //    }

    //    private void buttonShowColor_MouseEnter(object sender, RoutedEventArgs e)
    //    {
    //        popupSolidColorScale.IsOpen = true;
    //    }
    //    private void buttonPixelStats_Click(object sender, RoutedEventArgs e)
    //    {
    //        e.Handled = true;

    //        RaiseEvent(new RoutedEventArgs(AnalyzeDataEvent, this));
    //    }

    //    #region ScrollViewer
    //    Point? lastMousePositionOnTarget;
    //    Point? lastDragPoint;

    //    private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    //    {
    //        if (e.ExtentHeightChange != 0 || e.ExtentWidthChange != 0)
    //        {
    //            Point? targetBefore = null;
    //            Point? targetNow = null;

    //            if (!lastMousePositionOnTarget.HasValue)
    //            {
    //                if (DisplayItem.LastCenterPositionOnTarget.HasValue)
    //                {
    //                    var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
    //                    Point centerOfTargetNow = scrollViewer.TranslatePoint(centerOfViewport, image);

    //                    targetBefore = DisplayItem.LastCenterPositionOnTarget;
    //                    targetNow = centerOfTargetNow;
    //                }
    //            }
    //            else
    //            {
    //                targetBefore = lastMousePositionOnTarget;
    //                targetNow = Mouse.GetPosition(image);

    //                lastMousePositionOnTarget = null;
    //            }

    //            if (targetBefore.HasValue)
    //            {
    //                double dXInTargetPixels = targetNow.Value.X - targetBefore.Value.X;
    //                double dYInTargetPixels = targetNow.Value.Y - targetBefore.Value.Y;

    //                double multiplicatorX = e.ExtentWidth / image.Width;
    //                double multiplicatorY = e.ExtentHeight / image.Height;

    //                double newOffsetX = scrollViewer.HorizontalOffset - dXInTargetPixels * multiplicatorX;
    //                double newOffsetY = scrollViewer.VerticalOffset - dYInTargetPixels * multiplicatorY;

    //                if (double.IsNaN(newOffsetX) || double.IsNaN(newOffsetY))
    //                {
    //                    return;
    //                }

    //                scrollViewer.ScrollToHorizontalOffset(newOffsetX);
    //                scrollViewer.ScrollToVerticalOffset(newOffsetY);
    //            }
    //        }
    //    }
    //    private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    //    {
    //        e.Handled = true;

    //        if (scrollViewer == null) return;

    //        lastMousePositionOnTarget = Mouse.GetPosition(image);

    //        double originalWidth = image.ActualWidth / DisplayItem.Scale;
    //        double originalHeight = image.ActualHeight / DisplayItem.Scale;

    //        if (e.Delta > 0)
    //        {
    //            DisplayItem.Scale += 0.25f;
    //        }
    //        if (e.Delta < 0)
    //        {
    //            DisplayItem.Scale -= 0.25f;
    //        }

    //        if (DisplayItem.Scale < 1) DisplayItem.Scale = 1;

    //        DisplayItem.ImageTransformedWidth = originalWidth * DisplayItem.Scale;
    //        DisplayItem.ImageTransformedHeight = originalHeight * DisplayItem.Scale;

    //        //var centerOfViewport = new Point(scrollViewer.ViewportWidth / 2, scrollViewer.ViewportHeight / 2);
    //        //lastCenterPositionOnTarget = scrollViewer.TranslatePoint(centerOfViewport, image);

    //        scrollViewer.ScrollToHorizontalOffset(lastMousePositionOnTarget.Value.X);
    //        scrollViewer.ScrollToVerticalOffset(lastMousePositionOnTarget.Value.Y);
    //    }
    //    private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    //    {

    //    }
    //    private void ScrollViewer_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    //    {
    //        var mousePos = e.GetPosition(scrollViewer);
    //        if (mousePos.X <= scrollViewer.ViewportWidth && mousePos.Y < scrollViewer.ViewportHeight) //make sure we still can use the scrollbars
    //        {
    //            scrollViewer.Cursor = Cursors.SizeAll;
    //            lastDragPoint = mousePos;
    //            Mouse.Capture(scrollViewer);
    //        }
    //    }
    //    private void ScrollViewer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    //    {

    //    }
    //    private void ScrollViewer_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    //    {
    //        scrollViewer.Cursor = Cursors.Arrow;
    //        scrollViewer.ReleaseMouseCapture();
    //        lastDragPoint = null;

    //        e.Handled = true;
    //    }
    //    private void ScrollViewer_MouseMove(object sender, MouseEventArgs e)
    //    {
    //        if (e.LeftButton == MouseButtonState.Pressed)
    //        {
    //            DisplayImage di = new DisplayImage(DisplayItem.DisplayImageSource, DisplayItem.DataSource.DataName);
    //            DataObject obj = new DataObject("DisplayImage", di);
    //            DragDrop.DoDragDrop(this, obj, DragDropEffects.Copy);
    //            return;
    //        }
    //        if (lastDragPoint.HasValue)
    //        {
    //            Point posNow = e.GetPosition(scrollViewer);

    //            double dX = posNow.X - lastDragPoint.Value.X;
    //            double dY = posNow.Y - lastDragPoint.Value.Y;

    //            lastDragPoint = posNow;

    //            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - dX);
    //            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - dY);
    //         }
    //    }
    //    #endregion

    //    private void scrollViewer_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    //    {
    //        e.Handled = true;
    //    }

    //    private void cmCopyScale_Click(object sender, RoutedEventArgs e)
    //    {
    //        BitmapSource source = imageColorScale.Source as BitmapSource;
    //        if (source == null) return;

    //        Clipboard.SetImage(source);

    //        // ContextMenu doesn't have logical ClosableTabItem parent so this won't send the status message
    //        // and result in an exception
    //        //ClosableTabItem.SendStatusUpdate(this, "Color scale copied to clipboard.");
    //    }
    //    private void cmCopyImage_Click(object sender, RoutedEventArgs e)
    //    {
    //        BitmapSource source = DisplayItem.DisplayImageSource as BitmapSource;
    //        if (source == null) return;

    //        Clipboard.SetImage(source);

    //        // ContextMenu doesn't have logical ClosableTabItem parent so this won't send the status message
    //        // and result in an exception
    //        //ClosableTabItem.SendStatusUpdate(this, "Image copied to clipboard.");
    //    }

    //    private void contentButtonRemove_Click(object sender, RoutedEventArgs e)
    //    {
    //        e.Handled = true;

    //        RaiseEvent(new RoutedEventArgs(RemoveItemClickEvent, this));
    //    }
    //    private void contentButtonSave_Click(object sender, RoutedEventArgs e)
    //    {
    //        SaveFileDialog sfd = new SaveFileDialog();
    //        sfd.Filter = "Bitmap Images (.bmp)|*.bmp";

    //        if (sfd.ShowDialog() != true) return;

    //        BitmapSource source = DisplayItem.DisplayImageSource as BitmapSource;
    //        if (source == null) return;

    //        source.Save(sfd.FileName);
    //    }
    //    private void contentButtonCopy_Click(object sender, RoutedEventArgs e)
    //    {
    //        BitmapSource bs = image.Source as BitmapSource;
    //        if (bs == null) throw new ArgumentException("Invalid ImageSource");

    //        Clipboard.SetImage(bs);
    //    }
    //    private void contentButtonStats_Click(object sender, RoutedEventArgs e)
    //    {
    //        e.Handled = true;

    //        RaiseEvent(new RoutedEventArgs(AnalyzeDataEvent, this));
    //    }
    //    private void contentButtonResetSaturation_Click(object sender, RoutedEventArgs e)
    //    {
    //        DisplayItem.Saturation = DisplayItem.InitialSaturation;
    //    }
    //}
}