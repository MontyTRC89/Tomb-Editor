using System;
using System.Numerics;

namespace TombLib.LevelData
{
    // All possible shapes are reduced to these three basics.
    // If we need a plane volume, it will be a box or prism with one axis scale set to minimum.
    // If we need legacy "trigger" volume, it will be a box with one axis scale set to maximum.
    public enum VolumeShape : byte
    {
        Box, Prism, Sphere
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

    public class TriggerVolumeInstance : PositionBasedObjectInstance, ISpatial, ISizeable, IRotateableYX
    {
        private const float _defaultSize = 1024.0f;
        private const float _minBoxVolumeSize = 32.0f;
        private const float _minPrismVolumeSize = 128.0f;
        private const float _minSphereVolumeSize = 128.0f;
        private const float _planeSize = 32.0f;

        public VolumeShape Shape
        {
            get { return _shape; }
            set
            {
                _shape = value;
                Size = _size; // Re-clamp size on shape change
            }

        }
        private VolumeShape _shape = VolumeShape.Box;

        public Vector3 Size
        {
            get { return _size; }
            set
            {
                // Clamp scale differently according to shape
                switch (Shape)
                {
                    case VolumeShape.Box:
                        if (value.Length() >= _minBoxVolumeSize) _size = value; break;
                    case VolumeShape.Prism:
                        if (value.Length() >= _minPrismVolumeSize) _size = value; break;
                    case VolumeShape.Sphere:
                        {
                            float changedValue = 0.0f;
                            if (value.X != _size.X) changedValue = value.X;
                            if (value.Y != _size.X) changedValue = value.Y;
                            if (value.Z != _size.X) changedValue = value.Z;
                            if (changedValue >= _minSphereVolumeSize) _size.X = _size.Y = _size.Z = changedValue; break;
                        }
                    default:
                        _size = value; break;
                }

                // Additionally clamp each axis to possible minimum
                if (_size.X < _planeSize) _size.X = _planeSize;
                if (_size.Y < _planeSize) _size.Y = _planeSize;
                if (_size.Z < _planeSize) _size.Z = _planeSize;
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

        public VolumeActivators Activators { get; set; } = VolumeActivators.Lara;

        public VolumeScriptInstance Scripts { get; set; } = new VolumeScriptInstance() { Name = "New volume script" };

        public TriggerVolumeInstance() { }
        public TriggerVolumeInstance(VolumeShape shape) { Shape = shape; }

        public override string ToString()
        {
            string desc;
            switch (Shape)
            {
                case VolumeShape.Box:
                    desc = "Box Volume '" + Scripts.Name + "' (" + Size.X + ", " + Size.Y + ", " + Size.Z + ")";
                    break;
                case VolumeShape.Sphere:
                    desc = "Sphere Volume '" + Scripts.Name + "' (d = " + Size.X + ")";
                    break;
                case VolumeShape.Prism:
                    desc = "Prism Volume '" + Scripts.Name + "' (h = " + Size.X + ")";
                    break;
                default:
                    desc = "Unknown Volume '" + Scripts.Name + "'";
                    break;
            }


            string text = desc + " in room '" + (Room?.ToString() ?? "NULL") + "' " +
                          "at [" + SectorPosition.X + ", " + SectorPosition.Y + "] ";
            return text;
        }
    }
}
