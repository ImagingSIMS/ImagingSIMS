using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImagingSIMS.Common.Dialogs;

namespace ImagingSIMS.Controls.Tabs
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ImagingSIMS.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ImagingSIMS.Controls;assembly=ImagingSIMS.Controls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:ClosableTabItem/>
    ///
    /// </summary>
    public class ClosableTabItem : TabItem, IDisposable
    {
        TabType _tabType;

        public TabType TabType { get { return _tabType; } }
        public bool ButtonIsEnabled
        {
            get
            {
                Button closeButton = base.GetTemplateChild("ButtonClose") as Button;
                if (closeButton != null)
                {
                    return closeButton.IsEnabled;
                }
                return false;
            }
            set
            {
                Button closeButton = base.GetTemplateChild("ButtonClose") as Button;
                if (closeButton != null)
                {
                    closeButton.IsEnabled = value;
                }
            }
        }
        static ClosableTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ClosableTabItem), new FrameworkPropertyMetadata(typeof(ClosableTabItem)));
        }

        public ClosableTabItem()
        {
            _tabType = TabType.Startup;

            if (TabType == TabType.Display || TabType == TabType.Fusion ||
                TabType == TabType.HeightMap || TabType == TabType.DataRegistration ||
                TabType == TabType.SpectrumCrop || TabType == TabType.DataDisplay ||
                TabType == TabType.Cluster)
            {
                this.AllowDrop = true;
                this.Drop += ClosableTabItem_Drop;
            }

            // Keep this separate since this appears only in PNNL version
            if (TabType == TabType.Ratio || TabType == TabType.ImageStitch)
            {
                this.AllowDrop = true;
                this.Drop += ClosableTabItem_Drop;
            }
        }
        public ClosableTabItem(TabType TabType)
        {
            _tabType = TabType;

            if (TabType == TabType.Display || TabType == TabType.Fusion ||
                TabType == TabType.HeightMap || TabType == TabType.DataRegistration ||
                TabType == TabType.SpectrumCrop || TabType == TabType.DataDisplay || 
                TabType == TabType.Cluster)
            {
                this.AllowDrop = true;
                this.Drop += ClosableTabItem_Drop;
            }

            // Keep this separate since this appears only in PNNL version
            if (TabType == TabType.Ratio || TabType == TabType.ImageStitch)
            {
                this.AllowDrop = true;
                this.Drop += ClosableTabItem_Drop;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Button closeButton = GetTemplateChild("ButtonClose") as Button;
            if (closeButton != null)
            {
                closeButton.Click += new RoutedEventHandler(closeButton_Click);
            }

            MenuItem miCloseTab = GetTemplateChild("menuItemCloseTab") as MenuItem;
            if(miCloseTab != null)
            {
                miCloseTab.Click += new RoutedEventHandler(closeButton_Click);
            }

            MenuItem miCloseAllTabs = GetTemplateChild("menuItemCloseAllTabs") as MenuItem;
            if (miCloseTab != null)
            {
                miCloseAllTabs.Click += new RoutedEventHandler(closeAllTabs_Click);
            }

            MenuItem miCloseAllButThisTab = GetTemplateChild("menuItemCloseAllButThisTab") as MenuItem;
            if (miCloseTab != null)
            {
                miCloseAllButThisTab.Click += new RoutedEventHandler(closeAllButThisTab_Click);
            }
        }

        public void Close()
        {
            this.RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
            Dispose();
        }
        protected void closeButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
            Dispose();
        }
        protected void closeAllTabs_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new CloseMultipleTabsRoutedEventArgs(false, CloseMultipleTabsEvent, this));
        }
        protected void closeAllButThisTab_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new CloseMultipleTabsRoutedEventArgs(true, CloseMultipleTabsEvent, this));
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Button closeButton = base.GetTemplateChild("ButtonClose") as Button;
                if (closeButton != null)
                {
                    closeButton.Click -= new RoutedEventHandler(closeButton_Click);
                }
            }
        }

        public static readonly RoutedEvent CloseTabEvent = EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ClosableTabItem));
        public static readonly RoutedEvent StatusUpdatedEvent = EventManager.RegisterRoutedEvent("StatusUpdated", RoutingStrategy.Bubble,
            typeof(StatusUpdatedRoutedEventHandler), typeof(ClosableTabItem));
        public static readonly RoutedEvent CloseMultipleTabsEvent = EventManager.RegisterRoutedEvent("CloseMultipleTabs", RoutingStrategy.Bubble,
            typeof(CloseMultipleTabsEventHandler), typeof(ClosableTabItem));

        public event RoutedEventHandler CloseTab
        {
            add { AddHandler(CloseTabEvent, value); }
            remove { RemoveHandler(CloseTabEvent, value); }
        }
        public event StatusUpdatedRoutedEventHandler StatusUpdated
        {
            add { AddHandler(StatusUpdatedEvent, value); }
            remove { RemoveHandler(StatusUpdatedEvent, value); }
        }
        public event CloseMultipleTabsEventHandler CloseMultipleTabs
        {
            add { AddHandler(CloseMultipleTabsEvent, value); }
            remove { RemoveHandler(CloseMultipleTabsEvent, value); }
        }

        void ClosableTabItem_Drop(object sender, DragEventArgs e)
        {
            bool didDrop = false;

            if (_tabType == TabType.DataDisplay)
            {
                DataDisplayTab d2dt = this.Content as DataDisplayTab;
                if (d2dt == null) return;

                else if (e.Data.GetDataPresent("Data2D"))
                {
                    ImagingSIMS.Data.Data2D d = e.Data.GetData("Data2D") as ImagingSIMS.Data.Data2D;
                    if (d == null) return;

                    d2dt.AddDataSource(d);

                    didDrop = true;
                }
            }
            if (_tabType == TabType.Cluster)
            {
                ClusterTab ct = this.Content as ClusterTab;
                if (ct == null) return;

                if (e.Data.GetDataPresent("Data2D"))
                {
                    ImagingSIMS.Data.Data2D d = e.Data.GetData("Data2D") as ImagingSIMS.Data.Data2D;
                    if (d == null) return;

                    ct.DropData(d);
                    didDrop = true;
                }
                else if (e.Data.GetDataPresent("DisplayImage"))
                {
                    ImagingSIMS.Data.Imaging.DisplayImage di = e.Data.GetData("DisplayImage") as ImagingSIMS.Data.Imaging.DisplayImage;
                    if (di == null) return;

                    ct.DropImage(di);
                    didDrop = true;
                }
            }
            if (_tabType == TabType.Display)
            {
                DisplayTab ot = this.Content as DisplayTab;
                if (ot == null) return;

                if (e.Data.GetDataPresent(DataFormats.Bitmap))
                {
                    BitmapSource bs = (BitmapSource)e.Data.GetData(DataFormats.Bitmap);
                    if (bs == null) return;

                    ot.AddImage(new ImagingSIMS.Data.Imaging.DisplayImage(bs, "Title"));
                    didDrop = true;
                }
                else if (e.Data.GetDataPresent("DisplayImage"))
                {
                    if (e.Source is DisplayTab && (DisplayTab)e.Source == ot) return;
                    ImagingSIMS.Data.Imaging.DisplayImage image = (ImagingSIMS.Data.Imaging.DisplayImage)e.Data.GetData("DisplayImage");
                    ot.AddImage(image.Clone());
                    didDrop = true;
                }
                
            }
            if (_tabType == TabType.Fusion)
            {
                FusionTab ft = this.Content as FusionTab;
                if (ft == null) return;

                if (e.Data.GetDataPresent(DataFormats.Bitmap))
                {
                    BitmapSource bs = (BitmapSource)e.Data.GetData(DataFormats.Bitmap);
                    if (bs == null) return;

                    FusionDropBox fdb = new FusionDropBox();
                    Nullable<bool> dialogResult = fdb.ShowDialog();
                    if (dialogResult == true)
                    {
                        FusionDropResult dropResult = fdb.DropResult;
                        if (dropResult == FusionDropResult.SEM)
                        {
                            ft.SetHighRes(bs);
                        }
                        else if (dropResult == FusionDropResult.SIMS)
                        {
                            ft.SetLowRes(bs);
                        }
                        didDrop = true;
                    }
                }
                else if (e.Data.GetDataPresent("DisplayImage"))
                {
                    ImagingSIMS.Data.Imaging.DisplayImage image = (ImagingSIMS.Data.Imaging.DisplayImage)e.Data.GetData("DisplayImage");
                    BitmapSource bs = (BitmapSource)image.Source;
                    if (bs == null) return;

                    FusionDropBox fdb = new FusionDropBox();
                    Nullable<bool> dialogResult = fdb.ShowDialog();
                    if (dialogResult == true)
                    {
                        FusionDropResult dropResult = fdb.DropResult;
                        if (dropResult == FusionDropResult.SEM)
                        {
                            ft.SetHighRes(bs);
                        }
                        else if (dropResult == FusionDropResult.SIMS)
                        {
                            ft.SetLowRes(bs);
                        }
                        didDrop = true;
                    }
                }
                else if(e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    didDrop = false;
                    e.Handled = false;
                    return;
                }
            }
            if (_tabType == TabType.HeightMap)
            {
                HeightMapTab hmt = this.Content as HeightMapTab;
                if (hmt == null) return;

                if (e.Data.GetDataPresent(DataFormats.Bitmap))
                {
                    BitmapSource bs = (BitmapSource)e.Data.GetData(DataFormats.Bitmap);
                    if (bs == null) return;

                    HeightMapDropBox hmdb = new HeightMapDropBox();
                    Nullable<bool> dialogResult = hmdb.ShowDialog();
                    if (dialogResult == true)
                    {
                        HeightMapDropResult dropResult = hmdb.DropResult;
                        if (dropResult == HeightMapDropResult.Height)
                        {
                            hmt.SetHeight(bs);
                        }
                        else if (dropResult == HeightMapDropResult.Color)
                        {
                            hmt.SetColor(bs);
                        }
                        didDrop = true;
                    }
                }
                else if (e.Data.GetDataPresent("BitmapSource"))
                {
                    BitmapSource bs = (BitmapSource)e.Data.GetData("BitmapSource");
                    if (bs == null) return;

                    HeightMapDropBox hmdb = new HeightMapDropBox();
                    Nullable<bool> dialogResult = hmdb.ShowDialog();
                    if (dialogResult == true)
                    {
                        HeightMapDropResult dropResult = hmdb.DropResult;
                        if (dropResult == HeightMapDropResult.Height)
                        {
                            hmt.SetHeight(bs);
                        }
                        else if (dropResult == HeightMapDropResult.Color)
                        {
                            hmt.SetColor(bs);
                        }
                        didDrop = true;
                    }
                }

                else if (e.Data.GetDataPresent("DisplayImage"))
                {
                    ImagingSIMS.Data.Imaging.DisplayImage image = (ImagingSIMS.Data.Imaging.DisplayImage)e.Data.GetData("DisplayImage");
                    BitmapSource bs = (BitmapSource)image.Source;
                    if (bs == null) return;

                    HeightMapDropBox hmdb = new HeightMapDropBox();
                    Nullable<bool> dialogResult = hmdb.ShowDialog();
                    if (dialogResult == true)
                    {
                        HeightMapDropResult dropResult = hmdb.DropResult;
                        if (dropResult == HeightMapDropResult.Height)
                        {
                            hmt.SetHeight(bs);
                        }
                        else if (dropResult == HeightMapDropResult.Color)
                        {
                            hmt.SetColor(bs);
                        }
                        didDrop = true;
                    }
                }
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    didDrop = false;
                    e.Handled = false;
                    return;
                }

            }
            if (_tabType == TabType.DataRegistration)
            {
                DataRegistrationTab drt = this.Content as DataRegistrationTab;
                if (drt == null) return;

                RegistrationDropBox rdb = new RegistrationDropBox();
                Nullable<bool> dialogResult = rdb.ShowDialog();
                if (dialogResult != true) return;

                RegistrationDropResult result = rdb.DropResult;

                if (e.Data.GetDataPresent(DataFormats.Bitmap))
                {
                    BitmapSource bs = (BitmapSource)e.Data.GetData(DataFormats.Bitmap);
                    if (bs == null) return;

                    if (result == RegistrationDropResult.Moving)
                    {
                        drt.SetMovingImage(bs);
                        didDrop = true;
                    }
                    else if (result == RegistrationDropResult.Fixed)
                    {
                        drt.SetFixedImage(bs);
                        didDrop = true;
                    }
                }
                else if (e.Data.GetDataPresent("DisplayImage"))
                {
                    ImagingSIMS.Data.Imaging.DisplayImage image = (ImagingSIMS.Data.Imaging.DisplayImage)e.Data.GetData("DisplayImage");
                    BitmapSource bs = (BitmapSource)image.Source;
                    if (bs == null) return;

                    if (result == RegistrationDropResult.Moving)
                    {
                        drt.SetMovingImage(bs);
                        didDrop = true;
                    }
                    else if (result == RegistrationDropResult.Fixed)
                    {
                        drt.SetFixedImage(bs);
                        didDrop = true;
                    }
                }
                else if (e.Data.GetDataPresent("Data2D"))
                {
                    ImagingSIMS.Data.Data2D d = (ImagingSIMS.Data.Data2D)e.Data.GetData("Data2D");
                    BitmapSource bs = ImagingSIMS.Data.Imaging.ImageHelper.CreateColorScaleImage(d, Data.Imaging.ColorScaleTypes.ThermalWarm);

                    if (result == RegistrationDropResult.Moving)
                    {
                        drt.SetMovingImage(bs);
                        didDrop = true;
                    }
                    else if (result == RegistrationDropResult.Fixed)
                    {
                        drt.SetFixedImage(bs);
                        didDrop = true;
                    }
                }
                else if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    didDrop = false;
                    e.Handled = false;
                    return;
                }                
            }
            if (TabType == TabType.SpectrumCrop)
            {
                SpectrumCropTab sct = this.Content as SpectrumCropTab;
                if (sct == null) return;

                if (e.Data.GetDataPresent("boolArray"))
                {
                    bool[,] maskData = e.Data.GetData("boolArray") as bool[,];
                    if (maskData == null) return;

                    if (sct.DropMask(maskData))
                    {
                        didDrop = true;
                        e.Handled = true;
                    }
                }

                else if (e.Data.GetDataPresent("FoundClusters"))
                {
                    ImagingSIMS.Data.ClusterIdentification.FoundClusters foundClusters =
                        e.Data.GetData("FoundClusters") as ImagingSIMS.Data.ClusterIdentification.FoundClusters;
                    if (foundClusters == null) return;

                    sct.DropMask(foundClusters.MaskArray);

                    didDrop = true;
                }
            }

            if (didDrop)
            {
                TabControl tc = this.Parent as TabControl;
                if (tc == null) return;

                tc.SelectedItem = this;
            }
        }

        public bool CanUndo
        {
            get
            {
                if (!IsIHistory) return false;

                IHistory iHistory = (IHistory)this.Content;

                return iHistory.CanUndo();
            }
        }
        public bool CanRedo
        {
            get
            {
                if (!IsIHistory) return false;

                IHistory iHistory = (IHistory)this.Content;

                return iHistory.CanRedo();
            }
        }

        public void Undo()
        {
            if (!IsIHistory) return;

            IHistory iHistory = (IHistory)this.Content;
            iHistory.Undo();
        }
        public void Redo()
        {
            if (!IsIHistory) return;

            IHistory iHistory = (IHistory)this.Content;
            iHistory.Redo();
        }

        private bool IsIHistory
        {
            get
            {
                try
                {
                    UserControl control = this.Content as UserControl;
                    if (control == null) return false;

                    if (control is IHistory)
                    {
                        return true;
                    }
                    return false;
                }
                catch (InvalidCastException)
                {
                    return false;
                }
            }
        }

        public static void SendStatusUpdate(DependencyObject initial, string Message)
        {
            ClosableTabItem cti = GetTabItem(initial);
            cti.RaiseEvent(new StatusUpdatedRoutedEventArgs(Message, StatusUpdatedEvent));
        }
        
        public static ClosableTabItem GetTabItem(DependencyObject initial)
        {
            DependencyObject current = initial;

            while(current != null && current.GetType() != typeof(TabControl))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            TabControl tabControl = (TabControl)current;
            if (tabControl == null)
            {
                throw new ArgumentException("Could not find a TabControl in the given VisualTree");
            }

            foreach(ClosableTabItem c in tabControl.Items)
            {
                if (c.Content == initial) return c;

                int numChildren = VisualTreeHelper.GetChildrenCount(c.Content as DependencyObject);
            }

            throw new ArgumentException("Could not find a ClosableTabItem in the given VisualTree");
        }

        public static bool IsClosableTabItemHosted(DependencyObject initial)
        {
            try
            {
                ClosableTabItem cti = GetTabItem(initial);
                return cti != null;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static ClosableTabItem Create(UserControl Control, TabType TabType)
        {
            ClosableTabItem cti = new ClosableTabItem(TabType);
            cti.Header = TabType.ToString();

            Control.Margin = new Thickness(0);
            Control.HorizontalAlignment = HorizontalAlignment.Left;
            Control.VerticalAlignment = VerticalAlignment.Top;

            cti.Content = Control;

            return cti;
        }
        public static ClosableTabItem Create(UserControl Control, TabType TabType, string Header)
        {
            ClosableTabItem cti = new ClosableTabItem(TabType);
            cti.Header = Header;

            Control.Margin = new Thickness(0);
            Control.HorizontalAlignment = HorizontalAlignment.Left;
            Control.VerticalAlignment = VerticalAlignment.Top;

            cti.Content = Control;

            return cti;
        }
        public static ClosableTabItem Create(UserControl Control, TabType TabType, bool Stretch)
        {
            ClosableTabItem cti = new ClosableTabItem(TabType);
            cti.Header = TabType.ToString();

            Control.Margin = new Thickness(0);
            if (Stretch)
            {
                Control.HorizontalAlignment = HorizontalAlignment.Stretch;
                Control.VerticalAlignment = VerticalAlignment.Stretch;
            }
            else
            {
                Control.HorizontalAlignment = HorizontalAlignment.Left;
                Control.VerticalAlignment = VerticalAlignment.Top;
            }


            cti.Content = Control;

            return cti;
        }
        public static ClosableTabItem Create(UserControl Control, TabType TabType, string Header, bool Stretch)
        {
            ClosableTabItem cti = new ClosableTabItem(TabType);
            cti.Header = Header;

            Control.Margin = new Thickness(0);
            if (Stretch)
            {
                Control.HorizontalAlignment = HorizontalAlignment.Stretch;
                Control.VerticalAlignment = VerticalAlignment.Stretch;
            }
            else
            {
                Control.HorizontalAlignment = HorizontalAlignment.Left;
                Control.VerticalAlignment = VerticalAlignment.Top;
            }

            cti.Content = Control;

            return cti;
        }
    }

    public enum TabType
    {
        Startup, Data, Spectrum,
        Component, Display,
        Rendering, Fusion, Settings,
        TableSelector, PCA,
        Crop, SEM, RenderObject,
        Correction, ZCorrection, SpectrumCrop,
        HeightMap, DataDisplay, Cluster,
        DataRegistration, DepthProfile,
        Ratio, ImageStitch
    }


    public delegate void StatusUpdatedRoutedEventHandler(object sender, StatusUpdatedRoutedEventArgs e);
    public class StatusUpdatedRoutedEventArgs : RoutedEventArgs
    {
        string _message;

        public string Message
        {
            get { return _message; }
        }

        public StatusUpdatedRoutedEventArgs(string Message)
            : base()
        {
            _message = Message;
        }
        public StatusUpdatedRoutedEventArgs(string Message, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            _message = Message;
        }
        public StatusUpdatedRoutedEventArgs(string Message, RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            _message = Message;
        }
    }

    public delegate void CloseMultipleTabsEventHandler(object sender, CloseMultipleTabsRoutedEventArgs e);
    public class CloseMultipleTabsRoutedEventArgs : RoutedEventArgs
    {
        bool _closeAllButThis;

        public bool CloseAllButThis
        {
            get { return _closeAllButThis; }
        }

        public CloseMultipleTabsRoutedEventArgs(bool closeAllButThis)
            : base()
        {
            _closeAllButThis = closeAllButThis;
        }
        public CloseMultipleTabsRoutedEventArgs(bool closeAllButThis, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            _closeAllButThis = closeAllButThis;
        }
        public CloseMultipleTabsRoutedEventArgs(bool closeAllButThis, RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
            _closeAllButThis = closeAllButThis;
        }
    }
}
