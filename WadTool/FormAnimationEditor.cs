using System;
using System.Collections.Generic;
using System.Linq;
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
                _model.BuildAnimationPose(node.DirectXAnimation.KeyFrames[0]);
                trackFrames.Visible = true;
                trackFrames.MinValue = 0;
                trackFrames.MaxValue = node.WadAnimation.KeyFrames.Count - 1;

                // Load animation commands
                foreach (var cmd in node.WadAnimation.AnimCommands)
                    if (cmd.Type == WadAnimCommandType.PlaySound || cmd.Type == WadAnimCommandType.FlipEffect)
                        trackFrames.AnimationCommands.Add(cmd);
                trackFrames.UpdateAnimationCommands();
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
            if (_selectedNode != null)
            {
                _model.BuildAnimationPose(_selectedNode.DirectXAnimation.KeyFrames[trackFrames.Value]);
                panelRendering.CurrentKeyFrame = trackFrames.Value;
                panelRendering.Invalidate();
            }
        }

        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _moveable.Animations.Clear();
            foreach (var animation in _workingAnimations)
            {

            }
        }
    }
}
