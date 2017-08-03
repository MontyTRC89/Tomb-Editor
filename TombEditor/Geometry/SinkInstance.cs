namespace TombEditor.Geometry
{
    public class SinkInstance : ObjectInstance
    {
        public short Strength { get; set; }

        public SinkInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type => ObjectInstanceType.Sink;

        public override ObjectInstance Clone(int newId)
        {
            return new SinkInstance(newId, Room)
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
                Strength = Strength
            };
        }
    }
}
