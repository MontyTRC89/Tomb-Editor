using DarkUI.Controls;
using DarkUI.Docking;
using System;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Forms;
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class MainView : DarkDocument
    {
        private readonly Editor _editor;
        private readonly PopUpInfo popup = new PopUpInfo();

        public MainView()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            ClipboardEvents.ClipboardChanged += ClipboardEvents_ClipboardChanged;
            ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);
        }

        public void InitializeRendering(RenderingDevice device)
        {
            panel3D.InitializeRendering(device, _editor.Configuration.Rendering3D_Antialias);
        }

        public void AddToolbox(DarkFloatingToolbox toolbox)
        {
            if(!panel3D.Contains(toolbox))
                panel3D.Controls.Add(toolbox);
        }

        public void RemoveToolbox(DarkFloatingToolbox toolbox)
        {
            if(panel3D.Controls.Contains(toolbox))
                panel3D.Controls.Remove(toolbox);
        }

        public void MoveObjectRelative(PositionBasedObjectInstance instance, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            if (panel3D.Camera.RotationY < Math.PI * (1.0 / 4.0))
                EditorActions.MoveObjectRelative(instance, pos, precision, canGoOutsideRoom);
            else if (panel3D.Camera.RotationY < Math.PI * (3.0 / 4.0))
                EditorActions.MoveObjectRelative(instance, new Vector3(pos.Z, pos.Y, -pos.X), new Vector3(precision.Z, precision.Y, -precision.X), canGoOutsideRoom);
            else if (panel3D.Camera.RotationY < Math.PI * (5.0 / 4.0))
                EditorActions.MoveObjectRelative(instance, new Vector3(-pos.X, pos.Y, -pos.Z), new Vector3(-precision.X, precision.Y, -precision.Z), canGoOutsideRoom);
            else if (panel3D.Camera.RotationY < Math.PI * (7.0 / 4.0))
                EditorActions.MoveObjectRelative(instance, new Vector3(-pos.Z, pos.Y, pos.X), new Vector3(-precision.Z, precision.Y, precision.X), canGoOutsideRoom);
            else
                EditorActions.MoveObjectRelative(instance, pos, precision, canGoOutsideRoom);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;
                ClipboardEvents.ClipboardChanged -= ClipboardEvents_ClipboardChanged;
            }
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Gray out menu options that do not apply
            if (obj is Editor.SelectedObjectChangedEvent)
            {
                ObjectInstance selectedObject = _editor.SelectedObject;
                butCopy.Enabled = selectedObject is PositionBasedObjectInstance;
                butStamp.Enabled = selectedObject is PositionBasedObjectInstance;
            }

            if (obj is Editor.ModeChangedEvent)
                ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

            if (obj is Editor.SelectedSectorsChangedEvent)
            {
                bool validSectorSelection = _editor.SelectedSectors.Valid;

                butTextureFloor.Enabled = validSectorSelection;
                butTextureCeiling.Enabled = validSectorSelection;
                butTextureWalls.Enabled = validSectorSelection;
            }

            // Update editor mode
            if (obj is Editor.ModeChangedEvent)
            {
                EditorMode mode = ((Editor.ModeChangedEvent)obj).Current;
                butCenterCamera.Enabled = mode != EditorMode.Map2D;
                but2D.Checked = mode == EditorMode.Map2D;
                but3D.Checked = mode == EditorMode.Geometry;
                butLightingMode.Checked = mode == EditorMode.Lighting;
                butFaceEdit.Checked = mode == EditorMode.FaceEdit;

                panel2DMap.Visible = mode == EditorMode.Map2D;
                panel3D.Visible = mode == EditorMode.FaceEdit || mode == EditorMode.Geometry || mode == EditorMode.Lighting;

                butTextureFloor.Enabled = mode == EditorMode.FaceEdit;
                butTextureCeiling.Enabled = mode == EditorMode.FaceEdit;
                butTextureWalls.Enabled = mode == EditorMode.FaceEdit;
            }

            // Update flipmap toolbar button
            if (obj is Editor.SelectedRoomChangedEvent ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomPropertiesChangedEvent))
            {
                butFlipMap.Enabled = _editor.SelectedRoom.Flipped;
                butFlipMap.Checked = _editor.SelectedRoom.AlternateBaseRoom != null;
            }

            // Update portal opacity controls
            if (obj is Editor.ObjectChangedEvent ||
               obj is Editor.SelectedObjectChangedEvent)
            {
                var portal = _editor.SelectedObject as PortalInstance;
                butOpacityNone.Enabled = portal != null;
                butOpacitySolidFaces.Enabled = portal != null;
                butOpacityTraversableFaces.Enabled = portal != null;

                butOpacityNone.Checked = portal != null && portal.Opacity == PortalOpacity.None;
                butOpacitySolidFaces.Checked = portal != null && portal.Opacity == PortalOpacity.SolidFaces;
                butOpacityTraversableFaces.Checked = portal != null && portal.Opacity == PortalOpacity.TraversableFaces;
            }

            if (obj is Editor.MessageEvent)
            {
                var msg = (Editor.MessageEvent)obj;

                switch(msg.Type)
                {
                    case PopUpInfo.PopupType.None:
                        popup.ShowSimple(this, msg.Message);
                        break;
                    case PopUpInfo.PopupType.Info:
                        popup.ShowInfo(this, msg.Message);
                        break;
                    case PopUpInfo.PopupType.Warning:
                        popup.ShowWarning(this, msg.Message);
                        break;
                    case PopUpInfo.PopupType.Error:
                        popup.ShowError(this, msg.Message);
                        break;
                }
            }
        }

        private void ClipboardEvents_ClipboardChanged(object sender, EventArgs e)
        {
            butPaste.Enabled = _editor.Mode != EditorMode.Map2D && Clipboard.ContainsData(typeof(ObjectClipboardData).FullName);
        }

        // Opens editor's 3D view
        private void but3D_Click(object sender, EventArgs e)
        {
            EditorActions.SwitchMode(EditorMode.Geometry);
        }

        // Opens editor's 2D view
        private void but2D_Click(object sender, EventArgs e)
        {
            EditorActions.SwitchMode(EditorMode.Map2D);
        }

        private void butFaceEdit_Click(object sender, EventArgs e)
        {
            EditorActions.SwitchMode(EditorMode.FaceEdit);
        }

        private void butLightingMode_Click(object sender, EventArgs e)
        {
            EditorActions.SwitchMode(EditorMode.Lighting);
        }

        private void butCenterCamera_Click(object sender, EventArgs e)
        {
            _editor.ResetCamera();
        }

        private void butOpacityNone_Click(object sender, EventArgs e)
        {
            EditorActions.SetPortalOpacity(PortalOpacity.None, this);
        }

        private void butOpacitySolidFaces_Click(object sender, EventArgs e)
        {
            EditorActions.SetPortalOpacity(PortalOpacity.SolidFaces, this);
        }

        private void butOpacityTraversableFaces_Click(object sender, EventArgs e)
        {
            EditorActions.SetPortalOpacity(PortalOpacity.TraversableFaces, this);
        }

        private void butTextureFloor_Click(object sender, EventArgs e)
        {
            EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Floor);
        }

        private void butTextureCeiling_Click(object sender, EventArgs e)
        {
            EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Ceiling);
        }

        private void butTextureWalls_Click(object sender, EventArgs e)
        {
            EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Wall);
        }

        private void butAddCamera_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorActionPlace(false, (l, r) => new CameraInstance());
        }

        private void butAddFlybyCamera_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorActionPlace(false, (l, r) => new FlybyCameraInstance());
        }

        private void butAddSoundSource_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorActionPlace(false, (l, r) => new SoundSourceInstance());
        }

        private void butAddSink_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorActionPlace(false, (l, r) => new SinkInstance());
        }

        private void butAddImportedGeometry_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorActionPlace(false, (l, r) => new ImportedGeometryInstance());
        }

        private void butCompileLevel_Click(object sender, EventArgs e)
        {
            EditorActions.BuildLevel(false, this);
        }

        private void butCompileLevelAndPlay_Click(object sender, EventArgs e)
        {
            EditorActions.BuildLevelAndPlay(this);
        }

        private void butFlipMap_Click(object sender, EventArgs e)
        {
            butFlipMap.Checked = !butFlipMap.Checked;

            if (butFlipMap.Checked)
            {
                if (_editor.SelectedRoom.Flipped && _editor.SelectedRoom.AlternateRoom != null)
                    _editor.SelectedRoom = _editor.SelectedRoom.AlternateRoom;
            }
            else
            {
                if (_editor.SelectedRoom.Flipped && _editor.SelectedRoom.AlternateBaseRoom != null)
                    _editor.SelectedRoom = _editor.SelectedRoom.AlternateBaseRoom;
            }
        }

        private void butCopy_Click(object sender, EventArgs e)
        {
            EditorActions.TryCopyObject(_editor.SelectedObject, ParentForm);
        }

        private void butStamp_Click(object sender, EventArgs e)
        {
            EditorActions.TryStampObject(_editor.SelectedObject, ParentForm);
        }

        private void butPaste_Click(object sender, EventArgs e)
        {
            ObjectClipboardData data = Clipboard.GetDataObject().GetData(typeof(ObjectClipboardData)) as ObjectClipboardData;
            if (data == null)
                MessageBox.Show("Clipboard contains no object data.");
            else
                _editor.Action = new EditorActionPlace(false, (level, room) => data.MergeGetSingleObject(_editor));
        }

        private void butDrawPortals_Click(object sender, EventArgs e)
        {
            panel3D.DrawPortals = !panel3D.DrawPortals;
            butDrawPortals.Checked = panel3D.DrawPortals;
            panel3D.Invalidate();
        }

        private void butDrawHorizon_Click(object sender, EventArgs e)
        {
            panel3D.DrawHorizon = !panel3D.DrawHorizon;
            butDrawHorizon.Checked = panel3D.DrawHorizon;
            panel3D.Invalidate();
        }

        private void butDrawRoomNames_Click(object sender, EventArgs e)
        {
            panel3D.DrawRoomNames = !panel3D.DrawRoomNames;
            butDrawRoomNames.Checked = panel3D.DrawRoomNames;
            panel3D.Invalidate();
        }

        private void butDrawIllegalSlopes_Click(object sender, EventArgs e)
        {
            panel3D.DrawIllegalSlopes = !panel3D.DrawIllegalSlopes;
            butDrawIllegalSlopes.Checked = panel3D.DrawIllegalSlopes;
            panel3D.Invalidate();
        }

        private void butDrawMoveables_Click(object sender, EventArgs e)
        {
            panel3D.ShowMoveables = !panel3D.ShowMoveables;
            butDrawMoveables.Checked = panel3D.ShowMoveables;
            if (_editor.SelectedObject is MoveableInstance) _editor.SelectedObject = null;
            panel3D.Invalidate();
        }

        private void butDrawStatics_Click(object sender, EventArgs e)
        {
            panel3D.ShowStatics = !panel3D.ShowStatics;
            butDrawStatics.Checked = panel3D.ShowStatics;
            if (_editor.SelectedObject is StaticInstance) _editor.SelectedObject = null;
            panel3D.Invalidate();
        }

        private void butDrawImportedGeometry_Click(object sender, EventArgs e)
        {
            panel3D.ShowImportedGeometry = !panel3D.ShowImportedGeometry;
            butDrawImportedGeometry.Checked = panel3D.ShowImportedGeometry;
            if (_editor.SelectedObject is ImportedGeometryInstance) _editor.SelectedObject = null;
            panel3D.Invalidate();
        }

        private void butDrawOther_Click(object sender, EventArgs e)
        {
            panel3D.ShowOtherObjects = !panel3D.ShowOtherObjects;
            butDrawOther.Checked = panel3D.ShowOtherObjects;
            if (_editor.SelectedObject is LightInstance ||
                _editor.SelectedObject is CameraInstance ||
                _editor.SelectedObject is FlybyCameraInstance ||
                _editor.SelectedObject is SinkInstance ||
                _editor.SelectedObject is SoundSourceInstance)
                    _editor.SelectedObject = null;
            panel3D.Invalidate();
        }

        private void butDrawSlideDirections_Click(object sender, EventArgs e)
        {
            panel3D.DrawSlideDirections = !panel3D.DrawSlideDirections;
            butDrawSlideDirections.Checked = panel3D.DrawSlideDirections;
            panel3D.Invalidate();
        }

        private void butDisableGeometryPicking_Click(object sender, EventArgs e)
        {
            panel3D.DisablePickingForImportedGeometry = !panel3D.DisablePickingForImportedGeometry;
            butDisableGeometryPicking.Checked = panel3D.DisablePickingForImportedGeometry;
            if (_editor.SelectedObject is ImportedGeometryInstance) _editor.SelectedObject = null;
            panel3D.Invalidate();
        }

        private void drawAllRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel3D.DrawAllRooms = !panel3D.DrawAllRooms;
            panel3D.Invalidate();
        }
    }
}
