using TombLib.Types;

namespace TombLib.Wad
{
    public class WadAnimDispatch
    {
        public ushort InFrame { get; set; }
        public ushort OutFrame { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextFrame { get; set; }

        // New parameters for animation blending (TEN only).
        public ushort BlendFrameCount { get; set; }
        public ushort BlendEndFrame { get; set; }
        public BezierCurve2D BlendCurve { get; set; } = BezierCurve2D.Linear.Clone();

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
