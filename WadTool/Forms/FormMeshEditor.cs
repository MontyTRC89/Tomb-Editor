using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.Utils;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormMeshEditor : DarkForm
    {
        private class MeshTreeNode
        {
            public WadMesh WadMesh { get; set; }
            public IWadObjectId ObjectId { get; set; }

            public MeshTreeNode(IWadObjectId obj, WadMesh wadMesh)
            {
                ObjectId = obj;
                WadMesh = wadMesh;
            }
        }

        public bool ShowMeshList { get; set; } = false;
        public bool ShowEditingTools { get; set; } = true;
        public WadMesh SelectedMesh { get; set; }

        private Wad2 _wad;
        private DeviceManager _deviceManager;
        private WadToolClass _tool;

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

            panelMesh.InitializeRendering(_tool, _deviceManager);

            Size = MinimumSize; // Counteract DarkTabbedContainer designer UI
            tabsModes.LinkedControl = cbEditingMode;

            if (mesh == null) // Populate tree view
            {
                var moveablesNode = new DarkUI.Controls.DarkTreeNode("Moveables");
                foreach (var moveable in _wad.Moveables)
                {
                    var list = new List<DarkUI.Controls.DarkTreeNode>();
                    var moveableNode = new DarkUI.Controls.DarkTreeNode(moveable.Key.ToString(_wad.GameVersion));
                    for (int i = 0; i < moveable.Value.Meshes.Count(); i++)
                    {
                        var wadMesh = moveable.Value.Meshes.ElementAt(i);
                        var node = new DarkUI.Controls.DarkTreeNode(wadMesh.Name);
                        node.Tag = new MeshTreeNode(moveable.Key, wadMesh);
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
                    node.Tag = new MeshTreeNode(@static.Key, wadMesh);
                    staticNode.Nodes.Add(node);
                    staticsNode.Nodes.Add(staticNode);
                }
                lstMeshes.Nodes.Add(staticsNode);
            }
            else // If form is called with specific item and mesh, show only it and not meshtree.
            {
                panelMesh.Mesh = mesh;
                panelTree.Visible = false; // Lock up list to prevent wandering around

                MinimumSize = new Size(MinimumSize.Width - panelTree.Width, MinimumSize.Height);
                Size = new Size(Size.Width - panelTree.Width, Size.Height);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // These actions should happen when form is already initialized.
            // If moved to FormMeshEditor constructor, this code will break.

            if (lstMeshes.SelectedNodes.Count > 0)
                lstMeshes.EnsureVisible();
            UpdateUI();
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.MeshEditorElementChangedEvent)
            {
                switch (panelMesh.EditingMode)
                {
                    case MeshEditingMode.VertexRemap:
                        {
                            if (panelMesh.CurrentElement != -1)
                                nudVertexNum.Value = (decimal)panelMesh.CurrentElement;

                            nudVertexNum.Select(0, 5);
                            nudVertexNum.Focus();
                        }
                        break;

                    case MeshEditingMode.VertexAttributes:
                        {
                            if (panelMesh.CurrentElement == -1) return;

                            // Add missing data if needed
                            panelMesh.Mesh.GenerateMissingVertexData(); 

                            if (Control.ModifierKeys == Keys.Alt)
                            {
                                nudGlow.Value = panelMesh.Mesh.VertexAttributes[panelMesh.CurrentElement].Glow;
                                nudMove.Value = panelMesh.Mesh.VertexAttributes[panelMesh.CurrentElement].Move;
                                panelColor.BackColor = panelMesh.Mesh.VertexColors[panelMesh.CurrentElement].ToWinFormsColor();
                            }
                            else
                            {
                                if (cbApplyColor.Checked)
                                    panelMesh.Mesh.VertexColors[panelMesh.CurrentElement] = panelColor.BackColor.ToFloat3Color();

                                panelMesh.Mesh.VertexAttributes[panelMesh.CurrentElement] = new VertexAttributes() { Glow = (int)nudGlow.Value, Move = (int)nudMove.Value };
                                panelMesh.Invalidate();
                            }
                        }
                        break;

                    case MeshEditingMode.FaceAttributes:
                        {
                            if (panelMesh.CurrentElement == -1) return;

                            var poly = panelMesh.Mesh.Polys[panelMesh.CurrentElement];

                            if (Control.ModifierKeys == Keys.Alt)
                            {
                                switch (poly.Texture.BlendMode)
                                {
                                    default:
                                    case BlendMode.Normal:   cbBlendMode.SelectedIndex = 0; break;
                                    case BlendMode.Additive: cbBlendMode.SelectedIndex = 1; break;
                                    case BlendMode.Subtract: cbBlendMode.SelectedIndex = 2; break;
                                    case BlendMode.Exclude:  cbBlendMode.SelectedIndex = 3; break;
                                    case BlendMode.Screen:   cbBlendMode.SelectedIndex = 4; break;
                                    case BlendMode.Lighten:  cbBlendMode.SelectedIndex = 5; break;
                                }
                                nudShineStrength.Value = (decimal)poly.ShineStrength;
                            }
                            else
                            {
                                var selectedTexture = poly.Texture;
                                switch (cbBlendMode.SelectedIndex)
                                {
                                    default:
                                    case 0: selectedTexture.BlendMode = BlendMode.Normal;   break;
                                    case 1: selectedTexture.BlendMode = BlendMode.Additive; break;
                                    case 2: selectedTexture.BlendMode = BlendMode.Subtract; break;
                                    case 3: selectedTexture.BlendMode = BlendMode.Exclude;  break;
                                    case 4: selectedTexture.BlendMode = BlendMode.Screen;   break;
                                    case 5: selectedTexture.BlendMode = BlendMode.Lighten;  break;
                                }
                                poly.Texture = selectedTexture;
                                poly.ShineStrength = (byte)nudShineStrength.Value;
                                panelMesh.Mesh.Polys[panelMesh.CurrentElement] = poly;
                                panelMesh.Invalidate();
                            }
                        }
                        break;
                }
            }
        }

        private void UpdateUI()
        {
            panelEditing.Enabled = panelMesh.Mesh != null;

            var enableNud = panelMesh.EditingMode == MeshEditingMode.VertexRemap && panelMesh.CurrentElement != -1;
            if (enableNud) nudVertexNum.Value = panelMesh.CurrentElement;
            butRemapVertex.Enabled = enableNud;

            cbVertexNumbers.Enabled = panelMesh.EditingMode != MeshEditingMode.FaceAttributes;
            cbWireframe.Checked = panelMesh.WireframeMode;
            cbVertexNumbers.Checked = panelMesh.DrawVertexNumbers;

            if (ShowMeshList && ShowEditingTools)
            {
                btCancel.Visible = false;
                btOk.Location = btCancel.Location;
            }
            
            if (!ShowEditingTools)
            {
                panelMesh.EditingMode = MeshEditingMode.None;
                panelEditingTools.Visible = false;
            }

            panelMesh.Invalidate();
        }

        private void ShowSelectedMesh()
        {
            // Update big image view
            if (lstMeshes.SelectedNodes.Count > 0 && lstMeshes.SelectedNodes[0].Tag != null)
                panelMesh.Mesh = ((MeshTreeNode)lstMeshes.SelectedNodes[0].Tag).WadMesh;

            UpdateUI();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
                panelMesh.CurrentElement = -1;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void lstMeshes_Click(object sender, EventArgs e)
        {
            ShowSelectedMesh();
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

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cbVertexNumbers_CheckedChanged(object sender, EventArgs e)
        {
            panelMesh.DrawVertexNumbers = cbVertexNumbers.Checked;
            UpdateUI();
        }

        private void RemapSelectedVertex()
        {
            if (panelMesh.EditingMode != MeshEditingMode.VertexRemap || panelMesh.CurrentElement == -1 || panelMesh.Mesh == null || panelMesh.Mesh.VertexPositions.Count == 0)
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

            var oldVertex = panelMesh.Mesh.VertexPositions[newVertexIndex];
            panelMesh.Mesh.VertexPositions[newVertexIndex] = panelMesh.Mesh.VertexPositions[panelMesh.CurrentElement];
            panelMesh.Mesh.VertexPositions[panelMesh.CurrentElement] = oldVertex;

            var ov = panelMesh.CurrentElement;
            var nv = newVertexIndex;
            var count = 0;

            for (int j = 0; j < panelMesh.Mesh.Polys.Count; j++)
            {
                var done = false;
                var poly = panelMesh.Mesh.Polys[j];

                if (poly.Index0 == ov) { poly.Index0 = nv; done = true; } else if (poly.Index0 == nv) { poly.Index0 = ov; done = true; }
                if (poly.Index1 == ov) { poly.Index1 = nv; done = true; } else if (poly.Index1 == nv) { poly.Index1 = ov; done = true; }
                if (poly.Index2 == ov) { poly.Index2 = nv; done = true; } else if (poly.Index2 == nv) { poly.Index2 = ov; done = true; }

                if (poly.Shape == WadPolygonShape.Quad)
                {
                    if (poly.Index3 == ov) { poly.Index3 = nv; done = true; } else if (poly.Index3 == nv) { poly.Index3 = ov; done = true; }
                }

                if (done)
                {
                    panelMesh.Mesh.Polys[j] = poly;
                    count++;
                }
            }

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

        private void butRemapVertex_Click(object sender, EventArgs e)
        {
            RemapSelectedVertex();
        }

        private void butFindVertex_Click(object sender, EventArgs e)
        {
            if (panelMesh.Mesh == null)
                return;

            var newVertexIndex = (int)nudVertexNum.Value;
            if (newVertexIndex >= panelMesh.Mesh.VertexPositions.Count)
            {
                popup.ShowError(panelMesh, "Please specify index between 0 and " + (panelMesh.Mesh.VertexPositions.Count - 1) + ".");
                return;
            }
            panelMesh.CurrentElement = newVertexIndex;
        }

        private void cbWireframe_CheckedChanged(object sender, EventArgs e)
        {
            panelMesh.WireframeMode = cbWireframe.Checked;
            UpdateUI();
        }

        private void nudVertexNum_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                RemapSelectedVertex();
        }

        private void lstMeshes_KeyDown(object sender, KeyEventArgs e)
        {
            ShowSelectedMesh();
        }

        private void cbEditingMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelMesh.EditingMode = (MeshEditingMode)cbEditingMode.SelectedIndex + 1;
            UpdateUI();
        }

        private void butApplyToAllFaces_Click(object sender, EventArgs e)
        {
            if (panelMesh.EditingMode != MeshEditingMode.FaceAttributes || panelMesh.Mesh == null || panelMesh.Mesh.Polys.Count == 0)
                return;

            var currentShinyValue = (byte)nudShineStrength.Value;
            var currentBlendMode  = (BlendMode)cbBlendMode.SelectedIndex;

            for (int i = 0; i < panelMesh.Mesh.Polys.Count; i++)
            {
                var poly = panelMesh.Mesh.Polys[i];
                poly.Texture.BlendMode = currentBlendMode;
                poly.ShineStrength = currentShinyValue;
                panelMesh.Mesh.Polys[i] = poly;
            }

            panelMesh.Invalidate();
        }

        private void butApplyToAllVertices_Click(object sender, EventArgs e)
        {
            if (panelMesh.EditingMode != MeshEditingMode.VertexAttributes || panelMesh.Mesh == null || panelMesh.Mesh.VertexPositions.Count == 0)
                return;

            panelMesh.Mesh.GenerateMissingVertexData();

            var currentColor = panelColor.BackColor.ToFloat3Color();
            var currentGlow  = (int)nudGlow.Value;
            var currentMove  = (int)nudMove.Value;

            for (int i = 0; i < panelMesh.Mesh.VertexPositions.Count; i++)
            {
                panelMesh.Mesh.VertexColors[i] = currentColor;
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
    }
}
