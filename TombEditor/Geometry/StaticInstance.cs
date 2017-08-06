using System;

namespace TombEditor.Geometry
{
    public class StaticInstance : ItemInstance
    {
        public System.Drawing.Color Color { get; set; } = System.Drawing.Color.FromArgb(255, 128, 128, 128);

        public StaticInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type
        {
            get { return ObjectInstanceType.Static; }
        }

        public override ItemType ItemType
        {
            get { return new ItemType(true, WadObjectId); }
        }

        public override ObjectInstance Clone()
        {
            return new StaticInstance(0, Room)
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
                WadObjectId = WadObjectId,
                Color = Color
            };
        }
    }
}
