using System.Collections.Generic;

namespace TombLib.Graphics
{
    public class Submesh
    {
        public Material Material;
        public ushort NumIndices;
        public ushort StartIndex;
        public List<ushort> Indices = new List<ushort>();
    }
}
