using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

using Microsoft.Win32;

namespace ImagingSIMS.Common.Registry
{
    using System.Collections;
    using System.Collections.Specialized;
    using Registry = Microsoft.Win32.Registry;

    public enum Options { None }

    public class SettingsManager
    {
        private static RegSettings _regSettings;

        public static RegSettings RegSettings
        {
            get
            {
                if(_regSettings == null)
                {
                    _regSettings = new RegSettings();
                }
                return _regSettings;
            }
            set
            {
                if (_regSettings != value)
                {
                    _regSettings = value;
                }
            }
        }
    }
    public class RegSettings : DependencyObject, IDisposable
    {
        // To add new setting:
        // 1. Add new DependencyProperty and Propery accessor
        // 2. Add line in SaveSettings()
        // 3. Add line in ReadSettings()
        RegistryKey keyFiles = Registry.CurrentUser.CreateSubKey("Software\\ImagingSIMS3\\RecentFiles");
        RegistryKey keyOptions = Registry.CurrentUser.CreateSubKey("Software\\ImagingSIMS3\\UserOptions");
        RegistryKey keyCrash = Registry.CurrentUser.CreateSubKey("Software\\ImagingSIMS3\\OnCrash");

        public static readonly DependencyProperty RecentFilesProperty = DependencyProperty.Register("RecentFiles",
            typeof(FileList), typeof(RegSettings));
        public static readonly DependencyProperty ShowStartupProperty = DependencyProperty.Register("ShowStartup",
            typeof(bool), typeof(RegSettings));
        public static readonly DependencyProperty HasCrashedProperty = DependencyProperty.Register("HasCrashed",
            typeof(bool), typeof(RegSettings));
        public static readonly DependencyProperty CrashFilePathProperty = DependencyProperty.Register("CrashFilePath",
            typeof(string), typeof(RegSettings));
        public static readonly DependencyProperty CrashDateTimeProperty = DependencyProperty.Register("CrashDateTime",
            typeof(string), typeof(RegSettings));
        public static readonly DependencyProperty SaveQuickLoadProperty = DependencyProperty.Register("SaveQuickLoad",
            typeof(bool), typeof(RegSettings));
        public static readonly DependencyProperty HideLoadDialogProperty = DependencyProperty.Register("HideLoadDialog",
            typeof(bool), typeof(RegSettings));
        public static readonly DependencyProperty ClearPluginDataProperty = DependencyProperty.Register("ClearPluginData",
            typeof(bool), typeof(RegSettings));
        public static readonly DependencyProperty DefaultProgramProperty = DependencyProperty.Register("DefaultSettings",
            typeof(DefaultProgram), typeof(RegSettings));
        public static readonly DependencyProperty StartWithTraceProperty = DependencyProperty.Register("StartWithTrace",
            typeof(bool), typeof(RegSettings));
        public static readonly DependencyProperty SuppressRegistrationWarningsProperty = DependencyProperty.Register("SuppressRegistrationWarnings",
            typeof(bool), typeof(RegSettings));
        public static readonly DependencyProperty DataDisplayWidthProperty = DependencyProperty.Register("DataDisplayWidth",
            typeof(double), typeof(RegSettings));

