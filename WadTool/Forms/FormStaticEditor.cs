using DarkUI.Forms;
using SharpDX.Toolkit.Graphics;
using System;
using System.Numerics;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.Wad;
using WadTool.Controls;
using TombLib.Utils;
using System.Threading;

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

            panelRendering.Configuration = _tool.Configuration;
            panelRendering.Static = _workingStatic;
            panelRendering.DrawGrid = true;
            panelRendering.DrawGizmo = true;
            panelRendering.DrawLights = true;
            comboLightType.SelectedIndex = (int)_workingStatic.Mesh.LightingType;
            UpdateVisibilityBoxUI();
            UpdateCollisionBoxUI();
            UpdatePositionUI();
            UpdateLightsList();
            UpdateLightUI();

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

            _tool.ToggleUnsavedChanges();

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
            nudVisBoxMinX.Value = (decimal)_workingStatic.VisibilityBox.Minimum.X;
            nudVisBoxMinY.Value = (decimal)_workingStatic.VisibilityBox.Minimum.Y;
            nudVisBoxMinZ.Value = (decimal)_workingStatic.VisibilityBox.Minimum.Z;
            nudVisBoxMaxX.Value = (decimal)_workingStatic.VisibilityBox.Maximum.X;
            nudVisBoxMaxY.Value = (decimal)_workingStatic.VisibilityBox.Maximum.Y;
            nudVisBoxMaxZ.Value = (decimal)_workingStatic.VisibilityBox.Maximum.Z;
        }

        private void butCalculateVisibilityBox_Click(object sender, EventArgs e)
        {
            _workingStatic.VisibilityBox = _workingStatic.Mesh.CalculateBoundingBox(panelRendering.GizmoTransform);
            UpdateVisibilityBoxUI();
            panelRendering.Invalidate();
        }

        private void UpdateCollisionBoxUI()
        {
            nudColBoxMinX.Value = (decimal)_workingStatic.CollisionBox.Minimum.X;
            nudColBoxMinY.Value = (decimal)_workingStatic.CollisionBox.Minimum.Y;
            nudColBoxMinZ.Value = (decimal)_workingStatic.CollisionBox.Minimum.Z;
            nudColBoxMaxX.Value = (decimal)_workingStatic.CollisionBox.Maximum.X;
            nudColBoxMaxY.Value = (decimal)_workingStatic.CollisionBox.Maximum.Y;
            nudColBoxMaxZ.Value = (decimal)_workingStatic.CollisionBox.Maximum.Z;
        }

        private void butCalculateCollisionBox_Click(object sender, EventArgs e)
        {
            _workingStatic.CollisionBox = _workingStatic.Mesh.CalculateBoundingBox(panelRendering.GizmoTransform);
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
            if (!_doChangesInLighting || panelRendering.SelectedLight == null)
                return;

            panelRendering.SelectedLight.Intensity = (float)numIntensity.Value;
            panelRendering.UpdateLights();
        }

        private void numInnerRange_ValueChanged(object sender, EventArgs e)
        {
            if (!_doChangesInLighting || panelRendering.SelectedLight == null)
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
                    form.AddPreset(IOSettingsPresets.GeometryImportSettingsPresets);
                    if (form.ShowDialog(this) != DialogResult.OK)
                        return;
                    var mesh = WadMesh.ImportFromExternalModel(dialog.FileName, form.Settings);
                    if (mesh == null)
                    {
                        DarkMessageBox.Show(this, "Error while loading 3D model. Check that the file format \n" +
                                            "is supported, meshes are textured and texture file is present.",
                                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    _workingStatic.Mesh = mesh;
                    _workingStatic.VisibilityBox = _workingStatic.Mesh.CalculateBoundingBox(panelRendering.GizmoTransform);
                    _workingStatic.CollisionBox = _workingStatic.Mesh.CalculateBoundingBox(panelRendering.GizmoTransform);
                    _workingStatic.Version = DataVersion.GetNext();
                    _workingStatic.Mesh.CalculateNormals();

                    panelRendering.Invalidate();
                    UpdatePositionUI();
                    UpdateCollisionBoxUI();
                    UpdateVisibilityBoxUI();
                    UpdateLightUI();
                }
            }
        }

        private void nudVisBoxMinX_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMinX.Value;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(result,
                                                                       _workingStatic.VisibilityBox.Minimum.Y,
                                                                       _workingStatic.VisibilityBox.Minimum.Z),
                                                           new Vector3(_workingStatic.VisibilityBox.Maximum.X,
                                                                       _workingStatic.VisibilityBox.Maximum.Y,
                                                                       _workingStatic.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudVisBoxMinY_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMinY.Value;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(_workingStatic.VisibilityBox.Minimum.X,
                                                                       result,
                                                                       _workingStatic.VisibilityBox.Minimum.Z),
                                                           new Vector3(_workingStatic.VisibilityBox.Maximum.X,
                                                                       _workingStatic.VisibilityBox.Maximum.Y,
                                                                       _workingStatic.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudVisBoxMinZ_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMinZ.Value;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(_workingStatic.VisibilityBox.Minimum.X,
                                                                       _workingStatic.VisibilityBox.Minimum.Y,
                                                                       result),
                                                           new Vector3(_workingStatic.VisibilityBox.Maximum.X,
                                                                       _workingStatic.VisibilityBox.Maximum.Y,
                                                                       _workingStatic.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }



        private void nudVisBoxMaxX_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMaxX.Value;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(_workingStatic.VisibilityBox.Minimum.X,
                                                                     _workingStatic.VisibilityBox.Minimum.Y,
                                                                     _workingStatic.VisibilityBox.Minimum.Z),
                                                         new Vector3(result,
                                                                     _workingStatic.VisibilityBox.Maximum.Y,
                                                                     _workingStatic.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudVisBoxMaxY_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMaxY.Value;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(_workingStatic.VisibilityBox.Minimum.X,
                                                                     _workingStatic.VisibilityBox.Minimum.Y,
                                                                     _workingStatic.VisibilityBox.Minimum.Z),
                                                         new Vector3(_workingStatic.VisibilityBox.Maximum.X,
                                                                     result,
                                                                     _workingStatic.VisibilityBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudVisBoxMaxZ_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudVisBoxMaxZ.Value;

            _workingStatic.VisibilityBox = new BoundingBox(new Vector3(_workingStatic.VisibilityBox.Minimum.X,
                                                                     _workingStatic.VisibilityBox.Minimum.Y,
                                                                     _workingStatic.VisibilityBox.Minimum.Z),
                                                         new Vector3(_workingStatic.VisibilityBox.Maximum.X,
                                                                     _workingStatic.VisibilityBox.Maximum.Y,
                                                                     result));
            panelRendering.Invalidate();
        }

        private void nudColBoxMinX_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMinX.Value;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(result,
                                                                     _workingStatic.CollisionBox.Minimum.Y,
                                                                     _workingStatic.CollisionBox.Minimum.Z),
                                                         new Vector3(_workingStatic.CollisionBox.Maximum.X,
                                                                     _workingStatic.CollisionBox.Maximum.Y,
                                                                     _workingStatic.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudColBoxMinY_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMinY.Value;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(_workingStatic.CollisionBox.Minimum.X,
                                                                     result,
                                                                     _workingStatic.CollisionBox.Minimum.Z),
                                                         new Vector3(_workingStatic.CollisionBox.Maximum.X,
                                                                     _workingStatic.CollisionBox.Maximum.Y,
                                                                     _workingStatic.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudColBoxMinZ_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMinZ.Value;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(_workingStatic.CollisionBox.Minimum.X,
                                                                     _workingStatic.CollisionBox.Minimum.Y,
                                                                     result),
                                                         new Vector3(_workingStatic.CollisionBox.Maximum.X,
                                                                     _workingStatic.CollisionBox.Maximum.Y,
                                                                     _workingStatic.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudColBoxMaxX_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMaxX.Value;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(_workingStatic.CollisionBox.Minimum.X,
                                                                     _workingStatic.CollisionBox.Minimum.Y,
                                                                     _workingStatic.CollisionBox.Minimum.Z),
                                                         new Vector3(result,
                                                                     _workingStatic.CollisionBox.Maximum.Y,
                                                                     _workingStatic.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudColBoxMaxY_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMaxY.Value;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(_workingStatic.CollisionBox.Minimum.X,
                                                                     _workingStatic.CollisionBox.Minimum.Y,
                                                                     _workingStatic.CollisionBox.Minimum.Z),
                                                         new Vector3(_workingStatic.CollisionBox.Maximum.X,
                                                                     result,
                                                                     _workingStatic.CollisionBox.Maximum.Z));
            panelRendering.Invalidate();
        }

        private void nudColBoxMaxZ_ValueChanged(object sender, EventArgs e)
        {
            var result = (float)nudColBoxMaxZ.Value;

            _workingStatic.CollisionBox = new BoundingBox(new Vector3(_workingStatic.CollisionBox.Minimum.X,
                                                                     _workingStatic.CollisionBox.Minimum.Y,
                                                                     _workingStatic.CollisionBox.Minimum.Z),
                                                         new Vector3(_workingStatic.CollisionBox.Maximum.X,
                                                                     _workingStatic.CollisionBox.Maximum.Y,
                                                                     result));
            panelRendering.Invalidate();
        }

        private void cbDrawNormals_CheckedChanged(object sender, EventArgs e)
        {
            panelRendering.DrawNormals = cbDrawNormals.Checked;
            panelRendering.Invalidate();
        }

        private void butDeleteLight_Click(object sender, EventArgs e)
        {
            if (!_doChangesInLighting)
                return;

            if (panelRendering.SelectedLight == null)
                return;

            WadLight light = panelRendering.SelectedLight;

            var node = lstLights.SelectedNodes[0];
            panelRendering.SelectedLight = null;
            panelRendering.DeleteLight(light);
            panelRendering.UpdateLights();
            UpdateLightsList();
            UpdateLightUI();
            panelRendering.Invalidate();
        }

        private void butRecalcNormals_Click(object sender, EventArgs e)
        {
            if (_workingStatic.Mesh != null)
            {
                _workingStatic.Mesh.CalculateNormals();
                panelRendering.UpdateLights();
                panelRendering.Invalidate();
            }
        }

        private void comboLightType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboLightType.SelectedIndex==(int)WadMeshLightingType.Normals)
            {
                _workingStatic.Mesh.LightingType = WadMeshLightingType.Normals;
                _workingStatic.Lights.Clear();
                butAddLight.Enabled = false;
                butDeleteLight.Enabled = false;
                numIntensity.Enabled = false;
                numRadius.Enabled = false;
                numAmbient.Enabled = false;
                lstLights.Enabled = false;
                UpdateLightsList();
                UpdateLightUI();
                panelRendering.Invalidate();
            }
            else
            {
                _workingStatic.Mesh.LightingType = WadMeshLightingType.VertexColors;
                butAddLight.Enabled = true;
                butDeleteLight.Enabled = true;
                numIntensity.Enabled = true;
                numRadius.Enabled = true;
                numAmbient.Enabled = true;
                lstLights.Enabled = true;
                UpdateLightUI();
                panelRendering.Invalidate();
            }
        }

        private void nudPosX_ValueChanged(object sender, EventArgs e)
        {

            panelRendering.StaticPosition = new Vector3((float)nudPosX.Value, panelRendering.StaticPosition.Y, panelRendering.StaticPosition.Z);
            panelRendering.Invalidate();
        }

        private void nudPosY_ValueChanged(object sender, EventArgs e)
        {

            panelRendering.StaticPosition = new Vector3(panelRendering.StaticPosition.X, (float)nudPosY.Value, panelRendering.StaticPosition.Z);
            panelRendering.Invalidate();
        }

        private void nudPosZ_ValueChanged(object sender, EventArgs e)
        {

            panelRendering.StaticPosition = new Vector3(panelRendering.StaticPosition.X, panelRendering.StaticPosition.Y, (float)nudPosZ.Value);
            panelRendering.Invalidate();
        }

        public void UpdatePositionUI()
        {
            nudPosX.Value = (decimal)panelRendering.StaticPosition.X;
            nudPosY.Value = (decimal)panelRendering.StaticPosition.Y;
            nudPosZ.Value = (decimal)panelRendering.StaticPosition.Z;
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void butClearCollisionBox_Click(object sender, EventArgs e)
        {

            _workingStatic.CollisionBox = new BoundingBox();
            UpdateCollisionBoxUI();
            panelRendering.Invalidate();
        }

        private void butClearVisibilityBox_Click(object sender, EventArgs e)
        {
            _workingStatic.VisibilityBox = new BoundingBox();
            UpdateVisibilityBoxUI();
            panelRendering.Invalidate();
        }

        private void butExportMeshToFile_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export mesh";
                saveFileDialog.Filter = BaseGeometryExporter.FileExtensions.GetFilter(true);
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = "mqo";
                saveFileDialog.FileName = _workingStatic.Mesh.Name;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var settingsDialog = new GeometryIOSettingsDialog(new IOGeometrySettings() { Export = true }))
                    {
                        settingsDialog.AddPreset(IOSettingsPresets.RoomExportSettingsPresets);
                        settingsDialog.SelectPreset("Normal scale");

                        if (settingsDialog.ShowDialog() == DialogResult.OK)
                        {
                            BaseGeometryExporter.GetTextureDelegate getTextureCallback = txt =>
                            {
                                return "";
                            };

                            BaseGeometryExporter exporter = BaseGeometryExporter.CreateForFile(saveFileDialog.FileName, settingsDialog.Settings, getTextureCallback);
                            new Thread(() =>
                            {
                                var resultModel = WadMesh.PrepareForExport(saveFileDialog.FileName, _workingStatic.Mesh);

                                if (resultModel != null)
                                {
                                    if (exporter.ExportToFile(resultModel, saveFileDialog.FileName))
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    string errorMessage = "";
                                    return;
                                }
                            }).Start();
                        }
                    }
                }
            }
        }
    }
}
