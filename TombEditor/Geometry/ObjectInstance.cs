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
    }

    public abstract class SectorBasedObjectInstance : ObjectInstance
    {
        public byte X { get; set; }
        public byte Z { get; set; }
        public byte NumXBlocks { get; set; }
        public byte NumZBlocks { get; set; }

        public SectorBasedObjectInstance(int id, Room room)
            : base(id, room)
        { }

        public Rectangle Area
        {
            get { return new Rectangle(X, Z, X + NumXBlocks - 1, Z + NumZBlocks - 1); }
        }
    }

    public abstract class PositionBasedObjectInstance : ObjectInstance
    {
        public Vector3 Position { get; set; }

        public PositionBasedObjectInstance(int id, Room room)
            : base(id, room)
        { }

        public void Move(int deltaX, int deltaY, int deltaZ)
        {
            Position = Position + new Vector3(deltaX, deltaY, deltaZ);
        }
    }
}