using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Wad
{
    public class WadSprite : WadTexture, IDisposable
    {
        public Texture2D DirectXTexture { get; set; }
        public int TopSide { get; set; }
        public int BottomSide { get; set; }
        public int RightSide { get; set; }
        public int LeftSide { get; set; }

        public void Dispose()
        {
            if (DirectXTexture != null) DirectXTexture.Dispose();
        }
    }
}
