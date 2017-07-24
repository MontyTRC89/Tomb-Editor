using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;

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

        // Calculated position and specified target
        public Vector3 Position { get; private set; }
        public Vector3 Target { get; set; }

        public ArcBallCamera(Vector3 Target, float RotationX,
        float RotationY, float MinRotationY, float MaxRotationY,
        float Distance, float MinDistance, float MaxDistance,
        GraphicsDevice graphicsDevice)
            : base(graphicsDevice)
        {
            this.Target = Target;
            this.MinRotationY = MinRotationY;
            this.MaxRotationY = MaxRotationY;

            // Lock the y axis rotation between the min and max values
            this.RotationY = MathUtil.Clamp(RotationY, MinRotationY,
            MaxRotationY);
            this.RotationX = RotationX;
            this.MinDistance = MinDistance;
            this.MaxDistance = MaxDistance;

            // Lock the distance between the min and max values
            this.Distance = MathUtil.Clamp(Distance, MinDistance,
            MaxDistance);
        }

        public void Move(float DistanceChange)
        {
            this.Distance += DistanceChange;
            this.Distance = MathUtil.Clamp(Distance, MinDistance,
            MaxDistance);
        }

        public void Rotate(float RotationXChange, float RotationYChange)
        {
            this.RotationX += RotationXChange;
            this.RotationY += -RotationYChange;
            this.RotationY = MathUtil.Clamp(RotationY, MinRotationY,
            MaxRotationY);
        }

        public void Translate(Vector3 PositionChange)
        {
            this.Target += PositionChange;
        }

        public override void Update()
        {
            // Calculate rotation matrix from rotation values
            Matrix rotation = Matrix.RotationYawPitchRoll(RotationX, -RotationY, 0);

            // Translate down the Z axis by the desired distance
            // between the camera and object, then rotate that
            // vector to find the camera offset from the target
            Vector3 translation = new Vector3(0, 0, Distance);
            translation = Vector3.TransformCoordinate(translation, rotation);
            Position = Target + translation;
            
            Vector3 up = Vector3.TransformCoordinate(Vector3.UnitY, rotation);

            //new Vector3(0, 150, 0), Vector3.Up); 
            View = Matrix.LookAtLH(Position, Target, up);
        }

    }
}
