using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using ImagingSIMS.Common;
using ImagingSIMS.Controls;
using ImagingSIMS.Data;

namespace ImagingSIMS.MainApplication
{
    public class ImagingSIMSApplication : DependencyObject
    {
        public static readonly DependencyProperty WorkspaceProperty = DependencyProperty.Register("Workspace",
            typeof(Workspace), typeof(ImagingSIMSApplication));

        public Workspace Workspace
        {
            get { return (Workspace)GetValue(WorkspaceProperty); }
            set { SetValue(WorkspaceProperty, value); }
        }
    }

    public interface IImagingSIMSApplicaiton
    {
        void OpenWorkspace();
        void SaveWorkspace();
        void CloseWorkspace();
    }
}
