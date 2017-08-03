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
        public bool[] Flags { get; set; } = new bool[16];
        public bool Fixed { get; set; }
        public short DirectionX { get; set; }
        public short DirectionY { get; set; }

        public FlybyCameraInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type => ObjectInstanceType.FlyByCamera;

        public override ObjectInstance Clone(int newId)
        {
            var instance = new FlybyCameraInstance(newId, Room)
            {
                X = X,
                Y = Y,
                Z = Z,
                Ocb = Ocb,
                Rotation = Rotation,
                Invisible = Invisible,
                ClearBody = ClearBody,
                Bits =
                {
                    [0] = Bits[0],
                    [1] = Bits[1],
                    [2] = Bits[2],
                    [3] = Bits[3],
                    [4] = Bits[4]
                },
                Sequence = Sequence,
                Timer = Timer,
                Roll = Roll,
                Number = Number,
                Speed = Speed,
                Fov = Fov
            };


            for (int i = 0; i < 16; i++)
                instance.Flags[i] = Flags[i];
            instance.Fixed = Fixed;
            instance.DirectionX = DirectionX;
            instance.DirectionY = DirectionY;

            return instance;
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
