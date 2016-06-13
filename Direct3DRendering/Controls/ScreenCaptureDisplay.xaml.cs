using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SharpDX.Direct3D11;

using Microsoft.Win32;

using Texture2D = SharpDX.Toolkit.Graphics.Texture2D;

namespace ImagingSIMS.Direct3DRendering.Controls
{
    /// <summary>
    /// Interaction logic for ScreenCaptureDisplay.xaml
    /// </summary>
    public partial class ScreenCaptureDisplay : UserControl
    {
        int _interval;
        int _counter;

        ObservableCollection<BitmapSource> _captures;
        public ObservableCollection<BitmapSource> Captures
        {
            get { return _captures; }
            set { _captures = value; }
        }

        public static readonly DependencyProperty IsSavingProperty = DependencyProperty.Register("IsSaving",
            typeof(bool), typeof(ScreenCaptureDisplay));
        public static readonly DependencyProperty CanSaveProperty = DependencyProperty.Register("CanSave",
            typeof(bool), typeof(ScreenCaptureDisplay));

        public bool IsSaving
        {
            get { return (bool)GetValue(IsSavingProperty); }
            set { SetValue(IsSavingProperty, value); }
        }
        public bool CanSave
        {
            get { return (bool)GetValue(CanSaveProperty); }
            set { SetValue(CanSaveProperty, value); }
        }

        public ScreenCaptureDisplay()
        {
            Captures = new ObservableCollection<BitmapSource>();

            InitializeComponent();

            savePanel.Visibility = System.Windows.Visibility.Collapsed;
            CanSave = true;
        }

        public void AddCapture(byte[] ImageData, int Width, int Height)
        {
            for (int i = 0; i < ImageData.Length / 4; i++)
            {
                int startIndex = i * 4;

                byte[] temp = new byte[4];
                for (int j = 0; j < 4; j++)
                {
                    temp[j] = ImageData[startIndex + j];
                }

                ImageData[startIndex + 0] = temp[2];
                ImageData[startIndex + 1] = temp[1];
                ImageData[startIndex + 2] = temp[0];
                ImageData[startIndex + 3] = temp[3];
            }
            BitmapSource bs = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Bgr32, null, ImageData, Width * 4);

            Captures.Add(bs);
            listBoxImages.ScrollIntoView(listBoxImages.Items[listBoxImages.Items.Count - 1]);
        }
        public void TryCapture(byte[] ImageData, int Width, int Height, double FPS)
        {
            _interval = (int)(FPS / 20.0d);
            if (_counter >= _interval)
            {
                for (int i = 0; i < ImageData.Length / 4; i++)
                {
                    int startIndex = i * 4;

                    byte[] temp = new byte[4];
                    for (int j = 0; j < 4; j++)
                    {
                        temp[j] = ImageData[startIndex + j];
                    }

                    ImageData[startIndex + 0] = temp[2];
                    ImageData[startIndex + 1] = temp[1];
                    ImageData[startIndex + 2] = temp[0];
                    ImageData[startIndex + 3] = temp[3];
                }

                BitmapSource bs = BitmapSource.Create(Width, Height, 96, 96, PixelFormats.Bgr32, null, ImageData, Width * 4);

                Captures.Add(bs);
                listBoxImages.ScrollIntoView(listBoxImages.Items[listBoxImages.Items.Count - 1]);

                _counter = 0;
            }
            else
            {
                _counter++;
            }
        }

        private void CopyImage(object sender, RoutedEventArgs e)
        {
            if (!listBoxImages.HasItems) return;

            if (listBoxImages.SelectedItems.Count == 0) return;

            BitmapSource bs = (BitmapSource)listBoxImages.SelectedItem;
            if (bs == null) return;

            Clipboard.SetImage(bs);
        }

