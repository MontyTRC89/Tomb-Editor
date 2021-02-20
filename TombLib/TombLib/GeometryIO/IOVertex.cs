using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO
{
    public class IOVertex : IEquatable<IOVertex>
    {
        public Vector3 Position;
        public Vector2 UV;
        public Vector3 Color;

        public IOVertex(Vector3 position, Vector2 uv, Vector3 color)
        {
            Position = position;
            UV = uv;
            Color = color;
        }

        public bool Equals(IOVertex other)
        {
            return (Position == other.Position && UV == other.UV && Color == other.Color);
        }
    }
}
