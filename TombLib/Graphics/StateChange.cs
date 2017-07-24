using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Graphics
{
    public class StateChange
    {
        public short StateId;
        public List<AnimDispatch> AnimDispatches;

        public StateChange()
        {
            AnimDispatches = new List<AnimDispatch>();
        }
    }
}
