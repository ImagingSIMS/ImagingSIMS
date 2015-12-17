using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSIMS.Data
{
    public class SampleData
    {
        int _sizeX;
        int _sizeY;
        int _sizeZ;

        Data3D _sphereData;
        public Data3D SphereData
        {
            get { return _sphereData; }
        }

        public SampleData(string FilePath)
        {
            _sphereData = new Data3D();

            Load(FilePath);
        }
        public SampleData(int SizeX, int SizeY, int SizeZ)
        {
            _sizeX = SizeX;
            _sizeY = SizeY;
            _sizeZ = SizeZ;

            _sphereData = new Data3D();
        }

        public void CreateSphere(int CenterX, int CenterY, int CenterZ, int Radius)
        {
            double r = Radius;
            double r2 = Radius * Radius;

            for (int z = 0; z < _sizeZ; z++)
            {
                Data2D layer = new Data2D(_sizeX, _sizeY);
                layer.DataName = "Sphere layer " + z.ToString();
                for (int x = 0; x < _sizeX; x++)
                {
                    for (int y = 0; y < _sizeY; y++)
                    {
                        double sphere = ((x - CenterX) * (x - CenterX)) + ((y - CenterY) * (y - CenterY)) +
                               ((z - CenterZ) * (z - CenterZ));
                        if (sphere <= r2)
                        {
                            layer[x, y] = (float)(r2 - sphere + 255);
                        }
                        else layer[x, y] = 0;
                    }
                }
                _sphereData.AddLayer(layer);
            }
        }
        public void CreateSpheres(int[] CenterX, int[] CenterY, int[] CenterZ, int[] Radii)
        {
            int[, ,] hitCount = new int[_sizeX, _sizeY, _sizeZ];

            for (int z = 0; z < _sizeZ; z++)
            {
                Data2D layer = new Data2D(_sizeX, _sizeY);
                for (int x = 0; x < _sizeX; x++)
                {
                    for (int y = 0; y < _sizeY; y++)
                    {
                        double[] spheres = new double[2];
                        for (int i = 0; i < spheres.Length; i++)
                        {
                            double r = Radii[i];
                            double r2 = r * r;

                            double sphere = ((x - CenterX[i]) * (x - CenterX[i])) + ((y - CenterY[i]) * (y - CenterY[i])) +
                                ((z - CenterZ[i]) * (z - CenterZ[i]));

                            if (sphere <= r2)
                            {
                                hitCount[x, y, z]++;

                                double current = layer[x, y];
                                double newValue = (float)(r2 - sphere + 255);

                                layer[x, y] += (float)((current + newValue) / (double)hitCount[x, y, z]);
                            }
                        }
                    }
                }
                layer.DataName = "Sphere layer " + z.ToString();
                _sphereData.AddLayer(layer);
            }
        }

        public void Save(string FilePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(FilePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            }

            string path = Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileNameWithoutExtension(FilePath));
            path += ".isd";

            using (Stream s = File.OpenWrite(path))
            {
                BinaryWriter bw = new BinaryWriter(s);

                bw.Write(_sizeX);
                bw.Write(_sizeY);
                bw.Write(_sizeZ);
                for (int x = 0; x < _sizeX; x++)
                {
                    for (int y = 0; y < _sizeY; y++)
                    {
                        for (int z = 0; z < _sizeZ; z++)
                        {
                            bw.Write(_sphereData[x, y, z]);
                        }
                    }
                }
            }
        }
        public void Load(string FilePath)
        {
            try
            {
                Data2D[] dts;

                int sizeX;
                int sizeY;
                int sizeZ;

                int[, ,] intensityMatrix;

                using (Stream s = File.OpenRead(FilePath))
                {
                    BinaryReader br = new BinaryReader(s);

                    sizeX = br.ReadInt32();
                    sizeY = br.ReadInt32();
                    sizeZ = br.ReadInt32();

                    intensityMatrix = new int[sizeX, sizeY, sizeZ];

                    for (int x = 0; x < sizeX; x++)
                    {
                        for (int y = 0; y < sizeY; y++)
                        {
                            for (int z = 0; z < sizeZ; z++)
                            {
                                intensityMatrix[x, y, z] = br.ReadInt32();
                            }
                        }
                    }
                }

                dts = new Data2D[sizeZ];

                for (int z = 0; z < sizeZ; z++)
                {
                    Data2D dt = new Data2D(_sizeX, _sizeY);
                    dt.DataName = Path.GetFileNameWithoutExtension(FilePath) + "-Layer " + (z + 1).ToString();
                    for (int x = 0; x < sizeX; x++)
                    {

                    for (int y = 0; y < sizeY; y++)
                    {

                            dt[x,y] = intensityMatrix[x, y, z];
                        }

                    }
                    dts[z] = dt;
                }

            }
            catch (FileNotFoundException)
            {
                throw new Exception(FilePath + " was not found.");
            }
        }
    }
}