        public FileList RecentFiles
        {
            get { return (FileList)GetValue(RecentFilesProperty); }
            set { SetValue(RecentFilesProperty, value); }
        }
        public bool ShowStartup
        {
            get { return (bool)GetValue(ShowStartupProperty); }
            set { SetValue(ShowStartupProperty, value); }
        }
        public bool HasCrashed
        {
            get { return (bool)GetValue(HasCrashedProperty); }
            set { SetValue(HasCrashedProperty, value); }
        }
        public string CrashFilePath
        {
            get { return (string)GetValue(CrashFilePathProperty); }
            set { SetValue(CrashFilePathProperty, value); }
        }
        public string CrashDateTime
        {
            get { return (string)GetValue(CrashDateTimeProperty); }
            set { SetValue(CrashDateTimeProperty, value); }
        }
        public bool SaveQuickLoad
        {
            get { return (bool)GetValue(SaveQuickLoadProperty); }
            set { SetValue(SaveQuickLoadProperty, value); }
        }
        public bool HideLoadDialog
        {
            get { return (bool)GetValue(HideLoadDialogProperty); }
            set { SetValue(HideLoadDialogProperty, value); }
        }
        public bool ClearPluginData
        {
            get { return (bool)GetValue(ClearPluginDataProperty); }
            set { SetValue(ClearPluginDataProperty, value); }
        }
        public DefaultProgram DefaultProgram
        {
            get { return (DefaultProgram)GetValue(DefaultProgramProperty); }
            set { SetValue(DefaultProgramProperty, value); }
        }
        public bool StartWithTrace
        {
            get { return (bool)GetValue(StartWithTraceProperty); }
            set { SetValue(StartWithTraceProperty, value); }
        }
        public bool SuppressRegistrationWarnings
        {
            get { return (bool)GetValue(SuppressRegistrationWarningsProperty); }
            set { SetValue(SuppressRegistrationWarningsProperty, value); }
        }
        public double DataDisplayWidth
        {
            get { return (double)GetValue(DataDisplayWidthProperty); }
            set { SetValue(DataDisplayWidthProperty, value); }
        }

        public RegSettings()
        {
            RecentFiles = new FileList();
            RecentFiles.MaxSize = 9;
            RecentFiles.CollectionChanged += RecentFiles_CollectionChanged;

            SettingsManager.RegSettings = this;
        }

        private void RecentFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SaveFileList();
        }

