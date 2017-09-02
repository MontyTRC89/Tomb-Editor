using System.Runtime.InteropServices;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WadVector3
    {
        public float X;
        public float Y;
        public float Z;
    }
}
