using System.Collections.Generic;

namespace TombLib.GeometryIO
{
    public class IOSubmesh
    {
        public IOMaterial Material { get; private set; }
        public List<IOPolygon> Polygons { get; private set; } = new List<IOPolygon>();

        public IOSubmesh(IOMaterial material)
        {
            Material = material;
        }
    }
}