        private void SaveImages(object sender, RoutedEventArgs e)
        {
            if (!listBoxImages.HasItems) return;

            if (listBoxImages.SelectedItems.Count == 0) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            Nullable<bool> result = sfd.ShowDialog();
            if (result != true) return;

            BitmapSource[] toSave = new BitmapSource[listBoxImages.SelectedItems.Count];
            for (int i = 0; i < listBoxImages.SelectedItems.Count; i++)
            {
                toSave[i] = ((BitmapSource)listBoxImages.SelectedItems[i]).CloneCurrentValue() as BitmapSource;
                toSave[i].Freeze();
            }

            SaveArguments a = new SaveArguments()
            {
                SavePath = sfd.FileName,
                ToSave = toSave
            };

            CanSave = false;

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.DoWork += bw_DoWork;

            savePanel.Visibility = System.Windows.Visibility.Visible;

            bw.RunWorkerAsync(a);
        }
        private void SaveAll(object sender, RoutedEventArgs e)
        {
            if (!listBoxImages.HasItems) return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap Images (.bmp)|*.bmp";
            Nullable<bool> result = sfd.ShowDialog();
            if (result != true) return;

            BitmapSource[] toRemove = new BitmapSource[listBoxImages.Items.Count];
            for (int i = 0; i < listBoxImages.Items.Count; i++)
            {
                toRemove[i] = ((BitmapSource)listBoxImages.Items[i]).CloneCurrentValue() as BitmapSource;
                toRemove[i].Freeze();
            }

            SaveArguments a = new SaveArguments()
            {
                SavePath = sfd.FileName,
                ToSave = toRemove
            };

            CanSave = false;

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = true;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.DoWork += bw_DoWork;

            savePanel.Visibility = System.Windows.Visibility.Visible;

            bw.RunWorkerAsync(a);
        }
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            SaveArguments a = (SaveArguments)e.Argument;

            List<KeyValuePair<object, string>> notSaved = new List<KeyValuePair<object, string>>();

