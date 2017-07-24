using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Wad
{
    public class WadTexturePage
    {
        public WadTexturePageType Type;
        public byte[,] TexturePage;
    }

    public enum WadTexturePageType : byte
    {
        Shared = 0,
        Private = 1
    }
}
