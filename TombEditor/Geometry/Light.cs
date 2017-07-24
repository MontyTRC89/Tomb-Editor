using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
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
            Light l = new Geometry.Light();

            l.Color = this.Color;
            l.Intensity = this.Intensity;
            l.In = this.In;
            l.Out = this.Out;
            l.Active = this.Active;
            l.Type = this.Type;
            l.DirectionX = this.DirectionX;
            l.DirectionY = this.DirectionY;
            l.Len = this.Len;
            l.Cutoff = this.Cutoff;
            l.Position = new Vector3(Position.X, Position.Y, Position.Z);
            l.Face = Face;

            return l;
        }
    }

    public struct RayPathPoint
    {
        public int X;
        public int Z;
    }
}
