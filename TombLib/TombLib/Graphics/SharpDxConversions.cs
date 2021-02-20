using System.Numerics;
using System.Runtime.CompilerServices;

namespace TombLib.Graphics
{
    public static class SharpDxConversions
    {
        public static SharpDX.Matrix ToSharpDX(this Matrix4x4 matrix) => new SharpDX.Matrix(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SharpDX.Vector3 ToSharpDX(this Vector3 vec) => new SharpDX.Vector3(vec.X, vec.Y, vec.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SharpDX.Vector2 ToSharpDX(this Vector2 vec) => new SharpDX.Vector2(vec.X, vec.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToSystem(this SharpDX.Vector3 vec) => new Vector3(vec.X, vec.Y, vec.Z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ToSystem(this SharpDX.Vector2 vec) => new Vector2(vec.X, vec.Y);
    }
}
