using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadSpriteSequence
    {
        public uint ObjectID { get; set; }
        public List<WadSprite> Sprites { get; private set; }
     
        public WadSpriteSequence()
        {
            Sprites = new List<WadSprite>();
        }

        public override string ToString()
        {
            switch (ObjectID)
            {
                case 460: return "Sky Graphics";
                case 463: return "Default Sprites";
                case 464: return "Misc Sprites";
                case 494: return "NG Custom Sprites";
                default: return "Unknown Sprites";

            }
        }
    }
}
