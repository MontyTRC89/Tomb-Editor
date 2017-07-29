using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class AnimatedTexture
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public short Page { get; set; }
        public short NewID { get; set; }

        public LevelTexture Texture { get; set; }

        public List<short> NewIDList { get; set; } = new List<short>();

        public AnimatedTexture(short x, short y, short page)
        {
            X = x;
            Y = y;
            Page = page;
            
            NewIDList.Add(-1); // Normal Rectangle
            NewIDList.Add(-1); // Flipped Rectangle

            NewIDList.Add(-1); // Normal Triangle 1
            NewIDList.Add(-1); // Normal Triangle 2
            NewIDList.Add(-1); // Normal Triangle 3
            NewIDList.Add(-1); // Normal Triangle 4

            NewIDList.Add(-1); // Flipped Triangle 1
            NewIDList.Add(-1); // Flipped Triangle 2
            NewIDList.Add(-1); // Flipped Triangle 3
            NewIDList.Add(-1); // Flipped Triangle 4
        }
    }
}
