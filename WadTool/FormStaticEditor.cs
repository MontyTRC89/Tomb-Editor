using DarkUI.Forms;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.Wad;
using WadTool.Controls;
using TombLib.Utils;

namespace WadTool
{
    public partial class FormStaticEditor : DarkForm
    {
        public WadStatic Static { get; private set; }

        private readonly Wad2 _wad;
        private readonly WadStatic _workingStatic;
        private readonly WadToolClass _tool;
        private readonly GraphicsDevice _device;
        private bool _doChangesInLighting = false;

        public FormStaticEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadStatic @static)
        {
            _doChangesInLighting = false;

            InitializeComponent();

            _wad = wad;
            _tool = tool;
            _device = deviceManager.___LegacyDevice;

            Static = @static;

            _workingStatic = @static.Clone();
            panelRendering.InitializeRendering(tool, deviceManager);

            panelRendering.Static = _workingStatic;
            panelRendering.DrawGrid = true;
            panelRendering.DrawGizmo = true;
            panelRendering.DrawLights = true;
            UpdateVisibilityBoxUI();
            UpdateCollisionBoxUI();
            UpdateLightsList();

            numAmbient.Value = (decimal)_workingStatic.AmbientLight;

            _tool.EditorEventRaised += Tool_EditorEventRaised;

            _doChangesInLighting = true;
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.StaticSelectedLightChangedEvent)
                UpdateLightUI();
            else if (obj is WadToolClass.StaticLightsChangedEvent)
            {
                UpdateLightsList();
                UpdateLightUI();
                _workingStatic.Version = DataVersion.GetNext();
                panelRendering.Invalidate();
            }
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
            }

            // Assign the edited mesh to original static mesh
            _wad.Statics.Remove(_workingStatic.Id);
            _wad.Statics.Add(_workingStatic.Id, _workingStatic);

            _workingStatic.Version = DataVersion.GetNext();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void UpdateLightsList()
        {
            lstLights.Nodes.Clear();
            foreach (var light in _workingStatic.Lights)
            {
                var node = new DarkUI.Controls.DarkTreeNode("Light #" + _workingStatic.Lights.IndexOf(light));
                node.Tag = light;
                lstLights.Nodes.Add(node);
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

        private void butResetTranslation_Click(object sender, EventArgs e)
        {
            panelRendering.StaticPosition = Vector3.Zero;
            panelRendering.Invalidate();
        }

        private void butResetRotation_Click(object sender, EventArgs e)
        {
            panelRendering.StaticRotation = Vector3.Zero;
            panelRendering.Invalidate();
        }

        private void butResetScale_Click(object sender, EventArgs e)
        {
            panelRendering.StaticScale = 1.0f;
            panelRendering.Invalidate();
        }

        private void butAddLight_Click(object sender, EventArgs e)
        {
            panelRendering.Action = PanelRenderingStaticEditor.StaticEditorAction.PlaceLight;
        }

        public void UpdateLightUI()
        {
            if (panelRendering.SelectedLight != null)
            {
                numIntensity.Enabled = true;
                numIntensity.Value = (decimal)panelRendering.SelectedLight?.Intensity;
                numRadius.Enabled = true;
                numRadius.Value = (decimal)panelRendering.SelectedLight?.Radius;
            }
            else
            {
                numIntensity.Enabled = false;
                numRadius.Enabled = false;
            }
        }

        private void numIntensity_ValueChanged(object sender, EventArgs e)
        {
            if (!_doChangesInLighting)
                return;

            panelRendering.SelectedLight.Intensity = (float)numIntensity.Value;
            panelRendering.UpdateLights();
        }

        private void numInnerRange_ValueChanged(object sender, EventArgs e)
        {
            if (!_doChangesInLighting)
                return;

            panelRendering.SelectedLight.Radius = (float)numRadius.Value;
            panelRendering.UpdateLights();
        }

        private void numAmbient_ValueChanged(object sender, EventArgs e)
        {
            if (!_doChangesInLighting)
                return;

            _workingStatic.AmbientLight = (short)numAmbient.Value;
            panelRendering.UpdateLights();
        }

        private void cbDrawLights_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawLights = cbDrawLights.Checked;
            panelRendering.Invalidate();
        }

        private void FormStaticEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
                _workingStatic.Version = DataVersion.GetNext();
        }

        private void lstLights_Click(object sender, EventArgs e)
        {
            if (lstLights.SelectedNodes.Count == 0)
                return;

            var node = lstLights.SelectedNodes[0];
            panelRendering.SelectedLight = (WadLight)node.Tag;
            panelRendering.Invalidate();
        }

        private void butImportMeshFromFile_Click(object sender, EventArgs e)
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
                    _workingStatic.Mesh = WadMesh.ImportFromExternalModel(dialog.FileName, form.Settings);
                    _workingStatic.VisibilityBox = _workingStatic.Mesh.BoundingBox;
                    _workingStatic.CollisionBox = _workingStatic.Mesh.BoundingBox;
                    _workingStatic.Version = DataVersion.GetNext();
                    _workingStatic.Mesh.RecalculateNormals();
                    panelRendering.Invalidate();
                }
            }
        }

        private void tbVisibilityBoxMinX_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbVisibilityBoxMinX.Text, out result))
                return;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(result,
                                                                     _workingStatic.VisibilityBox.Minimum.Y,
                                                                     _workingStatic.VisibilityBox.Minimum.Z),
                                                         new Vector3(_workingStatic.VisibilityBox.Maximum.X,
                                                                     _workingStatic.VisibilityBox.Maximum.Y,
                                                                     _workingStatic.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void tbVisibilityBoxMinY_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbVisibilityBoxMinY.Text, out result))
                return;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(_workingStatic.VisibilityBox.Minimum.X,
                                                                     result,
                                                                     _workingStatic.VisibilityBox.Minimum.Z),
                                                         new Vector3(_workingStatic.VisibilityBox.Maximum.X,
                                                                     _workingStatic.VisibilityBox.Maximum.Y,
                                                                     _workingStatic.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void tbVisibilityBoxMinZ_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbVisibilityBoxMinZ.Text, out result))
                return;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(_workingStatic.VisibilityBox.Minimum.X,
                                                                     _workingStatic.VisibilityBox.Minimum.Y,
                                                                     result),
                                                         new Vector3(_workingStatic.VisibilityBox.Maximum.X,
                                                                     _workingStatic.VisibilityBox.Maximum.Y,
                                                                     _workingStatic.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void tbVisibilityBoxMaxX_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbVisibilityBoxMaxX.Text, out result))
                return;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(_workingStatic.VisibilityBox.Minimum.X,
                                                                     _workingStatic.VisibilityBox.Minimum.Y,
                                                                     _workingStatic.VisibilityBox.Minimum.Z),
                                                         new Vector3(result,
                                                                     _workingStatic.VisibilityBox.Maximum.Y,
                                                                     _workingStatic.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void tbVisibilityBoxMaxY_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbVisibilityBoxMaxY.Text, out result))
                return;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(_workingStatic.VisibilityBox.Minimum.X,
                                                                     _workingStatic.VisibilityBox.Minimum.Y,
                                                                     _workingStatic.VisibilityBox.Minimum.Z),
                                                         new Vector3(_workingStatic.VisibilityBox.Maximum.X,
                                                                     result,
                                                                     _workingStatic.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void tbVisibilityBoxMaxZ_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbVisibilityBoxMaxZ.Text, out result))
                return;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(_workingStatic.VisibilityBox.Minimum.X,
                                                                     _workingStatic.VisibilityBox.Minimum.Y,
                                                                     _workingStatic.VisibilityBox.Minimum.Z),
                                                         new Vector3(_workingStatic.VisibilityBox.Maximum.X,
                                                                     _workingStatic.VisibilityBox.Maximum.Y,
                                                                     result));
            panelRendering.Invalidate();
        }

        private void tbCollisionBoxMinX_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinX.Text, out result))
                return;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(result,
                                                                     _workingStatic.CollisionBox.Minimum.Y,
                                                                     _workingStatic.CollisionBox.Minimum.Z),
                                                         new Vector3(_workingStatic.CollisionBox.Maximum.X,
                                                                     _workingStatic.CollisionBox.Maximum.Y,
                                                                     _workingStatic.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void tbCollisionBoxMinY_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinY.Text, out result))
                return;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(_workingStatic.CollisionBox.Minimum.X,
                                                                     result,
                                                                     _workingStatic.CollisionBox.Minimum.Z),
                                                         new Vector3(_workingStatic.CollisionBox.Maximum.X,
                                                                     _workingStatic.CollisionBox.Maximum.Y,
                                                                     _workingStatic.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void tbCollisionBoxMinZ_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinZ.Text, out result))
                return;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(_workingStatic.CollisionBox.Minimum.X,
                                                                     _workingStatic.CollisionBox.Minimum.Y,
                                                                     result),
                                                         new Vector3(_workingStatic.CollisionBox.Maximum.X,
                                                                     _workingStatic.CollisionBox.Maximum.Y,
                                                                     _workingStatic.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void tbCollisionBoxMaxX_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxX.Text, out result))
                return;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(_workingStatic.CollisionBox.Minimum.X,
                                                                     _workingStatic.CollisionBox.Minimum.Y,
                                                                     _workingStatic.CollisionBox.Minimum.Z),
                                                         new Vector3(result,
                                                                     _workingStatic.CollisionBox.Maximum.Y,
                                                                     _workingStatic.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void tbCollisionBoxMaxY_TextChanged(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxY.Text, out result))
                return;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(_workingStatic.CollisionBox.Minimum.X,
                                                                     _workingStatic.CollisionBox.Minimum.Y,
                                                                     _workingStatic.CollisionBox.Minimum.Z),
                                                         new Vector3(_workingStatic.CollisionBox.Maximum.X,
                                                                     result,
                                                                     _workingStatic.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void tbCollisionBoxMaxZ_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxZ.Text, out result))
                return;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(_workingStatic.CollisionBox.Minimum.X,
                                                                     _workingStatic.CollisionBox.Minimum.Y,
                                                                     _workingStatic.CollisionBox.Minimum.Z),
                                                         new Vector3(_workingStatic.CollisionBox.Maximum.X,
                                                                     _workingStatic.CollisionBox.Maximum.Y,
                                                                     result));
            panelRendering.Invalidate();
        }
    }
}
