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
            string output = "";

            if (TriggerType == TriggerType.Antipad)
                output = "Antipad";
            if (TriggerType == TriggerType.Antitrigger)
                output = "Antitrigger";
            if (TriggerType == TriggerType.Combat)
                output = "Combat";
            if (TriggerType == TriggerType.Condition)
                output = "Condition";
            if (TriggerType == TriggerType.Dummy)
                output = "Dummy";
            if (TriggerType == TriggerType.Heavy)
                output = "Heavy";
            if (TriggerType == TriggerType.HeavyAntritrigger)
                output = "HeavyAntritrigger";
            if (TriggerType == TriggerType.HeavySwitch)
                output = "HeavySwitch";
            if (TriggerType == TriggerType.Key)
                output = "Key";
            if (TriggerType == TriggerType.Monkey)
                output = "Monkey";
            if (TriggerType == TriggerType.Pad)
                output = "Pad";
            if (TriggerType == TriggerType.Pickup)
                output = "Pickup";
            if (TriggerType == TriggerType.Switch)
                output = "Switch";
            if (TriggerType == TriggerType.Trigger)
                output = "Trigger";

            output += " (" + Id + ") for ";

            if (TargetType == TriggerTargetType.Camera)
            {
                CameraInstance instance = (CameraInstance)Editor.Instance.Level.Objects[Target];
                output += "Camera (" + instance.Id + ")";
            }

            if (TargetType == TriggerTargetType.FinishLevel)
            {
                output += "FinishLevel (" + Target + ")";
            }

            if (TargetType == TriggerTargetType.FlipEffect)
            {
                output += "FlipEffect (" + Target + ")";
            }

            if (TargetType == TriggerTargetType.FlipMap)
            {
                output += "FlipMap (" + Target + ")";
            }

            if (TargetType == TriggerTargetType.FlipOff)
            {
                output += "FlipOff (" + Target + ")";
            }

            if (TargetType == TriggerTargetType.FlipOn)
            {
                output += "FlipOn (" + Target + ")";
            }

            if (TargetType == TriggerTargetType.FlyByCamera)
            {
                //FlybyCameraInstance instance = (FlybyCameraInstance)_editor.Level.Objects[Target];
                output += "FlybyCamera (FLYBY" + /*instance.ID +*/ ")";
            }

            if (TargetType == TriggerTargetType.Fmv)
            {
                output += "FMV (" + Target + ")";
            }

            if (TargetType == TriggerTargetType.Object)
            {
                var instance = Editor.Instance.Level.Objects[Target];
                if (instance.Type == ObjectInstanceType.Moveable)
                {
                    var moveable = (MoveableInstance)instance;
                    output += Editor.Instance.MoveableNames[(int)moveable.Model.ObjectID] + " (" + instance.Id + ")";
                }
            }

            if (TargetType == TriggerTargetType.PlayAudio)
            {
                output += "CD Track (" + Target + ")";
            }

            if (TargetType == TriggerTargetType.Secret)
            {
                output += "Secret (" + Target + ")";
            }

            if (TargetType == TriggerTargetType.Sink)
            {
                var instance = (SinkInstance)Editor.Instance.Level.Objects[Target];
                output += "Sink (" + instance.Id + ")";
            }

            if (TargetType == TriggerTargetType.Target)
            {
                var instance = Editor.Instance.Level.Objects[Target];
                output += "Camera Target (" + instance.Id + ")";
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
