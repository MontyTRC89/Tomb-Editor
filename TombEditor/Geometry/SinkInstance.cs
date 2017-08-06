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
            return new SinkInstance(0, Room)
            {
                X = X,
                Y = Y,
                Z = Z,
                Ocb = Ocb,
                Rotation = Rotation,
                Invisible = Invisible,
                ClearBody = ClearBody,
                CodeBits = CodeBits,
                Strength = Strength
            };
        }
    }
}
