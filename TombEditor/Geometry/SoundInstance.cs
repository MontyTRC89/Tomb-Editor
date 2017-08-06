namespace TombEditor.Geometry
{
    public class SoundSourceInstance : ObjectInstance
    {
        public short SoundId { get; set; } = 0;
        public short Flags { get; set; } = 0;
        public byte CodeBits { get; set; } = 0; // Only the lower 5 bits are used.

        public SoundSourceInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type
        {
            get { return ObjectInstanceType.SoundSource; }
        }

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
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
