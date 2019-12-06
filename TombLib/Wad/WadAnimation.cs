using System.Collections.Generic;

namespace TombLib.Wad
{
    public class WadAnimation
    {
        public byte FrameRate { get; set; } = 1;
        public ushort StateId { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextFrame { get; set; }
        public ushort RealNumberOfFrames { get; set; } // FIXME: LEGACY, remove if turns out not needed
        public string Name { get; set; } = "Animation";

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
            var animation = (WadAnimation)MemberwiseClone();
            animation.KeyFrames = KeyFrames.ConvertAll(keyFrame => keyFrame.Clone());

            animation.AnimCommands = new List<WadAnimCommand>();
            foreach (var ac in AnimCommands)
                animation.AnimCommands.Add(ac.Clone());

            animation.StateChanges = new List<WadStateChange>();
            foreach (var sc in StateChanges)
                animation.StateChanges.Add(sc.Clone());

            return animation;
        }

        // FIXME: Addressed to Monty - please remove RealNumberOfFrames and all other deprecated values
        // some day, as it smells BADLY.

        public int GetRealNumberOfFrames(int keyFrameCount = -1)
        {
            if (keyFrameCount < 0) keyFrameCount = KeyFrames.Count;
            if (keyFrameCount == 0) return 0;

            return FrameRate * (keyFrameCount - 1) + 1;

            // HACK! For some reason, different FrameRate values have different formula!
            // Maybe legacy native tools (AnimEdit) bug or tomb4 hack?
            // Uncomment this code if straightforward recalculation turns out to be unusable.

            // switch (FrameRate)
            // {
            //     default:
            //     case 1:
            //         return FrameRate * (keyFrameCount - 1) + 1;
            //     case 2:
            //     case 4:
            //         return FrameRate * (keyFrameCount - 1);
            // }

        }
    }
}
