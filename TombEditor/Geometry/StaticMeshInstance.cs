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

        public StaticMeshInstance(int id, short room)
            : base(ObjectInstanceType.StaticMesh, id, room)
        { }

        public override IObjectInstance Clone()
        {
            StaticMeshInstance instance = new StaticMeshInstance(0, Room);

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

            instance.Model = Model;
            instance.ObjectID = ObjectID;
            instance.Color = Color;

            return instance;
        }
    }
}
