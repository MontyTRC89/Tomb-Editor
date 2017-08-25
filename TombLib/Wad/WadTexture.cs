using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TombLib.Utils;

namespace TombLib.Wad
{
    public class WadTexture : Texture
    {
        public WadTexturePageType Type;
        public string Hash;
        
        public override bool ReplaceMagentaWithTransparency => true;

        public new ImageC Image
        {
            get { return base.Image; }
            set { base.Image = value; }
        }

        public override Texture Clone()
        {
            return new WadTexture { Image = Image, Hash = Hash, Type = Type };
        }
    }    
}
