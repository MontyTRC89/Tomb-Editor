using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;
using TombLib.Wad.Catalog;

namespace TombLib.Wad
{
    public class WadSpriteSequence
    {
        public uint ObjectID { get; set; }
        public List<WadSprite> Sprites { get; private set; }
        public string Name { get; set; }

        public WadSpriteSequence()
        {
            Sprites = new List<WadSprite>();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
