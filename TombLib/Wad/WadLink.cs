using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{
    public struct WadLink
    {
        public WadLinkOpcode Opcode { get; }
        public Vector3 Offset { get; set; }

        public WadLink(WadLinkOpcode opcode, Vector3 offset)
        {
            Opcode = opcode;
            Offset = offset;
        }
    }
}
