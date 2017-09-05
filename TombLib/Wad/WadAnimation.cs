using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{
    public class WadAnimation
    {
        public byte FrameDuration;
        public ushort StateId;
        public int Speed;
        public int Acceleration;
        public int LateralSpeed;
        public int LateralAcceleration;
        public ushort NextAnimation;
        public ushort NextFrame;
        public ushort FrameStart;
        public ushort FrameEnd;

        public List<WadKeyFrame> KeyFrames = new List<WadKeyFrame>();
        public List<WadStateChange> StateChanges = new List<WadStateChange>();
        public List<WadAnimCommand> AnimCommands = new List<WadAnimCommand>();

        // Helper fields for level compiler
        public int KeyFramesOffset;
        public int KeyFramesSize;
        public ushort RealNumberOfFrames;
        public ushort FrameBase;
    }
}
