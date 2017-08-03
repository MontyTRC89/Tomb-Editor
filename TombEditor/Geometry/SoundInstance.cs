namespace TombEditor.Geometry
{
    public class SoundSourceInstance : ObjectInstance
    {
        public short SoundId { get; set; } = 0;
        public short Flags { get; set; } = 0;

        public SoundSourceInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type => ObjectInstanceType.SoundSource;

        public override ObjectInstance Clone()
        {
            return new SoundSourceInstance(Editor.Instance.Level.GetNewObjectId(), Room)
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
                SoundId = SoundId,
                Flags = Flags
            };
        }

        public override string ToString()
        {
            return "Sound " + SoundId +
                ", ID = " + Id +
                ", Room = " + Room.ToString() +
                ", X = " + Position.X +
                ", Y = " + Position.Y +
                ", Z = " + Position.Z;
        }
    }
}
