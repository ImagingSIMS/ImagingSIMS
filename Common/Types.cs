using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ImagingSIMS.Common
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public POINT(Point p)
        {
            x = (int)p.X;
            y = (int)p.Y;
        }

        public int x;
        public int y;
    }
    [StructLayout(LayoutKind.Sequential)]
    struct CUSTOMVERTEXNORMAL
    {
        public float x, y, z;
        public byte a, r, g, b;
        public float nx, ny, nz;
    }

   
    public static class HRESULT
    {
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void Check(int hr)
        {
            Marshal.ThrowExceptionForHR(hr);
        }
    }
}
