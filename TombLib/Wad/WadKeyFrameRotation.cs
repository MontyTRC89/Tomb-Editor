using System.Runtime.InteropServices;

namespace TombLib.Wad
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class WadKeyFrameRotation
    {
        public WadKeyFrameRotationAxis Axis;
        public double X;
        public double Y;
        public double Z;
    }
}
