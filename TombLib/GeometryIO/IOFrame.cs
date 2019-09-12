using System.Collections.Generic;
using System.Numerics;

namespace TombLib.GeometryIO
{
    public class IOFrame
    {
        public Vector3 Offset { get; set; }
        public List<Vector3> Angles { get; private set; }

        public IOFrame()
        {
            Angles = new List<Vector3>();
        }
    }
}
