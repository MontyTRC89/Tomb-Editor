using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormSkeletonEditor : DarkUI.Forms.DarkForm
    {
        private Wad2 _wad;
        private WadMoveable _moveable;
        private WadBone _workingSkeleton;
        private List<WadMeshBoneNode> _linearizedSkeleton;
        private WadToolClass _tool;

        public FormSkeletonEditor(WadToolClass tool, DeviceManager manager, Wad2 wad, WadMoveableId moveableId)
        {
            InitializeComponent();

            _wad = wad;
            _moveable = _wad.Moveables[moveableId];
            _tool = tool;

            panelRendering.InitializePanel(tool, manager);

            _tool.EditorEventRaised += Tool_EditorEventRaised;

            // Clone the skeleton and load it
            _workingSkeleton = _moveable.Skeleton.Clone(null);
            treeSkeleton.Nodes.Add(LoadSkeleton(null, _workingSkeleton, null));
            UpdateLinearizedNodesList();
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.BoneOffsetMovedEvent)
                UpdateSkeletonMatrices(treeSkeleton.Nodes[0], Matrix4x4.Identity);
        }

        private DarkTreeNode LoadSkeleton(WadMeshBoneNode parentNode, WadBone currentBone, DarkTreeNode parent)
        {
            var wadMesh = currentBone.Mesh;
            var wadMoveable = _moveable;

            DarkTreeNode node = new DarkTreeNode(currentBone.Name);
            node.Tag = new WadMeshBoneNode(parentNode, wadMesh, currentBone);

            foreach (var childBone in currentBone.Children)
            {
                var newChildNode = LoadSkeleton((WadMeshBoneNode)node.Tag, childBone, node);
                node.Nodes.Add(newChildNode);
                var tag = (WadMeshBoneNode)node.Tag;
                tag.Children.Add((WadMeshBoneNode)(newChildNode.Tag));
            }

            return node;
        }

        private void UpdateLinearizedNodesList()
        {
            _linearizedSkeleton = new List<WadMeshBoneNode>();
            LinearizeNodes(treeSkeleton.Nodes[0], Matrix4x4.Identity);

            panelRendering.Skeleton = _linearizedSkeleton;
            panelRendering.Invalidate();
        }

        private void LinearizeNodes(DarkTreeNode current, Matrix4x4 parentTransform)
        {
            var theNode = (WadMeshBoneNode)current.Tag;
            theNode.GlobalTransform = theNode.Bone.Transform * parentTransform;
            theNode.LinearizedIndex = _linearizedSkeleton.Count;

            _linearizedSkeleton.Add(theNode);

            foreach (var childNode in current.Nodes)
                LinearizeNodes(childNode, theNode.GlobalTransform);
        }

        public void UpdateSkeletonMatrices(DarkTreeNode current, Matrix4x4 parentTransform)
        {
            var node = (WadMeshBoneNode)current.Tag;
            node.GlobalTransform = node.Bone.Transform * parentTransform;

            foreach (var childNode in current.Nodes)
                UpdateSkeletonMatrices(childNode, node.GlobalTransform);
        }

        private void treeSkeleton_Click(object sender, EventArgs e)
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;
            panelRendering.SelectedNode = theNode;
            panelRendering.Invalidate();
        }

        private void cbDrawGizmo_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawGizmo = cbDrawGizmo.Checked;
            panelRendering.Invalidate();
        }

        private void cbDrawGrid_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawGrid = cbDrawGrid.Checked;
            panelRendering.Invalidate();
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            // First I have to count old skeleton bones count
            int originalBonesCount = _moveable.Skeleton.LinearizedBones.Count();
            int currentBonesCount = _workingSkeleton.LinearizedBones.Count();

            if (originalBonesCount < currentBonesCount)
            {
                if (DarkMessageBox.Show(this, "The original moveable has less bones than current unsaved skeleton. " +
                                        "Some extra angles will be added to keyframes of animations. " +
                                        "Do you really want to continue?", "Confirm", MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            if (originalBonesCount > currentBonesCount)
            {
                if (DarkMessageBox.Show(this, "The original moveable has more bones than current unsaved skeleton. " +
                                        "Some extra angles will be deleted from keyframes of animations. " +
                                        "Do you really want to continue?", "Confirm", MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            // The user agree to do this, so let's do it

            // First, we have to build the new skeleton
            _moveable.Skeleton = SaveSkeleton(null, treeSkeleton.Nodes[0]);

            // Now I have to change all animations
            if (currentBonesCount != originalBonesCount)
            {
                for (int i = 0; i < _moveable.Animations.Count; i++)
                {
                    for (int j = 0; j < _moveable.Animations[i].KeyFrames.Count; j++)
                    {
                        if (currentBonesCount > originalBonesCount)
                        {
                            for (int k = 0; k < currentBonesCount - originalBonesCount; k++)
                            {
                                var newAngle = new WadKeyFrameRotation();
                                //newAngle.Axis = WadKeyFrameRotationAxis.ThreeAxes;
                                _moveable.Animations[i].KeyFrames[j].Angles.Add(newAngle);
                            }
                        }
                        else
                        {
                            _moveable.Animations[i].KeyFrames[j].Angles.RemoveRange(currentBonesCount, originalBonesCount - currentBonesCount);
                        }
                    }
                }
            }

            // Now cause the moveable to reload
            _moveable.Version = DataVersion.GetNext();

            DialogResult = DialogResult.OK;
            Close();
        }

        private WadBone SaveSkeleton(WadBone parentBone, DarkTreeNode currentNode)
        {
            var currentBone = (WadMeshBoneNode)currentNode.Tag;
            var bone = new WadBone();

            bone.Name = currentBone.Bone.Name;
            bone.Parent = parentBone;
            bone.Translation = currentBone.Bone.Translation;
            bone.Mesh = currentBone.WadMesh;

            foreach (var childNode in currentNode.Nodes)
                bone.Children.Add(SaveSkeleton(bone, childNode));

            return bone;
        }

        private void butDeleteBone_Click(object sender, EventArgs e)
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            if (DarkMessageBox.Show(this, "Are you really sure to delete bone '" + theNode.Bone.Name + "' and " +
                                    "all its children?", "Delete bone",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            treeSkeleton.SelectedNodes[0].Remove();
        }

        private void butRenameBone_Click(object sender, EventArgs e)
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            using (var form = new FormInputBox("Rename bone", "Insert the name of the bone:", theNode.Bone.Name))
            {
                if (form.ShowDialog() == DialogResult.OK && form.Result != "")
                {
                    theNode.Bone.Name = form.Result;
                    treeSkeleton.SelectedNodes[0].Text = form.Result;
                    panelRendering.Invalidate();
                }
            }
        }
    }
}
