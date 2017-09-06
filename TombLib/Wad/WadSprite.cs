using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadSprite
    {
        public string Hash;
        public ushort Width { get { return (ushort)Texture.Width; } }
        public ushort Height { get { return (ushort)Texture.Height; } }
        public ImageC Texture;
    }
}
