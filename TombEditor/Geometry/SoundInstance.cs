namespace TombEditor.Geometry
{
    public class SoundInstance : ObjectInstance
    {
        public short SoundId { get; set; }

        public short Flags { get; set; }

        public SoundInstance(int id, Room room)
            : base(ObjectInstanceType.Sound, id, room)
        { }

        public override ObjectInstance Clone()
        {
            return new SoundInstance(0, Room)
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
    }
}
