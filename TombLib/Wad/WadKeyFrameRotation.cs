using System;
using System.Collections.Generic;
using System.Numerics;

namespace TombLib.Wad
{
    public struct WadKeyFrameRotation
    {
        private Vector3 _rotation;

        // We store angles as degrees, integer
        // Pitch, yaw, roll
        public Vector3 Rotations
        {
            get { return _rotation; }
            set { _rotation = value - MathC.Floor(value / 360) * 360; } // Range reduce into [0, 360]^3
        }

        public static WadKeyFrameRotation FromTrAngle(ref int index, List<short> frameData, bool isTr1, bool isTr4Or5)
        {
            short rot0 = frameData[index++];
            int frameMode = isTr1 ? 0 : (rot0 & 0xc000);
            switch (frameMode)
            {
                case 0: // Always if TR1
                    int rot1 = frameData[index++];
                    int rotX = ((rot0 & 0x3ff0) >> 4);
                    int rotY = (((rot1 & 0xfc00) >> 10) | ((rot0 & 0xf) << 6) & 0x3ff);
                    int rotZ = ((rot1) & 0x3ff);

                    return new WadKeyFrameRotation
                    {
                        Rotations = new Vector3(-rotX, rotY, -rotZ) * (360.0f / 1024.0f)
                    };
                case 0x4000:
                    if (isTr4Or5)
                        return new WadKeyFrameRotation
                        {
                            Rotations = new Vector3(-(rot0 & 0xfff), 0, 0) * (360.0f / 4096.0f)
                        };
                    else
                        return new WadKeyFrameRotation
                        {
                            Rotations = new Vector3(-(rot0 & 0x3ff), 0, 0) * (360.0f / 1024.0f)
                        };

                case 0x8000:
                    if (isTr4Or5)
                        return new WadKeyFrameRotation
                        {
                            Rotations = new Vector3(0, (rot0 & 0xfff), 0) * (360.0f / 4096.0f)
                        };
                    else
                        return new WadKeyFrameRotation
                        {
                            Rotations = new Vector3(0, (rot0 & 0x3ff), 0) * (360.0f / 1024.0f)
                        };

                case 0xc000:
                    if (isTr4Or5)
                        return new WadKeyFrameRotation
                        {
                            Rotations = new Vector3(0, 0, -(rot0 & 0xfff)) * (360.0f / 4096.0f)
                        };
                    else
                        return new WadKeyFrameRotation
                        {
                            Rotations = new Vector3(0, 0, -(rot0 & 0x3ff)) * (360.0f / 1024.0f)
                        };
                default:
                    throw new InvalidOperationException();
            }
        }

        public static void ToTrAngle(WadKeyFrameRotation angle, List<short> outFrameData, bool isTr1, bool isTr4Or5)
        {
            bool xAbout0 = Math.Abs(angle.Rotations.X) <= (1.0f / 2048);
            bool yAbout0 = Math.Abs(angle.Rotations.Y) <= (1.0f / 2048);
            bool zAbout0 = Math.Abs(angle.Rotations.Z) <= (1.0f / 2048);

            // Handle cases with only 1 axis
            if (!isTr1)
                if (yAbout0 && zAbout0) // Only rotation on first angle
                {
                    int rot;
                    if (isTr4Or5)
                        rot = 0xfff & (int)Math.Round(-angle.Rotations.X * (4096.0f / 360.0f));
                    else
                        rot = 0x3ff & (int)Math.Round(-angle.Rotations.X * (1024.0f / 360.0f));
                    outFrameData.Add((short)(0x4000 | rot));
                    return;
                }
                else if (xAbout0 && zAbout0) // Only rotation on second angle
                {
                    int rot;
                    if (isTr4Or5)
                        rot = 0xfff & (int)Math.Round(angle.Rotations.Y * (4096.0f / 360.0f));
                    else
                        rot = 0x3ff & (int)Math.Round(angle.Rotations.Y * (1024.0f / 360.0f));
                    outFrameData.Add((short)(0x8000 | rot));
                    return;
                }
                else if (xAbout0 && yAbout0) // Only rotation on third angle
                {
                    int rot;
                    if (isTr4Or5)
                        rot = 0xfff & (int)Math.Round(-angle.Rotations.Z * (4096.0f / 360.0f));
                    else
                        rot = 0x3ff & (int)Math.Round(-angle.Rotations.Z * (1024.0f / 360.0f));
                    outFrameData.Add((short)(0xc000 | rot));
                    return;
                }

            // Handle general case
            int rotX = 0x3ff & (int)Math.Round(-angle.Rotations.X * (1024.0f / 360.0f));
            int rotY = 0x3ff & (int)Math.Round(angle.Rotations.Y * (1024.0f / 360.0f));
            int rotZ = 0x3ff & (int)Math.Round(-angle.Rotations.Z * (1024.0f / 360.0f));

            outFrameData.Add((short)((rotX << 4) | ((rotY & 0xfc0) >> 6)));
            outFrameData.Add((short)(((rotY & 0x3f) << 10) | (rotZ & 0x3ff)));
        }

        public Vector3 RotationVectorInRadians => Rotations * ((float)Math.PI / 180.0f);
        public Matrix4x4 RotationMatrix =>
            Matrix4x4.CreateFromYawPitchRoll(
                RotationVectorInRadians.Y,
                RotationVectorInRadians.X,
                RotationVectorInRadians.Z);
    }
}
