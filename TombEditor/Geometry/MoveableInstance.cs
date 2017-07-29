using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TombLib.Graphics;
using TombLib.Wad;
using SharpDX;

namespace TombEditor.Geometry
{
    public class MoveableInstance : IObjectInstance
    {
        public SkinnedModel Model { get; set; }
        public int ObjectID { get; set; }

        public MoveableInstance(int id, Room room)
            : base(ObjectInstanceType.Moveable, id, room)
        { }

        public override IObjectInstance Clone()
        {
            return new MoveableInstance(0, Room)
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
                Model = Model,
                ObjectID = ObjectID
            };
        }
    }
}
