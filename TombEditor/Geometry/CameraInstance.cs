using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class CameraInstance : IObjectInstance
    {
        public short Sequence { get; set; }
        public short Timer { get; set; }
        public short Roll { get; set; }
        public short Number { get; set; }
        public short Speed { get; set; }
        public short FOV { get; set; }
        public short Flags { get; set; }
        public bool Fixed { get; set; }

        public CameraInstance(int id, short room)
            : base(ObjectInstanceType.Camera, id, room)
        { }

        public override IObjectInstance Clone()
        {
            CameraInstance instance = new CameraInstance(0, Room);

            instance.X = X;
            instance.Y = Y;
            instance.Z = Z;
            instance.OCB = OCB;
            instance.Rotation = Rotation;
            instance.Invisible = Invisible;
            instance.ClearBody = ClearBody;
            instance.Bits[0] = Bits[0];
            instance.Bits[1] = Bits[1];
            instance.Bits[2] = Bits[2];
            instance.Bits[3] = Bits[3];
            instance.Bits[4] = Bits[4];
            instance.Type = Type;

            instance.Sequence = Sequence;
            instance.Timer = Timer;
            instance.Roll = Roll;
            instance.Number = Number;
            instance.Speed = Speed;
            instance.FOV = FOV;
            instance.Flags = Flags;
            instance.Fixed = Fixed;

            return instance;
        }
    }
}
