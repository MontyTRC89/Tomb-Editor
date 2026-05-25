using System;
using System.Drawing;
using System.Numerics;

namespace TombLib.Graphics
{
    /// <summary>
    /// View-frustum culler. The 6 clip planes are extracted from the
    /// view x projection matrix (Gribb-Hartmann method) and box / sphere /
    /// point tests check that the candidate is in front of every plane (i.e.
    /// inside or straddling the frustum). Pure System.Numerics.
    /// </summary>
    public class Frustum
    {
        // Plane equations Ax + By + Cz + D = 0 stored as Vector4(A, B, C, D),
        // with the normal (A,B,C) pointing INTO the frustum.
        private Vector4 _left, _right, _bottom, _top, _near, _far;

        public void Update(Camera camera, Size viewportSize)
        {
            var vp = camera.GetViewProjectionMatrix(viewportSize.Width, viewportSize.Height);

            // Gribb-Hartmann plane extraction. Each plane = sum/diff of a VP
            // row with the W row.
            _left   = Normalize(new Vector4(vp.M14 + vp.M11, vp.M24 + vp.M21, vp.M34 + vp.M31, vp.M44 + vp.M41));
            _right  = Normalize(new Vector4(vp.M14 - vp.M11, vp.M24 - vp.M21, vp.M34 - vp.M31, vp.M44 - vp.M41));
            _bottom = Normalize(new Vector4(vp.M14 + vp.M12, vp.M24 + vp.M22, vp.M34 + vp.M32, vp.M44 + vp.M42));
            _top    = Normalize(new Vector4(vp.M14 - vp.M12, vp.M24 - vp.M22, vp.M34 - vp.M32, vp.M44 - vp.M42));
            _near   = Normalize(new Vector4(vp.M13,          vp.M23,          vp.M33,          vp.M43));
            _far    = Normalize(new Vector4(vp.M14 - vp.M13, vp.M24 - vp.M23, vp.M34 - vp.M33, vp.M44 - vp.M43));
        }

        private static Vector4 Normalize(Vector4 p)
        {
            float len = MathF.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
            return len > 0f ? p / len : p;
        }

        public bool Contains(BoundingBox box)
        {
            Vector3 min = box.Minimum;
            Vector3 max = box.Maximum;
            return BoxInsideOrIntersects(min, max, _left)
                && BoxInsideOrIntersects(min, max, _right)
                && BoxInsideOrIntersects(min, max, _bottom)
                && BoxInsideOrIntersects(min, max, _top)
                && BoxInsideOrIntersects(min, max, _near)
                && BoxInsideOrIntersects(min, max, _far);
        }

        public bool Contains(Vector3 point)
            => PlaneDistance(_left,   point) >= 0
            && PlaneDistance(_right,  point) >= 0
            && PlaneDistance(_bottom, point) >= 0
            && PlaneDistance(_top,    point) >= 0
            && PlaneDistance(_near,   point) >= 0
            && PlaneDistance(_far,    point) >= 0;

        public bool Contains(BoundingSphere sphere)
        {
            Vector3 c = sphere.Center;
            float r   = sphere.Radius;
            return PlaneDistance(_left,   c) >= -r
                && PlaneDistance(_right,  c) >= -r
                && PlaneDistance(_bottom, c) >= -r
                && PlaneDistance(_top,    c) >= -r
                && PlaneDistance(_near,   c) >= -r
                && PlaneDistance(_far,    c) >= -r;
        }

        // Standard AABB-vs-plane test using the "positive vertex" trick: for
        // each plane, pick the box corner closest in the plane's normal
        // direction. If even that corner is behind the plane, the whole box
        // is outside; otherwise the box is inside or straddling.
        private static bool BoxInsideOrIntersects(Vector3 min, Vector3 max, Vector4 plane)
        {
            Vector3 positive = new Vector3(
                plane.X >= 0 ? max.X : min.X,
                plane.Y >= 0 ? max.Y : min.Y,
                plane.Z >= 0 ? max.Z : min.Z);
            return PlaneDistance(plane, positive) >= 0;
        }

        private static float PlaneDistance(Vector4 plane, Vector3 p)
            => plane.X * p.X + plane.Y * p.Y + plane.Z * p.Z + plane.W;
    }
}
