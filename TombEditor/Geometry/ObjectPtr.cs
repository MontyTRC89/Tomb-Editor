using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombEditor.Geometry
{
    public struct ObjectPtr
    {
        public ObjectInstanceType Type { get; set; }
        public int Id { get; set; }

        public ObjectPtr(ObjectInstanceType type, int id)
        {
            Type = type;
            Id = id;
        }
        
        public static bool operator ==(ObjectPtr first, ObjectPtr second)
        {
            return (first.Type == second.Type) && (first.Id == second.Id);
        }

        public static bool operator !=(ObjectPtr first, ObjectPtr second)
        {
            return (first.Type != second.Type) || (first.Id != second.Id);
        }

        public override bool Equals(object obj)
        {
            return this == (ObjectPtr)obj;
        }

        public override int GetHashCode()
        {
            return (Type.GetHashCode() << 16) ^ Id.GetHashCode();
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
                case ObjectInstanceType.Static:
                    result = Editor.Instance.Level.Objects[Id].ToString();
                    break;
                case ObjectInstanceType.Portal:
                    var self = Id;
                    result = Editor.Instance.Level.Portals.First(p => p.Id == self).ToString();
                    break;
                case ObjectInstanceType.Trigger:
                    result = Editor.Instance.Level.Triggers[Id].ToString();
                    break;
                case ObjectInstanceType.Light:
                    if (Editor.Instance.SelectedRoom != null)
                        result = Editor.Instance.SelectedRoom.Lights[Id].ToString();
                    break;
            }
            return result;
        }
    }
}
