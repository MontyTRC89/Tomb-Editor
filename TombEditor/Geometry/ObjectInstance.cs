using SharpDX;

namespace TombEditor.Geometry
{
    public enum ObjectInstanceType : byte
    {
        Moveable,
        StaticMesh,
        Camera,
        Sink,
        Portal,
        Trigger,
        Sound,
        FlyByCamera
    }

    public abstract class ObjectInstance
    {
        public int Id { get; set; }
        public Room Room { get; set; }
        public Vector3 Position { get; set; }
        public short Ocb { get; set; }
        public short Rotation { get; set; }
        public bool Invisible { get; set; }
        public bool ClearBody { get; set; }
        public bool[] Bits { get; set; } = { false, false, false, false, false };
        public ObjectInstanceType Type { get; set; }
        public byte X { get; set; }
        public byte Z { get; set; }
        public short Y { get; set; }

        protected ObjectInstance(ObjectInstanceType type, int id, Room room)
        {
            Id = id;
            Room = room;
            Type = type;
        }

        public abstract ObjectInstance Clone();

        public void Move(int deltaX, int deltaY, int deltaZ)
        {
            Position = Position + new Vector3(deltaX, deltaY, deltaZ);
        }
    }
}
