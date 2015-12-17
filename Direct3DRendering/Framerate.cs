using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Direct3DRendering
{
    public class FramerateCounter
    {
        int frames = 0;
        int beginTime = Environment.TickCount;
        double fps = 30.0f;

        public FramerateCounter()
        {
        }

        public double GetFramerate()
        {
            frames++;
            int timeEllapse = Environment.TickCount - beginTime;
            if (timeEllapse > 1000)
            {
                fps = (double)(frames * 1000) / (double)(timeEllapse);
                frames = 0;
                beginTime = Environment.TickCount;
            }
            fps = (double)Math.Round(fps, 2);
            return fps;
        }
    }
}
