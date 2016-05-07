using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct3DEffectCompiler
{
    internal static class FilePaths
    {
        internal static string executablePath = @"C:\Program Files (x86)\Windows Kits\10\bin\x64\fxc.exe";
        internal static string sourceFolder = @"C:\Users\jay50\Source\Repos\jay5026\ImagingSIMS3\ImagingSIMS\Direct3DRendering\Shaders\";
        internal static string outputFolder = @"C:\Users\jay50\Source\Repos\jay5026\ImagingSIMS3\ImagingSIMS\Direct3DRendering\Shaders\";
        //internal static _outputFolder = @"C:\Users\jayt\Desktop\test\";
    }
    class Program
    {
        static void Main(string[] args)
        {
            // Check args to see if one or more paths were specified
            if (args.Length > 0)
            {
                if (args.Length >= 1)
                {
                    FilePaths.executablePath = args[0];
                }
                if (args.Length >= 2)
                {
                    FilePaths.sourceFolder = args[1];
                }
                if (args.Length >= 3)
                {
                    FilePaths.outputFolder = args[2];
                }
            }

            if (!File.Exists(FilePaths.executablePath))
            {
                Console.WriteLine("FXC.exe not found.");
                return;
            }
            if (!Directory.Exists(Path.GetDirectoryName(FilePaths.sourceFolder)))
            {
                Console.WriteLine("Could not find directory containing shaders.");
                return;
            }
            if (!Directory.Exists(Path.GetDirectoryName(FilePaths.outputFolder)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePaths.outputFolder));
            }

            const int numShaders = 7;

            string[,] shaders = new string[numShaders, 2]
            {
                {"Axes", "Axes"},
                {"BoundingBox", "BoundingBox"},
                {"CoordinateBox", "CoordinateBox"},
                {"HeightMap", "HeightMap"},
                {"Isosurface", "Volume"},
                {"Model", "Volume"},
                {"Raycast", "Volume"}
            };

            int errorCount = 0;
            for (int i = 0; i < numShaders; i++)
            {
                Shader ps = new Shader(shaders[i, 0], shaders[i, 1], true);
                if (!ps.Compile(true)) 
                    errorCount++;
                if (!ps.Compile(false))
                    errorCount++;

                Shader vs = new Shader(shaders[i, 0], shaders[i, 1], false);
                if (!vs.Compile(true)) 
                    errorCount++;
                if (!vs.Compile(false))
                    errorCount++;
            }

            if (errorCount > 0)
            {
                Console.WriteLine(string.Format("There were {0} errors compiling the shaders.", errorCount));
                Console.ReadLine();
            }
        }
    }

    internal class Shader
    {
        string _shaderName;
        bool _isPixelShader;
        bool _isVertexShader;
        string _hlslName;

        public Shader(string shaderName, string hlslName, bool isPixelShader)
        {
            this._shaderName = shaderName;
            this._isPixelShader = isPixelShader;
            this._isVertexShader = !isPixelShader;
            this._hlslName = hlslName;
        }

        public bool Compile(bool isDebug)
        {
            var arguments = generateArguments(isDebug);

            var info = new ProcessStartInfo(FilePaths.executablePath, arguments)
            {
                CreateNoWindow = true,
                ErrorDialog = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            var proc = Process.Start(info);
            var procResult = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode == 0)
            {
                Console.WriteLine(string.Format("Shader {0} ({1}{2}) has been compiled successfully.", 
                    _shaderName, (_isPixelShader ? "PS" : "VS"), (isDebug ? "-Debug" : "")));
            }
            else
            {
                Console.WriteLine(string.Format("Could not compile shader {0} ({1}{2}):",
                    _shaderName, (_isPixelShader ? "PS" : "VS"), (isDebug ? "-Debug" : "")));
                Console.WriteLine(procResult);
            }

            return proc.ExitCode == 0;
        }
        private string generateArguments(bool isDebug)
        {
            StringBuilder sb = new StringBuilder();

            if (isDebug)
            {
                sb.Append("/Od ");
                sb.Append("/Zi ");
            }

            sb.Append("/T ");

            if (_isPixelShader)
                sb.Append("ps_4_0 ");
            else if (_isVertexShader)
                sb.Append("vs_4_0 ");

            sb.Append("/E ");
            sb.Append(generateEntryMethod());

            sb.Append("/Fo ");
            sb.Append(generateFileName(isDebug));

            if (isDebug)
            {
                sb.Append("/Fd ");
                sb.Append(generatePDBName());
            }

            sb.Append(FilePaths.sourceFolder + _hlslName + ".hlsl ");

            return sb.ToString();
        }

        private string generateFileName(bool isDebug)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FilePaths.outputFolder);

            if (!isDebug)
                sb.Append(@"Release\");

            sb.Append(_shaderName);

            if (_isPixelShader)
                sb.Append(".pso");
            else if (_isVertexShader)
                sb.Append(".vso");

            return string.Format("\"{0}\" ", sb.ToString());
        }
        private string generatePDBName()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(FilePaths.outputFolder);

            sb.Append(_shaderName);

            if (_isPixelShader)
                sb.Append(".pso");
            else if (_isVertexShader)
                sb.Append(".vso");

            sb.Append(".pdb");

            return string.Format("\"{0}\" ", sb.ToString());

        }
        private string generateEntryMethod()
        {
            StringBuilder sb = new StringBuilder();

            if(_shaderName == "BoundingBox")            
                sb.Append("BBOX");            
            else if (_shaderName == "CoordinateBox")            
                sb.Append("CBOX");            
            else 
                sb.Append(_shaderName.ToUpper());

            if (_isPixelShader)
                sb.Append("_PS ");
            else if (_isVertexShader)
                sb.Append("_VS ");

            return sb.ToString();
        }
    }
}
