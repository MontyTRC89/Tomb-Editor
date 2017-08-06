using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class FlybyCameraInstance : ObjectInstance
    {
        public short Sequence { get; set; }
        public short Timer { get; set; }
        public short Roll { get; set; }
        public short Number { get; set; }
        public short Speed { get; set; }
        public short Fov { get; set; } = 45;
        public ushort Flags { get; set; }
        public bool Fixed { get; set; }
        public short DirectionX { get; set; }
        public short DirectionY { get; set; }

        public FlybyCameraInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type
        {
            get { return ObjectInstanceType.FlyByCamera; }
        }

        public override ObjectInstance Clone()
        {
            return new FlybyCameraInstance(0, Room)
            {
                X = X,
                Y = Y,
                Z = Z,
                Ocb = Ocb,
                Rotation = Rotation,
                Invisible = Invisible,
                ClearBody = ClearBody,
                CodeBits = CodeBits,
                Sequence = Sequence,
                Timer = Timer,
                Roll = Roll,
                Number = Number,
                Speed = Speed,
                Fov = Fov,
                Flags = Flags,
                Fixed = Fixed,
                DirectionX = DirectionX,
                DirectionY = DirectionY
            };
        }


        public override string ToString()
        {
            return "FlyBy " + (Fixed ? "Fixed" : "") +
                ", ID = " + Id +
                ", Room = " + Room.ToString() +
                ", X = " + Position.X +
                ", Y = " + Position.Y +
                ", Z = " + Position.Z;
        }
    }
}
