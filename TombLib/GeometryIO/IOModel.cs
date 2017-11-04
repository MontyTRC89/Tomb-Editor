using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.GeometryIO
{
    public class IOModel
    {
        public List<IOMesh> Meshes { get; private set; }
        public List<string> Textures { get; private set; }
        public BoundingBox BoundingBox { get; internal set; }
        public BoundingSphere BoundingSphere { get; internal set; }

        public IOModel()
        {
            Meshes = new List<IOMesh>();
            Textures = new List<string>();
        }
    }
}
