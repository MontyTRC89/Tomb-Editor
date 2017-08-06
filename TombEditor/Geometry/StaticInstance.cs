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
                CodeBits = CodeBits,
                WadObjectId = WadObjectId,
                Color = Color
            };
        }
    }
}
