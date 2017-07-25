using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
