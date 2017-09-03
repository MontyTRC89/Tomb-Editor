using System.Collections.Generic;

namespace TombLib.Graphics
{
    public class Animation
    {
        public string Name;
        public short Framerate;
        public float Speed;
        public float Acceleration;
        public short NextAnimation;
        public short NextFrame;
        public short StateId;
        public List<KeyFrame> KeyFrames = new List<KeyFrame>();
        public List<StateChange> StateChanges = new List<StateChange>();
        public List<ushort> AnimCommands = new List<ushort>();
    }
}
