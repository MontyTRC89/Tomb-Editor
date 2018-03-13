using System;
using System.Numerics;

namespace TombLib
{
    [Serializable]
    public struct VectorInt3 : IEquatable<VectorInt3>
    {
        public int X;
        public int Y;
        public int Z;

        public VectorInt3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static VectorInt3 Max(VectorInt3 first, VectorInt3 second) => new VectorInt3(Math.Max(first.X, second.X), Math.Max(first.Y, second.Y), Math.Max(first.Z, second.Z));
        public static VectorInt3 Min(VectorInt3 first, VectorInt3 second) => new VectorInt3(Math.Min(first.X, second.X), Math.Min(first.Y, second.Y), Math.Min(first.Z, second.Z));
        public static VectorInt3 operator +(VectorInt3 first, VectorInt3 second) => new VectorInt3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        public static VectorInt3 operator -(VectorInt3 first, VectorInt3 second) => new VectorInt3(first.X - second.X, first.Y - second.Y, first.Z - second.Z);
        public static VectorInt3 operator -(VectorInt3 value) => new VectorInt3(-value.X, -value.Y, -value.Z);
        public static VectorInt3 operator *(VectorInt3 value, int scale) => new VectorInt3(value.X * scale, value.Y * scale, value.Z * scale);
        public static VectorInt3 operator /(VectorInt3 value, int scale) => new VectorInt3(value.X / scale, value.Y / scale, value.Z / scale);
        public static bool operator ==(VectorInt3 first, VectorInt3 second) => first.X == second.X && first.Y == second.Y && first.Z == second.Z;
        public static bool operator !=(VectorInt3 first, VectorInt3 second) => first.X != second.X || first.Y != second.Y || first.Z != second.Z;
        public static implicit operator Vector3(VectorInt3 value) => new Vector3(value.X, value.Y, value.Z);
        public static explicit operator VectorInt3(Vector3 value) => FromRounded(value);

        public static VectorInt3 FromRounded(Vector3 value) => new VectorInt3(
            (int)Math.Min(Math.Max(Math.Round(value.X), int.MinValue), int.MaxValue),
            (int)Math.Min(Math.Max(Math.Round(value.Y), int.MinValue), int.MaxValue),
            (int)Math.Min(Math.Max(Math.Round(value.Z), int.MinValue), int.MaxValue));
        public static VectorInt3 FromFloor(Vector3 value) => new VectorInt3(
            (int)Math.Min(Math.Max(Math.Floor(value.X), int.MinValue), int.MaxValue),
            (int)Math.Min(Math.Max(Math.Floor(value.Y), int.MinValue), int.MaxValue),
            (int)Math.Min(Math.Max(Math.Floor(value.Z), int.MinValue), int.MaxValue));
        public static VectorInt3 FromCeiling(Vector3 value) => new VectorInt3(
            (int)Math.Min(Math.Max(Math.Ceiling(value.X), int.MinValue), int.MaxValue),
            (int)Math.Min(Math.Max(Math.Ceiling(value.Y), int.MinValue), int.MaxValue),
            (int)Math.Min(Math.Max(Math.Ceiling(value.Z), int.MinValue), int.MaxValue));

        public override string ToString() => "<" + X + ", " + Y + ", " + Z + ">";
        public override bool Equals(object obj)
        {
            if (!(obj is VectorInt3))
                return false;
            return this == (VectorInt3)obj;
        }
        public bool Equals(VectorInt3 other) => this == other;
        public override int GetHashCode() => unchecked(X + 249669787 * Y + 195115517 * Z); // Random prime
    }
}
