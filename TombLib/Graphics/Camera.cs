using System;
using System.Numerics;

namespace TombLib.Graphics
{
    public abstract class Camera
    {
        // Rotation around the two axes
        protected float _rotationY;
        public float RotationY
        {
            get { return _rotationY; }
            set { _rotationY = (float)(value - Math.Floor(value / (2 * Math.PI)) * (2 * Math.PI)); }
        }
        public float RotationX { get; set; }

        // Y axis rotation limits (radians)
        public float MinRotationX { get; set; }
        public float MaxRotationX { get; set; }

        // Distance between the target and camera
        public float Distance { get; set; }

        // Default camera distance, used for internal zoom/panning multiplier calculation
        protected readonly float DefaultDistance;

        // Distance limits
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Target { get; set; }

        public float FieldOfView { get; set; } = 0.872f;

        public Camera(Vector3 position, Vector3 target, float rotationX,
            float rotationY, float minRotationX, float maxRotationX,
            float distance, float minDistance, float maxDistance, float fieldOfView)
        {
            Position = position;
            Target = target;
            MinRotationX = minRotationX;
            MaxRotationX = maxRotationX;

            // Lock the y axis rotation between the min and max values
            RotationX = Math.Min(Math.Max(rotationX, minRotationX), maxRotationX);
            RotationY = rotationY;
            MinDistance = minDistance;
            MaxDistance = maxDistance;

            // Lock the distance between the min and max values
            Distance = Math.Min(Math.Max(distance, minDistance), maxDistance);

            DefaultDistance = distance;

            FieldOfView = fieldOfView;
        }

        public abstract Matrix4x4 GetViewProjectionMatrix(float width, float height);

        public abstract void Zoom(float distanceChange);

        public abstract void Rotate(float rotationXChange, float rotationYChange);

        public abstract void MoveCameraPlane(Vector3 movementVec);

        public abstract void MoveCameraLinear(Vector3 movementVec);

        public abstract Matrix4x4 GetRotationMatrix();

        public abstract Vector3 GetDirection();

        public abstract Vector3 GetPosition();

        public abstract Vector3 GetTarget();
    }
}
