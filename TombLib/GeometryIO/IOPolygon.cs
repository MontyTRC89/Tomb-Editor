using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
        public List<Vector2> UV { get; private set; }
        public List<Vector4> Colors { get; private set; }

        public IOPolygon(IOPolygonShape shape)
        {
            Shape = shape;
            Indices = new List<int>();
            UV = new List<Vector2>();
            Colors = new List<Vector4>();
        }
    }
}
