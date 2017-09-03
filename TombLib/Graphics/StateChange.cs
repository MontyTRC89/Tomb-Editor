using System.Collections.Generic;

namespace TombLib.Graphics
{
    public class StateChange
    {
        public short StateId;
        public List<AnimDispatch> AnimDispatches = new List<AnimDispatch>();
    }
}
