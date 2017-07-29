namespace TombEditor.Geometry
{
    public class SinkInstance : ObjectInstance
    {
        public short Strength { get; set; }

        public SinkInstance(int id, Room room)
            : base(ObjectInstanceType.Sink, id, room)
        { }

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
                Bits =
                {
                    [0] = Bits[0],
                    [1] = Bits[1],
                    [2] = Bits[2],
                    [3] = Bits[3],
                    [4] = Bits[4]
                },
                Type = Type,
                Strength = Strength
            };
        }
    }
}
