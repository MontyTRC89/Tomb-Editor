namespace TombEditor.Geometry
{
    public class TextureSound
    {
        public short X { get; set; }
        public short Y { get; set; }
        public short NewId { get; set; }
        public short Page { get; set; }
        public TextureSounds Sound { get; set; }

        public TextureSound(short x, short y, short page)
        {
            X = x;
            Y = y;
            Page = page;
            Sound = TextureSounds.Stone;
        }
    }
}
