using SharpDX;

namespace TombEditor.Geometry
{
    public class Light
    {
        public System.Drawing.Color Color { get; set; }
        public Vector3 Position { get; set; }
        public float Intensity { get; set; }
        public float In { get; set; }
        public float Out { get; set; }
        public bool Active { get; set; }
        public LightType Type { get; set; }
        public float DirectionX { get; set; }
        public float DirectionY { get; set; }
        public float Len { get; set; }
        public float Cutoff { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public BlockFaces Face { get; set; }

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
