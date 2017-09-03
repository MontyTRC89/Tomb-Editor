namespace TombEditor.Geometry
{
    public class StaticInstance : ItemInstance
    {
        public override ItemType ItemType => new ItemType(true, WadObjectId);

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
    }
}
