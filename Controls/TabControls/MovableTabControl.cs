using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ImagingSIMS.Controls.Tabs;

namespace ImagingSIMS.Controls.TabControls
{
    public interface IMovableTabControl
    {
        void AddTab(ClosableTabItem tabItem);
        void RemoveRab(ClosableTabItem tabItem);
    }

    public class MovableTabControl : TabControl, IMovableTabControl
    {
        public void AddTab(ClosableTabItem tabItem)
        {
            throw new NotImplementedException();
        }

        public void RemoveRab(ClosableTabItem tabItem)
        {
            throw new NotImplementedException();
        }
    }
}
