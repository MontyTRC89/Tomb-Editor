using System.Runtime.InteropServices;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class WadAnimDispatch
    {
        public ushort InFrame;
        public ushort OutFrame;
        public ushort NextAnimation;
        public ushort NextFrame;
    }
}
