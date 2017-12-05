using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TombLib
{
    public struct Space2
    {
        public Vector2 Start;
        public Vector2 End;

        public Space2(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }
    }
}
