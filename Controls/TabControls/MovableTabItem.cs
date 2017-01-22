using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImagingSIMS.Controls.Tabs;

namespace ImagingSIMS.Controls.TabControls
{
    public interface IMovableTabItem
    {
    }

    public class MovableTabItem : ClosableTabItem, IMovableTabItem
    {

    }
}
