using SharpDX;

namespace TombEditor.Geometry
{
    public enum ObjectInstanceType : byte
    {
        Moveable,
        Static,
        Camera,
        Sink,
        Portal,
        Trigger,
        SoundSource,
        FlyByCamera,
        // TODO Light does not derive from "ObjectInstance".
        // Are there side effects from this approach?
        // We should make light derive from ObjectInstance.
        Light
    }

    public abstract class ObjectInstance
    {
        public int Id { get; set; }
        public Room Room { get; set; }
        public Vector3 Position { get; set; }
        public short Ocb { get; set; } = 0;
        public short Rotation { get; set; } = 0;
        public bool Invisible { get; set; } = false;
        public bool ClearBody { get; set; } = false;
        public bool[] Bits { get; } = { false, false, false, false, false };
        public byte X { get; set; }
        public byte Z { get; set; }
        public short Y { get; set; }

        protected ObjectInstance(int id, Room room)
        {
            Id = id;
            Room = room;
        }

        public abstract ObjectInstance Clone();

        public abstract ObjectInstanceType Type { get; }

        public ObjectPtr ObjectPtr
        {
            get { return new ObjectPtr(Type, Id); }
        }

        public void Move(int deltaX, int deltaY, int deltaZ)
        {
            Position = Position + new Vector3(deltaX, deltaY, deltaZ);
        }
    }
}