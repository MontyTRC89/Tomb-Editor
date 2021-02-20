﻿using System;
using System.Numerics;

namespace TombLib.Graphics
{
    public class ArcBallCamera : Camera
    {
        public ArcBallCamera(Vector3 target, float rotationX,
            float rotationY, float minRotationX, float maxRotationX,
            float distance, float minDistance, float maxDistance, float fieldOfView)
            : base(Vector3.Zero, target, rotationX, rotationY, minRotationX, maxRotationX,
                 distance, minDistance, maxDistance, fieldOfView)
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
            float distanceMultiplier = (float)Math.Pow(Distance / DefaultDistance, 2 / (float)3);
            Target += MathC.HomogenousTransform(movementVec * distanceMultiplier, GetRotationMatrix());
        }

        public override void MoveCameraLinear(Vector3 movementVec)
        {
            Target += movementVec;
        }

        public override Matrix4x4 GetViewProjectionMatrix(float width, float height)
        {
            // Calculate up vector
            Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(RotationY, -RotationX, 0);
            Vector3 up = MathC.HomogenousTransform(Vector3.UnitY, rotation);

            //new Vector3(0, 150, 0), Vector3.Up);
            Matrix4x4 View = MathC.Matrix4x4CreateLookAtLH(GetPosition(), Target, up);
            float aspectRatio = width / height;
            Matrix4x4 Projection = MathC.Matrix4x4CreatePerspectiveFieldOfViewLH(FieldOfView, aspectRatio, 20.0f, 1000000.0f);
            Matrix4x4 result = View * Projection;
            return result;
        }

        public override Matrix4x4 GetRotationMatrix()
        {
            return Matrix4x4.CreateFromYawPitchRoll(RotationY, -RotationX, 0);
        }

        public override Vector3 GetDirection()
        {
            // Translate down the Z axis by the desired distance
            // between the camera and object, then rotate that
            // vector to find the camera offset from the target
            return MathC.HomogenousTransform(new Vector3(0, 0, Distance), GetRotationMatrix());
        }

        public override Vector3 GetPosition()
        {
            return Target + GetDirection();
        }

        public override Vector3 GetTarget()
        {
            return Target;
        }
    }
}
