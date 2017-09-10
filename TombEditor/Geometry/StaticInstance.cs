using System;

namespace TombEditor.Geometry
{
    public class StaticInstance : ItemInstance
    {
        public short Ocb { get; set; } = 0;
        public override ItemType ItemType => new ItemType(true, WadObjectId);

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
    }
}
