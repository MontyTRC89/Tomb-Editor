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
            X = x;
            Y = y;
            Page = page;
            Texture = txt;
        }
    }
}
