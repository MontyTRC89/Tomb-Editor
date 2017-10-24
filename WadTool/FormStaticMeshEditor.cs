using SharpDX;
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
    public partial class FormStaticMeshEditor : DarkUI.Forms.DarkForm
    {
        public WadStatic StaticMesh { get; private set; }

        private WadToolClass _tool;

        public FormStaticMeshEditor(WadStatic staticMesh)
        {
            InitializeComponent();

            _tool = WadToolClass.Instance;
            StaticMesh = staticMesh.Clone();
            panelRendering.InitializePanel(_tool.Device);
        }

        private void FormStaticMeshEditor_Load(object sender, EventArgs e)
        {
            panelRendering.StaticMesh = StaticMesh;
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            // Check if we need to transform mesh
            var transform = panelRendering.GizmoTransform;
            if (transform != Matrix.Identity)
            {
                for (int i = 0; i < StaticMesh.Mesh.VerticesPositions.Count; i++)
                {
                    var position = Vector3.Transform(StaticMesh.Mesh.VerticesPositions[i], transform);
                    StaticMesh.Mesh.VerticesPositions[i] = new Vector3(position.X, position.Y, position.Z);
                }

                for (int i = 0; i < StaticMesh.Mesh.VerticesNormals.Count; i++)
                {
                    var normal = Vector3.Transform(StaticMesh.Mesh.VerticesNormals[i], transform);
                    StaticMesh.Mesh.VerticesNormals[i] = new Vector3(normal.X, normal.Y, normal.Z);
                }

                // Dispose old model
                _tool.DestinationWad.DirectXStatics[StaticMesh.ObjectID].Dispose();
                _tool.DestinationWad.DirectXStatics.Remove(StaticMesh.ObjectID);
                _tool.DestinationWad.DirectXStatics.Add(StaticMesh.ObjectID, StaticModel.FromWad2(_tool.Device, 
                                                                                                  _tool.DestinationWad,
                                                                                                  StaticMesh, 
                                                                                                  _tool.DestinationWad.PackedTextures));
                StaticMesh.Mesh.UpdateHash();

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void butCalculateVisibilityBox_Click(object sender, EventArgs e)
        {
            StaticMesh.VisibilityBox = Wad2.CalculateBoundingBox(StaticMesh.Mesh, panelRendering.GizmoTransform);

            tbVisibilityBoxMinX.Text = StaticMesh.VisibilityBox.Minimum.X.ToString();
            tbVisibilityBoxMinY.Text = StaticMesh.VisibilityBox.Minimum.Y.ToString();
            tbVisibilityBoxMinZ.Text = StaticMesh.VisibilityBox.Minimum.Z.ToString();
            tbVisibilityBoxMaxX.Text = StaticMesh.VisibilityBox.Maximum.X.ToString();
            tbVisibilityBoxMaxY.Text = StaticMesh.VisibilityBox.Maximum.Y.ToString();
            tbVisibilityBoxMaxZ.Text = StaticMesh.VisibilityBox.Maximum.Z.ToString();

            panelRendering.Invalidate();
        }

        private void butCalculateCollisionBox_Click(object sender, EventArgs e)
        {
            StaticMesh.CollisionBox = Wad2.CalculateBoundingBox(StaticMesh.Mesh, panelRendering.GizmoTransform);

            tbCollisionBoxMinX.Text = StaticMesh.CollisionBox.Minimum.X.ToString();
            tbCollisionBoxMinY.Text = StaticMesh.CollisionBox.Minimum.Y.ToString();
            tbCollisionBoxMinZ.Text = StaticMesh.CollisionBox.Minimum.Z.ToString();
            tbCollisionBoxMaxX.Text = StaticMesh.CollisionBox.Maximum.X.ToString();
            tbCollisionBoxMaxY.Text = StaticMesh.CollisionBox.Maximum.Y.ToString();
            tbCollisionBoxMaxZ.Text = StaticMesh.CollisionBox.Maximum.Z.ToString();

            panelRendering.Invalidate();
        }

        private void cbVisibilityBox_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawVisibilityBox = cbVisibilityBox.Checked;
            panelRendering.Invalidate();
        }

        private void cbCollisionBox_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawCollisionBox = cbCollisionBox.Checked;
            panelRendering.Invalidate();
        }

        private void cbDrawGrid_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawGrid = cbDrawGrid.Checked;
            panelRendering.Invalidate();
        }
    }
}
