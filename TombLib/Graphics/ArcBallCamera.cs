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
        public float RotationX { get; set; }
        public float RotationY { get; set; }

        // Y axis rotation limits (radians)
        public float MinRotationY { get; set; }
        public float MaxRotationY { get; set; }

        // Distance between the target and camera
        public float Distance { get; set; }

        // Distance limits
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }

        // Specified target
        public Vector3 Target { get; set; }

        // Horizontal field of view angle of the camera in radians.
        public float FieldOfView { get; set; } = 0.872f;
        
        public ArcBallCamera(Vector3 target, float rotationX,
            float rotationY, float minRotationY, float maxRotationY,
            float distance, float minDistance, float maxDistance)
        {
            Target = target;
            MinRotationY = minRotationY;
            MaxRotationY = maxRotationY;

            // Lock the y axis rotation between the min and max values
            RotationY = MathUtil.Clamp(rotationY, minRotationY, maxRotationY);
            RotationX = rotationX;
            MinDistance = minDistance;
            MaxDistance = maxDistance;

            // Lock the distance between the min and max values
            Distance = MathUtil.Clamp(distance, minDistance, maxDistance);
        }

        public void Zoom(float distanceChange)
        {
            Distance += distanceChange;
            Distance = MathUtil.Clamp(Distance, MinDistance, MaxDistance);
        }

        public void Rotate(float rotationXChange, float rotationYChange)
        {
            RotationX += rotationXChange;
            RotationY -= rotationYChange;
            RotationY = MathUtil.Clamp(RotationY, MinRotationY, MaxRotationY);
        }

        public void MoveCameraPlane(Vector3 movementVec)
        {
            Target += Vector3.TransformCoordinate(movementVec, GetRotationMatrix());
        }
        
        public override Matrix GetViewProjectionMatrix(float width, float height)
        {
            // Calculate up vector
            Matrix rotation = Matrix.RotationYawPitchRoll(RotationX, -RotationY, 0);
            Vector3 up = Vector3.TransformCoordinate(Vector3.UnitY, rotation);

            //new Vector3(0, 150, 0), Vector3.Up); 
            Matrix View = Matrix.LookAtLH(GetPosition(), Target, up);
            float aspectRatio = width / height;
            Matrix Projection = Matrix.PerspectiveFovLH(FieldOfView, aspectRatio, 10.0f, 100000.0f);
            return View * Projection;
        }

        public Matrix GetRotationMatrix()
        {
            return Matrix.RotationYawPitchRoll(RotationX, -RotationY, 0);
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
