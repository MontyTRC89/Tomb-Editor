using SharpDX;
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

        public TriggerInstance(int id, short room)
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
                    output += _editor.MoveableNames[(int)moveable.Model.ObjectID] + " (" + instance.ID + ")";
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

        public Rectangle Area
        {
            get { return new Rectangle(X, Z, X + NumXBlocks - 1, Z + NumZBlocks - 1); }
        }

        public override IObjectInstance Clone()
        {
            TriggerInstance instance = new TriggerInstance(0, Room);

            instance.X = X;
            instance.Y = Y;
            instance.Z = Z;
            instance.OCB = OCB;
            instance.Rotation = Rotation;
            instance.Invisible = Invisible;
            instance.ClearBody = ClearBody;
            instance.Bits[0] = Bits[0];
            instance.Bits[1] = Bits[1];
            instance.Bits[2] = Bits[2];
            instance.Bits[3] = Bits[3];
            instance.Bits[4] = Bits[4];
            instance.Type = Type;

            return instance;
        }
    }
}
