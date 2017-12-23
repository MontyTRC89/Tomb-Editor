using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TombLib
{
    public static class MathC
    {
        public const float ZeroTolerance = 1e-6f; // Value a 8x higher than 1.19209290E-07F

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
        public unsafe static bool NearEqual(float a, float b)
        {
            // Check if the numbers are really close -- needed
            // when comparing numbers near zero.
            if (IsZero(a - b))
                return true;

            // Original from Bruce Dawson: http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
            int aInt = *(int*)&a;
            int bInt = *(int*)&b;

            // Different signs means they do not match.
            if ((aInt < 0) != (bInt < 0))
                return false;

            // Find the difference in ULPs.
            int ulp = Math.Abs(aInt - bInt);

            // Choose of maxUlp = 4
            // according to http://code.google.com/p/googletest/source/browse/trunk/include/gtest/internal/gtest-internal.h
            const int maxUlp = 4;
            return (ulp <= maxUlp);
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
            float num = a - b;
            return ((-epsilon <= num) && (num <= epsilon));
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
    }
}
