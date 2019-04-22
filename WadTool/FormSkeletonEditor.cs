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
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormSkeletonEditor : DarkUI.Forms.DarkForm
    {
        private Wad2 _wad;
        private WadMoveable _moveable;
        private List<WadMeshBoneNode> _linearizedSkeleton;
        private WadToolClass _tool;
        
        public FormSkeletonEditor(WadToolClass tool, DeviceManager manager, Wad2 wad, WadMoveableId moveableId)
        {
            InitializeComponent();

            _wad = wad;
            _moveable = _wad.Moveables[moveableId];
            _tool = tool;

            panelRendering.Configuration = _tool.Configuration;
            panelRendering.InitializeRendering(tool, manager);

            _tool.EditorEventRaised += Tool_EditorEventRaised;

            // Clone the skeleton and load it
            treeSkeleton.Nodes.Clear();
            treeSkeleton.Nodes.Add(LoadSkeleton());
            UpdateLinearizedNodesList();
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.BoneOffsetMovedEvent)
                UpdateSkeletonMatrices(treeSkeleton.Nodes[0], Matrix4x4.Identity);
        }

        private DarkTreeNode LoadSkeleton()
        {
            var nodes = new List<DarkTreeNode>();
            var stack = new Stack<DarkTreeNode>();
            var bones = _moveable.Bones;

            for (int i = 0; i < bones.Count; i++)
            {
                var newNode = new DarkTreeNode(bones[i].Name);
                newNode.Tag = new WadMeshBoneNode(null, bones[i].Mesh, bones[i]);
                var boneNode = new WadMeshBoneNode(null, bones[i].Mesh, bones[i].Clone());
                boneNode.Bone.Translation = Vector3.Zero;
                boneNode.GlobalTransform = Matrix4x4.Identity;
                newNode.Tag = boneNode;
                nodes.Add(newNode);
            }

            var currentNode = nodes[0];            

            for (int j = 1; j < bones.Count; j++)
            {
                int linkX = (int)bones[j].Translation.X;
                int linkY = (int)bones[j].Translation.Y;
                int linkZ = (int)bones[j].Translation.Z;

                var boneNode = nodes[j].Tag as WadMeshBoneNode;

                switch (bones[j].OpCode)
                {
                    case WadLinkOpcode.NotUseStack:
                        boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);
                        boneNode.Parent = currentNode.Tag as WadMeshBoneNode;
                        (currentNode.Tag as WadMeshBoneNode).Children.Add(boneNode);
                        nodes[j].ParentNode = currentNode;
                        currentNode.Nodes.Add(nodes[j]);
                        currentNode = nodes[j];

                        break;
                    case WadLinkOpcode.Pop:
                        if (stack.Count <= 0)
                            continue;
                        currentNode = stack.Pop();

                        boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);
                        boneNode.Parent = currentNode.Tag as WadMeshBoneNode;
                        (currentNode.Tag as WadMeshBoneNode).Children.Add(boneNode);
                        nodes[j].ParentNode = currentNode;
                        currentNode.Nodes.Add(nodes[j]);
                        currentNode = nodes[j];

                        break;
                    case WadLinkOpcode.Push:
                        stack.Push(currentNode);

                        boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);
                        boneNode.Parent = currentNode.Tag as WadMeshBoneNode;
                        (currentNode.Tag as WadMeshBoneNode).Children.Add(boneNode);
                        nodes[j].ParentNode = currentNode;
                        currentNode.Nodes.Add(nodes[j]);
                        currentNode = nodes[j];

                        break;
                    case WadLinkOpcode.Read:
                        if (stack.Count <= 0)
                            continue;
                        var bone = stack.Pop();

                        boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);
                        boneNode.Parent = bone.Tag as WadMeshBoneNode;
                        (bone.Tag as WadMeshBoneNode).Children.Add(boneNode);
                        nodes[j].ParentNode = currentNode;
                        currentNode.Nodes.Add(nodes[j]);
                        currentNode = nodes[j];

                        stack.Push(bone);

                        break;
                }
            }

            return nodes[0];
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
            /*int originalBonesCount = _moveable.Skeleton.LinearizedBones.Count();
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
            Close();*/
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
                if (form.ShowDialog(this) == DialogResult.OK && form.Result != "")
                {
                    theNode.Bone.Name = form.Result;
                    treeSkeleton.SelectedNodes[0].Text = form.Result;
                    panelRendering.Invalidate();
                }
            }
        }

        private void butLoadModel_Click(object sender, EventArgs e)
        {
            using (FileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = PathC.GetDirectoryNameTry(_tool.DestinationWad.FileName);
                dialog.FileName = PathC.GetFileNameTry(_tool.DestinationWad.FileName);
                dialog.Filter = BaseGeometryImporter.FileExtensions.GetFilter();
                dialog.Title = "Select a 3D file that you want to see imported.";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                using (var form = new GeometryIOSettingsDialog(new IOGeometrySettings()))
                {
                    form.AddPreset(IOSettingsPresets.SettingsPresets);
                    if (form.ShowDialog(this) != DialogResult.OK)
                        return;
                    var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;
                    var mesh = WadMesh.ImportFromExternalModel(dialog.FileName, form.Settings);
                    if (mesh == null)
                    {
                        DarkMessageBox.Show(this, "Error while loading the 3D model. Please check that the file " +
                                            "is one of the supported formats and that the meshes are textured",
                                            "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    theNode.WadMesh = mesh;
                    theNode.WadMesh.CalculateNormals();

                    // Now cause the moveable to reload
                    _moveable.Version = DataVersion.GetNext();
                    UpdateLinearizedNodesList();
                }
            }
        }

        private void InsertNewBone(WadMesh mesh, WadMeshBoneNode parentNode)
        {
            // Create the new bone
            var bone = new WadBone();
            bone.Mesh = mesh;
            bone.Parent = parentNode.Bone;
            bone.Name = "Bone_" + mesh.Name;
            bone.Parent.Children.Add(bone);

            // Create the new node
            var node = new WadMeshBoneNode(parentNode, mesh, bone);
            node.Parent = parentNode;
            parentNode.Children.Add(node);
            UpdateSkeletonMatrices(treeSkeleton.Nodes[0], Matrix4x4.Identity);

            treeSkeleton.Nodes.Clear();
            treeSkeleton.Nodes.Add(LoadSkeleton());
            UpdateLinearizedNodesList();
        }

        private void ReplaceExistingBone(WadMesh mesh, WadMeshBoneNode node)
        {
            node.Bone.Mesh = mesh;
            node.WadMesh = mesh;
            UpdateSkeletonMatrices(treeSkeleton.Nodes[0], Matrix4x4.Identity);

            treeSkeleton.Nodes.Clear();
            treeSkeleton.Nodes.Add(LoadSkeleton());
            UpdateLinearizedNodesList();
        }

        private void butSelectMesh_Click(object sender, EventArgs e)
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            using (var form = new FormMesh(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad))
            {
                if (form.ShowDialog() == DialogResult.Cancel)
                    return;

                InsertNewBone(form.SelectedMesh.Clone(), theNode);
                panelRendering.Invalidate();
            }
        }

        private void FormSkeletonEditor_Load(object sender, EventArgs e)
        {

        }

        private void butReplaceFromWad2_Click(object sender, EventArgs e)
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            using (var form = new FormMesh(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad))
            {
                if (form.ShowDialog() == DialogResult.Cancel)
                    return;

                ReplaceExistingBone(form.SelectedMesh.Clone(), theNode);
                panelRendering.Invalidate();
            }
        }

        private void butAddFromFile_Click(object sender, EventArgs e)
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            using (FileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = PathC.GetDirectoryNameTry(_tool.DestinationWad.FileName);
                dialog.FileName = PathC.GetFileNameTry(_tool.DestinationWad.FileName);
                dialog.Filter = BaseGeometryImporter.FileExtensions.GetFilter();
                dialog.Title = "Select a 3D file that you want to see imported.";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                using (var form = new GeometryIOSettingsDialog(new IOGeometrySettings()))
                {
                    form.AddPreset(IOSettingsPresets.SettingsPresets);
                    if (form.ShowDialog(this) != DialogResult.OK)
                        return;
                    var mesh = WadMesh.ImportFromExternalModel(dialog.FileName, form.Settings);
                    if (mesh == null)
                    {
                        DarkMessageBox.Show(this, "Error while loading the 3D model. Please check that the file " +
                                            "is one of the supported formats and that the meshes are textured",
                                            "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    InsertNewBone(mesh, theNode);
                    panelRendering.Invalidate();
                }
            }
        }

        private void butReplaceFromFile_Click(object sender, EventArgs e)
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            using (FileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = PathC.GetDirectoryNameTry(_tool.DestinationWad.FileName);
                dialog.FileName = PathC.GetFileNameTry(_tool.DestinationWad.FileName);
                dialog.Filter = BaseGeometryImporter.FileExtensions.GetFilter();
                dialog.Title = "Select a 3D file that you want to see imported.";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                using (var form = new GeometryIOSettingsDialog(new IOGeometrySettings()))
                {
                    form.AddPreset(IOSettingsPresets.SettingsPresets);
                    if (form.ShowDialog(this) != DialogResult.OK)
                        return;
                    var mesh = WadMesh.ImportFromExternalModel(dialog.FileName, form.Settings);
                    if (mesh == null)
                    {
                        DarkMessageBox.Show(this, "Error while loading the 3D model. Please check that the file " +
                                            "is one of the supported formats and that the meshes are textured",
                                            "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    ReplaceExistingBone(mesh, theNode);
                    panelRendering.Invalidate();
                }
            }
        }
    }
}
