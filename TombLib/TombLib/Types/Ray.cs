﻿using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using TombLib.Graphics;

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

        public static Ray GetPickRay(Vector2 screenPos, Matrix4x4 worldViewProjection, float width, float height)
        {
            if (!Matrix4x4.Invert(worldViewProjection, out Matrix4x4 inverse))
                return new Ray();

            Vector3 pos2D = new Vector3(screenPos.X / width * 2.0f - 1.0f, -(screenPos.Y / height * 2.0f - 1.0f), 0.0f);

            Vector3 nearPoint = inverse.TransformPerspectively(pos2D);
            Vector3 farPoint  = inverse.TransformPerspectively(pos2D + Vector3.UnitZ);

            return new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));
        }

        public static Ray GetPickRay(ArcBallCamera camera, Size size, float x, float y) =>
            GetPickRay(new Vector2(x, y), camera.GetViewProjectionMatrix(size.Width, size.Height), size.Width, size.Height);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Ray first, Ray second) => first.Position == second.Position && first.Direction == second.Direction;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Ray first, Ray second) => first.Position != second.Position || first.Direction != second.Direction;
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
