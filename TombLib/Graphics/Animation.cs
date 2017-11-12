using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Graphics
{
    public class Animation
    {
        public string Name { get; set; }
        public short Framerate { get; set; }
        public float Speed { get; set; }
        public float Acceleration { get; set; }
        public short NextAnimation { get; set; }
        public short NextFrame { get; set; }
        public short StateId { get; set; }
        public short RealNumberOfFrames { get; set; }
        public List<KeyFrame> KeyFrames { get; set; } = new List<KeyFrame>();
        public List<StateChange> StateChanges { get; set; } = new List<StateChange>();
        public List<ushort> AnimCommands { get; set; } = new List<ushort>();
    }
}
