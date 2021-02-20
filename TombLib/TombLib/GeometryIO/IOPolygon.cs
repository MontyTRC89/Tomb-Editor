using System.Collections.Generic;
using System.Numerics;

namespace TombLib.GeometryIO
{
    public enum IOPolygonShape
    {
        Quad,
        Triangle
    }

    public class IOPolygon
    {
        public IOPolygonShape Shape { get; private set; }
        public List<int> Indices { get; private set; }

        public IOPolygon(IOPolygonShape shape)
        {
            Shape = shape;
            Indices = new List<int>();
        }
    }
}
