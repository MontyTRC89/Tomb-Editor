using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using TombLib.NG;

namespace TombLib.LevelData
{
    public enum TriggerType : ushort
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

    public enum TriggerTargetType : ushort
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

    public interface ITriggerParameter : IEquatable<ITriggerParameter>
    { }
    public class TriggerParameterUshort : IEquatable<TriggerParameterUshort>, ITriggerParameter
    {
        public ushort Key { get; }
        public object NameObject { get; }
        public TriggerParameterUshort(ushort key, object nameObject = null)
        {
            Key = key;
            NameObject = nameObject;
        }
        public string Name => NameObject?.ToString();
        public override string ToString() => Name ?? Key.ToString();

        public static bool operator ==(TriggerParameterUshort first, TriggerParameterUshort second)
        {
            if (ReferenceEquals(first, null))
                return ReferenceEquals(second, null);
            else if (ReferenceEquals(second, null))
                return false;
            return first.Key == second.Key;
        }
        public static bool operator !=(TriggerParameterUshort first, TriggerParameterUshort second) => !(first == second);
        public override int GetHashCode() => Key;
        public override bool Equals(object other)
        {
            if (!(other is TriggerParameterUshort))
                return false;
            return this == (TriggerParameterUshort)other;
        }
        public bool Equals(TriggerParameterUshort other) => this == other;
        bool IEquatable<ITriggerParameter>.Equals(ITriggerParameter other) => Equals(other);
    }

    public class TriggerInstance : SectorBasedObjectInstance
    {
        public TriggerType TriggerType { get; set; } = TriggerType.Trigger;
        public TriggerTargetType TargetType { get; set; } = TriggerTargetType.FlipEffect;

        private ITriggerParameter _target = null;
        public ITriggerParameter Target
        {
            get { return _target; }
            set { UpdateEvents(ref _target, value); }
        }

        private ITriggerParameter _timer = null;
        public ITriggerParameter Timer
        {
            get { return _timer; }
            set { UpdateEvents(ref _timer, value); }
        }

        private ITriggerParameter _extra = null;
        public ITriggerParameter Extra
        {
            get { return _extra; }
            set { UpdateEvents(ref _extra, value); }
        }

        public bool OneShot { get; set; } = false;
        public byte CodeBits { get; set; } = 0x1f; // Only the lower 5 bits are used.

        public TriggerInstance(RectangleInt2 area)
            : base(area)
        { }

        public override string ToString()
        {
            string output = TriggerType.ToString() + " ";
            if (output.IndexOf("trigger", StringComparison.OrdinalIgnoreCase) == -1)
                output += "Trigger ";
            output += "in room '" + (Room?.ToString() ?? "NULL") + "' ";
            output += "on sectors [" + Area.X0 + ", " + Area.Y0 + " to " + Area.X1 + ", " + Area.Y1 + "] ";
            output += "for " + TargetType.ToString() + " ";
            output += "(Target: " + Target + ", Timer: " + Timer + ", Extra: " + Extra + ")";
            return output;
        }

        public override void AddToRoom(Level level, Room room)
        {
            base.AddToRoom(level, room);

            for (int x = Area.X0; x <= Area.X1; x++)
                for (int z = Area.Y0; z <= Area.Y1; z++)
                    room.Blocks[x, z].Triggers.Add(this);
        }

        public override void RemoveFromRoom(Level level, Room room)
        {
            base.RemoveFromRoom(level, room);

            for (int x = Area.X0; x <= Area.X1; x++)
                for (int z = Area.Y0; z <= Area.Y1; z++)
                    room.Blocks[x, z].Triggers.Remove(this);
        }

        private void ParameterDeletedEvent(ITriggerParameter parameter)
        {
            if (_target == parameter)
                _target = null;
            if (_timer == parameter)
                _timer = null;
            if (_extra == parameter)
                _extra = null;
        }
        private void ObjectParameterDeletedEvent(ObjectInstance instance)
        {
            ParameterDeletedEvent(instance);
        }

        private void RoomParameterDeletedEvent(Room instance)
        {
            ParameterDeletedEvent(instance);
        }

        private void UpdateEvents(ref ITriggerParameter oldValue, ITriggerParameter newValue)
        {
            if (oldValue == newValue)
                return;

            if (newValue is ObjectInstance)
                ((ObjectInstance)newValue).DeletedEvent += ObjectParameterDeletedEvent;
            if (newValue is Room)
                ((Room)newValue).DeletedEvent += RoomParameterDeletedEvent;

            if (newValue is ObjectInstance)
                ((ObjectInstance)newValue).DeletedEvent -= ObjectParameterDeletedEvent;
            if (newValue is Room)
                ((Room)newValue).DeletedEvent -= RoomParameterDeletedEvent;

            oldValue = newValue;
        }

        public bool IsPointingTo(ITriggerParameter value)
        {
            if (Target != null && Target.Equals(value))
                return true;
            if (Timer != null && Timer.Equals(value))
                return true;
            if (Extra != null && Extra.Equals(value))
                return true;
            return false;
        }

        public override void TransformRoomReferences(Func<Room, Room> transformRoom)
        {
            if (_target is Room)
                _target = transformRoom((Room)_target);
            if (_timer is Room)
                _timer = transformRoom((Room)_timer);
            if (_extra is Room)
                _extra = transformRoom((Room)_extra);
        }
    }
}
