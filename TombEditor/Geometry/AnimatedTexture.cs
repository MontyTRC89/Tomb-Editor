using System.Collections.Generic;

namespace TombEditor.Geometry
{
    public class AnimatedTexture
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public short Page { get; set; }
        public short NewId { get; set; }

        public LevelTexture Texture { get; set; }

        public List<short> NewIdList { get; set; } = new List<short>();

        public AnimatedTexture(short x, short y, short page)
        {
            X = x;
            Y = y;
            Page = page;
            
            NewIdList.Add(-1); // Normal Rectangle
            NewIdList.Add(-1); // Flipped Rectangle

            NewIdList.Add(-1); // Normal Triangle 1
            NewIdList.Add(-1); // Normal Triangle 2
            NewIdList.Add(-1); // Normal Triangle 3
            NewIdList.Add(-1); // Normal Triangle 4

            NewIdList.Add(-1); // Flipped Triangle 1
            NewIdList.Add(-1); // Flipped Triangle 2
            NewIdList.Add(-1); // Flipped Triangle 3
            NewIdList.Add(-1); // Flipped Triangle 4
        }
    }
}
