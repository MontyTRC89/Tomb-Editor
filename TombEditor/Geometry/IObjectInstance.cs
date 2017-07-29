using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public abstract class IObjectInstance
    {
        public int ID { get; set; }
        public Room Room { get; set; }
        public Vector3 Position { get; set; }
        public short OCB { get; set; }
        public short Rotation { get; set; }
        public bool Invisible { get; set; }
        public bool ClearBody { get; set; }
        public bool[] Bits { get; set; }
        public ObjectInstanceType Type { get; set; }
        public byte X { get; set; }
        public byte Z { get; set; }
        public short Y { get; set; }

        public IObjectInstance(ObjectInstanceType type, int id, Room room)
        {
            ID = id;
            Room = room;
            Type = type;
            Bits = new bool[] { false, false, false, false, false };
        }

        public abstract IObjectInstance Clone();

        public void Move(int deltaX, int deltaY, int deltaZ)
        {
            Position = Position + new Vector3(deltaX, deltaY, deltaZ);
        }
    }
}
