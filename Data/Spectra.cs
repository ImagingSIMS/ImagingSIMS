using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using ImagingSIMS.Common;
using ImagingSIMS.Common.Math;

namespace ImagingSIMS.Data.Spectra
{

    //struct LoadWorkerArguments
    //{
    //    public string[] FilePaths;
    //    public int BinStart;
    //    public int BinEnd;

    //    public void LoadWorkerResults()
    //    {
    //        BinStart = 0;
    //        BinEnd = 0;
    //        FilePaths = null;
    //    }
    //}

    //public delegate void MSLoadUpdatedEventHandler(object sender, MSLoadUpdatedEventArgs args);
    //public class MSLoadUpdatedEventArgs : EventArgs
    //{
    //    public int Percentage;
    //    public EventArgs e;

    //    public MSLoadUpdatedEventArgs(int Percentage, EventArgs e)
    //        : base()
    //    {
    //        this.e = e;
    //        this.Percentage = Percentage;
    //    }
    //}

}