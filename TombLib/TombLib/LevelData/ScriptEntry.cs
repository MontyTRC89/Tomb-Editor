using System;
using System.Numerics;
using TombLib.IO;

namespace TombLib.LevelData
{
    // All possible shapes are reduced to these three basics.
    // If we need a plane volume, it will be a box with one axis scale set to minimum.
    // If we need legacy "trigger" volume, it will be a box with one axis scale set to maximum.
    public enum VolumeShape : byte
    {
        Box, Sphere, Undefined
    }

    public enum VolumeEventMode
    {
        LevelScript, Constructor
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

    public class VolumeEvent
    {
        public VolumeEventMode Mode = VolumeEventMode.LevelScript;
        public string Function { get; set; } = string.Empty;
        public string Argument { get; set; } = string.Empty;

        // public VolumeEventConstructor Constructor { get; set; } // TODO

        public int CallCounter { get; set; } = 0;

        public void Write(BinaryWriterEx writer)
        {
            writer.Write((int)Mode);
            writer.Write(Function);
            writer.Write(Argument);
            writer.Write(CallCounter);
        }
    }

    public class VolumeEventSet : ICloneable
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
            var script = (VolumeEventSet)MemberwiseClone();
            script.Name = Name + " (copy)";
            return script;
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
                         ((Activators & VolumeActivators.Flybys) != 0 ? "Flybys cameras, " : "");
                result = result.Substring(0, result.Length - 2) + "\n";
            }

            result += (string.IsNullOrEmpty(OnEnter?.Function  ?? string.Empty) ? string.Empty : "OnEnter: "  + OnEnter.Function  + "\n") +
                      (string.IsNullOrEmpty(OnInside?.Function ?? string.Empty) ? string.Empty : "OnInside: " + OnInside.Function + "\n") +
                      (string.IsNullOrEmpty(OnLeave?.Function  ?? string.Empty) ? string.Empty : "OnLeave: "  + OnLeave.Function);

            return result;
        }

        public void Write(BinaryWriterEx writer)
        {
            writer.Write(Name);
            writer.Write((int)Activators);

            OnEnter.Write(writer);
            OnInside.Write(writer);
            OnLeave.Write(writer);
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
            return "Sphere Volume (d = " + Math.Round(Size) + ")" +
                   " in room '" + (Room?.ToString() ?? "NULL") + "' " +
                   "at [" + SectorPosition.X + ", " + SectorPosition.Y + "] \n" + 
                   EventSet?.GetDescription() ?? string.Empty;
        }
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
            string message = "Box Volume (" + Size.X + ", " + Size.Y + ", " + Size.Z + ")" +
                   " in room '" + (Room?.ToString() ?? "NULL") + "' " +
                   "at [" + SectorPosition.X + ", " + SectorPosition.Y + "] \n" + 
                   EventSet?.GetDescription() ?? string.Empty;

            return message;
        }
    }

    public abstract class VolumeInstance : PositionBasedObjectInstance, ISpatial
    {
        public VolumeShape Shape()
        {
            if (this is BoxVolumeInstance) return VolumeShape.Box;
            if (this is SphereVolumeInstance) return VolumeShape.Sphere;
            return VolumeShape.Undefined;
        }

        public VolumeEventSet EventSet { get; set; }
    }
}
