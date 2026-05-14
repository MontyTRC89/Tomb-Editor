using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace TombLib.Graphics
{
    public class Frustum
    {
        // Divisor used for position to keep frustum culling in the distance stable
        private const float _frustum_divisor = 1024.0f;

        // The 6 frustum planes (Left, Right, Bottom, Top, Near, Far)
        private Plane[] _planes = new Plane[6];

        // Cached camera parameters to avoid recomputing when nothing changed
        private Vector3 _cachedPosition;
        private Vector3 _cachedDirection;
        private Vector3 _cachedUp;
        private float _cachedFov;
        private bool _hasParams;

        public void Update(Camera camera, Size viewportSize)
        {
            // Pre-divide the position to make them small
            var pos = camera.GetPosition() / _frustum_divisor;
            var target = camera.Target / _frustum_divisor;
            var dir = Vector3.Normalize(target - pos);

            // Extract the up vector from the second column of the rotation matrix
            Matrix4x4 rotMatrix = camera.GetRotationMatrix();
            Vector3 up = new Vector3(rotMatrix.M21, rotMatrix.M22, rotMatrix.M23);

            float aspect = viewportSize.Width / (float)viewportSize.Height;
            float zFar = _frustum_divisor * 200;
            float zNear = 1 / _frustum_divisor;

            // Blow up FOV a bit for cases when out-of-bounds objects are used
            float fov = camera.FieldOfView * 1.2f;

            if (!_hasParams ||
                 _cachedPosition != pos ||
                 _cachedDirection != dir ||
                 _cachedUp != up ||
                 _cachedFov != fov)
            {
                _cachedPosition = pos;
                _cachedDirection = dir;
                _cachedUp = up;
                _cachedFov = fov;

                // Build view and projection matrices
                Matrix4x4 view = Matrix4x4.CreateLookAt(pos, pos + dir, up);
                Matrix4x4 proj = Matrix4x4.CreatePerspectiveFieldOfView(fov, aspect, zNear, zFar);
                Matrix4x4 vp = view * proj;

                // Extract 6 planes using the Gribb-Hartmann method
                ExtractPlanes(ref vp);
                _hasParams = true;
            }
        }

        /// <summary>
        /// Extracts and normalizes the 6 frustum planes from a view-projection matrix
        /// using the Gribb-Hartmann method.
        /// </summary>
        private void ExtractPlanes(ref Matrix4x4 vp)
        {
            // Left: row3 + row0
            _planes[0] = NormalizePlane(new Plane(
                vp.M14 + vp.M11,
                vp.M24 + vp.M21,
                vp.M34 + vp.M31,
                vp.M44 + vp.M41));

            // Right: row3 - row0
            _planes[1] = NormalizePlane(new Plane(
                vp.M14 - vp.M11,
                vp.M24 - vp.M21,
                vp.M34 - vp.M31,
                vp.M44 - vp.M41));

            // Bottom: row3 + row1
            _planes[2] = NormalizePlane(new Plane(
                vp.M14 + vp.M12,
                vp.M24 + vp.M22,
                vp.M34 + vp.M32,
                vp.M44 + vp.M42));

            // Top: row3 - row1
            _planes[3] = NormalizePlane(new Plane(
                vp.M14 - vp.M12,
                vp.M24 - vp.M22,
                vp.M34 - vp.M32,
                vp.M44 - vp.M42));

            // Near: row2
            _planes[4] = NormalizePlane(new Plane(
                vp.M13,
                vp.M23,
                vp.M33,
                vp.M43));

            // Far: row3 - row2
            _planes[5] = NormalizePlane(new Plane(
                vp.M14 - vp.M13,
                vp.M24 - vp.M23,
                vp.M34 - vp.M33,
                vp.M44 - vp.M43));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Plane NormalizePlane(Plane p)
        {
            float length = p.Normal.Length();
            if (length < 1e-10f)
                return p;
            return new Plane(p.Normal / length, p.D / length);
        }

        public bool Contains(BoundingBox box)
        {
            var min = box.Minimum / _frustum_divisor;
            var max = box.Maximum / _frustum_divisor;

            // For each plane, find the "positive vertex" (the corner most in the
            // direction of the plane normal). If it is behind the plane the entire
            // AABB is outside the frustum.
            for (int i = 0; i < 6; i++)
            {
                Vector3 normal = _planes[i].Normal;

                // Select the corner closest to the plane (positive vertex)
                Vector3 pVertex = new Vector3(
                    normal.X >= 0 ? max.X : min.X,
                    normal.Y >= 0 ? max.Y : min.Y,
                    normal.Z >= 0 ? max.Z : min.Z);

                if (DistanceToPlane(ref _planes[i], ref pVertex) < 0)
                    return false;
            }

            return true;
        }

        public bool Contains(Vector3 point)
        {
            var p = point / _frustum_divisor;

            for (int i = 0; i < 6; i++)
            {
                if (DistanceToPlane(ref _planes[i], ref p) < 0)
                    return false;
            }

            return true;
        }

        public bool Contains(BoundingSphere sphere)
        {
            var center = sphere.Center / _frustum_divisor;
            float radius = sphere.Radius / _frustum_divisor;

            for (int i = 0; i < 6; i++)
            {
                if (DistanceToPlane(ref _planes[i], ref center) < -radius)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the signed distance from a point to a plane.
        /// Positive means the point is on the side the normal points to.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float DistanceToPlane(ref Plane plane, ref Vector3 point)
        {
            return Vector3.Dot(plane.Normal, point) + plane.D;
        }
    }
}
