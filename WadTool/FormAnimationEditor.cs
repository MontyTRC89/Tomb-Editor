using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Graphics;
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

            // Load animations
            _workingAnimations = new List<AnimationNode>();
            foreach (var animation in _moveable.Animations)
                _workingAnimations.Add(new AnimationNode(animation.Clone(), Animation.FromWad2(_bones, animation)));
            ReloadAnimations();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _renderer.Dispose();
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void ReloadAnimations()
        {
            treeAnimations.Nodes.Clear();

            var list = new List<DarkUI.Controls.DarkTreeNode>();
            foreach (var animation in _workingAnimations)
            {
                var node = new DarkUI.Controls.DarkTreeNode(animation.WadAnimation.Name);
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
            if (node.WadAnimation.KeyFrames.Count != 0)
            {
                trackFrames.Visible = true;

                trackFrames.Minimum = 0;
                trackFrames.Maximum = node.DirectXAnimation.KeyFrames.Count - 1;
                SelectFrame(0);
                // Load animation commands
                /*foreach (var cmd in node.WadAnimation.AnimCommands)
                    if (cmd.Type == WadAnimCommandType.PlaySound || cmd.Type == WadAnimCommandType.FlipEffect)
                        trackFrames.AnimationCommands.Add(cmd);
                trackFrames.UpdateAnimationCommands();*/
            }
            else
            {
                trackFrames.Visible = false;
            }
            panelRendering.Animation = node;
            panelRendering.CurrentKeyFrame = 0;
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
                _model.BuildAnimationPose(keyFrame);
                panelRendering.CurrentKeyFrame = frameIndex;
                panelRendering.Invalidate();

                // Update GUI
                statusFrame.Text = "Frame: " + (trackFrames.Value + 1) + "/" + _selectedNode.WadAnimation.KeyFrames.Count;

                tbCollisionBoxMinX.Text = keyFrame.BoundingBox.Minimum.X.ToString();
                tbCollisionBoxMinY.Text = keyFrame.BoundingBox.Minimum.Y.ToString();
                tbCollisionBoxMinZ.Text = keyFrame.BoundingBox.Minimum.Z.ToString();
                tbCollisionBoxMaxX.Text = keyFrame.BoundingBox.Maximum.X.ToString();
                tbCollisionBoxMaxY.Text = keyFrame.BoundingBox.Maximum.Y.ToString();
                tbCollisionBoxMaxZ.Text = keyFrame.BoundingBox.Maximum.Z.ToString();
            }
        }

        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _moveable.Animations.Clear();
            foreach (var animation in _workingAnimations)
            {

            }
        }

        private void butCalculateCollisionBox_Click(object sender, EventArgs e)
        {
            if (_selectedNode != null)
            {
                var keyFrame = _selectedNode.DirectXAnimation.KeyFrames[trackFrames.Value];

                // First check for errors
                int value = 0;
                if (!Int32.TryParse(tbCollisionBoxMinX.Text,out value) ||
                    !Int32.TryParse(tbCollisionBoxMinY.Text, out value) ||
                    !Int32.TryParse(tbCollisionBoxMinZ.Text, out value) ||
                    !Int32.TryParse(tbCollisionBoxMaxX.Text, out value) ||
                    !Int32.TryParse(tbCollisionBoxMaxY.Text, out value) ||
                    !Int32.TryParse(tbCollisionBoxMaxZ.Text, out value))
                {

                }
               

                // Update GUI
                statusFrame.Text = "Frame: " + trackFrames.Value + "/" + trackFrames.Maximum;

                tbCollisionBoxMinX.Text = keyFrame.BoundingBox.Minimum.X.ToString();
                tbCollisionBoxMinY.Text = keyFrame.BoundingBox.Minimum.Y.ToString();
                tbCollisionBoxMinZ.Text = keyFrame.BoundingBox.Minimum.Z.ToString();
                tbCollisionBoxMaxX.Text = keyFrame.BoundingBox.Maximum.X.ToString();
                tbCollisionBoxMaxY.Text = keyFrame.BoundingBox.Maximum.Y.ToString();
                tbCollisionBoxMaxZ.Text = keyFrame.BoundingBox.Maximum.Z.ToString();
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
            wadAnimation.FrameDuration = 1;
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
        }

        private void DeleteAnimation()
        {
            if (_selectedNode != null)
            { 
                if (DarkMessageBox.Show(this, "Do you really want to delete '" + _selectedNode.WadAnimation.Name + "'?",
                                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    treeAnimations.Nodes.Remove(treeAnimations.SelectedNodes[0]);
                    if (treeAnimations.Nodes.Count != 0)
                        SelectAnimation(treeAnimations.Nodes[0].Tag as AnimationNode);
                    else
                        _selectedNode = null;
                    panelRendering.Invalidate();
                }
            }
        }

        private void drawGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawGrid = !panelRendering.DrawGrid;
            panelRendering.Invalidate();
        }

        private void drawGizmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawGizmo= !panelRendering.DrawGizmo;
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
                    keyFrame.RotationsMatrices.Add(Matrix4x4.CreateFromYawPitchRoll(0, 0, 0));
                    keyFrame.Translations.Add(bone.Translation);
                    keyFrame.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(bone.Translation));
                }
             
                _selectedNode.DirectXAnimation.KeyFrames.Insert(index, keyFrame);
            }
        }
    }
}
