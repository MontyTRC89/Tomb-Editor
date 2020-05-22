using SharpDX;
using System;
using System.Drawing;

namespace TombLib.Graphics
{
    public struct Frustum
    {
        private BoundingFrustum _frustum;

        public Frustum(Camera camera, Size viewportSize)
        {
            var pos = camera.GetPosition();
            var target = camera.Target;
            var dir = target - pos;
            dir = System.Numerics.Vector3.Normalize(dir);
            var frustumParams = new FrustumCameraParams()
            {
                Position = pos.ToSharpDX(),
                LookAtDir = dir.ToSharpDX(),
                UpDir = new Vector3(0.0f, -1.0f, 0.0f),
                FOV = camera.FieldOfView,
                AspectRatio = viewportSize.Width / viewportSize.Height,
                ZFar = camera.Distance,
                ZNear = camera.MinDistance
            };

            _frustum = BoundingFrustum.FromCamera(frustumParams);
        }

        public bool Contains(BoundingBox box)
        {
            var min = new Vector3(box.Minimum.X, box.Minimum.Y, box.Minimum.Z);
            var max = new Vector3(box.Maximum.X, box.Maximum.Y, box.Maximum.Z);
            var sharpBox = new SharpDX.BoundingBox(min, max);

            var contains = _frustum.Contains(ref sharpBox);
            return (contains == ContainmentType.Contains || contains == ContainmentType.Intersects);
        }

        public bool Contains(System.Numerics.Vector3 point)
        {
            var vec = new Vector3(point.X, point.Y, point.Z);
            var contains = _frustum.Contains(ref vec);
            return (contains == ContainmentType.Contains || contains == ContainmentType.Intersects);
        }

        public bool Contains(BoundingSphere sphere)
        {
            var vec = new Vector3(sphere.Center.X, sphere.Center.Y, sphere.Center.Z);
            var sharpSphere = new SharpDX.BoundingSphere(vec, sphere.Radius);
            var contains = _frustum.Contains(ref sharpSphere);
            return (contains == ContainmentType.Contains || contains == ContainmentType.Intersects);
        }
    }
}
