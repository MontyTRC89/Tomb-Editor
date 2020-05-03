using System;
using System.Numerics;

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
        Lara = 1,
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
        private const float _defaultScale = 8.0f;
        private const float _minPrismVolumeSize = 128.0f;

        public float Size => Scale * _minPrismVolumeSize;

        public float Scale
        {
            get { return _scale; }
            set
            {
                if (value >= 1.0f && value <= _minPrismVolumeSize)
                    _scale = value;
            }
        }
        private float _scale = _defaultScale;

        public float RotationY
        {
            get { return _rotationY; }
            set { _rotationY = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }
        private float _rotationY = 0.0f;

        public override string ToString()
        {
            return "Prism Volume '" + Scripts.Name + "' (h = " + Size + ")" +
                   " in room '" + (Room?.ToString() ?? "NULL") + "' " +
                   "at [" + SectorPosition.X + ", " + SectorPosition.Y + "] ";
        }
    }

    public class SphereVolumeInstance : VolumeInstance, IScaleable
    {
        private const float _defaultScale = 8.0f;
        private const float _minSphereVolumeSize = 128.0f;

        public float Size => Scale * _minSphereVolumeSize;

        public float Scale
        {
            get { return _scale; }
            set
            {
                if (value >= 1.0f && value <= _minSphereVolumeSize)
                    _scale = value;
            }
        }
        private float _scale = _defaultScale;

        public override string ToString()
        {
            return "Sphere Volume '" + Scripts.Name + "' (d = " + Size + ")" + 
                   " in room '" + (Room?.ToString() ?? "NULL") + "' " +
                   "at [" + SectorPosition.X + ", " + SectorPosition.Y + "] ";
        }
    }

    public class BoxVolumeInstance : VolumeInstance, ISizeable, IRotateableYX
    {
        private const float _minBoxVolumeSize = 32.0f;
        private const float _planeSize = 32.0f;

        public Vector3 Size
        {
            get { return _size; }
            set
            {
                if (value.Length() >= _minBoxVolumeSize)
                    _size = value;
            }
        }
        private Vector3 _size = new Vector3(_defaultSize);

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
            return "Box Volume '" + Scripts.Name + "' (" + Size.X + ", " + Size.Y + ", " + Size.Z + ")" +
                   " in room '" + (Room?.ToString() ?? "NULL") + "' " +
                   "at [" + SectorPosition.X + ", " + SectorPosition.Y + "] ";
        }
    }

    public abstract class VolumeInstance : PositionBasedObjectInstance, ISpatial
    {
        protected const float _defaultSize = 1024.0f;

        public VolumeShape Shape()
        {
            if (this is BoxVolumeInstance) return VolumeShape.Box;
            if (this is SphereVolumeInstance) return VolumeShape.Sphere;
            if (this is PrismVolumeInstance) return VolumeShape.Prism;
            return VolumeShape.Undefined;
        }

        public VolumeActivators Activators { get; set; } = VolumeActivators.Lara;
        public VolumeScriptInstance Scripts { get; set; } = new VolumeScriptInstance() { Name = "New volume script" };
    }
}
