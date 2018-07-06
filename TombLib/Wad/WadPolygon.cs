using TombLib.Utils;

namespace TombLib.Wad
{
    public enum WadPolygonShape : ushort
    {
        Quad = 0,
        Triangle = 1
    }

    public struct WadPolygon
    {
        public WadPolygonShape Shape;
        public int Index0;
        public int Index1;
        public int Index2;
        public int Index3;
        public TextureArea Texture;
        public byte ShineStrength;
    }
}
