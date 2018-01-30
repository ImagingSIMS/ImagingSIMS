using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    public static class ExtensionMethods
    {
        public static int Clamp(this int value, int min, int max)
        {
            return Math.Max(Math.Min(value, max), min);
        }
    }
}
