﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public enum TriggerType : byte
    {
        Trigger = 0,
        Pad = 1,
        Switch = 2,
        Key = 3,
        Pickup = 4,
        Heavy = 5,
        Antipad = 6,
        Combat = 7,
        Dummy = 8,
        Antitrigger = 9,
        HeavySwitch = 10,
        HeavyAntritrigger = 11,
        ConditionNg = 12
    }

    public enum TriggerTargetType : byte
    {
        Object = 0,
        Camera = 1,
        Sink = 2,
        FlipMap = 3,
        FlipOn = 4,
        FlipOff = 5,
        Target = 6,
        FinishLevel = 7,
        PlayAudio = 8,
        FlipEffect = 9,
        Secret = 10,
        ActionNg = 11,
        FlyByCamera = 12,
        ParameterNg = 13,
        FmvNg = 14,
        TimerfieldNg = 15,
    }

    public class TriggerInstance : SectorBasedObjectInstance
    {
        public TriggerType TriggerType { get; set; } = TriggerType.Trigger;
        public TriggerTargetType TargetType { get; set; } = TriggerTargetType.FlipEffect;
        public ObjectInstance TargetObj { get; set; } = null; //Used for following old trigger types: "Camera", "FlyByCamera", "Object", "Sink", "Target"
        public short TargetData { get; set; } = 0;
        public short Timer { get; set; } = 0;
        public bool OneShot { get; set; } = false;
        public byte CodeBits { get; set; } = 0x1f; // Only the lower 5 bits are used.
        public short ExtraData { get; set; } = 0;

        public TriggerInstance(Rectangle area)
            : base(area)
        { }

        public string TargetObjString => TargetObj?.ToString() ?? "NULL";

        public override string ToString()
        {
            string output = TriggerType.ToString() + " ";
            if (output.IndexOf("trigger", StringComparison.OrdinalIgnoreCase) == -1)
                output += "Trigger ";
            output += "in room '" + (Room?.ToString() ?? "NULL") + "' ";
            output += "on sectors [" + Area.X + ", " + Area.Y + " to " + Area.Right + ", " + Area.Bottom + "] ";
            output += "for " + TargetType.ToString() + " ";
            switch (TargetType)
            {
                case TriggerTargetType.Camera:
                case TriggerTargetType.Target:
                case TriggerTargetType.Sink:
                case TriggerTargetType.FlyByCamera:
                case TriggerTargetType.Object:
                    output += "(" + TargetObjString + ") ";
                    break;
                default:
                    output += "(" + TargetData + ") ";
                    break;
            }
            return output;
        }

        public override void AddToRoom(Level level, Room room)
        {
            base.AddToRoom(level, room);

            for (int x = Area.X; x <= Area.Right; x++)
                for (int z = Area.Y; z <= Area.Bottom; z++)
                    room.Blocks[x, z].Triggers.Add(this);
        }

        public override void RemoveFromRoom(Level level, Room room)
        {
            base.RemoveFromRoom(level, room);

            for (int x = Area.X; x <= Area.Right; x++)
                for (int z = Area.Y; z <= Area.Bottom; z++)
                    room.Blocks[x, z].Triggers.Remove(this);
        }

        public T CastTargetType<T>(Room room) where T : ObjectInstance
        {
            var castedObject = TargetObj as T;
            if (castedObject == null)
                throw new Exception("Object trigger target of trigger does mistakenly point to '" + TargetObjString +
                    "' instead of an " + typeof(T).ToString().Replace("Instance", "") + ". Trigger (Room: " + room + ") information: '" + this + "'");
            return castedObject;
        }

        public static bool UsesTargetObj(TriggerTargetType targetType)
        {
            switch (targetType)
            {
                case TriggerTargetType.Object:
                case TriggerTargetType.Camera:
                case TriggerTargetType.Target:
                case TriggerTargetType.FlyByCamera:
                case TriggerTargetType.Sink:
                case TriggerTargetType.ActionNg:
                    return true;
                default:
                    return false;
            }
        }
    }
}
