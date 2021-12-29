using SharpDX;

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
    }
}
