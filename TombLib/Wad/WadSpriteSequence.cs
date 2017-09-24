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
            if (ObjectNames.SpriteSequenceSlots.ContainsKey(ObjectID))
                return ObjectNames.SpriteSequenceSlots[ObjectID];
            else
                return "Unknown Sprites";
        }
    }
}
