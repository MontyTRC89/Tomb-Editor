namespace TombEditor.Geometry
{
    public class SoundSourceInstance : ObjectInstance
    {
        public short SoundId { get; set; } = 0;
        public short Flags { get; set; } = 0;

        public SoundSourceInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type
        {
            get { return ObjectInstanceType.SoundSource; }
        }

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
                CodeBits = CodeBits,
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
