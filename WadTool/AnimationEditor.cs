using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public enum AnimTransformMode
    {
        Simple,     // Brutally overwrite all properties for all frames
        Linear,     // Do simple linear interpolation, a-la WadMerger
        Smooth,     // Smootherstep interpolation
        Symmetric   // Nifty symmetric smoothstep interpolation
    }

    public class AnimationEditor
    {
        public List<AnimationNode> Animations;
        public AnimationNode CurrentAnim;
        public int CurrentFrameIndex;
        public VectorInt2 Selection;

        public WadToolClass Tool { get; private set; }
        public Wad2 Wad { get; private set; }
        public WadMoveable Moveable { get; private set; }

        // Clipboard
        public List<KeyFrame> ClipboardKeyFrames = new List<KeyFrame>();
        public AnimationNode ClipboardNode = null;

        // General state
        public bool Saved;
        public bool MadeChanges = false;

        // Editing state
        public AnimTransformMode TransformMode;

        // Internal state, used for transform functions
        private List<Vector3> _backupRot = new List<Vector3>();
        private List<Vector3> _backupPos = new List<Vector3>();
        private Vector3 _initialPos;
        private Vector3 _initialRot;

        // Helpers
        public bool SelectionIsEmpty => Selection.X == -1 || Selection.Y == -1;
        public bool ValidAnimationAndFrames => CurrentAnim != null && CurrentAnim.DirectXAnimation.KeyFrames.Count > 0;
        public KeyFrame CurrentKeyFrame
        {
            get
            {
                if (CurrentAnim == null || CurrentAnim.DirectXAnimation.KeyFrames.Count <= 0)
                    return null;

                if (CurrentFrameIndex >= CurrentAnim.DirectXAnimation.KeyFrames.Count)
                    CurrentFrameIndex = 0;
                return CurrentAnim.DirectXAnimation.KeyFrames[CurrentFrameIndex];
            }
        }

        public List<KeyFrame> ActiveFrames
        {
            get
            {
                if (SelectionIsEmpty)
                    return new List<KeyFrame>() { CurrentKeyFrame };
                else
                    return CurrentAnim.DirectXAnimation.KeyFrames.GetRange(Selection.X, Selection.Y - Selection.X + 1);
            }
        }

        public AnimationEditor(WadToolClass tool, Wad2 wad, WadMoveableId idToEdit)
        {
            Tool = tool;
            Wad = wad;
            Moveable = Wad.Moveables.ContainsKey(idToEdit) ? Wad.Moveables[idToEdit] : null;

            // NOTE: we work with a pair WadAnimation - Animation. All changes to animation data like name,
            // framerate, next animation, state changes... will be saved directly to WadAnimation.
            // All changes to keyframes will be instead stored directly in the renderer's Animation class.
            // While saving, WadAnimation and Animation will be combined and original animations will be overwritten.

            Animations = new List<AnimationNode>();
            for (int i = 0; i < Moveable.Animations.Count; i++)
            {
                var animation = Moveable.Animations[i].Clone(); ;
                Animations.Add(new AnimationNode(animation, Animation.FromWad2(Moveable.Bones, animation), i));
            }
        }

        public WadAnimation GetSavedAnimation(AnimationNode animation)
        {
            var wadAnim = animation.WadAnimation.Clone();
            var directxAnim = animation.DirectXAnimation;

            // I need only to convert DX keyframes to Wad2 keyframes
            wadAnim.KeyFrames.Clear();
            foreach (var directxKeyframe in directxAnim.KeyFrames)
            {
                var keyframe = new WadKeyFrame();

                // Create the new bounding box
                keyframe.BoundingBox = new TombLib.BoundingBox(directxKeyframe.BoundingBox.Minimum,
                                                               directxKeyframe.BoundingBox.Maximum);

                // For now we take the first translation as offset
                keyframe.Offset = new System.Numerics.Vector3(directxKeyframe.Translations[0].X,
                                                              directxKeyframe.Translations[0].Y,
                                                              directxKeyframe.Translations[0].Z);

                // Convert angles from radians to degrees and save them
                foreach (var rot in directxKeyframe.Rotations)
                {
                    var angle = new WadKeyFrameRotation();
                    angle.Rotations = new System.Numerics.Vector3(rot.X * 180.0f / (float)Math.PI,
                                                                  rot.Y * 180.0f / (float)Math.PI,
                                                                  rot.Z * 180.0f / (float)Math.PI);
                    keyframe.Angles.Add(angle);
                }

                wadAnim.KeyFrames.Add(keyframe);
            }

            return wadAnim;
        }

        public bool SaveChanges(IWin32Window owner = null)  // No owner - no confirmation!
        {
            if (owner != null && DarkMessageBox.Show(owner, "Do you really want to save changes to animations?", "Save changes",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return false;

            // Clear the old animations
            Moveable.Animations.Clear();

            // Combine WadAnimation and Animation classes
            foreach (var animation in Animations)
                Moveable.Animations.Add(GetSavedAnimation(animation));

            Moveable.Version = DataVersion.GetNext();
            return true;
        }

        public void UpdateTransform(int meshIndex, Vector3 newRot, Vector3 newPos)
        {
            // Backup everything and push undo on first occurence of editing
            if (!MadeChanges)
            {
                _initialPos = CurrentKeyFrame.Translations[0];
                _initialRot = CurrentKeyFrame.Rotations[meshIndex];

                _backupPos.Clear();
                _backupRot.Clear();

                ActiveFrames.ForEach(f => { _backupRot.Add(f.Rotations[meshIndex]); _backupPos.Add(f.Translations[0]); });

                Tool.UndoManager.PushAnimationChanged(this, CurrentAnim);
                MadeChanges = true;
            }

            // Calculate deltas for other frames processing
            var deltaPos = newPos - _initialPos;
            var deltaRot = newRot - _initialRot;

            // Define animation properties
            bool evolve = TransformMode != AnimTransformMode.Simple && ActiveFrames.Count > 1;
            bool smooth = TransformMode != AnimTransformMode.Linear && evolve;
            bool loop   = TransformMode == AnimTransformMode.Symmetric;

            // Calculate evolution
            float currentStep = 0;
            float frameCount = loop ? Selection.Y - Selection.X : CurrentFrameIndex - Selection.X;

            int index = 0;
            foreach (var keyframe in ActiveFrames)
            {
                float midFrame = loop ? CurrentFrameIndex - Selection.X : frameCount;
                float bias = (currentStep <= midFrame) ? currentStep / midFrame : (frameCount - currentStep) / (frameCount - midFrame);

                // Single-pass smoothstep doesn't look organic on fast animations, hence we're using 2-pass smootherstep here.
                float weight = smooth ? (float)MathC.SmoothStep(0, 1, MathC.SmoothStep(0, 1, bias)) : bias;

                // Apply deltas to backed-up transforms
                var currPos = _backupPos[index] + deltaPos;
                var currRot = _backupRot[index] + deltaRot;

                var translationVector = currPos;

                if (evolve)
                    translationVector = Vector3.Lerp(_backupPos[index], translationVector, weight);

                // Foolproof stuff in case user hardly messes with transform during playback...
                if (float.IsNaN(translationVector.X)) translationVector.X = 0;
                if (float.IsNaN(translationVector.Y)) translationVector.Y = 0;
                if (float.IsNaN(translationVector.Z)) translationVector.Z = 0;

                keyframe.Translations[0] = translationVector;
                keyframe.TranslationsMatrices[0] = Matrix4x4.CreateTranslation(translationVector);

                var rotVector = currRot;
                Quaternion finalQuat;
                if (evolve)
                {
                    // Calculate source and destination quats and decide on direction based on dot product.
                    var srcQuat = Quaternion.CreateFromYawPitchRoll(_backupRot[index].Y, _backupRot[index].X, _backupRot[index].Z);
                    var destQuat = Quaternion.CreateFromYawPitchRoll(currRot.Y, currRot.X, currRot.Z);
                    if (Quaternion.Dot(srcQuat, destQuat) < 0) Quaternion.Negate(srcQuat);
                    finalQuat = Quaternion.Lerp(srcQuat, destQuat, weight);
                }
                else
                    finalQuat = Quaternion.CreateFromYawPitchRoll(rotVector.Y, rotVector.X, rotVector.Z);

                // We're not converting quat to rotations because we have to check-up for NaNs
                var rotationVector = MathC.QuaternionToEuler(finalQuat);

                // Foolproof stuff in case user hardly messes with transform during playback...
                if (float.IsNaN(rotationVector.X)) rotationVector.X = 0;
                if (float.IsNaN(rotationVector.Y)) rotationVector.Y = 0;
                if (float.IsNaN(rotationVector.Z)) rotationVector.Z = 0;

                // NaNs filtered out, now we can put actual data.
                keyframe.Quaternions[meshIndex] = Quaternion.CreateFromYawPitchRoll(rotationVector.Y, rotationVector.X, rotationVector.Z);
                keyframe.Rotations[meshIndex] = rotationVector;

                index++;
                currentStep++; if (currentStep > frameCount) currentStep = frameCount;
            }
        }

        public void ReplaceAnimCommands(WadAnimCommand oldCommand, WadAnimCommand newCommand) => 
            Animations.ForEach(a => a.WadAnimation.AnimCommands.ForEach(ac => { if (ac == oldCommand) ac = newCommand.Clone(); }));
    }
}
