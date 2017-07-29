using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TombLib.Graphics;
using TombLib.Wad;
using SharpDX;

namespace TombEditor.Geometry
{
    public class StaticMeshInstance : IObjectInstance
    {
        public StaticModel Model { get; set; }
        public int ObjectID { get; set; }
        public System.Drawing.Color Color { get; set; } = System.Drawing.Color.FromArgb(255, 128, 128, 128);

        public StaticMeshInstance(int id, Room room)
            : base(ObjectInstanceType.StaticMesh, id, room)
        { }

        public override IObjectInstance Clone()
        {
            return new StaticMeshInstance(0, Room)
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
                ObjectID = ObjectID,
                Color = Color
            };
        }
    }
}
