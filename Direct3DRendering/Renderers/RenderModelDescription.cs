using SharpDX;

using ImagingSIMS.Common;

namespace ImagingSIMS.Direct3DRendering.Renderers
{
    public class RenderModelDescription
    {
        public Vector3 DataSize;
        public Vector3 ModelSize;
        public Vector3 ModelStart;
        public Vector3 ModelEnd;

        public RenderModelDescription()
        {
            DataSize = new Vector3();
            ModelSize = new Vector3();
            ModelStart = new Vector3();
            ModelEnd = new Vector3();
        }

        public Vector3 ConvertDataToModelCoordinate(Vector3 coordinate)
        {
            return new Vector3(
                coordinate.X * ModelSize.X / DataSize.X - (ModelSize.X / 2),
                coordinate.Y * ModelSize.Y / DataSize.Y - (ModelSize.Y / 2),
                coordinate.Z * ModelSize.Z / DataSize.Z - (ModelSize.Z / 2)
                );
        }
        public Vector3 ConvertModelToDataCoordinate(Vector3 coordinate)
        {
            return new Vector3(0);
        }

        public static explicit operator ModelParams(RenderModelDescription m)
        {
            return new ModelParams()
            {
                DataSize = new Vector4(m.DataSize, 0),
                ModelSize = new Vector4(m.ModelSize, 0),
                ModelStart = new Vector4(m.ModelStart, 0),
                ModelEnd = new Vector4(m.ModelEnd, 0)
            };
        }
    }
}
