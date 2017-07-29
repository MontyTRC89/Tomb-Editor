using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class SoundInstance : IObjectInstance
    {
        public short SoundID { get; set; }

        public short Flags { get; set; }

        public SoundInstance(int id, Room room)
            : base(ObjectInstanceType.Sound, id, room)
        { }

        public override IObjectInstance Clone()
        {
            return new SoundInstance(0, Room)
            {
                X = X,
                Y = Y,
                Z = Z,
                OCB = OCB,
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
                Type = Type,
                SoundID = SoundID,
                Flags = Flags
            };
        }
    }
}
