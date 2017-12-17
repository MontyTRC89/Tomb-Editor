using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace TombLib.Utils
{
    public static class MathExtensions
    {
        public static SharpDX.Rectangle Intersect(this SharpDX.Rectangle area, SharpDX.Rectangle other)
        {
            return new SharpDX.Rectangle(
                Math.Max(area.Left, other.Left),
                Math.Max(area.Top, other.Top),
                Math.Min(area.Right, other.Right),
                Math.Min(area.Bottom, other.Bottom));
        }

        public static SharpDX.Rectangle Union(this SharpDX.Rectangle area, SharpDX.Rectangle other)
        {
            return new SharpDX.Rectangle(
                Math.Min(area.Left, other.Left),
                Math.Min(area.Top, other.Top),
                Math.Max(area.Right, other.Right),
                Math.Max(area.Bottom, other.Bottom));
        }

        public static bool Contains(this SharpDX.Rectangle area, SharpDX.Rectangle other)
        {
            return ((area.X <= other.X) && (area.Right >= other.Right)) &&
                ((area.Y <= other.Y) && (area.Bottom >= other.Bottom));
        }

        public static bool Intersects(this SharpDX.Rectangle area, SharpDX.Rectangle other)
        {
            return (area.X <= other.Right) && (area.Right >= other.X) &&
                (area.Y <= other.Bottom) && (area.Bottom >= other.Y);
        }

        public static DrawingPoint Offset(this DrawingPoint basePoint, DrawingPoint point)
        {
            return new DrawingPoint(basePoint.X + point.X, basePoint.Y + point.Y);
        }

        public static SharpDX.Rectangle Offset(this SharpDX.Rectangle area, DrawingPoint point)
        {
            return new SharpDX.Rectangle(area.Left + point.X, area.Top + point.Y, area.Right + point.X, area.Bottom + point.Y);
        }

        public static DrawingPoint OffsetNeg(this DrawingPoint basePoint, DrawingPoint point)
        {
            return new DrawingPoint(basePoint.X - point.X, basePoint.Y - point.Y);
        }

        public static SharpDX.Rectangle OffsetNeg(this SharpDX.Rectangle area, DrawingPoint point)
        {
            return new SharpDX.Rectangle(area.Left - point.X, area.Top - point.Y, area.Right - point.X, area.Bottom - point.Y);
        }

        public static Vector2 ToVec2(this DrawingPoint basePoint)
        {
            return new Vector2(basePoint.X, basePoint.Y);
        }

        public static SharpDX.Rectangle Inflate(this SharpDX.Rectangle area, int width)
        {
            return new SharpDX.Rectangle(area.Left - width, area.Top - width, area.Right + width, area.Bottom + width);
        }

        public static SharpDX.Rectangle Inflate(this SharpDX.Rectangle area, int x, int y)
        {
            return new SharpDX.Rectangle(area.Left - x, area.Top - y, area.Right + x, area.Bottom + y);
        }

    }
}
