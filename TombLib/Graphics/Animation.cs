using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public List<KeyFrame> KeyFrames;
        public List<StateChange> StateChanges;
        public List<ushort> AnimCommands;

        public Animation()
        {
            KeyFrames = new List<KeyFrame>();
            StateChanges = new List<StateChange>();
            AnimCommands = new List<ushort>();
        }
    }
}
