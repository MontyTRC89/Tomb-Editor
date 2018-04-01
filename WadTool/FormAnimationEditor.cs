using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
        private List<WadAnimationNode> _workingAnimations;
        private DeviceManager _deviceManager;
        private List<WadBone> _bones;
        private WadAnimationNode _selectedNode;
        private AnimatedModel _model;

        public FormAnimationEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMoveableId id)
        {
            InitializeComponent();

            _tool = tool;
            _moveableId = id;
            _wad = wad;
            _moveable = _wad.Moveables[_moveableId];
            _model = _wad.DirectXMoveables[_moveableId];
            _deviceManager = deviceManager;

            // Initialize the panel
            panelRendering.InitializePanel(_tool, _wad, _deviceManager, _moveableId);

            // Get a copy of the skeleton in linearized form
            _bones = _moveable.Skeleton.LinearizedBones.ToList<WadBone>();

            // Load animations
            _workingAnimations = new List<WadAnimationNode>();
            foreach (var animation in _moveable.Animations)
                _workingAnimations.Add(new WadAnimationNode(animation.Clone(), Animation.FromWad2(_bones, animation)));
            ReloadAnimations();
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
            var node = (WadAnimationNode)treeAnimations.SelectedNodes[0].Tag;
            SelectAnimation(node);
        }

        private void SelectAnimation(WadAnimationNode node)
        {
            _selectedNode = node;
            if (node.DirectXAnimation.KeyFrames.Count != 0)
            {
                _model.BuildAnimationPose(node.DirectXAnimation.KeyFrames[0]);
                trackFrames.Visible = true;
                trackFrames.Minimum = 0;
                trackFrames.Maximum = node.DirectXAnimation.KeyFrames.Count - 1;
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
    }
}
