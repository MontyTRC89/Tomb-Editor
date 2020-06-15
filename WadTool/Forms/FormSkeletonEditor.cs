﻿using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormSkeletonEditor : DarkUI.Forms.DarkForm
    {
        private Wad2 _wad;
        private WadMoveable _moveable;
        private List<WadMeshBoneNode> _bones;
        private WadToolClass _tool;
        private WadMeshBoneNode _lastBone = null;
        private Dictionary<WadMeshBoneNode, DarkTreeNode> _nodesDictionary;
        private Point _startPoint;

        // Info
        private readonly PopUpInfo popup = new PopUpInfo();

        public FormSkeletonEditor(WadToolClass tool, DeviceManager manager, Wad2 wad, WadMoveableId moveableId)
        {
            InitializeComponent();

            _wad = wad;
            _moveable = _wad.Moveables[moveableId].Clone();
            _tool = tool;

            panelRendering.Configuration = _tool.Configuration;
            panelRendering.InitializeRendering(tool, manager);

            _tool.EditorEventRaised += Tool_EditorEventRaised;

            WadMoveable skin;
            var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(_tool.DestinationWad.GameVersion, _moveable.Id.TypeId));
            if (_tool.DestinationWad.Moveables.ContainsKey(skinId))
                skin = _tool.DestinationWad.Moveables[skinId];
            else
                skin = _tool.DestinationWad.Moveables[_moveable.Id];

            // Clone the skeleton and load it
            _bones = new List<WadMeshBoneNode>();
            for (int i = 0; i < _moveable.Bones.Count; i++)
            {
                var boneNode = new WadMeshBoneNode(null, skin.Bones[i].Mesh, _moveable.Bones[i]);
                boneNode.Bone.Translation = _moveable.Bones[i].Translation;
                boneNode.GlobalTransform = Matrix4x4.Identity;
                _bones.Add(boneNode);
            }

            treeSkeleton.Nodes.AddRange(LoadSkeleton());
            ExpandSkeleton();

            panelRendering.Skeleton = _bones;

            if (treeSkeleton.SelectedNodes.Count <= 0)
            {
                treeSkeleton.SelectNode(treeSkeleton.Nodes[0]);
                panelRendering.SelectedNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;
            }

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
            {
                UpdateUI();
                UpdateSkeletonMatrices(treeSkeleton.Nodes[0], Matrix4x4.Identity);
            }
        }

        private List<DarkTreeNode> LoadSkeleton()
        {
            treeSkeleton.Nodes.Clear();

            var nodes = new List<DarkTreeNode>();
            var stack = new Stack<DarkTreeNode>();
            _nodesDictionary = new Dictionary<WadMeshBoneNode, DarkTreeNode>();

            if (_bones.Count == 0)
                return nodes;

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

                var boneNode = _bones[j]; 
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

        public void UpdateUI()
        {
            if (panelRendering.SelectedNode == null)
                return;

            nudTransX.Value = (decimal)panelRendering.SelectedNode.Bone.Translation.X;
            nudTransY.Value = (decimal)panelRendering.SelectedNode.Bone.Translation.Y;
            nudTransZ.Value = (decimal)panelRendering.SelectedNode.Bone.Translation.Z;
            panelRendering.Invalidate();
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

            // Special case: we're inserting root node
            if (_bones.Count == 1 && index == -1) index = 0;

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
                    form.AddPreset(IOSettingsPresets.GeometryImportSettingsPresets);
                    if (form.ShowDialog(this) != DialogResult.OK)
                        return;
                    var mesh = WadMesh.ImportFromExternalModel(dialog.FileName, form.Settings);
                    if (mesh == null)
                    {
                        ImportFailed();
                        return;
                    }

                    ReplaceExistingBone(mesh, theNode);
                }
            }
        }

        private void ReplaceAllBonesFromFile()
        {
            var boneCount = _bones.Count;

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
                    form.AddPreset(IOSettingsPresets.GeometryImportSettingsPresets);
                    if (form.ShowDialog(this) != DialogResult.OK)
                        return;

                    var meshes = WadMesh.ImportFromExternalModel(dialog.FileName, form.Settings, false);
                    if (meshes == null || meshes.Count == 0)
                    {
                        ImportFailed();
                        return;
                    }

                    int meshCount;
                    if (meshes.Count > _bones.Count)
                    {
                        meshCount = _bones.Count;
                        popup.ShowError(panelRendering, "Mesh count is higher in imported model. Only first " + _bones.Count + " will be imported.");
                    }
                    else if (meshes.Count < _bones.Count)
                    {
                        meshCount = meshes.Count;
                        popup.ShowError(panelRendering, "Mesh count is lower in imported model. Only meshes up to " + meshes.Count + " will be replaced.");
                    }
                    else
                        meshCount = _bones.Count;

                    for (int i = 0; i < meshCount; i++)
                        ReplaceExistingBone(meshes[i], _bones[i]);
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
                    form.AddPreset(IOSettingsPresets.GeometryImportSettingsPresets);
                    if (form.ShowDialog(this) != DialogResult.OK)
                        return;
                    var mesh = WadMesh.ImportFromExternalModel(dialog.FileName, form.Settings);
                    if (mesh == null)
                    {
                        ImportFailed();
                        return;
                    }

                    InsertNewBone(mesh, theNode);
                }
            }
        }

        private void ImportFailed()
        {
            DarkMessageBox.Show(this, "Error while loading 3D model. Check that the file format \n" +
                                "is supported, meshes are textured and texture file is present.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SaveChanges()
        {
            // First check if skeleton is valid
            int numPop = 0;
            int numPush = 0;
            foreach (var bone in _bones)
            {
                if (bone.Bone.OpCode == WadLinkOpcode.Pop) numPop++;
                if (bone.Bone.OpCode == WadLinkOpcode.Push) numPush++;
            }

            // We can have more PUSH than POP, but the opposite case (POP more than PUSH) will result in a leak 
            // inside the previous moveables in the list
            if (numPop > numPush)
            {
                DarkMessageBox.Show(this, "Your mesh tree is unbalanced, you have added more POP than PUSH.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (treeSkeleton.Nodes.Count > 1)
            {
                DarkMessageBox.Show(this, "Your mesh tree is unbalanced, you must have a single bone as root.",
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

            _tool.ToggleUnsavedChanges();
        }

        private void treeSkeleton_MouseDown(object sender, MouseEventArgs e)
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

        private void treeSkeleton_Click(object sender, EventArgs e)
        {
            if (treeSkeleton.SelectedNodes.Count == 0)
                return;
            var theNode = (WadMeshBoneNode)treeSkeleton.SelectedNodes[0].Tag;
            panelRendering.SelectedNode = theNode;
            UpdateUI();
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

        private void formSkeletonEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Up)   MoveBoneUp();
            if (e.Control && e.KeyCode == Keys.Down) MoveBoneDown();
            if (e.Control && e.KeyCode == Keys.O)    ToggleBonePop();
            if (e.Control && e.KeyCode == Keys.P)    ToggleBonePush();
            e.Handled = true;
        }

        private void panelRendering_MouseUp(object sender, MouseEventArgs e)
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
            UpdateUI();
        } 

        private void panelRendering_MouseDown(object sender, MouseEventArgs e)
        {
            _startPoint = e.Location;
        }

        private void nudTransX_ValueChanged(object sender, EventArgs e)
        {
            if (panelRendering.SelectedNode == null) return;
            var trans = panelRendering.SelectedNode.Bone.Translation;
            panelRendering.SelectedNode.Bone.Translation = new Vector3((float)nudTransX.Value, trans.Y, trans.Z);
            _tool.BoneOffsetMoved();
        }

        private void nudTransY_ValueChanged(object sender, EventArgs e)
        {
            if (panelRendering.SelectedNode == null) return;
            var trans = panelRendering.SelectedNode.Bone.Translation;
            panelRendering.SelectedNode.Bone.Translation = new Vector3(trans.X, (float)nudTransY.Value, trans.Z);
            _tool.BoneOffsetMoved();
        }

        private void nudTransZ_ValueChanged(object sender, EventArgs e)
        {
            if (panelRendering.SelectedNode == null) return;
            var trans = panelRendering.SelectedNode.Bone.Translation;
            panelRendering.SelectedNode.Bone.Translation = new Vector3(trans.X, trans.Y, (float)nudTransZ.Value);
            _tool.BoneOffsetMoved();
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            SaveChanges();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butDeleteBone_Click(object sender, EventArgs e) => DeleteBone();
        private void butRenameBone_Click(object sender, EventArgs e) => RenameBone();
        private void butLoadModel_Click(object sender, EventArgs e) => ReplaceAllBonesFromFile();
        private void butCancel_Click(object sender, EventArgs e) => Close();
        private void butReplaceFromWad2_Click(object sender, EventArgs e) => ReplaceBoneFromWad2();
        private void butAddFromFile_Click(object sender, EventArgs e) => AddChildBoneFromFile();
        private void butReplaceFromFile_Click(object sender, EventArgs e) => ReplaceBoneFromFile();
        private void butSelectMesh_Click(object sender, EventArgs e) => AddChildBoneFromWad2();

        private void PopToolStripMenuItem_Click(object sender, EventArgs e) => ToggleBonePop();
        private void PushToolStripMenuItem_Click(object sender, EventArgs e) => ToggleBonePush();
        private void MoveUpToolStripMenuItem_Click(object sender, EventArgs e) => MoveBoneUp();
        private void MoveDownToolStripMenuItem_Click(object sender, EventArgs e) => MoveBoneDown();
        private void RenameToolStripMenuItem_Click(object sender, EventArgs e) => RenameBone();
        private void ReplaceToolStripMenuItem_Click(object sender, EventArgs e) => ReplaceBoneFromFile();
        private void ReplaceFromWad2ToolStripMenuItem_Click(object sender, EventArgs e) => ReplaceBoneFromWad2();
        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e) => DeleteBone();
        private void AddChildBoneFromFileToolStripMenuItem_Click(object sender, EventArgs e) => AddChildBoneFromFile();
        private void AddChildBoneToolStripMenuItem_Click(object sender, EventArgs e) => AddChildBoneFromWad2();
    }
}
