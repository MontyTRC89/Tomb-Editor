using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public struct WadSprite
    {
        public string Hash;
        public short Width;
        public short Height;
        public byte[,] TexturePage;
    }
}
