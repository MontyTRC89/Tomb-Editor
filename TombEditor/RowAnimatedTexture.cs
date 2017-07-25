using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor
{
    public class RowAnimatedTexture
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Page { get; set; }
        public int Texture { get; set; }

        public RowAnimatedTexture(short x, short y, short page, int txt)
        {
            this.X = x;
            this.Y = y;
            this.Page = page;
            this.Texture = txt;
        }
    }
}
