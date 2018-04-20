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

        public static Vector3 Project(Vector3 vector, Matrix4x4 worldViewProjection, float x, float y, float width, float height, float minDepth = 0, float maxDepth = 1)
        {
            var result = Vector3.Transform(vector, worldViewProjection);
            float a = vector.X * worldViewProjection.M14 + vector.Y * worldViewProjection.M24 + vector.Z * worldViewProjection.M34 + worldViewProjection.M44;

            if (!MathC.IsOne(a))
            {
                result = result / a;
            }

            result.X = (result.X + 1f) * 0.5f * width + x;
            result.Y = (-result.Y + 1f) * 0.5f * height + x;
            result.Z = result.Z * (maxDepth - minDepth) + minDepth;
            return result;
        }

        public static Vector3 Unproject(Vector3 source, Matrix4x4 worldViewProjection, float x, float y, float width, float height, float minDepth = 0, float maxDepth = 1)
        {
            Matrix4x4 inverse;
            if (!Matrix4x4.Invert(worldViewProjection, out inverse))
                return new Vector3();

            Vector3 vector = new Vector3(
                (source.X - x) / width * 2f - 1f,
                -((source.Y - y) / height * 2f - 1f),
                (source.Z - minDepth) / (maxDepth - minDepth));

            float a = vector.X * inverse.M14 + vector.Y * inverse.M24 + vector.Z * inverse.M34 + inverse.M44;
            vector = Vector3.Transform(vector, inverse);

            if (!MathC.IsOne(a))
            {
                vector = vector / a;
            }
            return vector;
        }

        public static Ray GetPickRay(Vector2 screenPos, Matrix4x4 worldViewProjection, float x, float y, float width, float height, float minDepth = 0, float maxDepth = 1)
        {
            Vector3 nearPoint = Unproject(new Vector3(screenPos.X, screenPos.Y, 0), worldViewProjection, x, y, width, height, minDepth, maxDepth);
            Vector3 farPoint = Unproject(new Vector3(screenPos.X, screenPos.Y, 1), worldViewProjection, x, y, width, height, minDepth, maxDepth);
            return new Ray(nearPoint, Vector3.Normalize(farPoint - nearPoint));
        }
    }
}
