using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib
{
    [Serializable]
    public struct RectangleInt2 : IEquatable<RectangleInt2>
    {
        public static readonly RectangleInt2 MaxMin = new RectangleInt2(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

        public VectorInt2 Start;
        public VectorInt2 End;

        public RectangleInt2(VectorInt2 start, VectorInt2 end)
        {
            Start = start;
            End = end;
        }

        public RectangleInt2(int x0, int y0, int x1, int y1)
        {
            Start = new VectorInt2(x0, y0);
            End = new VectorInt2(x1, y1);
        }

        public int X0
        {
            get { return Start.X; }
            set { Start.X = value; }
        }
        public int Y0
        {
            get { return Start.Y; }
            set { Start.Y = value; }
        }
        public int X1
        {
            get { return End.X; }
            set { End.X = value; }
        }
        public int Y1
        {
            get { return End.Y; }
            set { End.Y = value; }
        }

        public VectorInt2 Size => End - Start;
        public int Width => End.X - Start.X;
        public int Height => End.Y - Start.Y;

        public RectangleInt2 Intersect(RectangleInt2 other) =>
            new RectangleInt2(VectorInt2.Max(Start, other.Start), VectorInt2.Min(End, other.End));

        public RectangleInt2 Union(RectangleInt2 other) =>
            new RectangleInt2(VectorInt2.Min(Start, other.Start), VectorInt2.Max(End, other.End));

        public VectorInt2 GetMid() => (Start + End) / 2;

        public bool Contains(RectangleInt2 other) =>
            ((Start.X <= other.Start.X) && (End.X >= other.End.X)) &&
            ((Start.Y <= other.Start.Y) && (End.Y >= other.End.Y));

        public bool Contains(VectorInt2 point) =>
            ((Start.X <= point.X) && (End.X >= point.X)) &&
            ((Start.Y <= point.Y) && (End.Y >= point.Y));

        public bool Intersects(RectangleInt2 other) =>
            (Start.X <= other.End.X) && (End.X >= other.Start.X) &&
            (Start.Y <= other.End.Y) && (End.Y >= other.Start.Y);

        public RectangleInt2 Inflate(int width) =>
            new RectangleInt2(Start - new VectorInt2(width, width), End + new VectorInt2(width, width));

        public RectangleInt2 Inflate(VectorInt2 width) =>
            new RectangleInt2(Start - width, End + width);

        public RectangleInt2 Inflate(int width, int height) => Inflate(new VectorInt2(width, height));

        public static RectangleInt2 operator +(RectangleInt2 area, VectorInt2 offset) => new RectangleInt2(area.Start + offset, area.End + offset);
        public static RectangleInt2 operator -(RectangleInt2 area, VectorInt2 offset) => new RectangleInt2(area.Start - offset, area.End - offset);
        public static RectangleInt2 operator +(VectorInt2 offset, RectangleInt2 area) => new RectangleInt2(offset + area.Start, offset + area.End);
        public static RectangleInt2 operator -(VectorInt2 offset, RectangleInt2 area) => new RectangleInt2(offset - area.Start, offset - area.End);
        public static RectangleInt2 operator -(RectangleInt2 area) => new RectangleInt2(-area.Start, -area.End);

        public static bool operator ==(RectangleInt2 first, RectangleInt2 second) => (first.Start == second.Start) && (first.End == second.End);
        public static bool operator !=(RectangleInt2 first, RectangleInt2 second) => (first.Start != second.Start) || (first.End != second.End);
        public static implicit operator Rectangle2(RectangleInt2 value) => new Rectangle2(value.Start, value.End);
        public static explicit operator RectangleInt2(Rectangle2 value) => new RectangleInt2(VectorInt2.FromFloor(value.Start), VectorInt2.FromCeiling(value.Start));
        public static RectangleInt2 FromLTRB(VectorInt2 start, VectorInt2 size) => new RectangleInt2(start, start + size);
        public static RectangleInt2 FromLTRB(int x0, int y0, int width, int height) => new RectangleInt2(x0, y0, x0 + width, y0 + height);

        public bool Equals(RectangleInt2 other) => this == other;
        public override string ToString() => Start.ToString() + " to " + End.ToString();
        public override bool Equals(object obj)
        {
            if (!(obj is RectangleInt2))
                return false;
            return this == (RectangleInt2)obj;
        }
        public override int GetHashCode() => unchecked(Start.GetHashCode() + End.GetHashCode() * -460299429); // Random prime
    }
}
