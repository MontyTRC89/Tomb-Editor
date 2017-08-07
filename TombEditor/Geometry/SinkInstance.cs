namespace TombEditor.Geometry
{
    public class SinkInstance : PositionBasedObjectInstance
    {
        public short Strength { get; set; }

        public SinkInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type => ObjectInstanceType.Sink;

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
    }
}
