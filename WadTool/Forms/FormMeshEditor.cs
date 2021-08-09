using DarkUI.Controls;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormMeshEditor : DarkForm
    {
        private class MeshTreeNode
        {
            public IWadObjectId ObjectId { get; set; }
            public int MeshIndex { get; set; }
            public WadMesh WadMesh { get; set; }

            public MeshTreeNode(IWadObjectId obj, int index, WadMesh wadMesh)
            {
                ObjectId = obj;
                MeshIndex = index;
                WadMesh = wadMesh;
            }
        }

        private class FaceEdge : IEquatable<FaceEdge>
        {
            public int[] P { get; set; }

            public bool Equals(FaceEdge other) => (other.P[0] == P[0] && other.P[1] == P[1]) || (other.P[1] == P[0] && other.P[0] == P[1]);
            public override int GetHashCode() => P[0].GetHashCode() ^ P[1].GetHashCode();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowEditingTools
        {
            get { return _showEditingTools; }
            set { _showEditingTools = value; UpdateUI(); }
        }
        private bool _showEditingTools = true;

        public WadMesh SelectedMesh { get; set; }

        private Wad2 _wad;
        private DeviceManager _deviceManager;
        private WadToolClass _tool;

        // Interaction state
        private bool _readingValues = false;
        private MeshTreeNode _currentNode = null;
        private int _currentIndex = -1;

        // Preserve user-loaded textures until user leaves editor
        private List<WadTexture> _userTextures = new List<WadTexture>();

        private readonly PopUpInfo popup = new PopUpInfo();

        public FormMeshEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad)
            : this(tool, deviceManager, wad, null) { }

        public FormMeshEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMesh mesh)
        {
            InitializeComponent();

            _tool = tool;
            _wad = wad;
            _deviceManager = deviceManager;
            _tool.EditorEventRaised += Tool_EditorEventRaised;
            panelTextureMap.SelectedTextureChanged += (s, e) => UpdateStatusLabel();

            panelMesh.InitializeRendering(_tool, _deviceManager);
            panelTextureMap.Initialize(_tool);

            // Populate blending modes
            cbBlendMode.Items.Clear();
            TextureExtensions.BlendModeUserNames(_tool.DestinationWad.GameVersion).ForEach(s => cbBlendMode.Items.Add(s));

            tabsModes.LinkedControl = cbEditingMode;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _tool.Configuration);

            ReadConfig();
            PrepareUI(mesh);
            CalculateWindowDimensions();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // These actions should happen when form is already initialized.
            // If moved to FormMeshEditor constructor, this code will break.

            if (lstMeshes.SelectedNodes.Count > 0)
                lstMeshes.EnsureVisible();

            UpdateUI();
            RepopulateTextureList(butAllTextures.Checked);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                _tool.EditorEventRaised -= Tool_EditorEventRaised;
            }
            base.Dispose(disposing);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (ShowEditingTools)
                switch (keyData)
                {
                    case Keys.Escape:
                        panelMesh.CurrentElement = -1;
                        panelTextureMap.SelectedTexture = TextureArea.None;
                        break;

                    case (Keys.Control | Keys.Z):
                        _tool.UndoManager.Undo();
                        break;

                    case (Keys.Control | Keys.Y):
                        _tool.UndoManager.Redo();
                        break;

                    case Keys.OemMinus:
                    case Keys.Oemplus:
                    case Keys.Oem3:
                    case Keys.Oem5:
                        RotateTexture();
                        break;

                    case Keys.OemMinus | Keys.Shift:
                    case Keys.Oemplus | Keys.Shift:
                    case Keys.Oem3 | Keys.Shift:
                    case Keys.Oem5 | Keys.Shift:
                        MirrorTexture();
                        break;
                }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.MeshEditorElementChangedEvent)
            {
                var newIndex = (obj as WadToolClass.MeshEditorElementChangedEvent).ElementIndex;

                switch (panelMesh.EditingMode)
                {
                    case MeshEditingMode.VertexRemap:
                        {
                            if (newIndex == -1) return;

                            // Vertex remap mode doesn't need any major processing. 
                            // We just update UI and focus on the vertex number to ease keyboard interaction.

                            UpdateUI();
                            nudVertexNum.Select(0, 5);
                            nudVertexNum.Focus();
                        }
                        break;

                    case MeshEditingMode.VertexEffects:
                        {
                            if (newIndex == -1) return;

                            // Add missing data if needed
                            if (!panelMesh.Mesh.HasAttributes)
                                GenerateMissingVertexData();

                            if (Control.ModifierKeys == Keys.Alt) // Picking
                            {
                                nudGlow.Value = panelMesh.Mesh.VertexAttributes[newIndex].Glow;
                                nudMove.Value = panelMesh.Mesh.VertexAttributes[newIndex].Move;
                            }
                            else // Editing
                            {
                                panelMesh.Mesh.VertexAttributes[newIndex] = new VertexAttributes() { Glow = (int)nudGlow.Value, Move = (int)nudMove.Value };
                                panelMesh.Invalidate();
                            }
                        }
                        break;

                    case MeshEditingMode.VertexColorsAndNormals:
                        {
                            if (newIndex == -1) return;

                            // Add missing data if needed
                            if (!panelMesh.Mesh.HasColors || !panelMesh.Mesh.HasNormals)
                                GenerateMissingVertexData();

                            if (Control.ModifierKeys == Keys.Alt) // Picking
                            {
                                panelColor.BackColor = panelMesh.Mesh.VertexColors[newIndex].ToWinFormsColor();
                            }
                            else // Editing
                            {
                                panelMesh.Mesh.VertexColors[newIndex] = panelColor.BackColor.ToFloat3Color();
                                panelMesh.Invalidate();
                            }
                        }
                        break;

                    case MeshEditingMode.FaceAttributes:
                        {
                            if (newIndex == -1) return;

                            var poly = panelMesh.Mesh.Polys[newIndex];

                            if (Control.ModifierKeys == Keys.Alt) // Picking
                            {
                                if (cbSheen.Checked)
                                {
                                    nudShineStrength.Value = (decimal)poly.ShineStrength;
                                }

                                if (cbBlend.Checked)
                                {
                                    var bmIndex = poly.Texture.BlendMode.ToUserIndex();
                                    if (bmIndex < cbBlendMode.Items.Count) cbBlendMode.SelectedIndex = bmIndex;
                                    butDoubleSide.Checked = poly.Texture.DoubleSided;
                                }

                                if (cbTexture.Checked)
                                {
                                    panelTextureMap.ShowTexture(poly.Texture);
                                    if (comboCurrentTexture.Items.Contains(poly.Texture.Texture))
                                        comboCurrentTexture.SelectedItem = poly.Texture.Texture;
                                }
                            }
                            else // Editing
                            {
                                var currTexture = poly.Texture;

                                if (cbTexture.Checked)
                                {
                                    if (Control.ModifierKeys == Keys.None) // No modifiers - ordinary application
                                    {
                                        // If there's no currently selected texture, fall back to original poly texture
                                        if (panelTextureMap.VisibleTexture.IsAvailable && panelTextureMap.SelectedTexture != TextureArea.None)
                                        {
                                            currTexture = panelTextureMap.SelectedTexture;
                                            if (poly.IsTriangle)
                                                currTexture.TexCoord3 = currTexture.TexCoord2;
                                        }
                                    }
                                    else // Shift or control pressed - flip or rotate texture
                                    {
                                        if (Control.ModifierKeys == Keys.Control)
                                            currTexture.Mirror(poly.IsTriangle);
                                        else if (Control.ModifierKeys == Keys.Shift)
                                            // We actually need to rotate polygon indices to get correct behaviour like in Strpix.
                                            poly.Rotate(1, poly.IsTriangle); 
                                    }
                                }

                                if (cbSheen.Checked)
                                {
                                    poly.ShineStrength = (byte)nudShineStrength.Value;
                                }

                                if (cbBlend.Checked)
                                {
                                    currTexture.BlendMode = TextureExtensions.ToBlendMode(cbBlendMode.SelectedIndex);
                                    currTexture.DoubleSided = butDoubleSide.Checked;
                                }

                                poly.Texture = currTexture;
                                panelMesh.Mesh.Polys[newIndex] = poly;
                                panelMesh.Invalidate();
                            }
                        }
                        break;

                    case MeshEditingMode.Sphere:
                        GetSphereValues();
                        break;
                }

                // We don't need to update UI for sphere editing because needed UI is already updated.
                // Also we don't need that for vertex remap mode, because it was already done.

                if (panelMesh.EditingMode != MeshEditingMode.VertexRemap &&
                    panelMesh.EditingMode != MeshEditingMode.Sphere)
                    UpdateUI();
            }

            if (obj is WadToolClass.UndoStackChangedEvent)
            {
                var stackEvent = (WadToolClass.UndoStackChangedEvent)obj;
                butTbUndo.Enabled = stackEvent.UndoPossible;
                butTbRedo.Enabled = stackEvent.RedoPossible;
                UpdateUI();
            }

            if (obj is WadToolClass.MessageEvent)
            {
                var m = obj as WadToolClass.MessageEvent;
                PopUpInfo.Show(popup, this, panelMesh, m.Message, m.Type);
            }
        }

        private void ReadConfig()
        {
            butTbAlpha.Checked = _tool.Configuration.MeshEditor_AlphaTest;
            butTbAxis.Checked = _tool.Configuration.MeshEditor_DrawGrid;
            butTbBilinear.Checked = _tool.Configuration.MeshEditor_Bilinear;
            butTbWireframe.Checked = _tool.Configuration.MeshEditor_Wireframe;
        }

        private void PrepareUI(WadMesh mesh)
        {
            if (mesh == null) 
            {
                // Populate tree view

                var moveablesNode = new DarkUI.Controls.DarkTreeNode("Moveables");
                foreach (var moveable in _wad.Moveables)
                {
                    var list = new List<DarkUI.Controls.DarkTreeNode>();
                    var moveableNode = new DarkUI.Controls.DarkTreeNode(moveable.Key.ToString(_wad.GameVersion));
                    for (int i = 0; i < moveable.Value.Meshes.Count(); i++)
                    {
                        var wadMesh = moveable.Value.Meshes.ElementAt(i);
                        var node = new DarkUI.Controls.DarkTreeNode(wadMesh.Name);
                        node.Tag = new MeshTreeNode(moveable.Key, i, wadMesh);
                        list.Add(node);
                    }
                    moveableNode.Nodes.AddRange(list);
                    moveablesNode.Nodes.Add(moveableNode);
                }
                lstMeshes.Nodes.Add(moveablesNode);

                var staticsNode = new DarkUI.Controls.DarkTreeNode("Statics");
                foreach (var @static in _wad.Statics)
                {
                    var staticNode = new DarkUI.Controls.DarkTreeNode(@static.Key.ToString(_wad.GameVersion));
                    var wadMesh = @static.Value.Mesh;
                    var node = new DarkUI.Controls.DarkTreeNode(wadMesh.Name);
                    node.Tag = new MeshTreeNode(@static.Key, 0, wadMesh);
                    staticNode.Nodes.Add(node);
                    staticsNode.Nodes.Add(staticNode);
                }
                lstMeshes.Nodes.Add(staticsNode);

                // Always show all textures in tree mode

                butAllTextures.Checked = true;
                butAllTextures.Enabled = false;
            }
            else 
            {
                // If form is called with specific mesh, show only it and not meshtree.

                panelMesh.Mesh = mesh;
                panelTree.Visible = false;
                GetSphereValues();
            }
        }

        private void UpdateUI()
        {
            // Gray out editing pane if no mesh is selected (e.g. we're in tree view)
            panelEditingTools.Enabled = panelMesh.Mesh != null;

            // Disable vertex remap controls if no vertex is selected
            var enableRemap = panelMesh.EditingMode == MeshEditingMode.VertexRemap && 
                              panelMesh.CurrentElement != -1;
            if (enableRemap) nudVertexNum.Value = panelMesh.CurrentElement;
            butRemapVertex.Enabled = enableRemap;

            // Borrow rendering options from 3D panel itself
            butTbWireframe.Checked = panelMesh.WireframeMode;
            butTbAlpha.Checked = panelMesh.AlphaTest;
            cbExtra.Checked = panelMesh.DrawExtraInfo;

            // Hide cancel button in case editing mode is active in tree view.
            // It is needed because editing happens realtime, without keeping backup mesh.
            if (lstMeshes.Visible && ShowEditingTools)
            {
                btCancel.Visible = false;
                btOk.Location = btCancel.Location;
            }

            // Hide editing tools if flag is unset
            if (!ShowEditingTools)
            {
                panelMesh.EditingMode = MeshEditingMode.None;
                panelEditing.Visible = false;
                panelEditing.Visible = false;
                topBar.Visible = false;
            }

            // cbExtra checkbox toggles alternate mesh drawing mode.
            // In every editing mode it has different meaning, so we change label.
            switch (panelMesh.EditingMode)
            {
                case MeshEditingMode.FaceAttributes:
                    cbExtra.Text = "Show sheen";
                    break;
                case MeshEditingMode.VertexColorsAndNormals:
                    cbExtra.Text = "Show all normals";
                    break;
                case MeshEditingMode.VertexEffects:
                    cbExtra.Text = "Show all values";
                    break;
                case MeshEditingMode.VertexRemap:
                    cbExtra.Text = "Show all numbers";
                    break;
                case MeshEditingMode.Sphere:
                    cbExtra.Text = "Show gizmo";
                    break;
            }

            // In case user switched wad version to old game and picking 
            // unsupported blending mode, switch it back to default (opaque).
            if (cbBlendMode.SelectedIndex == -1)
                cbBlendMode.SelectedIndex = 0;

            // Disable I/O buttons if no current mesh is selected
            butTbExport.Enabled = !NoMesh();
            butTbImport.Enabled = !NoMesh();

            // Update status label
            UpdateStatusLabel();

            panelMesh.Invalidate();
        }

        private void UpdateStatusLabel()
        {
            var prompt = NoMesh() ? string.Empty : panelMesh.Mesh.VertexPositions.Count + " vertices, " + panelMesh.Mesh.Polys.Count + " faces, " + panelMesh.Mesh.TextureAreas.Count + " texture infos. ";

            if (panelTextureMap.SelectedTexture != TextureArea.None)
            {
                var quad = panelTextureMap.SelectedTexture.GetRect();
                prompt += "Selected texture: " + quad.Start + " to " + quad.End;
            }

            statusLabel.Text = prompt;
            Text = "Mesh editor" + (NoMesh() ? "" : " - " + panelMesh.Mesh.Name);
        }

        private void CalculateWindowDimensions()
        {
            // Subtract parasite UI elements from designer mode and recalc all needed dimensions

            var shift = tabsModes.Size.Height - tabVertexEffects.ClientSize.Height;

            tabsModes.Size = new Size(tabsModes.Size.Width, tabsModes.Size.Height - shift);
            panelEditingTools.Size = new Size(panelEditingTools.Size.Width,
                                             panelEditingTools.Size.Height - shift);
        }

        private bool NoMesh()
            => panelMesh.Mesh == null || panelMesh.Mesh.VertexPositions.Count == 0;

        private void GetSphereValues()
        {
            if (NoMesh()) return;

            // Sphere values are global, no need to refer to current element
            _readingValues = true;
            nudSphereX.Value = (decimal)panelMesh.Mesh.BoundingSphere.Center.X;
            nudSphereY.Value = (decimal)panelMesh.Mesh.BoundingSphere.Center.Y;
            nudSphereZ.Value = (decimal)panelMesh.Mesh.BoundingSphere.Center.Z;
            nudSphereRadius.Value = (decimal)panelMesh.Mesh.BoundingSphere.Radius;
            _readingValues = false;
        }

        private void ShowSelectedMesh()
        {
            if (lstMeshes.SelectedNodes.Count == 0)
                return;

            var newNode = (GetFirstChildNode(lstMeshes.SelectedNodes[0])).Tag as MeshTreeNode;
            if (newNode != null && newNode.WadMesh != panelMesh.Mesh)
            {
                // Save current node state
                SaveCurrentMesh();
                _tool.UndoManager.ClearAll();
                panelMesh.Mesh = newNode.WadMesh;
                _currentNode = newNode;

                GetSphereValues();
                UpdateUI();

                // Keep texture list the same if we're listing all textures
                RepopulateTextureList(butAllTextures.Checked);
            }
        }

        private void SaveCurrentMesh()
        {
            // No need to update tree as it's not visible
            if (!lstMeshes.Visible)
                return;

            // Save current node state
            if (_currentNode != null)
            {
                var obj = _wad.TryGet(_currentNode.ObjectId);

                if (obj is WadMoveable)
                    (obj as WadMoveable).Meshes[_currentNode.MeshIndex] = _currentNode.WadMesh = panelMesh.Mesh;
                else if (obj is WadStatic)
                    (obj as WadStatic).Mesh = _currentNode.WadMesh = panelMesh.Mesh;
            }
        }

        private DarkTreeNode GetFirstChildNode(DarkTreeNode node)
        {
            if (node.Tag != null && node.Tag is MeshTreeNode)
                return node;

            if (node.Nodes.Count == 0)
                return null;

            return GetFirstChildNode(node.Nodes[0]);
        }

        private void RemapSelectedVertex()
        {
            if (NoMesh() || panelMesh.CurrentElement == -1)
                return;

            if (nudVertexNum.Value == panelMesh.CurrentElement)
            {
                popup.ShowError(panelMesh, "Please specify other vertex number.");
                return;
            }

            var newVertexIndex = (int)nudVertexNum.Value;
            if (newVertexIndex >= panelMesh.Mesh.VertexPositions.Count)
            {
                popup.ShowError(panelMesh, "Please specify index between 0 and " + (panelMesh.Mesh.VertexPositions.Count - 1) + ".");
                nudVertexNum.Value = panelMesh.CurrentElement;
                return;
            }

            _tool.UndoManager.PushMeshChanged(panelMesh);

            var count = RemapSelectedVertex(panelMesh.CurrentElement, newVertexIndex);

            if (count > 0)
            {
                _tool.ToggleUnsavedChanges();

                var message = "Successfully replaced vertex " + panelMesh.CurrentElement + " with " + newVertexIndex + " in " + count + " faces.";

                if (newVertexIndex > panelMesh.SafeVertexRemapLimit)
                {
                    message += "\n" + "Specified vertex number is out of recommended bounds. Glitches may happen in game.";
                    popup.ShowWarning(panelMesh, message);
                }
                else
                    popup.ShowInfo(panelMesh, message);

                panelMesh.CurrentElement = newVertexIndex;
            }
        }

        private int RemapSelectedVertex(int oldIndex, int newIndex)
        {
            if (oldIndex >= panelMesh.Mesh.VertexPositions.Count || newIndex >= panelMesh.Mesh.VertexPositions.Count)
                return 0;

            var count = 0;
            var oldVertex = panelMesh.Mesh.VertexPositions[newIndex];
            panelMesh.Mesh.VertexPositions[newIndex] = panelMesh.Mesh.VertexPositions[oldIndex];
            panelMesh.Mesh.VertexPositions[oldIndex] = oldVertex;

            for (int j = 0; j < panelMesh.Mesh.Polys.Count; j++)
            {
                var done = false;
                var poly = panelMesh.Mesh.Polys[j];

                if (poly.Index0 == oldIndex) { poly.Index0 = newIndex; done = true; } else if (poly.Index0 == newIndex) { poly.Index0 = oldIndex; done = true; }
                if (poly.Index1 == oldIndex) { poly.Index1 = newIndex; done = true; } else if (poly.Index1 == newIndex) { poly.Index1 = oldIndex; done = true; }
                if (poly.Index2 == oldIndex) { poly.Index2 = newIndex; done = true; } else if (poly.Index2 == newIndex) { poly.Index2 = oldIndex; done = true; }

                if (poly.Shape == WadPolygonShape.Quad)
                {
                    if (poly.Index3 == oldIndex) { poly.Index3 = newIndex; done = true; } else if (poly.Index3 == newIndex) { poly.Index3 = oldIndex; done = true; }
                }

                if (done)
                {
                    panelMesh.Mesh.Polys[j] = poly;
                    count++;
                }
            }

            return count;
        }

        private int AutoFit()
        {
            if (NoMesh()) return 0;

            _tool.UndoManager.PushMeshChanged(panelMesh);

            // Collect all poly edges as index pairs

            var edges = new List<FaceEdge>();

            for (int i = 0; i < panelMesh.Mesh.Polys.Count; i++)
            {
                var poly = panelMesh.Mesh.Polys[i];

                edges.Add(new FaceEdge { P = new int[] { poly.Index0, poly.Index1 } });
                edges.Add(new FaceEdge { P = new int[] { poly.Index1, poly.Index2 } });

                if (poly.IsTriangle)
                    edges.Add(new FaceEdge { P = new int[] { poly.Index2, poly.Index0 } });
                else
                {
                    edges.Add(new FaceEdge { P = new int[] { poly.Index2, poly.Index3 } });
                    edges.Add(new FaceEdge { P = new int[] { poly.Index3, poly.Index0 } });
                }
            }

            // Filter out orphaned edges by counting index pairs. 
            // If index pair only occurs once, it means no adjacent similar edge exists, so
            // edge is leading to a mesh hole.

            var orphans = edges.GroupBy(x => x)
                               .Where(g => g.Count() == 1)
                               .Select(y => y.Key)
                               .ToList();

            var remappedIndexList = new List<int>();
            var count = 0;
            foreach (var orphan in orphans)
                foreach (var point in orphan.P)
                {
                    // Remap only needed vertices

                    if (!remappedIndexList.Contains(point) && point > panelMesh.SafeVertexRemapLimit)
                    {
                        // Find fitting vertex number to remap to

                        while (true)
                        {
                            if (orphans.Any(o => o.P[0] == count || o.P[1] == count))
                                count++;
                            else
                                break;

                            if (count > panelMesh.SafeVertexRemapLimit || count == panelMesh.Mesh.VertexPositions.Count - 1)
                                return count;
                        }

                        RemapSelectedVertex(point, count);
                        remappedIndexList.Add(point);
                        count++;
                    }
                }

            return count;
        }

        private void GenerateMissingVertexData()
        {
            if (panelMesh.Mesh.GenerateMissingVertexData())
                popup.ShowInfo(panelMesh, "Missing vertex data was automatically generated for this mesh.");
        }

        private void MirrorTexture()
        {
            if (panelTextureMap.VisibleTexture == null || panelTextureMap.VisibleTexture.IsUnavailable)
                return;

            var tm = panelTextureMap.SelectedTexture;
            tm.Mirror();
            panelTextureMap.SelectedTexture = tm;
        }

        private void RotateTexture()
        {
            if (panelTextureMap.VisibleTexture == null || panelTextureMap.VisibleTexture.IsUnavailable)
                return;

            var tr = panelTextureMap.SelectedTexture;
            tr.Rotate(1);
            panelTextureMap.SelectedTexture = tr;
        }

        private void RepopulateTextureList(bool wholeWad)
        {
            if (NoMesh() && !wholeWad)
            {
                comboCurrentTexture.Items.Clear();
                return;
            }

            var list = new List<Texture>(_userTextures);

            if (!NoMesh())
                foreach (var poly in panelMesh.Mesh.Polys)
                    if (!list.Exists(t => t == poly.Texture.Texture))
                        list.Add(poly.Texture.Texture);

            if (wholeWad)
            {
                foreach (var mesh in _tool.DestinationWad.MeshesUnique)
                    foreach (var poly in mesh.Polys)
                        if (!list.Exists(t => t == poly.Texture.Texture))
                            list.Add(poly.Texture.Texture);
            }

            // If count is the same it means no changes were made in texture list
            // and there's no need to actually repopulate.

            if (wholeWad && comboCurrentTexture.Items.Count == list.Count)
                return;

            comboCurrentTexture.Items.Clear();
            comboCurrentTexture.Items.AddRange(list.ToArray());

            if (comboCurrentTexture.Items.Count > 0)
                comboCurrentTexture.SelectedIndex = 0;

            panelTextureMap.SelectedTexture = TextureArea.None;
        }

        private void SearchTree()
        {
            if (string.IsNullOrEmpty(tbSearchMeshes.Text))
                return;

            var nodes = lstMeshes.GetAllNodes();
            var items = nodes.Select(n => n.Text).ToList();

            for (int i = _currentIndex + 1; i <= items.Count; i++)
            {
                if (i == items.Count)
                {
                    if (_currentIndex == -1)
                        break; // No match
                    else
                    {
                        i = -1;
                        _currentIndex = -1;
                        continue; // Restart search
                    }
                }

                if (items[i].IndexOf(tbSearchMeshes.Text, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    _currentIndex = i;
                    break;
                }
            }

            if (_currentIndex != -1)
            {
                lstMeshes.SelectNode(nodes[_currentIndex]);
                lstMeshes.EnsureVisible();
                ShowSelectedMesh();
            }
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            SelectedMesh = panelMesh.Mesh;
            _tool.ToggleUnsavedChanges();
            _tool.WadChanged(WadArea.Destination);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cbVertexNumbers_CheckedChanged(object sender, EventArgs e)
        {
            panelMesh.DrawExtraInfo = cbExtra.Checked;
            UpdateUI();
        }

        private void butRemapVertex_Click(object sender, EventArgs e)
        {
            RemapSelectedVertex();
        }

        private void butFindVertex_Click(object sender, EventArgs e)
        {
            if (NoMesh()) return;

            var newVertexIndex = (int)nudVertexNum.Value;
            if (newVertexIndex >= panelMesh.Mesh.VertexPositions.Count)
            {
                popup.ShowError(panelMesh, "Please specify index between 0 and " + (panelMesh.Mesh.VertexPositions.Count - 1) + ".");
                return;
            }
            panelMesh.CurrentElement = newVertexIndex;
        }

        private void nudVertexNum_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                RemapSelectedVertex();
        }

        private void cbEditingMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelMesh.EditingMode = (MeshEditingMode)cbEditingMode.SelectedIndex + 1;
            butPreview.Checked = false;
            UpdateUI();
        }

        private void butApplyToAllFaces_Click(object sender, EventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.FaceAttributes)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);

            var currentShinyValue = (byte)nudShineStrength.Value;
            var currentBlendMode = TextureExtensions.ToBlendMode(cbBlendMode.SelectedIndex);

            for (int i = 0; i < panelMesh.Mesh.Polys.Count; i++)
            {
                var poly = panelMesh.Mesh.Polys[i];

                if (cbSheen.Checked)
                    poly.ShineStrength = currentShinyValue;

                if (cbBlend.Checked)
                {
                    poly.Texture.BlendMode = currentBlendMode;
                    poly.Texture.DoubleSided = butDoubleSide.Checked;
                }

                if (cbTexture.Checked && panelTextureMap.SelectedTexture != TextureArea.None)
                    poly.Texture = panelTextureMap.SelectedTexture;

                panelMesh.Mesh.Polys[i] = poly;
            }

            UpdateStatusLabel();
            panelMesh.Invalidate();
        }

        private void butApplyToAllVertices_Click(object sender, EventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.VertexEffects)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);

            GenerateMissingVertexData();

            var currentGlow = (int)nudGlow.Value;
            var currentMove = (int)nudMove.Value;

            for (int i = 0; i < panelMesh.Mesh.VertexPositions.Count; i++)
            {
                panelMesh.Mesh.VertexAttributes[i].Glow = currentGlow;
                panelMesh.Mesh.VertexAttributes[i].Move = currentMove;
            }

            panelMesh.Invalidate();
        }

        private void panelColor_MouseDown(object sender, MouseEventArgs e)
        {
            using (var colorDialog = new RealtimeColorDialog())
            {
                colorDialog.Color = panelColor.BackColor;
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (panelColor.BackColor != colorDialog.Color)
                    panelColor.BackColor = colorDialog.Color;
            }
        }

        private void butRecalcNormals_Click(object sender, EventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.VertexColorsAndNormals)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);

            panelMesh.Mesh.CalculateNormals();
            panelMesh.Invalidate();
        }

        private void butRecalcNormalsAvg_Click(object sender, EventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.VertexColorsAndNormals)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);

            panelMesh.Mesh.CalculateNormals(false);
            panelMesh.Invalidate();
        }

        private void butApplyShadesToAllVertices_Click(object sender, EventArgs e)
        {
            if (NoMesh() || panelMesh.EditingMode != MeshEditingMode.VertexColorsAndNormals)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);

            GenerateMissingVertexData();

            var currentColor = panelColor.BackColor.ToFloat3Color();

            for (int i = 0; i < panelMesh.Mesh.VertexPositions.Count; i++)
                panelMesh.Mesh.VertexColors[i] = currentColor;

            panelMesh.Invalidate();
        }

        private void butConvertFromShades_Click(object sender, EventArgs e)
        {
            if (NoMesh()) return;

            // This function converts vertex attributes from legacy TE workflow which
            // interpreted vertex colors as glow/move flags. We convert exact shade value to exact
            // attribute value, because legacy compiler should convert it to a flag anyway, while
            // TEN compiler most likely will keep attribute value on a per-vertex basis.

            _tool.UndoManager.PushMeshChanged(panelMesh);

            panelMesh.Mesh.VertexAttributes.Clear();

            if (!panelMesh.Mesh.HasColors)
                panelMesh.Mesh.VertexAttributes = Enumerable.Repeat(new VertexAttributes(), panelMesh.Mesh.VertexPositions.Count).ToList();
            else
            {
                for (int i = 0; i < panelMesh.Mesh.VertexColors.Count; i++)
                {
                    var attr = new VertexAttributes();
                    var luma = panelMesh.Mesh.VertexColors[i].GetLuma();

                    if (luma < 0.5f) attr.Move = (int)(luma * 2.0f * 63.0f);
                    else if (luma < 1.0f) attr.Glow = (int)((luma - 0.5f) * 63.0f);

                    panelMesh.Mesh.VertexAttributes.Add(attr);
                }

                panelMesh.Mesh.VertexColors.Clear();
            }

            panelMesh.Invalidate();
        }

        private void butDoubleSide_Click(object sender, EventArgs e)
        {
            butDoubleSide.Checked = !butDoubleSide.Checked;
        }

        private void butAutoFit_Click(object sender, EventArgs e)
        {
            if (panelMesh.Mesh.VertexPositions.Count < panelMesh.SafeVertexRemapLimit)
            {
                popup.ShowInfo(panelMesh, "Vertex count is lower than remap limit. No auto-fitting is needed.");
                return;
            }

            var count = AutoFit();

            if (count == 0)
                popup.ShowWarning(panelMesh, "No vertices were auto-fitted. Possibly mesh is already remapped or contains no holes.");
            else
                popup.ShowInfo(panelMesh, "Auto-fitted " + count + " vertices.");

            panelMesh.Invalidate();
        }

        private void nudSphereData_ValueChanged(object sender, EventArgs e)
        {
            if (_readingValues)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);

            var newCoord = new Vector3((float)nudSphereX.Value, (float)nudSphereY.Value, (float)nudSphereZ.Value);
            panelMesh.Mesh.BoundingSphere = new BoundingSphere(newCoord, (float)nudSphereRadius.Value);
            panelMesh.Invalidate();
        }

        private void butResetSphere_Click(object sender, EventArgs e)
        {
            _tool.UndoManager.PushMeshChanged(panelMesh);

            panelMesh.Mesh.BoundingSphere = panelMesh.Mesh.CalculateBoundingSphere();
            panelMesh.Invalidate();
            GetSphereValues();
        }

        private void butPreview_Click(object sender, EventArgs e)
        {
            butPreview.Checked = !butPreview.Checked;

            if (butPreview.Checked)
                panelMesh.StartPreview();
            else
                panelMesh.StopPreview();
        }

        private void butAddTexture_Click(object sender, EventArgs e)
        {
            var paths = LevelFileDialog.BrowseFiles(this, null, null, "Load texture file", LevelTexture.FileExtensions).ToList();
            if (paths.Count == 0)
                return;

            foreach (var path in paths)
            {
                var image = ImageC.FromFile(path);
                image.ReplaceColor(new ColorC(255, 0, 255, 255), new ColorC(0, 0, 0, 0)); // Magenta to transparency for legacy reasons...

                var newTexture = new WadTexture(image);

                if (comboCurrentTexture.Items.Contains(newTexture))
                    continue;

                comboCurrentTexture.Items.Add(newTexture);
                comboCurrentTexture.SelectedItem = newTexture;

                _userTextures.Add(newTexture);
            }
        }

        private void comboCurrentTexture_SelectedValueChanged(object sender, EventArgs e)
        {
            var selectedTexture = comboCurrentTexture.SelectedItem as Texture;
            panelTextureMap.ResetVisibleTexture(selectedTexture, true);
        }

        private void butDeleteTexture_Click(object sender, EventArgs e)
        {
            if (panelMesh.Mesh.Polys.Any(p => p.Texture.Texture == panelTextureMap.VisibleTexture))
            {
                popup.ShowError(panelMesh, "Unable to remove selected texture because it's still used in mesh.");
                return;
            }

            if (_tool.DestinationWad.MeshTexturesUnique.Contains(panelTextureMap.VisibleTexture as WadTexture))
            {
                popup.ShowError(panelMesh, "Unable to remove selected texture because it's still used in wad.");
                return;
            }

            var index = comboCurrentTexture.Items.IndexOf(panelTextureMap.VisibleTexture);
            if (index != -1)
            {
                if (_userTextures.Contains(panelTextureMap.VisibleTexture))
                    _userTextures.Remove((WadTexture)panelTextureMap.VisibleTexture);

                comboCurrentTexture.Items.RemoveAt(index);
            }

            comboCurrentTexture.SelectedIndex = 0;
        }

        private void butExportTexture_Click(object sender, EventArgs e)
        {
            if (panelTextureMap.VisibleTexture == null ||
                panelTextureMap.VisibleTexture.IsUnavailable ||
                panelTextureMap.VisibleTexture.Image == null)
            {
                popup.ShowError(panelMesh, "Unable to save texture.\nSelected texture is invalid.");
                return;
            }

            using (var fileDialog = new SaveFileDialog())
            {
                try
                {
                    fileDialog.Filter = ImageC.SaveFileFileExtensions.GetFilter(true);
                    fileDialog.Title = "Choose a texture file name";
                    fileDialog.FileName = Path.GetFileNameWithoutExtension(panelTextureMap.VisibleTexture.ToString());
                    fileDialog.AddExtension = true;

                    DialogResult dialogResult = fileDialog.ShowDialog(this);
                    if (dialogResult != DialogResult.OK)
                        return;

                    panelTextureMap.VisibleTexture.Image.Save(fileDialog.FileName);
                }
                catch (Exception exc)
                {
                    popup.ShowError(panelMesh, "Unable to save texture. Exception: \n" + exc);
                }
            }
        }

        private void butAllTextures_Click(object sender, EventArgs e)
        {
            butAllTextures.Checked = !butAllTextures.Checked;
            RepopulateTextureList(butAllTextures.Checked);
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(comboCurrentTexture);
            searchPopUp.Show(this);
        }

        private void butTbUndo_Click(object sender, EventArgs e)
        {
            _tool.UndoManager.Undo();
        }

        private void butTbRedo_Click(object sender, EventArgs e)
        {
            _tool.UndoManager.Redo();
        }

        private void butTbWireframe_Click(object sender, EventArgs e)
        {
            _tool.Configuration.MeshEditor_Wireframe = panelMesh.WireframeMode = butTbWireframe.Checked;
            UpdateUI();
        }

        private void butTbAlpha_Click(object sender, EventArgs e)
        {
            _tool.Configuration.MeshEditor_AlphaTest = panelMesh.AlphaTest = butTbAlpha.Checked;
            UpdateUI();
        }

        private void butTbBilinear_Click(object sender, EventArgs e)
        {
            _tool.Configuration.MeshEditor_Bilinear = panelMesh.Bilinear = butTbBilinear.Checked;
            UpdateUI();
        }

        private void butTbAxis_Click(object sender, EventArgs e)
        {
            _tool.Configuration.MeshEditor_DrawGrid = panelMesh.DrawGrid = butTbAxis.Checked;
            UpdateUI();
        }

        private void butTbResetCamera_Click(object sender, EventArgs e)
        {
            panelMesh.ResetCamera();
        }

        private void butTbRotateTexture_Click(object sender, EventArgs e)
        {
            RotateTexture();
        }

        private void butTbMirrorTexture_Click(object sender, EventArgs e)
        {
            MirrorTexture();
        }

        private void FormMeshEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveCurrentMesh();
            _tool.UndoManager.ClearAll();
        }

        private void butSearchMeshes_Click(object sender, EventArgs e)
        {
            SearchTree();
        }

        private void tbSearchMeshes_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                SearchTree();
        }

        private void lstMeshes_SelectedNodesChanged(object sender, EventArgs e)
        {
            ShowSelectedMesh();
        }

        private void butTbImport_Click(object sender, EventArgs e)
        {
            var mesh = WadActions.ImportMesh(_tool, this);
            if (mesh == null)
                return;

            _tool.UndoManager.PushMeshChanged(panelMesh);
            panelMesh.Mesh = mesh;
            GetSphereValues();
            UpdateUI();
            RepopulateTextureList(butAllTextures.Checked);
        }

        private void butTbExport_Click(object sender, EventArgs e)
        {
            WadActions.ExportMesh(panelMesh.Mesh, _tool, this);
        }
    }
}
