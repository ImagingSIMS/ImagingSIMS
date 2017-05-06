using System;
using System.Windows.Media.Imaging;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;

using Color = System.Windows.Media.Color;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using System.Collections.Generic;
using System.Reflection;
using ImagingSIMS.Common;

namespace ImagingSIMS.Data.Imaging
{
    public class GPUImageGenerator : BaseImageGenerator
    {
        private Device _device;

        private object _deviceLock;

        protected ImageShaderFactory _shaderFactory;

        public GPUImageGenerator()
        {
            _device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            _deviceLock = new object();

            _shaderFactory = new Imaging.ImageShaderFactory(_device);
        }

        public override BitmapSource Create(Data2D data, Color solidColor)
        {
            var renderParams = new ImageRenderParams()
            {
                RenderColor = new Vector4(solidColor.R, solidColor.G, solidColor.B, solidColor.A) / 255f,
                DataMaximum = data.Maximum,
                Saturation = data.Maximum,
                Threshold = data.Minimum,
                SizeX = data.Width,
                SizeY = data.Height
            };
            var renderParamsBuffer = CreateParamsBuffer(renderParams);

            return DrawWithComputeShader(data, _shaderFactory.GetShader(ColorScaleTypes.Solid).Shader, renderParamsBuffer);
        }
        public override BitmapSource[] Create(ImageComponent[] components, ImagingParameters parameters)
        {
            throw new NotImplementedException();
        }
        public override BitmapSource Create(Data2D data, ColorScaleTypes scale)
        {
            var renderParams = new ImageRenderParams()
            {
                RenderColor = new Vector4(),
                DataMaximum = data.Maximum,
                Saturation = data.Maximum,
                Threshold = data.Minimum,
                SizeX = data.Width,
                SizeY = data.Height
            };
            var renderParamsBuffer = CreateParamsBuffer(renderParams);

            return DrawWithComputeShader(data, _shaderFactory.GetShader(scale).Shader, renderParamsBuffer);
        }
        public override BitmapSource Create(Data2D data, Color solidColor, float saturation)
        {
            var renderParams = new ImageRenderParams()
            {
                RenderColor = new Vector4(solidColor.R, solidColor.G, solidColor.B, solidColor.A) / 255f,
                DataMaximum = data.Maximum,
                Saturation = saturation,
                Threshold = data.Minimum,
                SizeX = data.Width,
                SizeY = data.Height,
                SizeZ = 1

            };
            var renderParamsBuffer = CreateParamsBuffer(renderParams);

            return DrawWithComputeShader(data, _shaderFactory.GetShader(ColorScaleTypes.Solid).Shader, renderParamsBuffer);
        }
        public override BitmapSource Create(Data2D data, ColorScaleTypes scale, float saturation)
        {
            var renderParams = new ImageRenderParams()
            {
                RenderColor = new Vector4(),
                DataMaximum = data.Maximum,
                Saturation = saturation,
                Threshold = data.Minimum,
                SizeX = data.Width,
                SizeY = data.Height
            };
            var renderParamsBuffer = CreateParamsBuffer(renderParams);

            return DrawWithComputeShader(data, _shaderFactory.GetShader(scale).Shader, renderParamsBuffer);
        }
        public override BitmapSource Create(Data2D data, Color solidColor, float saturation, float threshold)
        {
            var renderParams = new ImageRenderParams()
            {
                RenderColor = new Vector4(solidColor.R, solidColor.G, solidColor.B, solidColor.A) / 255f,
                DataMaximum = data.Maximum,
                Saturation = saturation,
                Threshold = threshold,
                SizeX = data.Width,
                SizeY = data.Height,
                SizeZ = 1
            };
            var renderParamsBuffer = CreateParamsBuffer(renderParams);

            return DrawWithComputeShader(data, _shaderFactory.GetShader(ColorScaleTypes.Solid).Shader, renderParamsBuffer);
        }
        public override BitmapSource Create(Data2D data, ColorScaleTypes scale, float saturation, float threshold)
        {
            var renderParams = new ImageRenderParams()
            {
                RenderColor = new Vector4(),
                DataMaximum = data.Maximum,
                Saturation = saturation,
                Threshold = threshold,
                SizeX = data.Width,
                SizeY = data.Height
            };
            var renderParamsBuffer = CreateParamsBuffer(renderParams);

            return DrawWithComputeShader(data, _shaderFactory.GetShader(scale).Shader, renderParamsBuffer);
        }

        protected BitmapSource DrawWithComputeShader(Data2D data, ComputeShader shader, Buffer renderParamsBuffer)
        {
            float[] dataBuffer = new float[data.Width * data.Height];
            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    dataBuffer[x * data.Height + y] = data[x, y];
                }
            }

            var dataStream = DataStream.Create(dataBuffer, true, true);

            var inputData = new Texture2D(_device, new Texture2DDescription()
            {
                Format = Format.R32_Float,
                Width = data.Width,
                Height = data.Height,
                MipLevels = 1,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ShaderResource,
                SampleDescription = new SampleDescription(1, 0),
                ArraySize = 1,
                CpuAccessFlags = CpuAccessFlags.None
            }, new[] { new DataBox(dataStream.DataPointer, data.Width * sizeof(float), data.Width * data.Height * sizeof(float)) });

            var outputData = new Texture2D(_device, new Texture2DDescription()
            {
                Format = Format.R32G32B32A32_Float,
                Width = data.Width,
                Height = data.Height,
                MipLevels = 1,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.UnorderedAccess,
                SampleDescription = new SampleDescription(1, 0),
                ArraySize = 1,
                CpuAccessFlags = CpuAccessFlags.None
            });

