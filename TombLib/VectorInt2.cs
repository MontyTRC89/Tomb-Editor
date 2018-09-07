using System;
using System.Drawing;
using System.Numerics;

namespace TombLib
{
    [Serializable]
    public struct VectorInt2 : IEquatable<VectorInt2>
    {
        public int X;
        public int Y;

        public VectorInt2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static VectorInt2 Max(VectorInt2 first, VectorInt2 second) => new VectorInt2(Math.Max(first.X, second.X), Math.Max(first.Y, second.Y));
        public static VectorInt2 Min(VectorInt2 first, VectorInt2 second) => new VectorInt2(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y));
        public static VectorInt2 operator +(VectorInt2 first, VectorInt2 second) => new VectorInt2(first.X + second.X, first.Y + second.Y);
        public static VectorInt2 operator -(VectorInt2 first, VectorInt2 second) => new VectorInt2(first.X - second.X, first.Y - second.Y);
        public static VectorInt2 operator -(VectorInt2 value) => new VectorInt2(-value.X, -value.Y);
        public static VectorInt2 operator *(VectorInt2 value, int scale) => new VectorInt2(value.X * scale, value.Y * scale);
        public static VectorInt2 operator /(VectorInt2 value, int scale) => new VectorInt2(value.X / scale, value.Y / scale);
        public static VectorInt2 operator *(VectorInt2 first, VectorInt2 second) => new VectorInt2(first.X * second.X, first.Y * second.Y);
        public static VectorInt2 operator /(VectorInt2 first, VectorInt2 second) => new VectorInt2(first.X / second.X, first.Y / second.Y);
        public static bool operator ==(VectorInt2 first, VectorInt2 second) => first.X == second.X && first.Y == second.Y;
        public static bool operator !=(VectorInt2 first, VectorInt2 second) => first.X != second.X || first.Y != second.Y;
        public static implicit operator Vector2(VectorInt2 value) => new Vector2(value.X, value.Y);
        public static explicit operator VectorInt2(Vector2 value) => FromRounded(value);
        public static VectorInt2 Zero => new VectorInt2(0, 0);
        public static VectorInt2 One => new VectorInt2(1, 1);

        public static VectorInt2 FromRounded(Vector2 value) => new VectorInt2(
            (int)Math.Min(Math.Max(Math.Round(value.X), int.MinValue), int.MaxValue),
            (int)Math.Min(Math.Max(Math.Round(value.Y), int.MinValue), int.MaxValue));
        public static VectorInt2 FromFloor(Vector2 value) => new VectorInt2(
            (int)Math.Min(Math.Max(Math.Floor(value.X), int.MinValue), int.MaxValue),
            (int)Math.Min(Math.Max(Math.Floor(value.Y), int.MinValue), int.MaxValue));
        public static VectorInt2 FromCeiling(Vector2 value) => new VectorInt2(
            (int)Math.Min(Math.Max(Math.Ceiling(value.X), int.MinValue), int.MaxValue),
            (int)Math.Min(Math.Max(Math.Ceiling(value.Y), int.MinValue), int.MaxValue));
        public static VectorInt2 FromPoint(Point value) => new VectorInt2(value.X, value.Y);

        public override string ToString() => "<" + X + ", " + Y + ">";
        public override bool Equals(object obj)
        {
            if (!(obj is VectorInt2))
                return false;
            return this == (VectorInt2)obj;
        }
        public bool Equals(VectorInt2 other) => this == other;
        public override int GetHashCode() => unchecked(X + 588671089 * Y); // Random prime
    }
}
