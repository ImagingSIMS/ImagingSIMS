using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Direct3DRendering
{
    public class RenderIsosurface
    {
        public TriangleSurface[] Triangles { get; set; }
        public Color Color { get; set; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        public RenderIsosurface(float[,,] volumeData, float isoValue, Color isoColor, int surfaceId)
        {
            Color = isoColor;

            Width = volumeData.GetLength(0);
            Height = volumeData.GetLength(1);
            Depth = volumeData.GetLength(2);

            calculateSurfaces(volumeData, isoValue, surfaceId);
        }

        private void calculateSurfaces(float[,,] volumeData, float isoValue, int surfaceId)
        {

        }
    }

    public class TriangleSurface
    {
        public Vector4[] Points { get; set; }
        public Vector4 Normal
        {
            get
            {
                Vector4 ab = Points[1] - Points[0];
                Vector4 ac = Points[2] - Points[0];
                Vector4 norm = Vector4.Cross(ab, ac);
                norm.Normalize();
                return new Vector4(norm, 1.0f);
            }
        }

        public TriangleSurface(Vector3[] points)
        {
            createSurface(points, 1.0f);
        }
        public TriangleSurface(Vector3[] points, float idMarker)
        {
            createSurface(points, idMarker);
        }
        private void createSurface(Vector3[] points, float idMarker)
        {
            if (points.Length != 3)
                throw new ArgumentException("Triangle surface must contain three points.");

            Points = new Vector4[3];
            for (int i = 0; i < 3; i++)
            {
                Points[i] = new Vector4(points[i], idMarker);
            }

            
        }
    }
}
