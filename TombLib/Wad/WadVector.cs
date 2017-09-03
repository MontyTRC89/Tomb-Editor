using System.Runtime.InteropServices;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WadVector
    {
        public short X;
        public short Y;
        public short Z;
    }
}
