using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class TriggerInstance : ObjectInstance
    {
        public byte NumXBlocks { get; set; }
        public byte NumZBlocks { get; set; }
        public TriggerType TriggerType { get; set; }
        public TriggerTargetType TargetType { get; set; }
        public int Target { get; set; }
        public short Timer { get; set; }
        public bool OneShot { get; set; }

        public TriggerInstance(int id, Room room)
            : base(ObjectInstanceType.Trigger, id, room)
        { }

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

        public Rectangle Area
        {
            get { return new Rectangle(X, Z, X + NumXBlocks - 1, Z + NumZBlocks - 1); }
        }

        public override ObjectInstance Clone()
        {
            return new TriggerInstance(0, Room)
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
                Type = Type
            };
        }
    }
}
