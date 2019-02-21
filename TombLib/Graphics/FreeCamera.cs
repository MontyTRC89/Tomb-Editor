using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Graphics
{
    public class FreeCamera : Camera
    {
        public FreeCamera(Vector3 position, float rotationX,
            float rotationY, float minRotationX, float maxRotationX,
            float fieldOfView)
            : base(position, Vector3.Zero, rotationX, rotationY, minRotationX, maxRotationX,
                 0, 0, 0, fieldOfView)
        {

        }

        public override void Zoom(float distanceChange)
        {
            float distanceMultiplier = Distance / DefaultDistance;
            Distance += distanceChange * distanceMultiplier;
            Distance = Math.Min(Math.Max(Distance, MinDistance), MaxDistance);
        }

        public override void Rotate(float rotationXChange, float rotationYChange)
        {
            RotationY += rotationXChange;
            RotationX -= rotationYChange;
            RotationX = Math.Min(Math.Max(RotationX, MinRotationX), MaxRotationX);
        }

        public override void MoveCameraPlane(Vector3 movementVec)
        {
            Matrix4x4 rotation = GetRotationMatrix();

            Vector3 look = MathC.HomogenousTransform(Vector3.UnitZ, rotation);
            Vector3 right = MathC.HomogenousTransform(Vector3.UnitX, rotation);
            Vector3 up = Vector3.Cross(look, right);

            Position -= movementVec.Z * look;
            Position -= movementVec.X * right;
        }

        public override void MoveCameraLinear(Vector3 movementVec)
        {

        }

        public override Matrix4x4 GetViewProjectionMatrix(float width, float height)
        {
            Matrix4x4 rotation = GetRotationMatrix();

            Vector3 look = MathC.HomogenousTransform(Vector3.UnitZ, rotation);
            Vector3 right = MathC.HomogenousTransform(Vector3.UnitX, rotation);
            Vector3 up = Vector3.Cross(look, right);

            Target = Position + 1024.0f * look;

            Matrix4x4 View = MathC.Matrix4x4CreateLookAtLH(Position, Target, up);

            float aspectRatio = width / height;
            Matrix4x4 Projection = MathC.Matrix4x4CreatePerspectiveFieldOfViewLH(FieldOfView, aspectRatio, 20.0f, 1000000.0f);
            Matrix4x4 result = View * Projection;
            return result;
        }

        public override Matrix4x4 GetRotationMatrix()
        {
            return Matrix4x4.CreateFromYawPitchRoll(RotationY, RotationX, 0);
        }

        public override Vector3 GetDirection()
        {
            Matrix4x4 rotation = GetRotationMatrix();
            Vector3 look = MathC.HomogenousTransform(Vector3.UnitZ, rotation);
            return Vector3.Normalize(look);
        }

        public override Vector3 GetPosition()
        {
            return Position;
        }

        public override Vector3 GetTarget()
        {
            return Target;
        }
    }
}
