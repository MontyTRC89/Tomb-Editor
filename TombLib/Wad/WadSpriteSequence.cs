using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public class WadSpriteSequence
    {
        public uint ObjectID { get; set; }
        public List<WadTexture> Sprites { get; private set; }

        public WadSpriteSequence()
        {
            Sprites = new List<WadTexture>();
        }
    }
}
