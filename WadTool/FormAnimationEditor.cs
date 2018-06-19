using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormAnimationEditor : DarkUI.Forms.DarkForm
    {
        private WadMoveableId _moveableId;
        private Wad2 _wad;
        private WadMoveable _moveable;
        private WadToolClass _tool;
        private List<AnimationNode> _workingAnimations;
        private DeviceManager _deviceManager;
        private List<WadBone> _bones;
        private AnimationNode _selectedNode;
        private WadRenderer _renderer;
        private AnimatedModel _model;
        private bool _saved = true;
        private Level _level;

        // Clipboard
        private KeyFrame _clipboardKeyFrame = null;
        private AnimationNode _clipboardNode = null;

        public FormAnimationEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMoveableId id)
        {
            InitializeComponent();

            _renderer = new WadRenderer(deviceManager.Device);
            _tool = tool;
            _moveableId = id;
            _wad = wad;
            _moveable = _wad.Moveables[_moveableId];
            _model = _renderer.GetMoveable(_moveable);
            _deviceManager = deviceManager;

            // Initialize the panel
            var skin = _moveableId;
            if (_moveableId.TypeId == 0)
            {
                if (_wad.SuggestedGameVersion == WadGameVersion.TR4_TRNG && _wad.Moveables.ContainsKey(WadMoveableId.LaraSkin))
                    skin = WadMoveableId.LaraSkin;
                if (_wad.SuggestedGameVersion == WadGameVersion.TR5 && _wad.Moveables.ContainsKey(WadMoveableId.LaraSkin))
                    skin = WadMoveableId.LaraSkin;
            }
            panelRendering.InitializePanel(_tool, _wad, _deviceManager, _moveableId, skin);

            // Get a copy of the skeleton in linearized form
            _bones = _moveable.Skeleton.LinearizedBones.ToList<WadBone>();

            // Load skeleton in combobox
            comboSkeleton.Items.Add("--- Select a mesh ---");
            foreach (var bone in panelRendering.Model.Bones)
                comboSkeleton.Items.Add(bone.Name);
            comboSkeleton.SelectedIndex = 0;

            // NOTE: we work with a pair WadAnimation - Animation. All changes to animation data like name, 
            // framerate, next animation, state changes... will be saved directly to WadAnimation.
            // All changes to keyframes will be instead stored directly in the renderer's Animation class.
            // While saving, WadAnimation and Animation will be combined and original animations will be overwritten.
            _workingAnimations = new List<AnimationNode>();
            foreach (var animation in _moveable.Animations)
                _workingAnimations.Add(new AnimationNode(animation.Clone(), Animation.FromWad2(_bones, animation)));
            ReloadAnimations();

            tool.EditorEventRaised += Tool_EditorEventRaised;
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.AnimationEditorMeshSelectedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorMeshSelectedEvent;
                if (e != null)
                    comboSkeleton.SelectedIndex = e.Model.Meshes.IndexOf(e.Mesh) + 1;
                else
                    comboSkeleton.SelectedIndex = 0;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _renderer.Dispose();
            if (disposing && (components != null))
            {
                components.Dispose();
                _tool.EditorEventRaised -= Tool_EditorEventRaised;
            }
            base.Dispose(disposing);
        }

        private void ReloadAnimations()
        {
            treeAnimations.Nodes.Clear();

            var list = new List<DarkUI.Controls.DarkTreeNode>();
            int index = 0;
            foreach (var animation in _workingAnimations)
            {
                var node = new DarkUI.Controls.DarkTreeNode(index++ + ": " + animation.WadAnimation.Name);
                node.Tag = animation;
                list.Add(node);
            }

            treeAnimations.Nodes.AddRange(list);
        }

        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void treeAnimations_Click(object sender, EventArgs e)
        {
            if (treeAnimations.SelectedNodes.Count == 0)
                return;
            var node = (AnimationNode)treeAnimations.SelectedNodes[0].Tag;
            SelectAnimation(node);
        }

        private void SelectAnimation(AnimationNode node)
        {
            _selectedNode = node;

            tbName.Text = node.WadAnimation.Name;
            tbFramerate.Text = node.WadAnimation.FrameRate.ToString();
            tbNextAnimation.Text = node.WadAnimation.NextAnimation.ToString();
            tbNextFrame.Text = node.WadAnimation.NextFrame.ToString();
            tbStateId.Text = node.WadAnimation.StateId.ToString();
            tbStartVelocity.Text = node.WadAnimation.StartVelocity.ToString();
            tbEndVelocity.Text = node.WadAnimation.EndVelocity.ToString();
            tbLateralStartVelocity.Text = node.WadAnimation.StartLateralVelocity.ToString();
            tbLateralEndVelocity.Text = node.WadAnimation.EndLateralVelocity.ToString();

            // TODO: deprecated stuff
            tbSpeed.Text = (node.WadAnimation.Speed / 65536.0f).ToString();
            tbAccel.Text = (node.WadAnimation.Acceleration / 65536.0f).ToString();
            tbLatSpeed.Text = (node.WadAnimation.LateralSpeed / 65536.0f).ToString();
            tbLatAccel.Text = (node.WadAnimation.LateralAcceleration / 65536.0f).ToString();

            panelRendering.CurrentKeyFrame = 0;
            panelRendering.SelectedMesh = null;
            panelRendering.Animation = node;

            if (node.DirectXAnimation.KeyFrames.Count != 0)
            {
                trackFrames.Visible = true;

                OnKeyframesListChanged();
                SelectFrame(0);
            }
            else
            {
                trackFrames.Visible = false;
                statusFrame.Text = "";
            }

            panelRendering.Invalidate();

        }

        private void trackFrames_ValueChanged(object sender, EventArgs e)
        {
            SelectFrame(trackFrames.Value);
        }

        private void SelectFrame(int frameIndex)
        {
            if (_selectedNode != null)
            {
                var keyFrame = _selectedNode.DirectXAnimation.KeyFrames[frameIndex];
                panelRendering.Model.BuildAnimationPose(keyFrame);
                panelRendering.CurrentKeyFrame = frameIndex;
                panelRendering.Invalidate();

                // Update GUI
                OnKeyframesListChanged();

                tbCollisionBoxMinX.Text = keyFrame.BoundingBox.Minimum.X.ToString();
                tbCollisionBoxMinY.Text = keyFrame.BoundingBox.Minimum.Y.ToString();
                tbCollisionBoxMinZ.Text = keyFrame.BoundingBox.Minimum.Z.ToString();
                tbCollisionBoxMaxX.Text = keyFrame.BoundingBox.Maximum.X.ToString();
                tbCollisionBoxMaxY.Text = keyFrame.BoundingBox.Maximum.Y.ToString();
                tbCollisionBoxMaxZ.Text = keyFrame.BoundingBox.Maximum.Z.ToString();
            }
        }

        private void OnKeyframesListChanged()
        {
            if (_selectedNode != null)
            {
                trackFrames.Minimum = 0;
                trackFrames.Maximum = _selectedNode.DirectXAnimation.KeyFrames.Count - 1;
                statusFrame.Text = "Frame: " + (trackFrames.Value + 1) + "/" + _selectedNode.DirectXAnimation.KeyFrames.Count;
            }
        }

        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(this, "Do you really want to save changes to animations?", "Save changes",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            SaveChanges();

            DialogResult = DialogResult.OK;
            Close();
        }

        private WadAnimation SaveAnimationChanges(AnimationNode animation)
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

        private void SaveChanges()
        {
            // Clear the old animations
            _moveable.Animations.Clear();

            // Combine WadAnimation and Animation classes
            foreach (var animation in _workingAnimations)
                _moveable.Animations.Add(SaveAnimationChanges(animation));

            ReloadAnimations();

            _moveable.Version = DataVersion.GetNext();
            _saved = true;
        }

        private void butCalculateCollisionBox_Click(object sender, EventArgs e)
        {
            CalculateKeyframeBoundingBox(panelRendering.CurrentKeyFrame);
        }

        private void CalculateKeyframeBoundingBox(int index)
        {
            if (_selectedNode != null)
            {
                var keyFrame = _selectedNode.DirectXAnimation.KeyFrames[index];
                keyFrame.CalculateBoundingBox(panelRendering.Model, panelRendering.Skin);

                panelRendering.Invalidate();

                tbCollisionBoxMinX.Text = keyFrame.BoundingBox.Minimum.X.ToString();
                tbCollisionBoxMinY.Text = keyFrame.BoundingBox.Minimum.Y.ToString();
                tbCollisionBoxMinZ.Text = keyFrame.BoundingBox.Minimum.Z.ToString();
                tbCollisionBoxMaxX.Text = keyFrame.BoundingBox.Maximum.X.ToString();
                tbCollisionBoxMaxY.Text = keyFrame.BoundingBox.Maximum.Y.ToString();
                tbCollisionBoxMaxZ.Text = keyFrame.BoundingBox.Maximum.Z.ToString();

                _saved = false;
            }
        }

        private void addNewToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            AddNewAnimation();
        }

        private void butDeleteAnimation_Click(object sender, EventArgs e)
        {
            DeleteAnimation();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteAnimation();
        }

        private void AddNewAnimation()
        {
            var wadAnimation = new WadAnimation();
            wadAnimation.FrameRate = 1;
            wadAnimation.Name = "New Animation " + _workingAnimations.Count;

            var keyFrame = new WadKeyFrame();
            foreach (var bone in _moveable.Skeleton.LinearizedBones)
                keyFrame.Angles.Add(new WadKeyFrameRotation());
            wadAnimation.KeyFrames.Add(keyFrame);

            var dxAnimation = Animation.FromWad2(_moveable.Skeleton.LinearizedBones.ToList(), wadAnimation);
            var node = new AnimationNode(wadAnimation, dxAnimation);
            var treeNode = new DarkUI.Controls.DarkTreeNode(wadAnimation.Name);
            treeNode.Tag = node;

            _workingAnimations.Add(node);
            treeAnimations.Nodes.Add(treeNode);

            _saved = false;
        }

        private void DeleteAnimation()
        {
            if (_selectedNode != null)
            {
                if (DarkMessageBox.Show(this, "Do you really want to delete '" + _selectedNode.WadAnimation.Name + "'?",
                                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int currentIndex = _workingAnimations.IndexOf(_selectedNode);

                    // Update all references
                    for (int i = 0; i < _workingAnimations.Count; i++)
                    {
                        // Ignore the animation I'm deleting
                        if (i == currentIndex)
                            continue;

                        var animation = _workingAnimations[i];

                        // Update NextAnimation
                        if (animation.WadAnimation.NextAnimation > currentIndex)
                            animation.WadAnimation.NextAnimation--;

                        // Update state changes
                        foreach (var stateChange in animation.WadAnimation.StateChanges)
                            foreach (var dispatch in stateChange.Dispatches)
                                if (dispatch.NextAnimation > currentIndex)
                                    dispatch.NextAnimation--;
                    }

                    // Remove the animation
                    _workingAnimations.Remove(_selectedNode);

                    // Update GUI
                    treeAnimations.Nodes.Remove(treeAnimations.SelectedNodes[0]);
                    if (treeAnimations.Nodes.Count != 0)
                        SelectAnimation(treeAnimations.Nodes[0].Tag as AnimationNode);
                    else
                        _selectedNode = null;
                    panelRendering.Invalidate();

                    _saved = false;
                }
            }
        }

        private void InsertMultipleFrames()
        {
            using (var form = new FormInputBox("Add (n) frames", "How many frames do you want to add to current animation?", "1"))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    int framesCount = 0;
                    if (!int.TryParse(form.Result, out framesCount) || framesCount <= 0)
                    {
                        DarkMessageBox.Show(this, "You must insert a number greater than 0",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Add frames
                    for (int i = 0; i < framesCount; i++)
                        AddNewKeyFrame(panelRendering.CurrentKeyFrame + 1);
                }
            }
        }

        private void DeleteFrame()
        {
            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                if (DarkMessageBox.Show(this, "Do you really want to delete frame " + panelRendering.CurrentKeyFrame + "?",
                                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int currentIndex = _workingAnimations.IndexOf(_selectedNode);

                    // Update all references
                    for (int i = 0; i < _workingAnimations.Count; i++)
                    {
                        // Ignore the animation I'm deleting
                        if (i == currentIndex)
                            continue;

                        var animation = _workingAnimations[i];

                        // Update NextAnimation
                        /*  if (animation.WadAnimation.NextFrame > panelRendering.CurrentKeyFrame)
                              animation.WadAnimation.NextFrame--;

                          // Update state changes
                          foreach (var stateChange in animation.WadAnimation.StateChanges)
                              foreach (var dispatch in stateChange.Dispatches)
                                  if (dispatch.NextAnimation > currentIndex)
                                      dispatch.NextAnimation--;*/
                    }

                    // Remove the frame
                    _selectedNode.DirectXAnimation.KeyFrames.RemoveAt(panelRendering.CurrentKeyFrame);

                    // Update GUI
                    OnKeyframesListChanged();
                    if (_selectedNode.DirectXAnimation.KeyFrames.Count != 0)
                        SelectFrame(panelRendering.CurrentKeyFrame);
                    else
                        statusFrame.Text = "";
                    panelRendering.Invalidate();

                    _saved = false;
                }
            }
        }

        private void CutFrame()
        {
            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                _clipboardKeyFrame = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame];
                DeleteFrame();
            }
        }

        private void CopyFrame()
        {
            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                _clipboardKeyFrame = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].Clone();
            }
        }

        private void PasteFrame()
        {
            if (_clipboardKeyFrame != null && _selectedNode != null)
            {
                _selectedNode.DirectXAnimation.KeyFrames.Insert(panelRendering.CurrentKeyFrame, _clipboardKeyFrame);
                _clipboardKeyFrame = null;
                OnKeyframesListChanged();
                SelectFrame(panelRendering.CurrentKeyFrame);
                _saved = false;
            }
        }

        private void ReplaceFrame()
        {
            if (_clipboardKeyFrame != null && _selectedNode != null)
            {
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame] = _clipboardKeyFrame;
                _clipboardKeyFrame = null;
                SelectFrame(panelRendering.CurrentKeyFrame);
                _saved = false;
            }
        }

        private void CutAnimation()
        {
            if (_selectedNode != null)
            {
                _clipboardNode = _selectedNode;
                DeleteAnimation();
            }
        }

        private void CopyAnimation()
        {
            if (_selectedNode != null)
            {
                _clipboardNode =_selectedNode.Clone();
            }
        }

        private void PasteAnimation()
        {
            if (_clipboardNode != null && _selectedNode != null)
            {
                int animationIndex = _workingAnimations.IndexOf(_selectedNode) + 1;
                _workingAnimations.Insert(animationIndex, _clipboardNode);
                _clipboardNode.DirectXAnimation.Name += " - Copy";
                _clipboardNode.WadAnimation.Name += " - Copy";
                _clipboardNode = null;
                ReloadAnimations();
                SelectAnimation(_workingAnimations[animationIndex]);
                _saved = false;
            }
        }

        private void ReplaceAnimation()
        {
            if (_clipboardNode != null && _selectedNode != null)
            {
                int animationIndex = _workingAnimations.IndexOf(_selectedNode);
                _workingAnimations[animationIndex] = _clipboardNode;
                _clipboardNode.DirectXAnimation.Name += " - Copy";
                _clipboardNode.WadAnimation.Name += " - Copy";
                _clipboardNode = null;
                ReloadAnimations();
                SelectAnimation(_workingAnimations[animationIndex]);
                _saved = false;
            }
        }

        private void SplitAnimation()
        {
            if (_selectedNode != null)
            {
                // I need at least 1 frame in the middle
                if (_selectedNode.DirectXAnimation.KeyFrames.Count < 3)
                {
                    DarkMessageBox.Show(this, "You must have at least 3 frames for splitting the animation", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if we have selected the first or the last frame
                int numFrames = _selectedNode.DirectXAnimation.KeyFrames.Count;
                if (panelRendering.CurrentKeyFrame == 0 || panelRendering.CurrentKeyFrame == numFrames - 1)
                {
                    DarkMessageBox.Show(this, "You can't set the first or the last frame for splitting the animation", "Error",
                                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                var newWadAnimation = _selectedNode.WadAnimation.Clone();
                var newDirectXAnimation = _selectedNode.DirectXAnimation.Clone();

                int numFrames1 = panelRendering.CurrentKeyFrame;
                int numFrames2 = numFrames - numFrames1;

                // Remove frames from the two animations
                _selectedNode.DirectXAnimation.KeyFrames.RemoveRange(panelRendering.CurrentKeyFrame + 1, numFrames2 - 1);
                newDirectXAnimation.KeyFrames.RemoveRange(0, panelRendering.CurrentKeyFrame);

                // Add the new animation at the bottom of the list
                newWadAnimation.Name += " - splitted";
                _workingAnimations.Add(new AnimationNode(newWadAnimation, newDirectXAnimation));

                // Update the GUI
                ReloadAnimations();
                SelectAnimation(_selectedNode);

                _saved = false;
            }
        }

        private void drawGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawGrid = !panelRendering.DrawGrid;
            panelRendering.Invalidate();
        }

        private void drawGizmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawGizmo = !panelRendering.DrawGizmo;
            panelRendering.Invalidate();
        }

        private void drawCollisionBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawCollisionBox = !panelRendering.DrawCollisionBox;
            panelRendering.Invalidate();
        }

        private void insertFrameAfterCurrentOneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewKeyFrame(panelRendering.CurrentKeyFrame + 1);
        }

        private void AddNewKeyFrame(int index)
        {
            if (_selectedNode != null)
            {
                var keyFrame = new KeyFrame();
                foreach (var bone in _moveable.Skeleton.LinearizedBones)
                {
                    keyFrame.Rotations.Add(Vector3.Zero);
                    // keyFrame.RotationsMatrices.Add(Matrix4x4.CreateFromYawPitchRoll(0, 0, 0));
                    keyFrame.Quaternions.Add(Quaternion.Identity);
                    keyFrame.Translations.Add(bone.Translation);
                    keyFrame.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(bone.Translation));
                }

                if (_selectedNode.DirectXAnimation.KeyFrames.Count == 0)
                    index = 0;

                _selectedNode.DirectXAnimation.KeyFrames.Insert(index, keyFrame);
                OnKeyframesListChanged();
                _saved = false;
            }
        }

        private void tbName_Validated(object sender, EventArgs e)
        {
            if (_selectedNode != null && tbName.Text.Trim() != "")
            {
                _selectedNode.WadAnimation.Name = tbName.Text.Trim();
                treeAnimations.SelectedNodes[0].Text = treeAnimations.SelectedNodes[0].VisibleIndex + ": " + _selectedNode.WadAnimation.Name;
                _saved = false;
            }
        }

        private void tbFramerate_Validated(object sender, EventArgs e)
        {
            byte result = 0;
            if (!byte.TryParse(tbFramerate.Text, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.FrameRate = result;
                _saved = false;
            }
        }

        private void tbNextAnimation_Validated(object sender, EventArgs e)
        {
            ushort result = 0;
            if (!ushort.TryParse(tbNextAnimation.Text, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.NextAnimation = result;
                _saved = false;
            }
        }

        private void tbNextFrame_Validated(object sender, EventArgs e)
        {
            ushort result = 0;
            if (!ushort.TryParse(tbNextFrame.Text, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.NextFrame = result;
                _saved = false;
            }
        }

        private void tbStateId_Validated(object sender, EventArgs e)
        {
            ushort result = 0;
            if (!ushort.TryParse(tbStateId.Text, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.StateId = result;
                _saved = false;
            }
        }

        private void butDeleteFrame_Click(object sender, EventArgs e)
        {
            DeleteFrame();
        }

        private void tbCollisionBoxMinX_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinX.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Minimum = new Vector3(result, bb.Minimum.Y, bb.Minimum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                _saved = false;
            }
        }

        private void tbCollisionBoxMinY_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinY.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Minimum = new Vector3(bb.Minimum.X, result, bb.Minimum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                _saved = false;
            }
        }

        private void tbCollisionBoxMinZ_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinZ.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Minimum = new Vector3(bb.Minimum.X, bb.Minimum.Y, result);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                _saved = false;
            }
        }

        private void tbCollisionBoxMaxX_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxX.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Maximum = new Vector3(result, bb.Maximum.Y, bb.Maximum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                _saved = false;
            }
        }

        private void tbCollisionBoxMaxY_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxY.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Maximum = new Vector3(bb.Maximum.X, result, bb.Maximum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                _saved = false;
            }
        }

        private void tbCollisionBoxMaxZ_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxZ.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Maximum = new Vector3(bb.Maximum.X, bb.Maximum.Y, result);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
                _saved = false;
            }
        }

        private void deleteFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteFrame();
        }

        private void comboSkeleton_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboSkeleton.SelectedIndex < 1)
                return;
            panelRendering.SelectedMesh = panelRendering.Model.Meshes[comboSkeleton.SelectedIndex - 1];
            panelRendering.Invalidate();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CutFrame();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CopyFrame();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteFrame();
        }

        private void pasteReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReplaceFrame();
        }

        private void calculateCollisionBoxForCurrentFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CalculateKeyframeBoundingBox(panelRendering.CurrentKeyFrame);
        }

        private void calculateBoundingBoxForAllFramesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedNode != null)
            {
                int startFrame = panelRendering.CurrentKeyFrame;
                for (int i = 0; i < _selectedNode.DirectXAnimation.KeyFrames.Count; i++)
                {
                    SelectFrame(i);
                    CalculateKeyframeBoundingBox(i);
                }
                SelectFrame(startFrame);
            }
        }

        private void butTbAddAnimation_Click(object sender, EventArgs e)
        {
            AddNewAnimation();
        }

        private void butTbDeleteAnimation_Click(object sender, EventArgs e)
        {
            DeleteAnimation();
        }

        private void butTbAddFrame_Click(object sender, EventArgs e)
        {
            AddNewKeyFrame(panelRendering.CurrentKeyFrame + 1);
        }

        private void butTbDeleteFrame_Click(object sender, EventArgs e)
        {
            DeleteFrame();
        }

        private void butTbCutFrame_Click(object sender, EventArgs e)
        {
            CutFrame();
        }

        private void butTbCopyFrame_Click(object sender, EventArgs e)
        {
            CopyFrame();
        }

        private void butTbPasteFrame_Click(object sender, EventArgs e)
        {
            PasteFrame();
        }

        private void insertnFramesAfterCurrentOneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InsertMultipleFrames();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!_saved)
            {
                var result = DarkMessageBox.Show(this, "Do you have unsaved changes. Do you want to save changes to animations?",
                                                 "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                    SaveChanges();
                else if (result == DialogResult.Cancel)
                    return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void curToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CutAnimation();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CopyAnimation();
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            PasteAnimation();
        }

        private void pasteReplaceToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ReplaceAnimation();
        }

        private void butTbCutAnimation_Click(object sender, EventArgs e)
        {
            CutAnimation();
        }

        private void butTbCopyAnimation_Click(object sender, EventArgs e)
        {
            CopyAnimation();
        }

        private void butTbPasteAnimation_Click(object sender, EventArgs e)
        {
            PasteAnimation();
        }

        private void tbSpeed_Validated(object sender, EventArgs e)
        {
            float result = 0;
            string toParse = tbSpeed.Text.Replace(",", ".");
            if (!float.TryParse(toParse, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.Speed = (int)Math.Round(result * 65536.0f, 0);
                _saved = false;
            }
        }

        private void tbAccel_Validated(object sender, EventArgs e)
        {
            float result = 0;
            string toParse = tbAccel.Text.Replace(",", ".");
            if (!float.TryParse(toParse, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.Acceleration = (int)Math.Round(result * 65536.0f, 0);
                _saved = false;
            }
        }

        private void tbLatSpeed_Validated(object sender, EventArgs e)
        {
            float result = 0;
            string toParse = tbLatSpeed.Text.Replace(",", ".");
            if (!float.TryParse(toParse, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.LateralSpeed = (int)Math.Round(result * 65536.0f, 0);
                _saved = false;
            }
        }

        private void tbLatAccel_TextChanged(object sender, EventArgs e)
        {
            float result = 0;
            string toParse = tbLatAccel.Text.Replace(",", ".");
            if (!float.TryParse(toParse, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.LateralAcceleration = (int)Math.Round(result * 65536.0f, 0);
                _saved = false;
            }
        }

        private void interpolateFramesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelInterpolate.Visible = !panelInterpolate.Visible;
        }

        private void butInterpolateSetCurrent1_Click(object sender, EventArgs e)
        {
            tbInterpolateFrame1.Text = panelRendering.CurrentKeyFrame.ToString();
        }

        private void butInterpolateSetCurrent2_Click(object sender, EventArgs e)
        {
            tbInterpolateFrame2.Text = panelRendering.CurrentKeyFrame.ToString();
        }

        private void butInterpolateFrames_Click(object sender, EventArgs e)
        {
            // Check for correct input by the designer
            int numFrames = 0;
            if (!int.TryParse(tbInterpolateNumFrames.Text, out numFrames) || numFrames <= 0)
            {
                DarkMessageBox.Show(this, "You must insert a valid value for frames count", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int frameIndex1 = int.Parse(tbInterpolateFrame1.Text);
            int frameIndex2 = int.Parse(tbInterpolateFrame2.Text);

            if (frameIndex1 >= frameIndex2)
            {
                DarkMessageBox.Show(this, "The first frame index can't be greater than the second frame index", "Error",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var frame1 = _selectedNode.DirectXAnimation.KeyFrames[frameIndex1];

            // Now calculate how many frames I must insert
            int numFramesToAdd = numFrames - (frameIndex2 - frameIndex1) + 1;
            for (int i = 0; i < numFramesToAdd; i++)
            {
                var keyFrame = new KeyFrame();
                foreach (var bone in _moveable.Skeleton.LinearizedBones)
                {
                    keyFrame.Rotations.Add(Vector3.Zero);
                    keyFrame.Quaternions.Add(Quaternion.Identity);
                    keyFrame.Translations.Add(frame1.Translations[0] + bone.Translation);
                    keyFrame.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(frame1.Translations[0] + bone.Translation));
                }

                frameIndex2++;
                _selectedNode.DirectXAnimation.KeyFrames.Insert(frameIndex1 + 1 + i, keyFrame);
            }

            OnKeyframesListChanged();
            _saved = false;

            var frame2 = _selectedNode.DirectXAnimation.KeyFrames[frameIndex2];

            // Slerp factor
            float k = 1.0f / (numFrames + 1);

            // Now I have the right number of frames and I can do slerp
            for (int i = 0; i < numFrames; i++)
            {
                var keyframe = _selectedNode.DirectXAnimation.KeyFrames[frameIndex1 + i + 1];

                // Lerp translation of root bone
                keyframe.Translations[0] = Vector3.Lerp(frame1.Translations[0], frame2.Translations[0], k * (i + 1));
                keyframe.TranslationsMatrices[0] = Matrix4x4.CreateTranslation(keyframe.Translations[0]);

                // Slerp of quaternions
                for (int j = 0; j < keyframe.Quaternions.Count; j++)
                {
                    keyframe.Quaternions[j] = Quaternion.Slerp(frame1.Quaternions[j], frame2.Quaternions[j], k * (i + 1));
                    keyframe.Rotations[j] = MathC.QuaternionToEuler(keyframe.Quaternions[j]);
                }
            }

            // All done! Now I reset a bit the GUI
            SelectFrame(panelRendering.CurrentKeyFrame);

            _saved = false;
        }

        private void butEditStateChanges_Click(object sender, EventArgs e)
        {
            if (_selectedNode != null)
            {
                using (var form = new FormStateChangesEditor(_selectedNode.WadAnimation.StateChanges))
                {
                    if (form.ShowDialog() != DialogResult.OK)
                        return;

                    // Add the new state changes
                    _selectedNode.WadAnimation.StateChanges.Clear();
                    _selectedNode.WadAnimation.StateChanges.AddRange(form.StateChanges);

                    _saved = false;
                }
            }
        }

        private void timerPlayAnimation_Tick(object sender, EventArgs e)
        {
            /*if (_playAnimation)
            {
                // Get selected moveable
                var wad = _tool.GetWad(_tool.MainSelection.Value.WadArea);
                var moveableId = (WadMoveableId)_tool.MainSelection.Value.Id;
                var moveable = wad.Moveables[moveableId];
                if (moveable == null || moveable.Animations.Count == 0)
                {
                    _playAnimation = false;
                    return;
                }

                // Get selected animation
                if (treeAnimations.SelectedNodes.Count == 0)
                    return;
                var node = treeAnimations.SelectedNodes[0];
                var animationIndex = (int)node.Tag;
                if (animationIndex >= moveable.Animations.Count)
                {
                    _playAnimation = false;
                    return;
                }
                var animation = moveable.Animations[animationIndex];

                // Update animation
                if (panel3D.KeyFrameIndex >= animation.RealNumberOfFrames)
                    panel3D.KeyFrameIndex = 0;
                else
                    panelRendering.KeyFrameIndex++;
                panel3D.Draw();
            }*/
        }

        private void StopAnimation()
        {

        }

        private void tbStartVelocity_Validated(object sender, EventArgs e)
        {
            float result = 0;
            string toParse = tbStartVelocity.Text.Replace(",", ".");
            if (!float.TryParse(toParse, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.StartVelocity = result;
                _saved = false;
            }
        }

        private void tbEndVelocity_Validated(object sender, EventArgs e)
        {
            float result = 0;
            string toParse = tbEndVelocity.Text.Replace(",", ".");
            if (!float.TryParse(toParse, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.EndVelocity = result;
                _saved = false;
            }
        }

        private void tbLateralStartVelocity_Validated(object sender, EventArgs e)
        {
            float result = 0;
            string toParse = tbLateralStartVelocity.Text.Replace(",", ".");
            if (!float.TryParse(toParse, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.StartLateralVelocity = result;
                _saved = false;
            }
        }

        private void tbLateralEndVelocity_Validated(object sender, EventArgs e)
        {
            float result = 0;
            string toParse = tbLateralEndVelocity.Text.Replace(",", ".");
            if (!float.TryParse(toParse, out result))
                return;

            if (_selectedNode != null)
            {
                _selectedNode.WadAnimation.EndLateralVelocity = result;
                _saved = false;
            }
        }

        private void butTbReplaceAnimation_Click(object sender, EventArgs e)
        {
            ReplaceAnimation();
        }

        private void butTbReplaceFrame_Click(object sender, EventArgs e)
        {
            ReplaceFrame();
        }

        private void splitAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SplitAnimation();
        }

        private void butTbSplitAnimation_Click(object sender, EventArgs e)
        {
            SplitAnimation();
        }

        private void butEditAnimCommands_Click(object sender, EventArgs e)
        {
            if (_selectedNode != null)
            {
                using (var form = new FormAnimCommandsEditor(_tool, _selectedNode.WadAnimation.AnimCommands))
                {
                    if (form.ShowDialog() != DialogResult.OK)
                        return;

                    // Add the new state changes
                    _selectedNode.WadAnimation.AnimCommands.Clear();
                    _selectedNode.WadAnimation.AnimCommands.AddRange(form.AnimCommands);

                    _saved = false;
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedNode == null)
                return;

            if (saveFileDialogExport.ShowDialog() == DialogResult.Cancel)
                return;

            var animationToSave = SaveAnimationChanges(_selectedNode);

            if (!WadActions.ExportAnimation(_moveable, animationToSave, saveFileDialogExport.FileName))
            {
                DarkMessageBox.Show(this, "Can't export current animation to XML file",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedNode == null)
                return;

            if (openFileDialogImport.ShowDialog() == DialogResult.Cancel)
                return;

            var animation = WadActions.ImportAnimationFromXml(_wad, openFileDialogImport.FileName);
            if (animation == null)
            {
                DarkMessageBox.Show(this, "Can't import a valid animation",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (animation.KeyFrames[0].Angles.Count != _bones.Count)
            {
                DarkMessageBox.Show(this, "You can only import an animation with the same number of bones of the current moveable",
                                   "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (DarkMessageBox.Show(this, "Do you want to overwrite current animation?", "Import animation",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _selectedNode.WadAnimation = animation;
                _selectedNode.DirectXAnimation = Animation.FromWad2(_bones, animation);
                SelectAnimation(_selectedNode);
                SelectFrame(0);
                panelRendering.Invalidate();
            }
        }

        private void loadPrj2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _level = Prj2Loader.LoadFromPrj2("testwall.prj2", null, new Prj2Loader.Settings { IgnoreWads = true });
            panelRendering.Level = _level;

            // Load rooms into the combo box
            comboRooms.Enabled = true;
            comboRooms.Items.Clear();
            comboRooms.Items.Add("--- Select room ---");
            foreach (var room in _level.Rooms)
                if (room != null)
                    comboRooms.Items.Add(room);
            
            // Prepare the texture atlas
            panelRendering.RebuildRoomsTextureAtlas();

            panelRendering.Invalidate();
        }

        private void comboRooms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboRooms.SelectedIndex == 0)
            {
                panelRendering.Room = null;
                panelRendering.RoomPosition = Vector3.Zero;
            }
            else
            {
                panelRendering.Room = (Room)comboRooms.SelectedItem;
                panelRendering.RoomPosition = panelRendering.Room.GetLocalCenter();
            }

            panelRendering.Invalidate();
        }
    }
}
