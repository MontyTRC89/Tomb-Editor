using DarkUI.Forms;
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
    public partial class FormMesh : DarkUI.Forms.DarkForm
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

        public WadMesh SelectedMesh { get; set; }

        private Wad2 _wad;
        private DeviceManager _deviceManager;
        private WadToolClass _tool;

        public FormMesh(WadToolClass tool, DeviceManager deviceManager, Wad2 wad)
        {
            InitializeComponent();

            _tool = tool;
            _wad = wad;
            _deviceManager = deviceManager;

            panelMesh.InitializeRendering(_tool, _deviceManager);

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

        private void lstMeshes_Click(object sender, EventArgs e)
        {
            // Update big image view
            if (lstMeshes.SelectedNodes.Count <= 0 || lstMeshes.SelectedNodes[0].Tag == null)
                return;

            panelMesh.Mesh = ((MeshTreeNode)lstMeshes.SelectedNodes[0].Tag).WadMesh;
            panelMesh.Invalidate();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            // Update big image view
            if (lstMeshes.SelectedNodes.Count <= 0 || lstMeshes.SelectedNodes[0].Tag == null)
                return;

            SelectedMesh = ((MeshTreeNode)lstMeshes.SelectedNodes[0].Tag).WadMesh;

            _tool.ToggleUnsavedChanges();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cbShowVertices_CheckedChanged(object sender, EventArgs e)
        {
            panelMesh.DrawVertices = cbShowVertices.Checked;
        }


        private void RemapSelectedVertex()
        {
            if (panelMesh.CurrentVertex == -1 || SelectedMesh.VerticesPositions.Count == 0)
                return;
            
            var newVertexIndex = (int)nudVertexNum.Value;
            if (newVertexIndex >= SelectedMesh.VerticesPositions.Count)
            {
                DarkMessageBox.Show(this, "Please specify index between 0 and " + (SelectedMesh.VerticesPositions.Count - 1) + ".", "Wrong index", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var oldVertex = SelectedMesh.VerticesPositions[newVertexIndex];
            SelectedMesh.VerticesPositions[newVertexIndex] = SelectedMesh.VerticesPositions[panelMesh.CurrentVertex];
            SelectedMesh.VerticesPositions[panelMesh.CurrentVertex] = oldVertex;
            panelMesh.CurrentVertex = newVertexIndex;
        }

        private void butRemapVertex_Click(object sender, EventArgs e)
        {
            RemapSelectedVertex();
        }
    }
}
