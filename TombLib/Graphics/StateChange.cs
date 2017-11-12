using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombLib.Graphics
{
    public class StateChange
    {
        public short StateId { get; set; }
        public List<AnimDispatch> AnimDispatches { get; set; } = new List<AnimDispatch>();
    }
}
