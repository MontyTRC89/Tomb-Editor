using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class TriggerInstance : SectorBasedObjectInstance
    {
        public TriggerType TriggerType { get; set; }
        public TriggerTargetType TargetType { get; set; }
        public int Target { get; set; }
        public short Timer { get; set; }
        public bool OneShot { get; set; }
        public byte CodeBits { get; set; } = 0; // Only the lower 5 bits are used.

        public TriggerInstance(int id, Room room)
            : base(id, room)
        { }

        public override ObjectInstanceType Type
        {
            get { return ObjectInstanceType.Trigger; }
        }

        public override string ToString()
        {
            string output = "Unknown trigger";
            switch (TriggerType)
            {
                case TriggerType.Antipad:
                    output = "Antipad Trigger";
                    break;
                case TriggerType.Antitrigger:
                    output = "Antitrigger";
                    break;
                case TriggerType.Combat:
                    output = "Combat Trigger";
                    break;
                case TriggerType.Condition:
                    output = "Condition Trigger";
                    break;
                case TriggerType.Dummy:
                    output = "Dummy Trigger";
                    break;
                case TriggerType.Heavy:
                    output = "Heavy Trigger";
                    break;
                case TriggerType.HeavyAntritrigger:
                    output = "HeavyAntritrigger";
                    break;
                case TriggerType.HeavySwitch:
                    output = "HeavySwitch Trigger";
                    break;
                case TriggerType.Key:
                    output = "Key Trigger";
                    break;
                case TriggerType.Monkey:
                    output = "Monkey Trigger";
                    break;
                case TriggerType.Pad:
                    output = "Pad Trigger";
                    break;
                case TriggerType.Pickup:
                    output = "Pickup Trigger";
                    break;
                case TriggerType.Switch:
                    output = "Switch Trigger";
                    break;
                case TriggerType.Trigger:
                    output = "Trigger";
                    break;
            }

            output += " (" + Id + ") for ";

            switch (TargetType)
            {
                case TriggerTargetType.Camera:
                    output += "Camera (" + Editor.Instance.Level.Objects[Target].ToString() + ")";
                    break;
                case TriggerTargetType.FinishLevel:
                    output += "FinishLevel (" + Target + ")";
                    break;
                case TriggerTargetType.FlipEffect:
                    output += "FlipEffect (" + Target + ")";
                    break;
                case TriggerTargetType.FlipMap:
                    output += "FlipMap (" + Target + ")";
                    break;
                case TriggerTargetType.FlipOff:
                    output += "FlipOff (" + Target + ")";
                    break;
                case TriggerTargetType.FlipOn:
                    output += "FlipOn (" + Target + ")";
                    break;
                case TriggerTargetType.FlyByCamera:
                    output += "FlybyCamera (FLYBY" + Editor.Instance.Level.Objects[Target].ToString() + ")";
                    break;
                case TriggerTargetType.Fmv:
                    output += "FMV (" + Target + ")";
                    break;
                case TriggerTargetType.Object:
                    output += "Movable (" + Editor.Instance.Level.Objects[Target].ToString() + ")";
                    break;
                case TriggerTargetType.PlayAudio:
                    output += "CD Track (" + Target + ")";
                    break;
                case TriggerTargetType.Secret:
                    output += "Secret (" + Target + ")";
                    break;
                case TriggerTargetType.Sink:
                    output += "Sink (" + Editor.Instance.Level.Objects[Target].ToString() + ")";
                    break;
                case TriggerTargetType.Target:
                    output += "Camera Target (" + Editor.Instance.Level.Objects[Target].ToString() + ")";
                    break;
                default:
                    output += "Unkown";
                    break;
            }

            return output;
        }
        
        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
    }
}
