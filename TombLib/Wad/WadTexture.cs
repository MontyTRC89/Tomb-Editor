using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Wad
{
    public class WadTexture
    {
        public WadTexturePageType Type;
        public string Hash;
        public short Width;
        public short Height;
        public byte[,] TexturePage;
    }    
}
