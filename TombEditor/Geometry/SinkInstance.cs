namespace TombEditor.Geometry
{
    public class SinkInstance : ObjectInstance
    {
        public short Strength { get; set; }

        public SinkInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type
        {
            get { return ObjectInstanceType.Sink; }
        }

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
    }
}
