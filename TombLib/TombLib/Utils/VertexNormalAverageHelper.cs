using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Utils
{
    public class VertexNormalAverageHelper
    {
        public Vector3 Position { get; set; }
        public List<Vector3> Normals { get; private set; }
        public List<int> Indices { get; private set; }
        public Vector3 Normal { get; set; }
        public int NumVertices { get; set; }

        public VertexNormalAverageHelper()
        {
            Normals = new List<Vector3>();
            Indices = new List<int>();
            Normal = Vector3.Zero;
            NumVertices = 0;
        }
    }
}
