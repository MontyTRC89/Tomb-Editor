using System;

namespace TombEditor.Geometry
{
    public class StaticInstance : ItemInstance
    {
        public System.Drawing.Color Color { get; set; } = System.Drawing.Color.FromArgb(255, 128, 128, 128);
        
        public override ItemType ItemType => new ItemType(true, WadObjectId);

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
    }
}