            var outputDataCopy = new Texture2D(_device, new Texture2DDescription()
            {
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                Format = outputData.Description.Format,
                OptionFlags = ResourceOptionFlags.None,
                ArraySize = outputData.Description.ArraySize,
                Height = outputData.Description.Height,
                Width = outputData.Description.Width,
                MipLevels = 1,
                SampleDescription = outputData.Description.SampleDescription
            });

            var inputView = new ShaderResourceView(_device, inputData);
            var outputView = new UnorderedAccessView(_device, outputData);

            lock (_deviceLock)
            {
                _device.ImmediateContext.ComputeShader.SetConstantBuffer(0, renderParamsBuffer);
                _device.ImmediateContext.ComputeShader.Set(shader);

                _device.ImmediateContext.ComputeShader.SetShaderResource(0, inputView);
                _device.ImmediateContext.ComputeShader.SetUnorderedAccessView(0, outputView);

                _device.ImmediateContext.Dispatch((int)Math.Ceiling(data.Width / 32d), (int)Math.Ceiling(data.Height / 32d), 1);

                _device.ImmediateContext.CopyResource(outputData, outputDataCopy);
            }

            var mapSource = _device.ImmediateContext.MapSubresource(outputDataCopy, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
            float[] outputBuffer = new float[data.Width * data.Height * 4];
            Marshal.Copy(mapSource.DataPointer, outputBuffer, 0, data.Width * data.Height * 4);

            var imageData = new Data3D(data.Width, data.Height, 4);
            for (int y = 0; y < imageData.Height; y++)
            {
                for (int x = 0; x < imageData.Width; x++)
                {
                    // extract BGRA
                    float[] temp = new float[4];
                    Array.Copy(outputBuffer, 4 * (x * imageData.Height + y), temp, 0, 4);

                    imageData[x, y, 0] = temp[2] * 255f;
                    imageData[x, y, 1] = temp[1] * 255f;
                    imageData[x, y, 2] = temp[0] * 255f;
                    imageData[x, y, 3] = temp[3] * 255f;
                }
            }

            Disposer.RemoveAndDispose(ref renderParamsBuffer);
            Disposer.RemoveAndDispose(ref dataStream);
            Disposer.RemoveAndDispose(ref inputData);
            Disposer.RemoveAndDispose(ref outputData);
            Disposer.RemoveAndDispose(ref outputDataCopy);
            Disposer.RemoveAndDispose(ref inputView);
            Disposer.RemoveAndDispose(ref outputView);

            return Create(imageData);
        }

        protected Buffer CreateParamsBuffer(ImageRenderParams renderParams)
        {
            var buffer = new Buffer(_device, Marshal.SizeOf(typeof(ImageRenderParams)),
                ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            _device.ImmediateContext.UpdateSubresource(ref renderParams, buffer);

            return buffer;
        }

        protected static float[] UnpackColor(uint value)
        {
            return new float[]
            {
                (value & 0x00FF0000) >> 16,
                (value & 0x0000FF00) >> 8,
                (value & 0x000000FF),
                (value & 0xFF000000) >> 24
            };
        }
    }

    public class ImageShader
    {
        public bool IsLoaded { get; set; }
        public string FilePath { get; set; }
        public string EntryPoint { get; set; }
        public ComputeShader Shader { get; set; }

        public ImageShader(string filePath, string entryPoint)
        {
            FilePath = filePath;
            EntryPoint = entryPoint;
        }

        public void LoadBytecode(Device device)
        {
            var csBytecode = ShaderBytecode.CompileFromFile($@"Shaders\{FilePath}", EntryPoint, "cs_5_0");
            Shader = new ComputeShader(device, csBytecode);
            IsLoaded = true;
        }
    }
    public class ImageShaderFactory
    {
        Device _device;
        Dictionary<ColorScaleTypes, ImageShader> _shaders;

        public ImageShaderFactory(Device device)
        {
            _device = device;
            _shaders = new Dictionary<ColorScaleTypes, ImageShader>();
        }

        public ImageShader GetShader(ColorScaleTypes colorScale)
        {
            if (_shaders.ContainsKey(colorScale))
            {
                return _shaders[colorScale];
            }

            else
            {
                var attribute = EnumEx.GetAttributeOfType<GPUShaderDescriptionAttribute>(colorScale);
                if (attribute == null)
                    throw new ArgumentException("Could not determine the shader type");

                var shader = new ImageShader(attribute.ShaderName, attribute.ShaderEntryPoint);

                try
                {
                    shader.LoadBytecode(_device);
                }
                catch (NullReferenceException nrex)
                {
                    throw new ArgumentException($"Unable to load shader for '{colorScale}' from bytecode.", nrex);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Unble to generate shader", ex);
                }

                _shaders.Add(colorScale, shader);

                return shader;
            }
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 48)]
    public struct ImageRenderParams
    {
        [FieldOffset(0)]
        public Vector4 RenderColor;

        [FieldOffset(16)]
        public float DataMaximum;

        [FieldOffset(20)]
        public float Saturation;

        [FieldOffset(24)]
        public float Threshold;

        [FieldOffset(28)]
        public float SizeX;

        [FieldOffset(32)]
        public float SizeY;

        [FieldOffset(36)]
        public float SizeZ;

        [FieldOffset(40)]
        private float padding_1;

        [FieldOffset(44)]
        private float padding_2;
    }
}
