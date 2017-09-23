using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Wad
{
    public class WadAnimDispatch
    {
        public ushort InFrame { get; set; }
        public ushort OutFrame { get; set; }
        public ushort NextAnimation { get; set; }
        public ushort NextFrame { get; set; }

        public WadAnimDispatch Clone()
        {
            var dispatch = new WadAnimDispatch();

            dispatch.InFrame = InFrame;
            dispatch.OutFrame = OutFrame;
            dispatch.NextAnimation = NextAnimation;
            dispatch.NextFrame = NextFrame;

            return dispatch;
        }
    }
}
