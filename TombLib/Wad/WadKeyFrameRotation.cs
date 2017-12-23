using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{
    public class WadKeyFrameRotation
    {
        public WadKeyFrameRotationAxis Axis { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        // I store angles in the old way because passing between int and floats cause a loss of precision
        // This property builds the DirectX rotation matrix from these angles
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

                        matrix = Matrix4x4.CreateFromYawPitchRoll((float)rotY, (float)rotX, (float)rotZ);
                        break;

                    case WadKeyFrameRotationAxis.AxisX:
                        rotX = -X / 4096.0f * 2 * (float)Math.PI;
                        matrix = Matrix4x4.CreateRotationX((float)rotX);
                        break;

                    case WadKeyFrameRotationAxis.AxisY:
                        rotY = Y / 4096.0f * 2 * (float)Math.PI;
                        matrix = Matrix4x4.CreateRotationY((float)rotY);
                        break;

                    case WadKeyFrameRotationAxis.AxisZ:
                        rotZ = -Z / 4096.0f * 2 * (float)Math.PI;
                        matrix = Matrix4x4.CreateRotationZ((float)rotZ);
                        break;
                }

                return matrix;
            }
        }

        public WadKeyFrameRotation Clone()
        {
            var rotation = new WadKeyFrameRotation();

            rotation.Axis = Axis;
            rotation.X = X;
            rotation.Y = Y;
            rotation.Z = Z;

            return rotation;
        }
    }
}
