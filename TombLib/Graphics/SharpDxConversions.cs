using System.Numerics;

namespace TombLib.Graphics
{
    public static class SharpDxConversions
    {
        public static SharpDX.Vector2 ToSharpDX(this Vector2 vector) => new SharpDX.Vector2(vector.X, vector.Y);
        public static Vector2 ToStd(this SharpDX.Vector2 vector) => new Vector2(vector.X, vector.Y);

        public static SharpDX.Vector3 ToSharpDX(this Vector3 vector) => new SharpDX.Vector3(vector.X, vector.Y, vector.Z);
        public static Vector3 ToStd(this SharpDX.Vector3 vector) => new Vector3(vector.X, vector.Y, vector.Z);

        public static SharpDX.Vector4 ToSharpDX(this Vector4 vector) => new SharpDX.Vector4(vector.X, vector.Y, vector.Z, vector.W);
        public static Vector4 ToStd(this SharpDX.Vector4 vector) => new Vector4(vector.X, vector.Y, vector.Z, vector.W);

        public static SharpDX.Matrix ToSharpDX(this Matrix4x4 matrix) => new SharpDX.Matrix(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        public static Matrix4x4 ToStd(this SharpDX.Matrix matrix) => new Matrix4x4(
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44);

        public static Vector3 Project(this SharpDX.ViewportF viewport, Vector3 vector, Matrix4x4 worldViewProjection)
        {
            var result = Vector3.Transform(vector, worldViewProjection);
            float a = (((vector.X * worldViewProjection.M14) + (vector.Y * worldViewProjection.M24)) + (vector.Z * worldViewProjection.M34)) + worldViewProjection.M44;

            if (!MathC.IsOne(a))
            {
                result = (result / a);
            }

            result.X = (((result.X + 1f) * 0.5f) * viewport.Width) + viewport.X;
            result.Y = (((-result.Y + 1f) * 0.5f) * viewport.Height) + viewport.Y;
            result.Z = (result.Z * (viewport.MaxDepth - viewport.MinDepth)) + viewport.MinDepth;
            return result;
        }

        public static Vector3 Unproject(this SharpDX.ViewportF viewport, Vector3 source, Matrix4x4 worldViewProjection)
        {
            Matrix4x4 inverse;
            if (!Matrix4x4.Invert(worldViewProjection, out inverse))
                return new Vector3();

            Vector3 vector = new Vector3(
                (((source.X - viewport.X) / (viewport.Width)) * 2f) - 1f,
                -((((source.Y - viewport.Y) / (viewport.Height)) * 2f) - 1f),
                (source.Z - viewport.MinDepth) / (viewport.MaxDepth - viewport.MinDepth));

            float a = (((vector.X * inverse.M14) + (vector.Y * inverse.M24)) + (vector.Z * inverse.M34)) + inverse.M44;
            vector = Vector3.Transform(vector, inverse);

            if (!MathC.IsOne(a))
            {
                vector = (vector / a);
            }
            return vector;
        }

        public static Ray GetPickRay(this SharpDX.ViewportF viewport, Vector2 screenPos, Matrix4x4 worldViewProjection)
        {
            Vector3 nearPoint = viewport.Unproject(new Vector3(screenPos.X, screenPos.Y, 0), worldViewProjection);
            Vector3 farPoint = viewport.Unproject(new Vector3(screenPos.X, screenPos.Y, 1), worldViewProjection);
            return new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));
        }
    }
}
