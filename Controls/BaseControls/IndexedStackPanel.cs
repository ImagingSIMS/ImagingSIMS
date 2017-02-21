using System.Windows;
using System.Windows.Controls;

namespace ImagingSIMS.Controls.BaseControls
{
    public class IndexedStackPanel : StackPanel
    {
        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index",
            typeof(int), typeof(IndexedStackPanel));

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }
    }
}
