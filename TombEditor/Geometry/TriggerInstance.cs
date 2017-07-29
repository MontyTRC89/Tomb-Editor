using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class TriggerInstance : IObjectInstance
    {
        public byte NumXBlocks { get; set; }
        public byte NumZBlocks { get; set; }
        public TriggerType TriggerType { get; set; }
        public TriggerTargetType TargetType { get; set; }
        public int Target { get; set; }
        public short Timer { get; set; }
        public bool OneShot { get; set; }

        private Editor _editor = Editor.Instance;

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

            output += " (" + ID + ") for ";

            if (TargetType == TriggerTargetType.Camera)
            {
                CameraInstance instance = (CameraInstance)_editor.Level.Objects[Target];
                output += "Camera (" + instance.ID + ")";
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

            if (TargetType == TriggerTargetType.FMV)
            {
                output += "FMV (" + Target + ")";
            }

            if (TargetType == TriggerTargetType.Object)
            {
                IObjectInstance instance = _editor.Level.Objects[Target];
                if (instance.Type == ObjectInstanceType.Moveable)
                {
                    MoveableInstance moveable = (MoveableInstance)instance;
                    output += _editor.MoveablesObjectIds[(int)moveable.Model.ObjectID] + " (" + instance.ID + ")";
                }
                else
                {

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
                SinkInstance instance = (SinkInstance)_editor.Level.Objects[Target];
                output += "Sink (" + instance.ID + ")";
            }

            if (TargetType == TriggerTargetType.Target)
            {
                IObjectInstance instance = _editor.Level.Objects[Target];
                output += "Camera Target (" + instance.ID + ")";
            }

            return output;
        }

        public override IObjectInstance Clone()
        {
            return new TriggerInstance(0, Room)
            {
                X = X,
                Y = Y,
                Z = Z,
                OCB = OCB,
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
