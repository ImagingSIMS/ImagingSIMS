using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace ImagingSIMS.Direct3DRendering.Controls
{
    /// <summary>
    /// Interaction logic for ShaderDisplay.xaml
    /// </summary>
    public partial class ShaderDisplay : UserControl
    {        
        ObservableCollection<KeyValuePair<string, string>> _shaders;
        public ObservableCollection<KeyValuePair<string, string>> Shaders
        {
            get { return _shaders; }
            set { _shaders = value; }
        }

        public ShaderDisplay()
        {
            InitializeComponent();

            _shaders = new ObservableCollection<KeyValuePair<string, string>>();
        }

        public void LoadShaders(Device Device)
        {
            List<string> files = new List<string>();
            files.AddRange( System.IO.Directory.GetFiles("Shaders"));

            var results = from file in files
                            where file.EndsWith(".pso") || file.EndsWith(".vso") || file.EndsWith(".gso")
                            select file;

            foreach (string result in results)
            {
                string shaderName = System.IO.Path.GetFileName(result);

                var byteCode = ShaderBytecode.FromFile(result);

                Shaders.Add(new KeyValuePair<string, string>(shaderName, FormatDisassembledByteCode(byteCode.Disassemble())));
            }            

            listShaders.ItemsSource = Shaders;
        }

        private void listShaders_SelectionChanged(object sender, RoutedEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb == null) return;

            KeyValuePair<string, string> shader = (KeyValuePair<string, string>)lb.SelectedItem;

            textBoxShaderContent.Text = shader.Value;
            
        }

        private static string FormatDisassembledByteCode(string disassembledByteCode)
        {
            StringBuilder builder = new StringBuilder();

            var split = System.Text.RegularExpressions.Regex.Split(disassembledByteCode, "\n");

            for (int i = 0; i < split.Length; i++)
            {
                string lineNumber = (i + 1).ToString("00000");
                builder.Append(lineNumber);
                builder.Append("    ");
                builder.AppendLine(split[i]);
            }

            return builder.ToString();
        }
    }

    public enum ShaderType
    {
        Model, Raycast, BoundingBox, Axes, HeightMap
    }
}
