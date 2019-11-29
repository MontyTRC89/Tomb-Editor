using TombLib.Wad;

namespace TombLib.Graphics
{
    public class AnimationNode
    {
        public int Index { get; set; }
        public WadAnimation WadAnimation { get; set; }
        public Animation DirectXAnimation { get; set; }

        public AnimationNode(WadAnimation wadAnim, Animation dxAnim, int index)
        {
            Index = index;
            WadAnimation = wadAnim;
            DirectXAnimation = dxAnim;
        }
        
        public AnimationNode Clone(int index = -1)
        {
            return new AnimationNode(WadAnimation.Clone(), DirectXAnimation.Clone(), index == -1 ? Index : index);
        }
    }
}
