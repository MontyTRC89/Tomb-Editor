using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using TombLib.IO;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombLib.LevelData
{
    // All possible shapes are reduced to these three basics.
    // If we need a plane volume, it will be a box with one axis scale set to minimum.
    // If we need legacy "trigger" volume, it will be a box with one axis scale set to maximum.

    public enum VolumeShape : byte
    {
        Box, Sphere, Undefined
    }

    // In future, we will allow not just to assign manually written Lua scripts from script file,
    // but also automatically construct visual scripts from predefined Lua templates.

    public enum VolumeEventMode
    {
        LevelScript, NodeEditor
    }

    // Possible activator flags. If none is set, volume is disabled.

    [Flags]
    public enum VolumeActivators : int
    {
        None = 0,
        Player = 1,
        NPCs = 2,
        OtherMoveables = 4,
        Statics = 8,
        Flybys = 16,
        PhysicalObjects = 32 // Future-proofness for Bullet
    }

    public class VolumeEvent : ICloneable, IEquatable<VolumeEvent>
    {
        private const int _noCallCounter = -1;

        public VolumeEventMode Mode = VolumeEventMode.NodeEditor;
        public string Function { get; set; } = string.Empty;
        public string Argument { get; set; } = string.Empty;
        public Vector2 NodePosition = new Vector2(float.MaxValue);
        public List<TriggerNode> Nodes { get; set; } = new List<TriggerNode>();

        public int CallCounter { get; set; } = 0; // How many times event can be called

        object ICloneable.Clone()
        {
            return Clone();
        }

        public VolumeEvent Clone()
        {
            var evt = (VolumeEvent)MemberwiseClone();

            evt.Argument = Argument;
            evt.Function = Function;
            evt.CallCounter = CallCounter;
            evt.Mode = Mode;
            evt.NodePosition = NodePosition;
            evt.Nodes = new List<TriggerNode>();
            Nodes.ForEach(n => evt.Nodes.Add(n.Clone()));
            
            return evt;
        }

        public bool Equals(VolumeEvent other)
        {
            return
                Mode == other.Mode &&
                Function == other.Function &&
                Argument == other.Argument &&
                CallCounter == other.CallCounter &&
                NodePosition == other.NodePosition &&
                Nodes.Count == other.Nodes.Count &&
                Nodes.TrueForAll(n => n.GetHashCode() == other.Nodes[Nodes.IndexOf(n)].GetHashCode());
        }

        public string GenerateFunctionName(List<VolumeEventSet> eventSets)
        {
            var belongedSet = eventSets.FirstOrDefault(s => s.OnInside == this || s.OnLeave == this || s.OnEnter == this);
            if (belongedSet == null)
                return "UNKNOWN";

            var trimmedName = "__" + eventSets.IndexOf(belongedSet).ToString().PadLeft(4, '0') + "_" +
                                     Regex.Replace(belongedSet.Name, @"\s", string.Empty);

            if (this == belongedSet.OnInside)
                return trimmedName + "_OnInside";
            else if (this == belongedSet.OnLeave)
                return trimmedName + "_OnLeave";
            else if (this == belongedSet.OnEnter)
                return trimmedName + "_OnEnter";

            return "UNKNOWN";
        }

        public void Write(BinaryWriterEx writer, List<VolumeEventSet> eventSets)
        {
            writer.Write((int)Mode);

            if (Mode == VolumeEventMode.NodeEditor)
            {
                var funcName = GenerateFunctionName(eventSets);
                writer.Write(Nodes.Count > 0 ? funcName : string.Empty);
                writer.Write(ScriptingUtils.ParseNodes(Nodes, funcName));
            }
            else
            {
                writer.Write(Function);
                writer.Write(Argument.Replace("\\n", "\n")); // Unconvert newline shortcut
            }

            writer.Write(CallCounter != 0 ? CallCounter : _noCallCounter);
        }
    }

    public class VolumeEventSet : ICloneable, IEquatable<VolumeEventSet>
    {
        public string Name = string.Empty;
        public VolumeActivators Activators;

        // Every volume's events can be reduced to these three.
        // If resulting volume should be one-shot trigger, we'll only use "OnEnter" event.

        public VolumeEvent OnEnter;
        public VolumeEvent OnLeave;
        public VolumeEvent OnInside;

        public VolumeEventSet()
        {
            Activators = VolumeActivators.Player;
            OnEnter = new VolumeEvent();
            OnInside = new VolumeEvent();
            OnLeave = new VolumeEvent();
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public VolumeEventSet Clone()
        {
            var set = (VolumeEventSet)MemberwiseClone();

            set.OnEnter = OnEnter.Clone();
            set.OnInside = OnInside.Clone();
            set.OnLeave = OnLeave.Clone();

            return set;
        }

        public string GetDescription()
        {
            string result = string.Empty;

            if (Activators != VolumeActivators.None)
            {
                result = "Event set '" + Name + "', Activated by: " +
                         ((Activators & VolumeActivators.Player) != 0 ? "Lara, " : "") +
                         ((Activators & VolumeActivators.NPCs) != 0 ? "NPCs, " : "") +
                         ((Activators & VolumeActivators.OtherMoveables) != 0 ? "Other objects, " : "") +
                         ((Activators & VolumeActivators.Statics) != 0 ? "Statics, " : "") +
                         ((Activators & VolumeActivators.Flybys) != 0 ? "Flybys, " : "");
                result = result.Substring(0, result.Length - 2) + " \n";
            }

            result += (string.IsNullOrEmpty(OnEnter?.Function  ?? string.Empty) ? string.Empty : "OnEnter: "  + OnEnter.Function  + "\n") +
                      (string.IsNullOrEmpty(OnInside?.Function ?? string.Empty) ? string.Empty : "OnInside: " + OnInside.Function + "\n") +
                      (string.IsNullOrEmpty(OnLeave?.Function  ?? string.Empty) ? string.Empty : "OnLeave: "  + OnLeave.Function);

            return result;
        }

        public void Write(BinaryWriterEx writer, List<VolumeEventSet> eventSets)
        {
            writer.Write(Name);
            writer.Write((int)Activators);

            OnEnter.Write(writer, eventSets);
            OnInside.Write(writer, eventSets);
            OnLeave.Write(writer, eventSets);
        }

        public bool Equals(VolumeEventSet other)
        {
            return
                Name == other.Name &&
                Activators == other.Activators &&
                OnEnter.Equals(other.OnEnter) &&
                OnInside.Equals(other.OnInside) &&
                OnLeave.Equals(other.OnLeave);
        }
    }

    public class SphereVolumeInstance : VolumeInstance, IScaleable
    {
        private const float _defaultScale = 8.0f;
        private const float _minSize = 128.0f;

        public float Size => Scale * _minSize;

        public float DefaultScale => Level.BlockSizeUnit;
        public float Scale
        {
            get { return _scale; }
            set
            {
                if (value >= 1.0f && value <= _minSize)
                    _scale = value;
            }
        }
        private float _scale = _defaultScale;

        public override string ToString()
        {
            return "Sphere Volume" + GetScriptIDOrName(true) +
                   ", Room = " + (Room?.ToString() ?? "NULL") + "\n" +
                   EventSet?.GetDescription() ?? string.Empty;
        }

        public override string ShortName() => "Sphere volume" + ", Room = " + (Room?.ToString() ?? "NULL") + GetScriptIDOrName();
    }

    public class BoxVolumeInstance : VolumeInstance, ISizeable, IRotateableYX
    {
        protected const float _maxSize = ushort.MaxValue;
        protected const float _minSize = 32.0f;

        public Vector3 DefaultSize => new Vector3(Level.BlockSizeUnit);

        public Vector3 Size
        {
            get { return _size; }
            set
            {
                _size = new Vector3(MathC.Clamp(value.X, _minSize, _maxSize),
                                    MathC.Clamp(value.Y, _minSize, _maxSize),
                                    MathC.Clamp(value.Z, _minSize, _maxSize));
            }
        }
        private Vector3 _size = new Vector3(Level.BlockSizeUnit);

        public float RotationY
        {
            get { return _rotationY; }
            set { _rotationY = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }
        private float _rotationY = 0.0f;

        public float RotationX
        {
            get { return _rotationX; }
            set { _rotationX = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }
        private float _rotationX = 0.0f;

        public override string ToString()
        {
            return "Box Volume" + GetScriptIDOrName(true) +
                   ", Room = " + (Room?.ToString() ?? "NULL") + "\n" +
                   EventSet?.GetDescription() ?? string.Empty;
        }

        public override string ShortName() => "Box volume" + ", Room = " + (Room?.ToString() ?? "NULL") + GetScriptIDOrName();
    }

    public abstract class VolumeInstance : PositionAndScriptBasedObjectInstance, ISpatial
    {
        public VolumeShape Shape()
        {
            if (this is BoxVolumeInstance) return VolumeShape.Box;
            if (this is SphereVolumeInstance) return VolumeShape.Sphere;
            return VolumeShape.Undefined;
        }

        public VolumeEventSet EventSet { get; set; }

        public override void CopyDependentLevelSettings(Room.CopyDependentLevelSettingsArgs args)
        {
            base.CopyDependentLevelSettings(args);

            if (EventSet == null)
                return;

            if (args.DestinationLevelSettings.EventSets.Contains(EventSet))
                return;

            args.DestinationLevelSettings.EventSets.Add(EventSet);
        }

        public abstract string ShortName();
    }
}