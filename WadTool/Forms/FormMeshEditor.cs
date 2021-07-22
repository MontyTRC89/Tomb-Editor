using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormMesh : DarkForm
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

        public bool GenericMode { get; set; }
        public WadMesh SelectedMesh { get; set; }

        private Wad2 _wad;
        private DeviceManager _deviceManager;
        private WadToolClass _tool;

        private readonly PopUpInfo popup = new PopUpInfo();

        public FormMesh(WadToolClass tool, DeviceManager deviceManager, Wad2 wad)
            : this(tool, deviceManager, wad, null) { }

        public FormMesh(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMesh mesh)
        {
            InitializeComponent();

            _tool = tool;
            _wad = wad;
            _deviceManager = deviceManager;
            _tool.EditorEventRaised += Tool_EditorEventRaised;

            panelMesh.InitializeRendering(_tool, _deviceManager);

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

            // If form is called with specific item and mesh, show only it and not meshtree.

            if (mesh != null)
            {
                panelMesh.Mesh = mesh;
                panelTree.Visible = false; // Lock up list to prevent wandering around
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
            if (obj is WadToolClass.MeshEditorVertexChangedEvent)
            {
                UpdateUI();
                nudVertexNum.Select(0, 5);
                nudVertexNum.Focus();
            }
        }

        private void UpdateUI()
        {
            var enableNud = panelMesh.CurrentVertex != -1;
            if (enableNud) nudVertexNum.Value = panelMesh.CurrentVertex;
            butRemapVertex.Enabled = enableNud;

            btCancel.Visible = !GenericMode;
            cbWireframe.Checked = _tool.Configuration.MeshEditor_DrawWireframe;
            cbShowVertices.Checked = _tool.Configuration.MeshEditor_DrawVertices;
            cbVertexNumbers.Checked = _tool.Configuration.MeshEditor_DrawVertexNumbers;

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
                panelMesh.CurrentVertex = -1;

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
            if (!GenericMode)
            {
                SelectedMesh = panelMesh.Mesh;
                _tool.ToggleUnsavedChanges();
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cbShowVertices_CheckedChanged(object sender, EventArgs e)
        {
            _tool.Configuration.MeshEditor_DrawVertices = cbShowVertices.Checked;
            UpdateUI();
        }

        private void cbVertexNumbers_CheckedChanged(object sender, EventArgs e)
        {
            _tool.Configuration.MeshEditor_DrawVertexNumbers = cbVertexNumbers.Checked;
            UpdateUI();
        }

        private void RemapSelectedVertex()
        {
            if (panelMesh.CurrentVertex == -1 || panelMesh.Mesh == null || panelMesh.Mesh.VerticesPositions.Count == 0)
                return;

            if (nudVertexNum.Value == panelMesh.CurrentVertex)
            {
                popup.ShowError(panelMesh, "Please specify other vertex number.");
                return;
            }
            
            var newVertexIndex = (int)nudVertexNum.Value;
            if (newVertexIndex >= panelMesh.Mesh.VerticesPositions.Count)
            {
                popup.ShowError(panelMesh, "Please specify index between 0 and " + (panelMesh.Mesh.VerticesPositions.Count - 1) + ".");
                nudVertexNum.Value = panelMesh.CurrentVertex;
                return;
            }

            var oldVertex = panelMesh.Mesh.VerticesPositions[newVertexIndex];
            panelMesh.Mesh.VerticesPositions[newVertexIndex] = panelMesh.Mesh.VerticesPositions[panelMesh.CurrentVertex];
            panelMesh.Mesh.VerticesPositions[panelMesh.CurrentVertex] = oldVertex;

            var ov = panelMesh.CurrentVertex;
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

                var message = "Successfully replaced vertex " + panelMesh.CurrentVertex + " with " + newVertexIndex + " in " + count + " faces.";

                if (newVertexIndex > panelMesh.SafeRemapLimit)
                {
                    message += "\n" + "Specified vertex number is out of recommended bounds. Glitches may happen in game.";
                    popup.ShowWarning(panelMesh, message);
                }
                else
                    popup.ShowInfo(panelMesh, message);

                panelMesh.CurrentVertex = newVertexIndex;
            }
        }

        private void butRemapVertex_Click(object sender, EventArgs e)
        {
            RemapSelectedVertex();
        }

        private void butFindVertex_Click(object sender, EventArgs e)
        {
            var newVertexIndex = (int)nudVertexNum.Value;
            if (newVertexIndex >= panelMesh.Mesh.VerticesPositions.Count)
            {
                popup.ShowError(panelMesh, "Please specify index between 0 and " + (panelMesh.Mesh.VerticesPositions.Count - 1) + ".");
                return;
            }
            panelMesh.CurrentVertex = newVertexIndex;
        }

        private void cbWireframe_CheckedChanged(object sender, EventArgs e)
        {
            _tool.Configuration.MeshEditor_DrawWireframe = cbWireframe.Checked;
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
    }
}
