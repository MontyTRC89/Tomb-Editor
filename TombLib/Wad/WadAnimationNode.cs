using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Graphics;
using TombLib.Wad;

namespace TombLib.Wad
{ 
    [Serializable]
    public class WadAnimationNode
    {
        public WadAnimation WadAnimation { get; set; }
        public Animation DirectXAnimation { get; set; }

        public WadAnimationNode(WadAnimation wadAnim, Animation dxAnim)
        {
            WadAnimation = wadAnim;
            DirectXAnimation = dxAnim;
        }
    }
}
