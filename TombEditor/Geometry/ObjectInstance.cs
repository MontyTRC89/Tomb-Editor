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

    public struct ObjectPtr
    {
        public ObjectInstanceType Type { get; set; }
        public int Index { get; set; }

        public ObjectPtr(ObjectInstanceType type, int index)
        {
            Type = type;
            Index = index;
        }


        public static bool operator ==(ObjectPtr first, ObjectPtr second)
        {
            return (first.Type == second.Type) && (first.Index == second.Index);
        }

        public static bool operator !=(ObjectPtr first, ObjectPtr second)
        {
            return (first.Type != second.Type) || (first.Index != second.Index);
        }

        public override bool Equals(object obj)
        {
            return this == (ObjectPtr)obj;
        }

        public override int GetHashCode()
        {
            return (Type.GetHashCode() << 16) ^ Index.GetHashCode();
        }

        public override string ToString()
        {
            string result = "Unknown";
            // HACK for now until we get proper references
            switch (Type)
            {
                case ObjectInstanceType.Camera:
                case ObjectInstanceType.FlyByCamera:
                case ObjectInstanceType.Moveable:
                case ObjectInstanceType.Sink:
                case ObjectInstanceType.SoundSource:
                case ObjectInstanceType.StaticMesh:
                    result = Editor.Instance.Level.Objects[Index].ToString();
                    break;
                case ObjectInstanceType.Portal:
                    result = Editor.Instance.Level.Portals[Index].ToString();
                    break;
                case ObjectInstanceType.Trigger:
                    result = Editor.Instance.Level.Triggers[Index].ToString();
                    break;
                case ObjectInstanceType.Light:
                    if (Editor.Instance.SelectedRoom != null)
                        result = Editor.Instance.SelectedRoom.Lights[Index].ToString();
                    break;
            }
            return result;
        }
    }
}