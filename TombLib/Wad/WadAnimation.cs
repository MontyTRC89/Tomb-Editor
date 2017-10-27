using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TombLib.Graphics;

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
        public string Name { get; set; }

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
            Name = "Animation";
        }

        public WadAnimation Clone()
        {
            var animation = new WadAnimation();

            animation.FrameDuration = FrameDuration;
            animation.StateId = StateId;
            animation.Speed = Speed;
            animation.Acceleration = Acceleration;
            animation.LateralSpeed = LateralSpeed;
            animation.LateralAcceleration = LateralAcceleration;
            animation.NextAnimation = NextAnimation;
            animation.NextFrame = NextFrame;
            animation.FrameStart = FrameStart;
            animation.FrameEnd = FrameEnd;
            animation.Name = Name;

            animation.RealNumberOfFrames = RealNumberOfFrames;
            animation.FrameBase = FrameBase;
            
            foreach (var keyframe in KeyFrames)
                animation.KeyFrames.Add(keyframe.Clone());

            foreach (var change in StateChanges)
                animation.StateChanges.Add(change.Clone());

            foreach (var command in AnimCommands)
                animation.AnimCommands.Add(command.Clone());

            return animation;
        }
    }
}
