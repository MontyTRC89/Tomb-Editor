using SharpDX;

namespace TombEditor.Geometry
{
    public class Light
    {
        public LightType Type { get; set; }
        public Vector3 Position { get; set; }
        public System.Drawing.Color Color { get; set; } = System.Drawing.Color.White;
        public float Intensity { get; set; } = 0.5f;
        public float In { get; set; } = 1.0f;
        public float Out { get; set; } = 5.0f;
        public float DirectionX { get; set; } = 0.0f;
        public float DirectionY { get; set; } = 0.0f;
        public float Len { get; set; } = 2.0f;
        public float Cutoff { get; set; } = 3.0f;
        public bool Enabled { get; set; } = true;
        public bool CastsShadows { get; set; } = true;
        public bool IsDynamicallyUsed { get; set; } = true;
        public bool IsStaticallyUsed { get; set; } = true;

        public Light(LightType type, Vector3 position)
        {
            Type = type;
            Position = position;

            switch (type)
            {
                case LightType.Shadow:
                    Intensity *= -1;
                    CastsShadows = false;
                    break;
                case LightType.Spot:
                    In = 20.0f;
                    Out = 25.0f;
                    break;
                case LightType.Effect:
                    In = 0.99f;
                    Out = 1.0f;
                    IsDynamicallyUsed = false;
                    break;
                case LightType.FogBulb:
                    CastsShadows = false;
                    IsStaticallyUsed = false;
                    break;
            }
        }

        public Light Clone()
        {
            return (Light)(this.MemberwiseClone());
        }

        public override string ToString()
        {
            return "Light " + Type.ToString() +
                ", X = " + Position.X +
                ", Y = " + Position.Y +
                ", Z = " + Position.Z;
        }
    }
}
