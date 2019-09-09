using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;

namespace WadTool
{
    public abstract class AnimationEditorUndoRedoInstance : UndoRedoInstance
    {
        public AnimationEditor Parent { get; internal set; }
        protected int AnimCount;

        protected AnimationEditorUndoRedoInstance(AnimationEditor parent) { Parent = parent; AnimCount = Parent.WorkingAnimations.Count; }
    }

    public class AnimationUndoInstance : AnimationEditorUndoRedoInstance
    {
        private AnimationNode Animation;

        public AnimationUndoInstance(AnimationEditor parent, AnimationNode anim) : base(parent)
        {
            Animation = anim.Clone();

            Valid = () => Parent.WorkingAnimations.Count == AnimCount &&
                          Animation.DirectXAnimation != null &&
                          Animation.WadAnimation != null && 
                          Animation.Index >= 0;

            UndoAction = () =>
            {
                Parent.WorkingAnimations[Animation.Index] = Animation;
                Parent.Tool.AnimationEditorAnimationChanged(Animation);
            };

            RedoInstance = () => new AnimationUndoInstance(Parent, Parent.WorkingAnimations[Animation.Index]);
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

        public void PushAnimationChanged(AnimationEditor editor, AnimationNode anim) => Push(new AnimationUndoInstance(editor, anim));
    }
}
