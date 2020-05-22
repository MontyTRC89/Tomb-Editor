using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace TombLib.Graphics
{
    public struct Frustum
    {
        //Divisor used for position to keep frustum culling in the distance stable
        private const float FRUSTUM_DIVISOR = 1024.0f; 
        private BoundingFrustum _frustum;

        public Frustum(Camera camera, Size viewportSize)
        {
            //Pre-divide the position to make them small
            var pos = camera.GetPosition() / FRUSTUM_DIVISOR;
            var target = camera.Target / FRUSTUM_DIVISOR;
            var dir = target - pos;
            dir = System.Numerics.Vector3.Normalize(dir);
            var frustumParams = new FrustumCameraParams()
            {
                Position = pos.ToSharpDX(),
                LookAtDir = dir.ToSharpDX(),
                UpDir = new Vector3(0.0f, 1.0f, 0.0f),
                FOV = camera.FieldOfView * 1.2f,
                AspectRatio = (float)viewportSize.Width / (float)viewportSize.Height,
                ZFar = FRUSTUM_DIVISOR * 200,
                ZNear = 1 / FRUSTUM_DIVISOR,
                
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

        public List<System.Numerics.Vector3> GetDebugPoints()
        {
            var vertices = _frustum.GetCorners();

            List<System.Numerics.Vector3> result = new List<System.Numerics.Vector3>();

            foreach (var v in vertices)
                result.Add(new System.Numerics.Vector3(v.X, v.Y, v.Z));

            return result;
        }
    }
}
