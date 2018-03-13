namespace TombLib.Wad
{
    public struct WadAnimDispatch
    {
        public ushort InFrame { get; set; }
        public ushort OutFrame { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextFrame { get; set; }
    }
}
