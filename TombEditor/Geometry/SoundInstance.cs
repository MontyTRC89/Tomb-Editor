namespace TombEditor.Geometry
{
    public class SoundSourceInstance : ObjectInstance
    {
        public short SoundId { get; set; }

        public short Flags { get; set; }

        public SoundSourceInstance(int id, Room room)
            : base(ObjectInstanceType.SoundSource, id, room)
        { }

        public override ObjectInstance Clone()
        {
            return new SoundSourceInstance(0, Room)
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
