using System.Collections.Generic;

namespace TombLib.Graphics
{
    public class Submesh
    {
        public Material Material { get; private set; }
        public int BaseIndex { get; set; }
        public int NumIndices { get { return Indices.Count; } }
        public List<int> Indices { get; set; } = new List<int>();

        public Submesh(Material material)
        {
            Material = material;
        }
    }
}
