using System;
using System.Numerics;

namespace TombLib.Wad
{
    public struct WadKeyFrameRotation
    {
        // We store angles as degrees, integer
        public Vector3 Rotations { get; set; }

        public float OriginalX { get; set; }
        public float OriginalY { get; set; }
        public float OriginalZ { get; set; }

        public static WadKeyFrameRotation FromTrAngle(WadKeyFrameRotationAxis axis, int x, int y, int z)
        {
            var angle = new WadKeyFrameRotation();

            float rotX = 0;
            float rotY = 0;
            float rotZ = 0;

            angle.OriginalX = x;
            angle.OriginalY = y;
            angle.OriginalZ = z;

            switch (axis)
            {
                case WadKeyFrameRotationAxis.ThreeAxes:
                    rotX = -x / 1024.0f * 360;
                    rotY = y / 1024.0f * 360;
                    rotZ = -z / 1024.0f * 360;
                    break;

                case WadKeyFrameRotationAxis.AxisX:
                    rotX = -x / 4096.0f * 360;
                    break;

                case WadKeyFrameRotationAxis.AxisY:
                    rotY = y / 4096.0f * 360;
                    break;

                case WadKeyFrameRotationAxis.AxisZ:
                    rotZ = -z / 4096.0f * 360;
                    break;
            }

            angle.Rotations = new Vector3(rotX, rotY, rotZ);
            return angle;
        }

        public static void ToTrAngle(WadKeyFrameRotation angle, out WadKeyFrameRotationAxis axis, out int rotX, out int rotY, out int rotZ)
        {
            if (angle.Rotations.X != 0 && angle.Rotations.Y == 0 && angle.Rotations.Z == 0)
            {
                axis = WadKeyFrameRotationAxis.AxisX;
                rotX = (int)(-angle.Rotations.X * 4096 / 360);
                rotY = 0;
                rotZ = 0;
            }
            else if (angle.Rotations.X == 0 && angle.Rotations.Y != 0 && angle.Rotations.Z == 0)
            {
                axis = WadKeyFrameRotationAxis.AxisY;
                rotX = 0;
                rotY = (int)(angle.Rotations.Y * 4096 / 360);
                rotZ = 0;
            }
            else if (angle.Rotations.X == 0 && angle.Rotations.Y == 0 && angle.Rotations.Z != 0)
            {
                axis = WadKeyFrameRotationAxis.AxisZ;
                rotX = 0;
                rotY = 0;
                rotZ = (int)(-angle.Rotations.Z * 4096 / 360);
            }
            else
            {
                axis = WadKeyFrameRotationAxis.ThreeAxes;
                rotX = (int)(-angle.Rotations.X * 1024 / 360);
                rotY = (int)(angle.Rotations.Y * 1024 / 360);
                rotZ = (int)(-angle.Rotations.Z * 1024 / 360);
            }
        }

        public Vector3 RotationVectorInRadians
        {
            get
            {
                return new Vector3(Rotations.X / 180.0f * (float)Math.PI, 
                                   Rotations.Y / 180.0f * (float)Math.PI, 
                                   Rotations.Z / 180.0f * (float)Math.PI);
            }
        }

        public Matrix4x4 RotationMatrix
        {
            get
            {
                return Matrix4x4.CreateFromYawPitchRoll(RotationVectorInRadians.Y, 
                                                        RotationVectorInRadians.X, 
                                                        RotationVectorInRadians.Z);
            }
        }
    }
}
