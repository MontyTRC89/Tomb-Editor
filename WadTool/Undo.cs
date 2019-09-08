using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using TombLib;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public abstract class WadToolUndoRedoInstance : UndoRedoInstance
    {
        public WadToolUndoManager Parent;

        public Room Room { get; internal set; }
        protected WadToolUndoRedoInstance(WadToolUndoManager parent) { Parent = parent; }
    }

    public class AnimationUndoInstance : WadToolUndoRedoInstance
    {
        private AnimationNode Animation;
        private AnimationNode OriginalAnimation;

        public AnimationUndoInstance(WadToolUndoManager parent, AnimationNode anim) : base(parent)
        {
            OriginalAnimation = anim;
            Animation = anim.Clone();

            Valid = () => Animation.DirectXAnimation != null && Animation.WadAnimation != null && Animation.Index >= 0;

            UndoAction = () =>
            {
                OriginalAnimation = Animation;
                Parent.Tool.AnimationEditorRequestAnimationChange(Animation);
            };

            RedoInstance = () => new AnimationUndoInstance(Parent, OriginalAnimation);
        }
    }

    public class WadToolUndoManager : UndoManager
    {
        public WadToolClass Tool;

        public WadToolUndoManager(WadToolClass tool, int undoDepth) : base(undoDepth)
        {
            Tool = tool;

            UndoStackChanged += (s, e) => Tool.UndoStackChanged();
            MessageSent += (s, e) => Tool.SendMessage(e.Message, TombLib.Forms.PopupType.Warning);
        }

        public void PushAnimationChanged(AnimationNode anim) => Push(new AnimationUndoInstance(this, anim));
    }
}
