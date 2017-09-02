using System.Runtime.InteropServices;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class WadStateChange
    {
        public ushort StateId;
        public ushort NumDispatches;
        public WadAnimDispatch[] Dispatches;
    }
}
