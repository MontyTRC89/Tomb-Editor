using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
        public ushort FrameStart { get; set; }
        public ushort FrameEnd { get; set; }

        public List<WadKeyFrame> KeyFrames { get; private set; }
        public List<WadStateChange> StateChanges { get; private set; }
        public List<WadAnimCommand> AnimCommands { get; private set; }

        // Helper fields for level compiler
        public int KeyFramesOffset { get; set; }
        public int KeyFramesSize { get; set; }
        public ushort RealNumberOfFrames { get; set; }
        public ushort FrameBase { get; set; }

        public WadAnimation()
        {
            KeyFrames = new List<WadKeyFrame>();
            StateChanges = new List<WadStateChange>();
            AnimCommands = new List<WadAnimCommand>();
        }
    }
}
