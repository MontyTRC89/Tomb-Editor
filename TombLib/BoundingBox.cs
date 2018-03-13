using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace TombLib
{
    /// <summary>
    /// Represents an axis-aligned bounding box in three dimensional space.
    /// </summary>
    public struct BoundingBox : IEquatable<BoundingBox>
    {
        public Vector3 Minimum;
        public Vector3 Maximum;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoundingBox(Vector3 minimum, Vector3 maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public Vector3 Size => this.Maximum - this.Minimum;
        public Vector3 Center => (this.Maximum + this.Minimum) * 0.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoundingBox Intersec(BoundingBox other)
        {
            return new BoundingBox(Vector3.Max(Minimum, other.Minimum), Vector3.Min(Maximum, other.Maximum));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoundingBox Union(BoundingBox other)
        {
            return new BoundingBox(Vector3.Min(Minimum, other.Minimum), Vector3.Max(Maximum, other.Maximum));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingBox first, BoundingBox second) => (first.Minimum == second.Minimum) && (first.Maximum == second.Maximum);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingBox first, BoundingBox second) => (first.Minimum != second.Minimum) || (first.Maximum != second.Maximum);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => "Box from " + Minimum + " to " + Maximum;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => unchecked((Minimum.GetHashCode() * 397) ^ Maximum.GetHashCode());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BoundingBox other) => Minimum.Equals(other.Minimum) && Maximum.Equals(other.Maximum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other)
        {
            if (!(other is BoundingBox))
                return false;
            return Equals((BoundingBox)other);
        }
    }
}
