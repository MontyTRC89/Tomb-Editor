using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TombLib
{
    public struct BoundingSphere : IEquatable<BoundingSphere>
    {
        public Vector3 Center;
        public float Radius;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoundingSphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(BoundingSphere first, BoundingSphere second) => (first.Center == second.Center) && (first.Radius == second.Radius);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(BoundingSphere first, BoundingSphere second) => (first.Center != second.Center) || (first.Radius != second.Radius);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => "Sphere at " + Center + " with radius " + Radius;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => unchecked((Center.GetHashCode() * 397) ^ Radius.GetHashCode());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(BoundingSphere other)
        {
            return Center.Equals(other.Center) && Radius.Equals(other.Radius);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other)
        {
            if (!(other is BoundingSphere))
                return false;
            return Equals((BoundingSphere)other);
        }
    }
}
