using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormStaticEditor : DarkForm
    {
        public WadStatic Static { get; private set; }

        private readonly Wad2 _wad;
        private readonly WadStatic _workingStatic;

        public FormStaticEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadStatic @static)
        {
            InitializeComponent();

            _wad = wad;
            Static = @static;

            _workingStatic = @static.Clone();
            panelRendering.InitializePanel(tool, deviceManager);

            panelRendering.Static = _workingStatic;
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
                for (int i = 0; i < _workingStatic.Mesh.VerticesPositions.Count; i++)
                {
                    var position = MathC.HomogenousTransform(_workingStatic.Mesh.VerticesPositions[i], transform);
                    _workingStatic.Mesh.VerticesPositions[i] = new Vector3(position.X, position.Y, position.Z);
                }

                for (int i = 0; i < _workingStatic.Mesh.VerticesNormals.Count; i++)
                {
                    var normal = MathC.HomogenousTransform(_workingStatic.Mesh.VerticesNormals[i], transform);
                    _workingStatic.Mesh.VerticesNormals[i] = new Vector3(normal.X, normal.Y, normal.Z);
                }

                _workingStatic.Mesh.UpdateHash();

                // Now check in Wad2 for already existing mesh
                var movebles = new List<WadMoveable>();
                var statics = new List<WadStatic>();

                foreach (var moveable in _wad.Moveables)
                {
                    foreach (var moveableMesh in moveable.Value.Meshes)
                    {
                        if (moveableMesh.Hash == Static.Mesh.Hash)
                        {
                            movebles.Add(moveable.Value);
                        }
                    }
                }

                foreach (var @static in _wad.Statics)
                {
                    if (@static.Value.Id != Static.Id &&
                        @static.Value.Mesh.Hash == Static.Mesh.Hash)
                    {
                        statics.Add(@static.Value);
                    }
                }

                // Assign the edited mesh to original static mesh
                _wad.Statics.Remove(_workingStatic.Id);
                _wad.Statics.Add(_workingStatic.Id, _workingStatic);
                _wad.PrepareDataForDirectX();

                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void UpdateVisibilityBoxUI()
        {
            tbVisibilityBoxMinX.Text = _workingStatic.VisibilityBox.Minimum.X.ToString();
            tbVisibilityBoxMinY.Text = _workingStatic.VisibilityBox.Minimum.Y.ToString();
            tbVisibilityBoxMinZ.Text = _workingStatic.VisibilityBox.Minimum.Z.ToString();
            tbVisibilityBoxMaxX.Text = _workingStatic.VisibilityBox.Maximum.X.ToString();
            tbVisibilityBoxMaxY.Text = _workingStatic.VisibilityBox.Maximum.Y.ToString();
            tbVisibilityBoxMaxZ.Text = _workingStatic.VisibilityBox.Maximum.Z.ToString();
        }

        private void butCalculateVisibilityBox_Click(object sender, EventArgs e)
        {
            _workingStatic.VisibilityBox = Static.Mesh.CalculateBoundingBox(panelRendering.GizmoTransform);
            UpdateVisibilityBoxUI();
            panelRendering.Invalidate();
        }

        private void UpdateCollisionBoxUI()
        {
            tbCollisionBoxMinX.Text = _workingStatic.CollisionBox.Minimum.X.ToString();
            tbCollisionBoxMinY.Text = _workingStatic.CollisionBox.Minimum.Y.ToString();
            tbCollisionBoxMinZ.Text = _workingStatic.CollisionBox.Minimum.Z.ToString();
            tbCollisionBoxMaxX.Text = _workingStatic.CollisionBox.Maximum.X.ToString();
            tbCollisionBoxMaxY.Text = _workingStatic.CollisionBox.Maximum.Y.ToString();
            tbCollisionBoxMaxZ.Text = _workingStatic.CollisionBox.Maximum.Z.ToString();
        }

        private void butCalculateCollisionBox_Click(object sender, EventArgs e)
        {
            _workingStatic.CollisionBox = Static.Mesh.CalculateBoundingBox(panelRendering.GizmoTransform);
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
