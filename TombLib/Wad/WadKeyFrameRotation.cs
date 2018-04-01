using System;
using System.Numerics;

namespace TombLib.Wad
{
    public struct WadKeyFrameRotation
    {
        public WadKeyFrameRotationAxis Axis { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        // I store angles in the old way because passing between int and floats cause a loss of precision
        // This property builds the DirectX rotation matrix from these angles
        public Vector3 RotationVector
        {
            get
            {
                var rotationVector = Vector3.Zero;

                float rotX = 0;
                float rotY = 0;
                float rotZ = 0;

                switch (Axis)
                {
                    case WadKeyFrameRotationAxis.ThreeAxes:
                        rotX = -X / 1024.0f * 2 * (float)Math.PI;
                        rotY = Y / 1024.0f * 2 * (float)Math.PI;
                        rotZ = -Z / 1024.0f * 2 * (float)Math.PI;

                        rotationVector = new Vector3(rotX, rotY, rotZ); // Matrix4x4.CreateFromYawPitchRoll(rotY, rotX, rotZ);
                        break;

                    case WadKeyFrameRotationAxis.AxisX:
                        rotX = -X / 4096.0f * 2 * (float)Math.PI;
                        rotationVector = new Vector3(rotX, 0, 0); // Matrix4x4.CreateRotationX(rotX);
                        break;

                    case WadKeyFrameRotationAxis.AxisY:
                        rotY = Y / 4096.0f * 2 * (float)Math.PI;
                        rotationVector = new Vector3(0, rotY, 0); // Matrix4x4.CreateRotationY(rotY);
                        break;

                    case WadKeyFrameRotationAxis.AxisZ:
                        rotZ = -Z / 4096.0f * 2 * (float)Math.PI;
                        rotationVector = new Vector3(0, 0, rotZ); // Matrix4x4.CreateRotationZ(rotZ);
                        break;
                }

                return rotationVector;
            }
        }

        public Matrix4x4 RotationMatrix
        {
            get
            {
                Matrix4x4 matrix = Matrix4x4.Identity;

                float rotX = 0;
                float rotY = 0;
                float rotZ = 0;

                switch (Axis)
                {
                    case WadKeyFrameRotationAxis.ThreeAxes:
                        rotX = -X / 1024.0f * 2 * (float)Math.PI;
                        rotY = Y / 1024.0f * 2 * (float)Math.PI;
                        rotZ = -Z / 1024.0f * 2 * (float)Math.PI;

                        matrix = Matrix4x4.CreateFromYawPitchRoll(rotY, rotX, rotZ);
                        break;

                    case WadKeyFrameRotationAxis.AxisX:
                        rotX = -X / 4096.0f * 2 * (float)Math.PI;
                        matrix = Matrix4x4.CreateRotationX(rotX);
                        break;

                    case WadKeyFrameRotationAxis.AxisY:
                        rotY = Y / 4096.0f * 2 * (float)Math.PI;
                        matrix = Matrix4x4.CreateRotationY(rotY);
                        break;

                    case WadKeyFrameRotationAxis.AxisZ:
                        rotZ = -Z / 4096.0f * 2 * (float)Math.PI;
                        matrix = Matrix4x4.CreateRotationZ(rotZ);
                        break;
                }

                return matrix;
            }
        }
    }
}
