using SharpDX;
using System;

namespace TombEditor.Geometry
{
    public enum LightType : byte
    {
        Light, Shadow, Spot, Effect, Sun, FogBulb
    }

    public enum LightParameter
    {
        Intensity, InnerRange, OuterRange, InnerAngle, OuterAngle, RotationX, RotationY
    }

    public class Light : PositionBasedObjectInstance, IRotateableYX
    {
        public LightType Type { get; }
        public Vector3 Color { get; set; } = new Vector3(1.0f, 1.0f, 1.0f); // Normalized float. (1.0 meaning normal brightness, 2.0 is the maximal brightness supported by tomb4.exe)
        public float Intensity { get; set; } = 0.5f;
        public float InnerRange { get; set; } = 1.0f;
        public float OuterRange { get; set; } = 5.0f;
        public float InnerAngle { get; set; } = 20.0f;
        public float OuterAngle { get; set; } = 25.0f;
        public bool Enabled { get; set; } = true;
        public bool CastsShadows { get; set; } = true;
        public bool IsDynamicallyUsed { get; set; } = true;
        public bool IsStaticallyUsed { get; set; } = true;

        private float _rotationX = 0.0f;
        private float _rotationY = 0.0f;

        /// <summary> Degrees in the range [-90, 90] </summary>
        public float RotationX
        {
            get { return _rotationX; }
            set { _rotationX = Math.Max(-90, Math.Min(90, value)); }
        }

        /// <summary> Degrees in the range [0, 360) </summary>
        public float RotationY
        {
            get { return _rotationY; }
            set { _rotationY = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }

        public Light(LightType type)
        {
            Type = type;
            switch (type)
            {
                case LightType.Shadow:
                    Intensity *= -1;
                    CastsShadows = false;
                    break;
                case LightType.Effect:
                    InnerRange = 0.99f;
                    OuterRange = 1.0f;
                    IsDynamicallyUsed = false;
                    break;
                case LightType.FogBulb:
                    CastsShadows = false;
                    IsStaticallyUsed = false;
                    break;
            }
        }

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)(this.MemberwiseClone());
        }

        public override string ToString()
        {
            return "Light " + Type.ToString() +
                ", X = " + Position.X +
                ", Y = " + Position.Y +
                ", Z = " + Position.Z;
        }

        public override void AddToRoom(Level level, Room room)
        {
            base.AddToRoom(level, room);
        }

        public override void RemoveFromRoom(Level level, Room room)
        {
            base.RemoveFromRoom(level, room);
        }
    }
}
