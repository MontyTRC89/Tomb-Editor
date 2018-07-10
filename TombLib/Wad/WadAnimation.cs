using System.Collections.Generic;

namespace TombLib.Wad
{
    public class WadAnimation
    {
        public byte FrameRate { get; set; }
        public ushort StateId { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextFrame { get; set; }
        public ushort RealNumberOfFrames { get; set; }
        public string Name { get; set; } = "Animation";

        // TODO: old deprecated stuff
        public int Speed { get; set; }
        public int Acceleration { get; set; }
        public int LateralSpeed { get; set; }
        public int LateralAcceleration { get; set; }

        // New velocities. Originally Core's AnimEdit had Start Velocity and End Velocity pairs and
        // acceleration is obtained used the equations of motion: v = v0 + a * t where in our case
        // t is (Number of KeyFrames + 1) * FrameRate
        public float StartVelocity { get; set; }
        public float EndVelocity { get; set; }
        public float StartLateralVelocity { get; set; }
        public float EndLateralVelocity { get; set; }

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
