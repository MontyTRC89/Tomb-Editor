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
    public partial class FormStaticMeshEditor : DarkUI.Forms.DarkForm
    {
        public WadStatic StaticMesh { get; private set; }

        private WadToolClass _tool;
        private WadStatic _workingStaticMesh;

        public FormStaticMeshEditor(WadStatic staticMesh)
        {
            InitializeComponent();

            _tool = WadToolClass.Instance;
            StaticMesh = staticMesh;
            _workingStaticMesh = staticMesh.Clone();
            _workingStaticMesh.Mesh = staticMesh.Mesh.Clone();
            panelRendering.InitializePanel(_tool.Device);
        }

        private void FormStaticMeshEditor_Load(object sender, EventArgs e)
        {
            panelRendering.StaticMesh = _workingStaticMesh;
            panelRendering.DrawGrid = true;
            panelRendering.DrawGizmo = true;
            UpdateVisibilityBoxUI();
            UpdateCollisionBoxUI();
        }

        private void butSaveChanges_Click(object sender, EventArgs e)
        {
            // Check if we need to transform mesh
            var transform = panelRendering.GizmoTransform;
            if (transform != Matrix4x4.Identity)
            {
                for (int i = 0; i < _workingStaticMesh.Mesh.VerticesPositions.Count; i++)
                {
                    var position = MathC.HomogenousTransform(_workingStaticMesh.Mesh.VerticesPositions[i], transform);
                    _workingStaticMesh.Mesh.VerticesPositions[i] = new Vector3(position.X, position.Y, position.Z);
                }

                for (int i = 0; i < _workingStaticMesh.Mesh.VerticesNormals.Count; i++)
                {
                    var normal = MathC.HomogenousTransform(_workingStaticMesh.Mesh.VerticesNormals[i], transform);
                    _workingStaticMesh.Mesh.VerticesNormals[i] = new Vector3(normal.X, normal.Y, normal.Z);
                }

                _workingStaticMesh.Mesh.UpdateHash();

                // Now check in Wad2 for already existing mesh
                var movebles = new List<WadMoveable>();
                var statics = new List<WadStatic>();

                foreach (var moveable in _tool.DestinationWad.Moveables)
                {
                    foreach (var moveableMesh in moveable.Value.Meshes)
                    {
                        if (moveableMesh.Hash == StaticMesh.Mesh.Hash)
                        {
                            movebles.Add(moveable.Value);
                            continue;
                        }
                    }
                }

                foreach (var staticMesh in _tool.DestinationWad.Statics)
                {
                    if (staticMesh.Value.ObjectID != StaticMesh.ObjectID &&
                        staticMesh.Value.Mesh.Hash == StaticMesh.Mesh.Hash)
                    {
                        statics.Add(staticMesh.Value);
                        continue;
                    }
                }

                // Now I have a list of moveables and statics that are using the mesh I've edited
                if (movebles.Count == 0 && statics.Count == 0)
                {
                    // Remove the old mesh
                    _tool.DestinationWad.Meshes.Remove(StaticMesh.Mesh.Hash);
                }

                // Assign the edited mesh to original static mesh
                StaticMesh.Mesh = _workingStaticMesh.Mesh;

                // Add the edited mesh to meshes list
                _tool.DestinationWad.Meshes.Add(_workingStaticMesh.Mesh.Hash, _workingStaticMesh.Mesh);

                // Dispose old model
                _tool.DestinationWad.DirectXStatics[StaticMesh.ObjectID].Dispose();
                _tool.DestinationWad.DirectXStatics.Remove(StaticMesh.ObjectID);
                _tool.DestinationWad.DirectXStatics.Add(StaticMesh.ObjectID, StaticModel.FromWad2(_tool.Device,
                                                                                                  _tool.DestinationWad,
                                                                                                  StaticMesh,
                                                                                                  _tool.DestinationWad.PackedTextures));

                // Assign bounding boxes
                StaticMesh.VisibilityBox = _workingStaticMesh.VisibilityBox;
                StaticMesh.CollisionBox = _workingStaticMesh.CollisionBox;

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void UpdateVisibilityBoxUI()
        {
            tbVisibilityBoxMinX.Text = _workingStaticMesh.VisibilityBox.Minimum.X.ToString();
            tbVisibilityBoxMinY.Text = _workingStaticMesh.VisibilityBox.Minimum.Y.ToString();
            tbVisibilityBoxMinZ.Text = _workingStaticMesh.VisibilityBox.Minimum.Z.ToString();
            tbVisibilityBoxMaxX.Text = _workingStaticMesh.VisibilityBox.Maximum.X.ToString();
            tbVisibilityBoxMaxY.Text = _workingStaticMesh.VisibilityBox.Maximum.Y.ToString();
            tbVisibilityBoxMaxZ.Text = _workingStaticMesh.VisibilityBox.Maximum.Z.ToString();
        }

        private void butCalculateVisibilityBox_Click(object sender, EventArgs e)
        {
            _workingStaticMesh.VisibilityBox = Wad2.CalculateBoundingBox(StaticMesh.Mesh, panelRendering.GizmoTransform);
            UpdateVisibilityBoxUI();
            panelRendering.Invalidate();
        }

        private void UpdateCollisionBoxUI()
        {
            tbCollisionBoxMinX.Text = _workingStaticMesh.CollisionBox.Minimum.X.ToString();
            tbCollisionBoxMinY.Text = _workingStaticMesh.CollisionBox.Minimum.Y.ToString();
            tbCollisionBoxMinZ.Text = _workingStaticMesh.CollisionBox.Minimum.Z.ToString();
            tbCollisionBoxMaxX.Text = _workingStaticMesh.CollisionBox.Maximum.X.ToString();
            tbCollisionBoxMaxY.Text = _workingStaticMesh.CollisionBox.Maximum.Y.ToString();
            tbCollisionBoxMaxZ.Text = _workingStaticMesh.CollisionBox.Maximum.Z.ToString();
        }

        private void butCalculateCollisionBox_Click(object sender, EventArgs e)
        {
            _workingStaticMesh.CollisionBox = Wad2.CalculateBoundingBox(StaticMesh.Mesh, panelRendering.GizmoTransform);
            UpdateCollisionBoxUI();
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

        private void cbDrawGizmo_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawGizmo = cbDrawGizmo.Checked;
            panelRendering.Invalidate();
        }
    }
}
