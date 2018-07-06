namespace TombLib.Wad
{
    public class WadAnimDispatch
    {
        public ushort InFrame { get; set; }
        public ushort OutFrame { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextFrame { get; set; }

        public WadAnimDispatch() { }

        public WadAnimDispatch(ushort inFrame, ushort outFrame, ushort nextAnimation, ushort nextFrame)
        {
            InFrame = inFrame;
            OutFrame = outFrame;
            NextAnimation = nextAnimation;
            NextFrame = nextFrame;
        }
    }
}