        //Call this on workspace dispose and settings tab save
        public bool SaveSettings()
        {
            try
            {
                SaveFileList();

                keyOptions.SetValue("ShowStartup", BoolInt.Convert(ShowStartup));
                keyOptions.SetValue("SaveQuickLoad", BoolInt.Convert(SaveQuickLoad));
                keyOptions.SetValue("HideLoadDialog", BoolInt.Convert(HideLoadDialog));
                keyOptions.SetValue("ClearPluginData", BoolInt.Convert(ClearPluginData));
                keyOptions.SetValue("DefaultProgram", (int)DefaultProgram);
                keyOptions.SetValue("StartWithTrace", BoolInt.Convert(StartWithTrace));
                keyOptions.SetValue("SuppressRegistrationWarnings", BoolInt.Convert(SuppressRegistrationWarnings));
                keyOptions.SetValue("DataDisplayWidth", DataDisplayWidth);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        private void SaveFileList()
        {
            for (int i = 0; i < RecentFiles.Count; i++)
            {
                keyFiles.SetValue(string.Format("File{0}", (i + 1).ToString()), RecentFiles[i]);
            }
            //for (int i = RecentFiles.Count; i < RecentFiles.MaxSize; i++)
            //{
            //    if (keyFiles.GetValue(string.Format("File{0}", (i + 1).ToString())) != null)
            //    {
            //        keyFiles.SetValue(string.Format("File{0}", (i + 1).ToString()), string.Empty);
            //    }
            //}
        }
        //Call this in workspace constructor
        public bool ReadSettings()
        {
            try
            {
                int numRecentFiles = keyFiles.ValueCount;
                if (numRecentFiles > 0) RecentFiles.Clear();
                numRecentFiles = numRecentFiles < RecentFiles.MaxSize ? numRecentFiles : RecentFiles.MaxSize;

                for (int i = 0; i < numRecentFiles; i++)
                {
                    string file = (string)keyFiles.GetValue(string.Format("File{0}", (i + 1).ToString()), "");
                    if (file == String.Empty || file == "") continue;
                    RecentFiles.Insert(i, file);
                }

                ShowStartup = BoolInt.Convert((int)keyOptions.GetValue("ShowStartup", 1));
                SaveQuickLoad = BoolInt.Convert((int)keyOptions.GetValue("SaveQuickLoad", 1));
                HideLoadDialog = BoolInt.Convert((int)keyOptions.GetValue("HideLoadDialog", 0));
                ClearPluginData = BoolInt.Convert((int)keyOptions.GetValue("ClearPluginData", 0));
                DefaultProgram = (DefaultProgram)((int)keyOptions.GetValue("DefaultProgram", 1));
                StartWithTrace = BoolInt.Convert((int)keyOptions.GetValue("StartWithTrace", 0));
                SuppressRegistrationWarnings = BoolInt.Convert((int)keyOptions.GetValue("SuppressRegistrationWarnings", 0));
                DataDisplayWidth = double.Parse((string)keyOptions.GetValue("DataDisplayWidth", 225d));

                HasCrashed = BoolInt.Convert((int)keyCrash.GetValue("HasCrashed", 0));
                if (HasCrashed)
                {
                    CrashFilePath = (string)keyCrash.GetValue("CrashFilePath", "");
                    CrashDateTime = (string)keyCrash.GetValue("CrashDateTime", "");
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        //Call this on exception shutdown
        public bool SaveCrashSettings(string FilePath, out string Result)
        {
            Result = "Succeeded";
            try
            {
                HasCrashed = true;
                CrashFilePath = FilePath;
                CrashDateTime = DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString();

                keyCrash.SetValue("HasCrashed", BoolInt.Convert(true));
                keyCrash.SetValue("CrashFilePath", FilePath);
                keyCrash.SetValue("CrashDateTime", DateTime.Now.ToShortDateString() + "-" + DateTime.Now.ToShortTimeString());
            }
            catch (Exception ex)
            {
                Result = ex.Message;
                return false;
            }
            return true;
        }
        public bool ClearCrashSettings()
        {
            try
            {
                HasCrashed = false;
                CrashFilePath = "";
                CrashDateTime = "";

                keyCrash.SetValue("HasCrashed", BoolInt.Convert(false));
                keyCrash.SetValue("CrashFilePath", "");
                keyCrash.SetValue("CrashDateTime", "");
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    keyFiles.Close();
                    keyOptions.Close();
                    keyCrash.Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~RegSettings() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

    public class FileList : ObservableCollection<string>
    {
        public int MaxSize { get; set; }

        public new void Add(string file)
        {
            if (Contains(file))
            {
                int index = IndexOf(file);
                Move(index, 0);
            }
            else
            {
                if (Count >= MaxSize)
                {
                    if (Count > 0)
                    {
                        RemoveAt(Count - 1);
                    }
                }
                Insert(0, file);
            }
        }

        public override string ToString()
        {
            return $"FileList- Count: {Count} Max: {MaxSize}";
        }
    }
    internal static class BoolInt
    {
        public static bool Convert(int i)
        {
            if (i != 0 && i != 1) throw new InvalidCastException("Specified int value is not 0 or 1.");
            return i == 1;
        }
        public static int Convert(bool b)
        {
            return b ? 1 : 0;
        }
    }

    public class RegSetting<T>
    {
        T _value;
        Type _type;
        string _name;
        string _folder;
        string _path;

        public RegSetting(T Value, string Name, string Folder)
        {
            _value = Value;
            _name = Name;
            _folder = Folder;
            _path = "Software\\ImagingSIMS3\\" + Folder;
            _type = typeof(T);
        }

        public void Save(RegistryKey Key)
        {
            if (_type == typeof(string))
            {
                Key.SetValue(_name, _value);
            }
            else if (_type == typeof(int))
            {
                Key.SetValue(_name, _value);
            }
            else if (_type == typeof(bool))
            {
                Key.SetValue(_name, BoolInt.Convert((bool)(object)_value));
            }
        }

        public static implicit operator T(RegSetting<T> r)
        {
            return r._value;
        }
    }

    public enum DefaultProgram
    {
        [Description("J105")]
        J105 = 1,
        [Description("Bio-ToF")]
        BioToF = 2,
    }
}