            int i = 0;
            int pos = 0;
            foreach (BitmapSource bs in a.ToSave)
            {
                if (bs == null)
                {
                    notSaved.Add(new KeyValuePair<object, string>("MissingObject", "BitmapSource is null."));
                    continue;
                }

                if (a.ToSave.Length == 1)
                {
                    using (var fileStream = new FileStream(a.SavePath, FileMode.Create))
                    {
                        try
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bs));
                            encoder.Save(fileStream);
                        }
                        catch (Exception ex)
                        {
                            notSaved.Add(new KeyValuePair<object, string>(bs, ex.Message));
                        }
                    }
                }

                else
                {
                    string path = a.SavePath.Insert(a.SavePath.Length - 4, "_" + (++i).ToString());
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        try
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(bs));
                            encoder.Save(fileStream);
                        }
                        catch (Exception ex)
                        {
                            notSaved.Add(new KeyValuePair<object, string>(bs, ex.Message));
                        }
                    }
                }
                pos++;
                bw.ReportProgress(Percentage.GetPercent(pos, a.ToSave.Length));
            }
            e.Result = notSaved;
        }
        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressSave.Value = e.ProgressPercentage;
        }
        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                savePanel.Visibility = System.Windows.Visibility.Collapsed;

                MessageBox.Show("An error occured during the save process. " + e.Error.Message,
                    "Save", MessageBoxButton.OK, MessageBoxImage.Stop);
                goto Cleanup;
            }

            List<KeyValuePair<object, string>> notSaved = (List<KeyValuePair<object, string>>)e.Result;

            if (notSaved.Count > 0)
            {
                savePanel.Visibility = System.Windows.Visibility.Collapsed;

                string list = "";
                foreach (KeyValuePair<object, string> kvp in notSaved)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list = list.Remove(list.Length - 2, 2);
                MessageBox.Show("The following images could not be saved:\n" + list, "Captures", MessageBoxButton.OK, MessageBoxImage.Information);
                goto Cleanup;
            }
            else
            {
                savePanel.Visibility = System.Windows.Visibility.Collapsed;
                goto Cleanup;
            }

        Cleanup:
            {
                BackgroundWorker bw = sender as BackgroundWorker;
                bw.RunWorkerCompleted -= bw_RunWorkerCompleted;
                bw.ProgressChanged -= bw_ProgressChanged;
                bw.DoWork -= bw_DoWork;
                CanSave = true;
            }
        }

        private void RemoveImages(object sender, RoutedEventArgs e)
        {
            int numberToRemove = listBoxImages.SelectedItems.Count;
            if (numberToRemove == 0) return;

            string message = string.Empty;
            if (numberToRemove == 1)
            {
                message = "Delete 1 screen capture?";
            }
            else message = string.Format("Delete {0} screen captures?", numberToRemove);

            message += "\n\nClick OK to proceed or Cancel to return.";
            MessageBoxResult result = MessageBox.Show(message,
                "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (result == MessageBoxResult.Cancel) return;

            List<KeyValuePair<object, string>> notRemoved = new List<KeyValuePair<object, string>>();
            BitmapSource[] toRemove = new BitmapSource[numberToRemove];
            for (int i = 0; i < numberToRemove; i++)
            {
                toRemove[i] = (BitmapSource)listBoxImages.SelectedItems[i];
            }

            for (int i = 0; i < numberToRemove; i++)
            {
                if (toRemove[i] == null)
                {
                    notRemoved.Add(new KeyValuePair<object, string>(listBoxImages.SelectedItems[i], "BitmapSource is null."));
                    continue;
                }
                if (!Captures.Contains(toRemove[i]))
                {
                    notRemoved.Add(new KeyValuePair<object, string>(toRemove[i], "BitmapSource not found in collection."));
                    continue;
                }
                Captures.Remove(toRemove[i]);
            }
            
            if (notRemoved.Count > 0)
            {
                savePanel.Visibility = System.Windows.Visibility.Collapsed;

                string list = "";
                foreach (KeyValuePair<object, string> kvp in notRemoved)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list = list.Remove(list.Length - 2, 2);
                MessageBox.Show("The following images could not be saved:\n" + list, "Captures", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void RemoveAll(object sender, RoutedEventArgs e)
        {
            int numberToRemove = listBoxImages.Items.Count;
            if (numberToRemove == 0) return;

            MessageBoxResult result = MessageBox.Show(string.Format("Delete {0} screen capture(s)?\n\n", numberToRemove) + "Click OK to proceed or Cancel to return.",
                "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
            if (result == MessageBoxResult.Cancel) return;

            List<KeyValuePair<object, string>> notRemoved = new List<KeyValuePair<object, string>>();
            BitmapSource[] toRemove = new BitmapSource[numberToRemove];
            for (int i = 0; i < numberToRemove; i++)
            {
                toRemove[i] = (BitmapSource)listBoxImages.Items[i];
            }

            for (int i = 0; i < numberToRemove; i++)
            {
                if (toRemove[i] == null)
                {
                    notRemoved.Add(new KeyValuePair<object, string>(listBoxImages.Items[i], "BitmapSource is null."));
                    continue;
                }
                if (!Captures.Contains(toRemove[i]))
                {
                    notRemoved.Add(new KeyValuePair<object, string>(toRemove[i], "BitmapSource not found in collection."));
                    continue;
                }
                Captures.Remove(toRemove[i]);
            }

            if (notRemoved.Count > 0)
            {
                savePanel.Visibility = System.Windows.Visibility.Collapsed;

                string list = "";
                foreach (KeyValuePair<object, string> kvp in notRemoved)
                {
                    list += string.Format("{0}: {1}\n", kvp.Key.ToString(), kvp.Value);
                }
                list = list.Remove(list.Length - 2, 2);
                MessageBox.Show("The following images could not be saved:\n" + list, "Captures", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void listBoxImages_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right) e.Handled = true;
        }
    }
    internal struct SaveArguments
    {
        public BitmapSource[] ToSave;
        public string SavePath;
    }
}
