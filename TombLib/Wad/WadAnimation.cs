using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class WadAnimation
    {
        public byte FrameDuration;
        public ushort StateId;
        public short Unknown1;
        public short Speed;
        public int Acceleration;
        public int Unknown2;
        public int Unknown3;
        public ushort NextAnimation;
        public ushort NextFrame;
        public List<WadKeyFrame> KeyFrames;
        public List<WadStateChange> StateChanges;
        public List<short> AnimCommands;

        public WadAnimation()
        {
            KeyFrames = new List<WadKeyFrame>();
            StateChanges = new List<WadStateChange>();
            AnimCommands = new List<short>();
        }
    }
}
