using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;

namespace TombLib.Graphics
{
    public class ArcBallCamera : Camera
    {
        // Rotation around the two axes
        private float _rotationY;
        public float RotationY
        {
            get { return _rotationY; }
            set  { _rotationY = (float)(value - Math.Floor(value / (2 * Math.PI)) * (2 * Math.PI)); }
        }
        public float RotationX { get; set; }

        // Y axis rotation limits (radians)
        public float MinRotationX { get; set; }
        public float MaxRotationX { get; set; }

        // Distance between the target and camera
        public float Distance { get; set; }

        // Distance limits
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }

        // Specified target
        public Vector3 Target { get; set; }

        // Horizontal field of view angle of the camera in radians.
        public float FieldOfView { get; set; } = 0.872f;

        // Default camera distance, used for internal zoom/panning multiplier calculation
        private float DefaultDistance;

        public ArcBallCamera(Vector3 target, float rotationY,
            float rotationX, float minRotationX, float maxRotationX,
            float distance, float minDistance, float maxDistance, float fieldOfView)
        {
            Target = target;
            MinRotationX = minRotationX;
            MaxRotationX = maxRotationX;

            // Lock the y axis rotation between the min and max values
            RotationX = MathUtil.Clamp(rotationX, minRotationX, maxRotationX);
            RotationY = rotationY;
            MinDistance = minDistance;
            MaxDistance = maxDistance;

            // Lock the distance between the min and max values
            Distance = MathUtil.Clamp(distance, minDistance, maxDistance);

            DefaultDistance = distance;

            FieldOfView = fieldOfView;
        }

        public void Zoom(float distanceChange)
        {
            float distanceMultiplier = Distance / DefaultDistance;
            Distance += distanceChange * distanceMultiplier;
            Distance = MathUtil.Clamp(Distance, MinDistance, MaxDistance);
        }

        public void Rotate(float rotationXChange, float rotationYChange)
        {
            RotationY += rotationXChange;
            RotationX -= rotationYChange;
            RotationX = MathUtil.Clamp(RotationX, MinRotationX, MaxRotationX);
        }

        public void MoveCameraPlane(Vector3 movementVec)
        {
            float distanceMultiplier = (float)Math.Pow((Distance / DefaultDistance), (float)2 / (float)3);
            Target += Vector3.TransformCoordinate(movementVec * distanceMultiplier, GetRotationMatrix());
        }

        public void MoveCameraLinear(Vector3 movementVec)
        {
            Target += movementVec;
        }

        public override Matrix GetViewProjectionMatrix(float width, float height)
        {
            // Calculate up vector
            Matrix rotation = Matrix.RotationYawPitchRoll(RotationY, -RotationX, 0);
            Vector3 up = Vector3.TransformCoordinate(Vector3.UnitY, rotation);

            //new Vector3(0, 150, 0), Vector3.Up);
            Matrix View = Matrix.LookAtLH(GetPosition(), Target, up);
            float aspectRatio = width / height;
            Matrix Projection = Matrix.PerspectiveFovLH(FieldOfView, aspectRatio, 20.0f, 1000000.0f);
            return View * Projection;
        }

        public Matrix GetRotationMatrix()
        {
            return Matrix.RotationYawPitchRoll(RotationY, -RotationX, 0);
        }

        public Vector3 GetDirection()
        {
            // Translate down the Z axis by the desired distance
            // between the camera and object, then rotate that
            // vector to find the camera offset from the target
            return Vector3.TransformCoordinate(new Vector3(0, 0, Distance), GetRotationMatrix());
        }

        public Vector3 GetPosition()
        {
            return Target + GetDirection();
        }
    }
}
