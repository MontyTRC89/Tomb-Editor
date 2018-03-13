using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace TombLib
{
    public struct Ray : IEquatable<Ray>
    {
        public Vector3 Position;
        public Vector3 Direction;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Ray(Vector3 position, Vector3 direction)
        {
            Position = position;
            Direction = direction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Ray first, Ray second) => (first.Position == second.Position) && (first.Direction == second.Direction);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Ray first, Ray second) => (first.Position != second.Position) || (first.Direction != second.Direction);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => "Ray at " + Position + " in direction " + Direction;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => unchecked((Position.GetHashCode() * 397) ^ Direction.GetHashCode());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Ray other) => Position.Equals(other.Position) && Direction.Equals(other.Direction);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other)
        {
            if (!(other is Ray))
                return false;
            return Equals((Ray)other);
        }
    }
}
