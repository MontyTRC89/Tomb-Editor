using System.Collections.Generic;

namespace TombLib.Wad
{
    public class WadAnimation
    {
        public byte FrameDuration { get; set; }
        public ushort StateId { get; set; }
        public int Speed { get; set; }
        public int Acceleration { get; set; }
        public int LateralSpeed { get; set; }
        public int LateralAcceleration { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextFrame { get; set; }
        public ushort FrameEnd { get; set; }
        public ushort RealNumberOfFrames { get; set; }
        public string Name { get; set; } = "Animation";

        public List<WadKeyFrame> KeyFrames { get; private set; } = new List<WadKeyFrame>();
        public List<WadStateChange> StateChanges { get; private set; } = new List<WadStateChange>();
        public List<WadAnimCommand> AnimCommands { get; private set; } = new List<WadAnimCommand>();

        public WadAnimation Clone()
        {
            // TODO Investigate if we actually need 'RealNumberOfFrames'.

            var animation = (WadAnimation)MemberwiseClone();
            animation.KeyFrames = KeyFrames.ConvertAll(keyFrame => keyFrame.Clone());
            animation.StateChanges = new List<WadStateChange>(StateChanges);
            animation.AnimCommands = new List<WadAnimCommand>(AnimCommands);
            return animation;
        }
    }
}
