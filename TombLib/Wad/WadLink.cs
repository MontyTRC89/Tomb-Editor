using System.Runtime.InteropServices;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WadLink
    {
        public WadLinkOpcode Opcode;
        public int X;
        public int Y;
        public int Z;
    }
}
