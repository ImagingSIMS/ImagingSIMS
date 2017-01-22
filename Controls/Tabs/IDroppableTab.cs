using System.Windows;

namespace ImagingSIMS.Controls.Tabs
{
    public interface IDroppableTab
    {
        void HandleDragDrop(object sender, DragEventArgs e);
    }
}
