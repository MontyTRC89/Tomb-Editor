using System;
using System.Numerics;
using System.Transactions.Configuration;

namespace TombLib.LevelData
{
    // All possible shapes are reduced to these three basics.
    // If we need a plane volume, it will be a box or prism with one axis scale set to minimum.
    // If we need legacy "trigger" volume, it will be a box with one axis scale set to maximum.
    public enum VolumeShape : byte
    {
        Box, Prism, Sphere, Undefined
    }

    // Possible activator flags. If none is set, volume is disabled.
    [Flags]
    public enum VolumeActivators : ushort
    {
        Player = 1,
        NPCs = 2,
        OtherMoveables = 4,
        Statics = 8,
        Flybys = 16,
        PhysicalObjects = 32 // Future-proofness for Bullet
    }

    // Every volume's events can be reduced to these three.
    // If resulting volume should be one-shot trigger, we'll only use "OnEnter" event.

    public class VolumeScriptInstance : ScriptInstance, ICloneable
    {
        public string OnEnter  { get; set; } = string.Empty;
        public string OnLeave  { get; set; } = string.Empty;
        public string OnInside { get; set; } = string.Empty;

        object ICloneable.Clone()
        {
            return Clone();
        }

        public VolumeScriptInstance Clone()
        {
            var script = (VolumeScriptInstance)MemberwiseClone();
            script.Name = Name;
            return script;
        }
    }

    public class PrismVolumeInstance : VolumeInstance, IScaleable, IRotateableY
    {
        private const float _defaultScale = 32.0f;
        private const float _maxScale = 256.0f;
        private const float _scaleStep = 32.0f;


        public float Size => Scale * _scaleStep;

        public float DefaultScale => 32.0f;
        public float Scale
        {
            get { return _scale; }
            set
            {
                if (value >= 1.0f && value <= _maxScale)
                    _scale = value;
            }
        }
        private float _scale = _defaultScale;

        public float RotationY
        {
            get { return _rotationY; }
            set 
            {
                var roundedValue = Math.Round(value / 90.0f) * 90.0f;
                _rotationY = (float)(roundedValue - Math.Floor(roundedValue / 360.0) * 360.0); 
            }
        }
        private float _rotationY = 0.0f;

        public override string ToString()
        {
            return "Prism Volume '" + Scripts.Name + "' (h = " + Math.Round(Size) + ")" +
                   " in room '" + (Room?.ToString() ?? "NULL") + "' " +
                   "at [" + SectorPosition.X + ", " + SectorPosition.Y + "] ";
        }
    }

    public class SphereVolumeInstance : VolumeInstance, IScaleable
    {
        private const float _defaultScale = 8.0f;
        private const float _minSize = 128.0f;

        public float Size => Scale * _minSize;

        public float DefaultScale => Level.WorldUnit;
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
            return "Sphere Volume '" + Scripts.Name + "' (d = " + Math.Round(Size) + ")" + 
                   " in room '" + (Room?.ToString() ?? "NULL") + "' " +
                   "at [" + SectorPosition.X + ", " + SectorPosition.Y + "] ";
        }
    }

    public class BoxVolumeInstance : VolumeInstance, ISizeable, IRotateableYX
    {
        protected const float _maxSize = ushort.MaxValue;
        protected const float _minSize = 32.0f;

        public Vector3 DefaultSize => new Vector3(Level.WorldUnit);

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
        private Vector3 _size = new Vector3(Level.WorldUnit);

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
            string message = "Box Volume '" + Scripts.Name + "' (" + Size.X + ", " + Size.Y + ", " + Size.Z + ")" +
                   " in room '" + (Room?.ToString() ?? "NULL") + "' " +
                   "at [" + SectorPosition.X + ", " + SectorPosition.Y + "] \n";
            message += "Activated by: " +
                     ((Activators & VolumeActivators.Player) != 0 ? "Lara, " : "") +
                     ((Activators & VolumeActivators.NPCs) != 0 ? "NPCs, " : "") +
                     ((Activators & VolumeActivators.OtherMoveables) != 0 ? "Other moveables, " : "") +
                     ((Activators & VolumeActivators.Statics) != 0 ? "Statics, " : "") +
                     ((Activators & VolumeActivators.Flybys) != 0 ? "Flybys cameras, " : "");
            message = message.Substring(0, message.Length - 2) + "\n";
            message += (Scripts.OnEnter != "" ? "OnEnter: " + Scripts.OnEnter + "\n" : "") +
                (Scripts.OnInside != "" ? "OnInside: " + Scripts.OnInside + "\n" : "") +
               (Scripts.OnLeave != "" ? "OnLeave: " + Scripts.OnLeave : "");

            return message;
        }
    }

    public abstract class VolumeInstance : PositionBasedObjectInstance, ISpatial
    {
        public VolumeShape Shape()
        {
            if (this is BoxVolumeInstance) return VolumeShape.Box;
            if (this is SphereVolumeInstance) return VolumeShape.Sphere;
            if (this is PrismVolumeInstance) return VolumeShape.Prism;
            return VolumeShape.Undefined;
        }

        public bool OneShot { get; set; } = false;
        public VolumeActivators Activators { get; set; } = VolumeActivators.Player;
        public VolumeScriptInstance Scripts { get; set; } = new VolumeScriptInstance() { Name = "New volume script" };
    }
}
