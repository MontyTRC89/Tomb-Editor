using SharpDX;
using System;
using System.Drawing;

namespace TombLib.Graphics
{
    public class Frustum
    {
        // Divisor used for position to keep frustum culling in the distance stable
        private const float FRUSTUM_DIVISOR = 1024.0f; 
        private BoundingFrustum _frustum;
        private System.Numerics.Vector3 _lastGoodDir;

        public Frustum()
        {
            _lastGoodDir = new System.Numerics.Vector3(float.MinValue);
        }

        public void Update(Camera camera, Size viewportSize)
        {
            // Pre-divide the position to make them small
            var pos = camera.GetPosition() / FRUSTUM_DIVISOR;
            var target = camera.Target / FRUSTUM_DIVISOR;
            var dir = System.Numerics.Vector3.Normalize(target - pos);
            
            // Keep last good direction for cases when NaN may happen, otherwise
            // use last good direction
            if (Math.Abs(dir.Y) == 1)
                dir = _lastGoodDir;
            else
                _lastGoodDir = dir;

            var up = System.Numerics.Vector3.Cross(System.Numerics.Vector3.Cross(dir, new System.Numerics.Vector3(0, 1, 0)), dir);

            var frustumParams = new FrustumCameraParams()
            {
                Position = pos.ToSharpDX(),
                LookAtDir = dir.ToSharpDX(),
                UpDir = up.ToSharpDX(),
                AspectRatio = viewportSize.Width / (float)viewportSize.Height,
                ZFar = FRUSTUM_DIVISOR * 200,
                ZNear = 1 / FRUSTUM_DIVISOR,

                // Blow up FOV a bit for cases when out-of-bounds objects are used
                FOV = camera.FieldOfView * 1.2f, 
            };

            _frustum = BoundingFrustum.FromCamera(frustumParams);
        }

        public bool Contains(BoundingBox box)
        {
            var min = new Vector3(box.Minimum.X, box.Minimum.Y, box.Minimum.Z) / FRUSTUM_DIVISOR;
            var max = new Vector3(box.Maximum.X, box.Maximum.Y, box.Maximum.Z) / FRUSTUM_DIVISOR;
            var sharpBox = new SharpDX.BoundingBox(min, max);
            var contains = _frustum.Contains(ref sharpBox);
            return (contains == ContainmentType.Contains || contains == ContainmentType.Intersects);
        }

        public bool Contains(System.Numerics.Vector3 point)
        {
            var vec = new Vector3(point.X, point.Y, point.Z) / FRUSTUM_DIVISOR;
            var contains = _frustum.Contains(ref vec);
            return (contains == ContainmentType.Contains || contains == ContainmentType.Intersects);
        }

        public bool Contains(BoundingSphere sphere)
        {
            var vec = new Vector3(sphere.Center.X, sphere.Center.Y, sphere.Center.Z) / FRUSTUM_DIVISOR;
            var sharpSphere = new SharpDX.BoundingSphere(vec, sphere.Radius / FRUSTUM_DIVISOR) ;
            var contains = _frustum.Contains(ref sharpSphere);
            return (contains == ContainmentType.Contains || contains == ContainmentType.Intersects);
        }
    }
}
