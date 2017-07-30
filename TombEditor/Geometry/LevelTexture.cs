namespace TombEditor.Geometry
{
    public class LevelTexture
    {
        public short Width { get; set; }
        public short Height { get; set; }
        public short X { get; set; }
        public short Y { get; set; }
        public short Page { get; set; }
        public bool Transparent { get; set; }
        public bool DoubleSided { get; set; }
        public int Id { get; set; }
        public int OldId { get; set; }
        public int NewX { get; set; }
        public int NewY { get; set; }
        public int NewPage { get; set; }
        public bool AlphaTest { get; set; }
        public bool Animated { get; set; }
        public int AnimatedSequence { get; set; }
        public int AnimatedTexture { get; set; }
    }
}
