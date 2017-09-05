using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{
    public class WadKeyFrameRotation
    {
        public WadKeyFrameRotationAxis Axis;
        public int X;
        public int Y;
        public int Z;

        // I store angles in the old way because passing between int and floats cause a loss of precision
        // This property builds the DirectX rotation matrix from these angles
        public Matrix RotationMatrix
        {
            get
            {
                Matrix matrix = Matrix.Identity;

                float rotX = 0;
                float rotY = 0;
                float rotZ = 0;

                switch (Axis)
                {
                    case WadKeyFrameRotationAxis.ThreeAxes:
                        rotX = -X / 1024.0f * 2 * (float)Math.PI;
                        rotY = Y / 1024.0f * 2 * (float)Math.PI;
                        rotZ = -Z / 1024.0f * 2 * (float)Math.PI;

                        matrix = Matrix.RotationYawPitchRoll((float)rotY, (float)rotX, (float)rotZ);
                        break;

                    case WadKeyFrameRotationAxis.AxisX:
                        rotX = -X / 4096.0f * 2 * (float)Math.PI;
                        matrix = Matrix.RotationX((float)rotX);
                        break;

                    case WadKeyFrameRotationAxis.AxisY:
                        rotY = Y / 4096.0f * 2 * (float)Math.PI;
                        matrix = Matrix.RotationY((float)rotY);
                        break;

                    case WadKeyFrameRotationAxis.AxisZ:
                        rotZ = -Z / 4096.0f * 2 * (float)Math.PI;
                        matrix = Matrix.RotationZ((float)rotZ);
                        break;
                }

                return matrix;
            }
        }
    }
}
