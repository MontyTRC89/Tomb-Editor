using DarkUI.Controls;
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
using TombLib.Graphics;
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
            var model = _wad.DirectXMoveables[_moveable.Id];
            var wadMoveable = _moveable;
            var dxMesh = model.Meshes[wadMoveable.Meshes.IndexOf(wadMesh)];

            DarkTreeNode node = new DarkTreeNode(currentBone.Name);
            node.Tag = new WadMeshBoneNode(parentNode, wadMesh, currentBone, dxMesh);

            foreach (var childBone in currentBone.Children)
                node.Nodes.Add(LoadSkeleton((WadMeshBoneNode)node.Tag, childBone, node));

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
            node.Bone.Transform = Matrix4x4.CreateTranslation(node.Bone.Translation);
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
    }
}
