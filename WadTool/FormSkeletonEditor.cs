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
        private List<WadMeshBoneNode> _bones;
        private WadToolClass _tool;
        private List<int> _bonesOrder;
        private WadMeshBoneNode _lastBone = null;
        private Dictionary<WadMeshBoneNode, DarkTreeNode> _nodesDictionary;
        private Point _startPoint;

        public FormSkeletonEditor(WadToolClass tool, DeviceManager manager, Wad2 wad, WadMoveableId moveableId)
        {
            InitializeComponent();

            _wad = wad;
            _moveable = _wad.Moveables[moveableId].Clone();
            _tool = tool;

            panelRendering.Configuration = _tool.Configuration;
            panelRendering.InitializeRendering(tool, manager);

            _tool.EditorEventRaised += Tool_EditorEventRaised;

            // Clone the skeleton and load it
            _bones = new List<WadMeshBoneNode>();
            _bonesOrder = new List<int>();
            for (int i = 0; i < _moveable.Bones.Count; i++)
            {
                var boneNode = new WadMeshBoneNode(null, _moveable.Bones[i].Mesh, _moveable.Bones[i]);
                boneNode.Bone.Translation = _moveable.Bones[i].Translation;
                boneNode.GlobalTransform = Matrix4x4.Identity;
                _bones.Add(boneNode);
                _bonesOrder.Add(i);
            }

            treeSkeleton.Nodes.AddRange(LoadSkeleton());
            ExpandSkeleton();

            panelRendering.Skeleton = _bones;
            panelRendering.Invalidate();
        }

        private void ExpandSkeleton()
        {
            treeSkeleton.ExpandAllNodes();
            if (_lastBone != null && _nodesDictionary.ContainsKey(_lastBone))
                treeSkeleton.SelectNode(_nodesDictionary[_lastBone]);
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.BoneOffsetMovedEvent)
                UpdateSkeletonMatrices(treeSkeleton.Nodes[0], Matrix4x4.Identity);
        }

        private List<DarkTreeNode> LoadSkeleton()
        {
            treeSkeleton.Nodes.Clear();

            var nodes = new List<DarkTreeNode>();
            var stack = new Stack<DarkTreeNode>();
            _nodesDictionary = new Dictionary<WadMeshBoneNode, DarkTreeNode>();

            var rootNode = new DarkTreeNode("0: " + _bones[0].Bone.Name);
            rootNode.Tag = _bones[0];
            rootNode.Expanded = true;
            nodes.Add(rootNode);
            _nodesDictionary.Add(_bones[0], rootNode);

            var currentNode = nodes[0];

            for (int j = 1; j < _bones.Count; j++)
            {
                int linkX = (int)_bones[j].Bone.Translation.X;
                int linkY = (int)_bones[j].Bone.Translation.Y;
                int linkZ = (int)_bones[j].Bone.Translation.Z;

                var boneNode = _bones[j]; // nodes[j].Tag as WadMeshBoneNode;
                string op = "";
                if (boneNode.Bone.OpCode == WadLinkOpcode.Pop) op = "POP ";
                if (boneNode.Bone.OpCode == WadLinkOpcode.Push) op = "PUSH ";
                if (boneNode.Bone.OpCode == WadLinkOpcode.Read) op = "READ ";

                var newNode = new DarkTreeNode(j.ToString() + ": " + op + _bones[j].Bone.Name);
                newNode.Tag = _bones[j];

                _nodesDictionary.Add(_bones[j], newNode);

                switch (_bones[j].Bone.OpCode)
                {
                    case WadLinkOpcode.NotUseStack:
                        boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);
                        newNode.ParentNode = currentNode;
                        currentNode.Nodes.Add(newNode);
                        currentNode = newNode;

                        break;
                    case WadLinkOpcode.Pop:
                        boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);

                        if (stack.Count > 0)
                        {
                            currentNode = stack.Pop();
                            newNode.ParentNode = currentNode;
                            currentNode.Nodes.Add(newNode);
                            currentNode = newNode;
                        }
                        else
                        {
                            nodes.Add(newNode);
                            currentNode = newNode;
                        }

                        break;
                    case WadLinkOpcode.Push:
                        stack.Push(currentNode);

                        boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);
                        newNode.ParentNode = currentNode;
                        currentNode.Nodes.Add(newNode);
                        currentNode = newNode;

                        break;
                    case WadLinkOpcode.Read:
                        boneNode.Bone.Translation = new Vector3(linkX, linkY, linkZ);

                        if (stack.Count > 0)
                        {
                            var bone = stack.Pop();
                            newNode.ParentNode = bone;
                            bone.Nodes.Add(newNode);
                            currentNode = newNode;
                            stack.Push(bone);
                        }
                        else
                        {
                            nodes.Add(newNode);
                            currentNode = newNode;
                        }

                        break;
                }
            }

            UpdateSkeletonMatrices(nodes[0], Matrix4x4.Identity);

            return nodes;
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
            // First check if skeleton is valid
            int numPop = 0;
            int numPush = 0;
            foreach (var bone in _bones)
            {
                if (bone.Bone.OpCode == WadLinkOpcode.Pop) numPop++;
                if (bone.Bone.OpCode == WadLinkOpcode.Push) numPush++;
            }

            // We can have more push than pop, but the opposite case (pop more than push) will result in a leak 
            // inside the previous moveables in the list
            if (numPop > numPush)
            {
                DarkMessageBox.Show(this, "Your mesh tree is unbalanced, you have added more POP than PUSH.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Insert new bones in moveable
            _moveable.Bones.Clear();
            foreach (var bone in _bones)
                _moveable.Bones.Add(bone.Bone);

            // Replace the moveable
            _wad.Moveables[_moveable.Id] = _moveable;

            // Now cause the moveable to reload
            _moveable.Version = DataVersion.GetNext();

            DialogResult = DialogResult.OK;
            Close();

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
            DeleteBone();
        }

        private void butRenameBone_Click(object sender, EventArgs e)
        {
            RenameBone();
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
                }
            }
        }

        private void InsertNewBone(WadMesh mesh, WadMeshBoneNode parentNode)
        {
            // Create the new bone
            var bone = new WadBone();
            bone.Mesh = mesh;
            bone.Name = "Bone_" + mesh.Name;
            bone.OpCode = WadLinkOpcode.NotUseStack;

            // Create the new node
            var node = new WadMeshBoneNode(parentNode, mesh, bone);

            // Insert the bone
            int index = _bones.IndexOf(parentNode);
            _bones.Insert(index + 1, node);

            // Add angles to animations
            foreach (var animation in _moveable.Animations)
                foreach (var kf in animation.KeyFrames)
                    kf.Angles.Insert(index, new WadKeyFrameRotation());

            // Reload skeleton
            _lastBone = node;
            treeSkeleton.Nodes.AddRange(LoadSkeleton());
            ExpandSkeleton();

            panelRendering.Skeleton = _bones;
            panelRendering.Invalidate();
        }

        private void ReplaceExistingBone(WadMesh mesh, WadMeshBoneNode node)
        {
            node.Bone.Mesh = mesh;
            node.WadMesh = mesh;

            // Reload skeleton
            _lastBone = node;
            treeSkeleton.Nodes.AddRange(LoadSkeleton());
            ExpandSkeleton();

            panelRendering.Skeleton = _bones;
            panelRendering.Invalidate();
        }

        private void butSelectMesh_Click(object sender, EventArgs e)
        {
            AddChildBoneFromWad2();
        }

        private void FormSkeletonEditor_Load(object sender, EventArgs e)
        {

        }

        private void butReplaceFromWad2_Click(object sender, EventArgs e)
        {
            ReplaceBoneFromWad2();
        }

        private void butAddFromFile_Click(object sender, EventArgs e)
        {
            AddChildBoneFromFile();
        }

        private void butReplaceFromFile_Click(object sender, EventArgs e)
        {
            ReplaceBoneFromFile();
        }

        private void TreeSkeleton_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (treeSkeleton.SelectedNodes.Count == 0)
                    return;
                var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

                pushToolStripMenuItem.Checked = (theNode.Bone.OpCode == WadLinkOpcode.Push || theNode.Bone.OpCode == WadLinkOpcode.Read);
                popToolStripMenuItem.Checked = (theNode.Bone.OpCode == WadLinkOpcode.Pop || theNode.Bone.OpCode == WadLinkOpcode.Read);

                cmBone.Show(treeSkeleton.PointToScreen(new Point(e.X, e.Y)));
            }
        }

        private void PopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleBonePop();
        }

        private void ToggleBonePop()
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            // Change opcode
            theNode.Bone.OpCode = theNode.Bone.OpCode ^ WadLinkOpcode.Pop;

            // Reload skeleton
            _lastBone = theNode;
            treeSkeleton.Nodes.AddRange(LoadSkeleton());
            ExpandSkeleton();

            panelRendering.Skeleton = _bones;
            panelRendering.Invalidate();
        }

        private void PushToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleBonePush();
        }

        private void ToggleBonePush()
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            // Change opcode
            theNode.Bone.OpCode = theNode.Bone.OpCode ^ WadLinkOpcode.Push;

            // Reload skeleton
            _lastBone = theNode;
            treeSkeleton.Nodes.AddRange(LoadSkeleton());
            ExpandSkeleton();

            panelRendering.Skeleton = _bones;
            panelRendering.Invalidate();
        }

        private void MoveUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveBoneUp();
        }

        private void MoveBoneUp()
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            // Swap bones
            int oldIndex = _bones.IndexOf(theNode);
            if (oldIndex == 0) return;
            WadMeshBoneNode temp = _bones[oldIndex];
            _bones[oldIndex] = _bones[oldIndex - 1];
            _bones[oldIndex - 1] = temp;

            // Fix keyframes
            foreach (var animation in _moveable.Animations)
                foreach (var kf in animation.KeyFrames)
                    for (int i = 0; i < kf.Angles.Count; i++)
                    {
                        var tempAngles = kf.Angles[oldIndex];
                        kf.Angles[oldIndex] = kf.Angles[oldIndex + -1];
                        kf.Angles[oldIndex - 1] = tempAngles;
                    }

            // Reload skeleton
            _lastBone = temp;
            treeSkeleton.Nodes.AddRange(LoadSkeleton());
            ExpandSkeleton();

            panelRendering.Skeleton = _bones;
            panelRendering.Invalidate();
        }

        private void MoveDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MoveBoneDown();
        }

        private void MoveBoneDown()
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            // Swap bones
            int oldIndex = _bones.IndexOf(theNode);
            if (oldIndex == _bones.Count - 1) return;
            WadMeshBoneNode temp = _bones[oldIndex];
            _bones[oldIndex] = _bones[oldIndex + 1];
            _bones[oldIndex + 1] = temp;

            // Fix keyframes
            foreach (var animation in _moveable.Animations)
                foreach (var kf in animation.KeyFrames)
                    for (int i = 0; i < kf.Angles.Count; i++)
                    {
                        var tempAngles = kf.Angles[oldIndex];
                        kf.Angles[oldIndex] = kf.Angles[oldIndex + 1];
                        kf.Angles[oldIndex + 1] = tempAngles;
                    }

            // Reload skeleton
            _lastBone = temp;
            treeSkeleton.Nodes.AddRange(LoadSkeleton());
            ExpandSkeleton();

            panelRendering.Skeleton = _bones;
            panelRendering.Invalidate();
        }

        private void FormSkeletonEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up && e.Control)
            {
                MoveBoneUp();
            }

            if (e.KeyCode == Keys.Down && e.Control)
            {
                MoveBoneDown();
            }

            if (e.KeyCode == Keys.O && e.Control)
            {
                ToggleBonePop();
            }

            if (e.KeyCode == Keys.P && e.Control)
            {
                ToggleBonePush();
            }

            e.Handled = true;
        }

        private void ReplaceBoneFromFile()
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
                }
            }
        }

        private void ReplaceBoneFromWad2()
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            using (var form = new FormMesh(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad))
            {
                if (form.ShowDialog() == DialogResult.Cancel)
                    return;

                ReplaceExistingBone(form.SelectedMesh.Clone(), theNode);
            }
        }

        private void DeleteBone()
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            if (DarkMessageBox.Show(this, "Are you really sure to delete bone '" + theNode.Bone.Name + "'?" +
                                    "Angles associated to this bone will be deleted from all keyframes of all animations.",
                                    "Delete bone",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            // Delete the bone
            int index = _bones.IndexOf(theNode);
            _bones.RemoveAt(index);

            // Remove angles from animations
            foreach (var animation in _moveable.Animations)
                foreach (var kf in animation.KeyFrames)
                    kf.Angles.RemoveAt(index);

            // Reload skeleton
            _lastBone = null;
            treeSkeleton.Nodes.AddRange(LoadSkeleton());
            ExpandSkeleton();

            panelRendering.Skeleton = _bones;
            panelRendering.Invalidate();
        }

        private void RenameBone()
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

        private void RenameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenameBone();
        }

        private void ReplaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReplaceBoneFromFile();
        }

        private void ReplaceFromWad2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ReplaceBoneFromWad2();
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteBone();
        }

        private void AddChildBoneFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddChildBoneFromFile();
        }

        private void AddChildBoneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddChildBoneFromWad2();
        }

        private void AddChildBoneFromWad2()
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;

            using (var form = new FormMesh(_tool, DeviceManager.DefaultDeviceManager, _tool.DestinationWad))
            {
                if (form.ShowDialog() == DialogResult.Cancel)
                    return;

                InsertNewBone(form.SelectedMesh.Clone(), theNode);
            }
        }

        private void AddChildBoneFromFile()
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
                }
            }
        }

        private void PanelRendering_MouseUp(object sender, MouseEventArgs e)
        {
            if (panelRendering.SelectedNode != null)
            {
                if (e.Button == MouseButtons.Right &&
                    Math.Abs(e.X - _startPoint.X) < 2.0f &&
                    Math.Abs(e.Y - _startPoint.Y) < 2.0f)
                    cmBone.Show(panelRendering.PointToScreen(new Point(e.X, e.Y)));
                if (_nodesDictionary.ContainsKey(panelRendering.SelectedNode))
                    treeSkeleton.SelectNode(_nodesDictionary[panelRendering.SelectedNode]);
            }

            _startPoint = new Point(0, 0);
        } 

        private void PanelRendering_MouseDown(object sender, MouseEventArgs e)
        {
            _startPoint = e.Location;
        }
    }
}
