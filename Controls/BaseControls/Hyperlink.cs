using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

namespace ImagingSIMS.Controls.BaseControls
{
    class Hyperlink : TextBlock
    {
        public static readonly DependencyProperty LinkTargetProperty = DependencyProperty.Register("LinkTarget",
            typeof(string), typeof(Hyperlink));

        public string LinkTarget
        {
            get { return (string)GetValue(LinkTargetProperty); }
            set { SetValue(LinkTargetProperty, value); }
        }

        static Hyperlink()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Hyperlink), new FrameworkPropertyMetadata(typeof(Hyperlink)));
        }

        public Hyperlink()
        {
            MouseDown += Hyperlink_MouseDown;
        }

        private void Hyperlink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                System.Diagnostics.Process.Start(LinkTarget);
            }
        }
    }
}
