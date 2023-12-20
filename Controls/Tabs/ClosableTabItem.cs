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

        public static readonly DependencyProperty IsCurrentDragTargetProperty = DependencyProperty.Register("IsCurrentDragTarget",
            typeof(bool), typeof(ClosableTabItem));

        public bool IsCurrentDragTarget
        {
            get { return (bool)GetValue(IsCurrentDragTargetProperty); }
            set { SetValue(IsCurrentDragTargetProperty, value); }
        }

        public ClosableTabItem()
            : this(TabType.Startup)
        {
        }
        public ClosableTabItem(TabType TabType)
        {
            _tabType = TabType;

            AllowDrop = true;
            Drop += ClosableTabItem_Drop;
            DragEnter += ClosableTabItem_DragEnter;
            DragLeave += ClosableTabItem_DragLeave;
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

                DragEnter -= ClosableTabItem_DragEnter;
                DragLeave -= ClosableTabItem_DragLeave;
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

        private void ClosableTabItem_Drop(object sender, DragEventArgs e)
        {
            IsCurrentDragTarget = false;

            var tab = Content as IDroppableTab;
            if (tab == null) return;

            tab.HandleDragDrop(sender, e);

            if(e.Handled)
            {
                var tc = Parent as TabControl;
                if (tc != null)
                {
                    tc.SelectedItem = this;
                }
            }
        }
        private void ClosableTabItem_DragEnter(object sender, DragEventArgs e)
        {
            IsCurrentDragTarget = true;
        }
        private void ClosableTabItem_DragLeave(object sender, DragEventArgs e)
        {
            IsCurrentDragTarget = false;
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

        /// <summary>
        /// Creates a ClosableTabItem for the given user control
        /// </summary>
        /// <param name="Control">Content for the TabItem</param>
        /// <param name="TabType">Type of TabItem to create</param>
        /// <returns>A ClosableTabItem that can be added to a TabControl</returns>
        public static ClosableTabItem Create(UserControl Control, TabType TabType)
        {
            ClosableTabItem cti = new ClosableTabItem(TabType);
            cti.Header = TabType.ToString();

            Control.Margin = new Thickness(0);
            Control.HorizontalAlignment = HorizontalAlignment.Stretch;
            Control.VerticalAlignment = VerticalAlignment.Stretch;

            cti.Content = Control;

            return cti;
        }
        /// <summary>
        /// Creates a ClosableTabItem for the given user control
        /// </summary>
        /// <param name="Control">Content for the TabItem</param>
        /// <param name="TabType">Type of TabItem to create</param>
        /// <param name="Header">Header to appear in the TabItem</param>
        /// <returns>A ClosableTabItem that can be added to a TabControl</returns>
        public static ClosableTabItem Create(UserControl Control, TabType TabType, string Header)
        {
            ClosableTabItem cti = new ClosableTabItem(TabType);
            cti.Header = Header;

            Control.Margin = new Thickness(0);
            Control.HorizontalAlignment = HorizontalAlignment.Stretch;
            Control.VerticalAlignment = VerticalAlignment.Stretch;

            cti.Content = Control;

            return cti;
        }
    }

    public enum TabType
    {
        Startup, Data, Spectrum,
        Component, Display,
        Rendering, Fusion, Settings, FusionPoint,
        TableSelector, PCA,
        Crop, SEM, RenderObject,
        Correction, ZCorrection, SpectrumCrop,
        HeightMap, DataDisplay, Cluster,
        DataRegistration, DepthProfile, EditWorkspace,
        Ratio, ImageStitch, DataMath, ImageOverlay
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
