using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using TombLib.Utils;

namespace TombLib
{
    public static class MathC
    {
        public const float ZeroTolerance = 1e-6f; // Value a 8x higher than 1.19209290E-07F

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorInt2 To2(this VectorInt3 vec) => new VectorInt2(vec.X, vec.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 To2(this Vector3 vec) => new Vector2(vec.X, vec.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 To2(this Vector4 vec) => new Vector2(vec.X, vec.Y);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 To3(this Vector4 vec) => new Vector3(vec.X, vec.Y, vec.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 TransformPerspectively(this Matrix4x4 matrix, Vector3 vec)
        {
            Vector4 transformedVec = Vector4.Transform(new Vector4(vec, 1.0f), matrix);
            return transformedVec.To3() / transformedVec.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Round(Vector2 v) => new Vector2((float)Math.Round(v.X), (float)Math.Round(v.Y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Round(Vector3 v) => new Vector3((float)Math.Round(v.X), (float)Math.Round(v.Y), (float)Math.Round(v.Z));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Round(Vector4 v) => new Vector4((float)Math.Round(v.X), (float)Math.Round(v.Y), (float)Math.Round(v.Z), (float)Math.Round(v.W));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Floor(Vector2 v) => new Vector2((float)Math.Floor(v.X), (float)Math.Floor(v.Y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Floor(Vector3 v) => new Vector3((float)Math.Floor(v.X), (float)Math.Floor(v.Y), (float)Math.Floor(v.Z));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Floor(Vector4 v) => new Vector4((float)Math.Floor(v.X), (float)Math.Floor(v.Y), (float)Math.Floor(v.Z), (float)Math.Floor(v.W));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Ceiling(Vector2 v) => new Vector2((float)Math.Ceiling(v.X), (float)Math.Ceiling(v.Y));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Ceiling(Vector3 v) => new Vector3((float)Math.Ceiling(v.X), (float)Math.Ceiling(v.Y), (float)Math.Ceiling(v.Z));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Ceiling(Vector4 v) => new Vector4((float)Math.Ceiling(v.X), (float)Math.Ceiling(v.Y), (float)Math.Ceiling(v.Z), (float)Math.Ceiling(v.W));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 RoundToHalf(Vector2 v) => Round(v * 2.0f) / 2.0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Clamp(double value, double min, double max) => value < min? min : value > max ? max : value;

        /// <summary>
        /// Checks if a and b are almost equals, taking into account the magnitude of floating point numbers (unlike <see cref="WithinEpsilon"/> method). See Remarks.
        /// See remarks.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <returns><c>true</c> if a almost equal to b, <c>false</c> otherwise</returns>
        /// <remarks>
        /// The code is using the technique described by Bruce Dawson in
        /// <a href="http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/">Comparing Floating point numbers 2012 edition</a>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe bool NearEqual(float a, float b)
        {
            // Check if the numbers are really close -- needed
            // when comparing numbers near zero.
            if (IsZero(a - b))
                return true;

            // Original from Bruce Dawson: http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
            int aInt = *(int*)&a;
            int bInt = *(int*)&b;

            // Different signs means they do not match.
            if (aInt < 0 != bInt < 0)
                return false;

            // Find the difference in ULPs.
            int ulp = Math.Abs(aInt - bInt);

            // Choose of maxUlp = 4
            // according to http://code.google.com/p/googletest/source/browse/trunk/include/gtest/internal/gtest-internal.h
            const int maxUlp = 4;
            return ulp <= maxUlp;
        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to zero (0.0f); otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(float a)
        {
            return Math.Abs(a) < ZeroTolerance;
        }

        /// <summary>
        /// Determines whether the specified value is close to one (1.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to one (1.0f); otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsOne(float a)
        {
            return IsZero(a - 1.0f);
        }

        /// <summary>
        /// Checks if a - b are almost equals within a float epsilon.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <param name="epsilon">Epsilon value</param>
        /// <returns><c>true</c> if a almost equal to b within a float epsilon, <c>false</c> otherwise</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WithinEpsilon(float a, float b, float epsilon)
        {
            // Quickly return simple equality in case e=0
            if (epsilon == 0.0f) return a == b;

            float num = a - b;
            return -epsilon <= num && num <= epsilon;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 HomogenousTransform(Vector3 vector, Matrix4x4 matrix)
        {
            Vector4 result = Vector4.Transform(vector, matrix);
            result *= 1.0f / result.W;
            return new Vector3(result.X, result.Y, result.Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Plane CreatePlaneAtPoint(Vector3 position, Vector3 normal)
        {
            return new Plane(normal, -Vector3.Dot(normal, position));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Matrix4x4ChangeHandedness(Matrix4x4 matrix)
        {
            matrix.M31 = -matrix.M31;
            matrix.M32 = -matrix.M32;
            matrix.M33 = -matrix.M33;
            matrix.M34 = -matrix.M34;
            return matrix;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Matrix4x4CreateLookAtLH(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            return Matrix4x4.CreateLookAt(cameraPosition, cameraPosition * 2 - cameraTarget, cameraUpVector);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Matrix4x4CreateOrthographicLH(float width, float height, float zNearPlane, float zFarPlane)
        {
            return Matrix4x4ChangeHandedness(Matrix4x4.CreateOrthographic(width, height, zNearPlane, zFarPlane));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Matrix4x4CreateOrthographicOffCenterLH(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
        {
            return Matrix4x4ChangeHandedness(Matrix4x4.CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Matrix4x4CreatePerspectiveLH(float width, float height, float nearPlaneDistance, float farPlaneDistance)
        {
            return Matrix4x4ChangeHandedness(Matrix4x4.CreatePerspective(width, height, nearPlaneDistance, farPlaneDistance));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Matrix4x4CreatePerspectiveFieldOfViewLH(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
        {
            return Matrix4x4ChangeHandedness(Matrix4x4.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4x4 Matrix4x4CreatePerspectiveOffCenterLH(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance)
        {
            return Matrix4x4ChangeHandedness(Matrix4x4.CreatePerspectiveOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance));
        }

        // Code taken from Wikipedia:
        // https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#Quaternion_to_Euler_Angles_Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 QuaternionToEuler(Quaternion q)
        {
            // Wikipedia uses a different convention.
            // Convert to that by swapping parameters.
            double x = q.Z;
            double y = q.X;
            double z = q.Y;
            double w = q.W;

            // Handle singularity case
            // Inspired by: http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
            double sinp = 2 * (w * y - z * x);
            const double singularityLimit = 0.9999995;
            if (Math.Abs(sinp) > singularityLimit)
                if (sinp > 0)
                    return new Vector3((float)Math.PI * 0.5f, 0, 2.0f * (float)Math.Atan2(x, w));
                else
                    return new Vector3((float)-Math.PI * 0.5f, 0, -2.0f * (float)Math.Atan2(x, w));

            // Roll (x-axis rotation)
            double sinr = 2 * (w * x + y * z);
            double cosr = 1 - 2 * (x * x + y * y);
            double roll = Math.Atan2(sinr, cosr);

            // Pitch (y-axis rotation)
            double pitch;
            if (Math.Abs(sinp) >= 1)
                pitch = (sinp > 0 ? Math.PI * 0.5f : -Math.PI * 0.5f);
            else
                pitch = Math.Asin(sinp);

            // Yaw (z-axis rotation)
            double siny = 2 * (w * z + x * y);
            double cosy = 1 - 2 * (y * y + z * z);
            double yaw = Math.Atan2(siny, cosy);

            return new Vector3((float)pitch, (float)yaw, (float)roll);
        }

        public static double Lerp(double value1, double value2, double amount)
            => value1 * (1 - amount) + value2 * amount;

        // Code taken from XnaGeometry library, Copyright В© 2006 The Mono.Xna Team (MIT License)
        public static double SmoothStep(double value1, double value2, double amount)
        {
            amount = Clamp(amount, 0f, 1f);
            double result = Hermite(value1, 0f, value2, 0f, amount);
            return result;
        }

        // Code taken from XnaGeometry library, Copyright В© 2006 The Mono.Xna Team (MIT License)
        public static double Hermite(double value1, double tangent1, double value2, double tangent2, double amount)
        {
            // All transformed to double not to lose precission
            // Otherwise, for high numbers of param:amount the result is NaN instead of Infinity
            double v1 = value1, v2 = value2, t1 = tangent1, t2 = tangent2, s = amount, result;
            double sCubed = s * s * s;
            double sSquared = s * s;

            if (amount == 0f)
                result = value1;
            else if (amount == 1f)
                result = value2;
            else
                result = (2 * v1 - 2 * v2 + t2 + t1) * sCubed +
                    (3 * v2 - 3 * v1 - 2 * t1 - t2) * sSquared +
                    t1 * s +
                    v1;
            return (double)result;
        }

        // Useful for storing a dictionary of vertices
        public static Hash GetVector3Hash(Vector3 p)
        {
            byte[] buffer;
            using (var ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(p.X), 0, 4);
                ms.Write(BitConverter.GetBytes(p.Y), 0, 4);
                ms.Write(BitConverter.GetBytes(p.Z), 0, 4);
                buffer = ms.ToArray();
            }
            return Hash.FromByteArray(buffer);
        }

        public static float CalculateArea(Vector2 coord0, Vector2 coord1)
        {
            return (coord1.X - coord0.X) * (coord1.Y + coord0.Y);
        }

        public static float CalculateArea(Vector2 coord0, Vector2 coord1, Vector2 coord2, Vector2? coord3 = null)
        {
            if (!coord3.HasValue)
                return CalculateArea(new Vector2[3] { coord0, coord1, coord2 });
            else
                return CalculateArea(new Vector2[4] { coord0, coord1, coord2, coord3.Value });
        }

        public static float CalculateArea(Vector2[] coords)
        {
            if (coords.Length == 3)
            {
                return (CalculateArea(coords[0], coords[1]) +
                        CalculateArea(coords[1], coords[2]) +
                        CalculateArea(coords[2], coords[0])) * 0.5f;
            }
            else
            {
                return (CalculateArea(coords[0], coords[1]) +
                        CalculateArea(coords[1], coords[2]) +
                        CalculateArea(coords[2], coords[3]) +
                        CalculateArea(coords[3], coords[0])) * 0.5f;
            }
        }
    }
}
