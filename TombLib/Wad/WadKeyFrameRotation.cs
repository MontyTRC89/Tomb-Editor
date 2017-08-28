using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

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
