using SharpDX;
using System.Drawing;

namespace TombLib.Graphics
{
    public class Frustum
    {
        // Divisor used for position to keep frustum culling in the distance stable
        private const float FRUSTUM_DIVISOR = 1024.0f; 

        private BoundingFrustum _frustum;
        private FrustumCameraParams? _frustumParams = null;

        public void Update(Camera camera, Size viewportSize)
        {
            // Pre-divide the position to make them small
            var pos = camera.GetPosition() / FRUSTUM_DIVISOR;
            var target = camera.Target / FRUSTUM_DIVISOR;
            var dir = System.Numerics.Vector3.Normalize(target - pos);

            //Extract the up vector from the second column of the rotation matrix
            System.Numerics.Matrix4x4 rotMatrix = camera.GetRotationMatrix();
            System.Numerics.Vector3 up = new System.Numerics.Vector3(rotMatrix.M21, rotMatrix.M22, rotMatrix.M23); 

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

            if (!_frustumParams.HasValue ||
                 _frustumParams.Value.Position != frustumParams.Position ||
                 _frustumParams.Value.LookAtDir != frustumParams.LookAtDir ||
                 _frustumParams.Value.UpDir != frustumParams.UpDir ||
                 _frustumParams.Value.FOV != frustumParams.FOV)
            {
                _frustumParams = frustumParams;
                _frustum = BoundingFrustum.FromCamera(frustumParams);
            }
        }

        public bool Contains(BoundingBox box)
        {
            var min = box.Minimum.ToSharpDX() / FRUSTUM_DIVISOR;
            var max = box.Maximum.ToSharpDX() / FRUSTUM_DIVISOR;
            var sharpBox = new SharpDX.BoundingBox(min, max);
            var contains = _frustum.Contains(ref sharpBox);
            return (contains == ContainmentType.Contains || contains == ContainmentType.Intersects);
        }

        public bool Contains(System.Numerics.Vector3 point)
        {
            var vec = point.ToSharpDX() / FRUSTUM_DIVISOR;
            var contains = _frustum.Contains(ref vec);
            return (contains == ContainmentType.Contains || contains == ContainmentType.Intersects);
        }

        public bool Contains(BoundingSphere sphere)
        {
            var vec = sphere.Center.ToSharpDX() / FRUSTUM_DIVISOR;
            var sharpSphere = new SharpDX.BoundingSphere(vec, sphere.Radius / FRUSTUM_DIVISOR);
            var contains = _frustum.Contains(ref sharpSphere);
            return (contains == ContainmentType.Contains || contains == ContainmentType.Intersects);
        }
    }
}
