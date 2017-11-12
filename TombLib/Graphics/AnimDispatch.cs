using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Graphics
{
    public class AnimDispatch
    {
        public short LowFrame { get; set; }
        public short HighFrame { get; set; }
        public short NextAnimation { get; set; }
        public short NextFrame { get; set; }
    }
}
