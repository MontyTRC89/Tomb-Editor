﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TombLib.Graphics;
using TombLib.Wad;

namespace TombLib.Graphics
{
    public class AnimationNode
    {
        public int Index { get; private set; }
        public WadAnimation WadAnimation { get; set; }
        public Animation DirectXAnimation { get; set; }

        public AnimationNode(WadAnimation wadAnim, Animation dxAnim, int index)
        {
            Index = index;
            WadAnimation = wadAnim;
            DirectXAnimation = dxAnim;
        }
        
        public AnimationNode Clone()
        {
            return new AnimationNode(WadAnimation.Clone(), DirectXAnimation.Clone(), Index);
        }
    }
}
