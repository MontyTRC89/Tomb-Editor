namespace TombEditor.Geometry
{
    public class CameraInstance : ObjectInstance
    {
        public short Sequence { get; set; }
        public short Timer { get; set; }
        public short Roll { get; set; }
        public short Number { get; set; }
        public short Speed { get; set; }
        public short Fov { get; set; }
        public short Flags { get; set; }
        public bool Fixed { get; set; }

        public CameraInstance(int id, Room room)
            : base(ObjectInstanceType.Camera, id, room)
        { }

        public override ObjectInstance Clone()
        {
            return new CameraInstance(0, Room)
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
                Sequence = Sequence,
                Timer = Timer,
                Roll = Roll,
                Number = Number,
                Speed = Speed,
                Fov = Fov,
                Flags = Flags,
                Fixed = Fixed
            };
        }
    }
}
