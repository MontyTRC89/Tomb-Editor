using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Docking;
using TombEditor.Geometry;
using TombEditor.Controls;
using TombLib.Utils;
using DarkUI.Forms;
using SharpDX;

namespace TombEditor.ToolWindows
{
    public partial class MainView : DarkDocument
    {
        private Editor _editor;

        public MainView()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        public void Initialize(DeviceManager _deviceManager)
        {
            panel3D.InitializePanel(_deviceManager);

            // Update 3D view
            EditorActions.SwitchMode(EditorMode.Geometry);
        }

        public void AddToolbox(FloatingToolbox toolbox)
        {
            panel3D.Controls.Add(toolbox);
        }

        public void RemoveToolbox(FloatingToolbox toolbox)
        {
            panel3D.Controls.Remove(toolbox);
        }

        public void MoveObjectRelative(PositionBasedObjectInstance instance, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            if (panel3D.Camera.RotationY < (Math.PI * (1.0 / 4.0)))
                EditorActions.MoveObjectRelative(instance, pos, precision, canGoOutsideRoom);
            else if (panel3D.Camera.RotationY < (Math.PI * (3.0 / 4.0)))
                EditorActions.MoveObjectRelative(instance, new Vector3(pos.Z, pos.Y, -pos.X), new Vector3(precision.Z, precision.Y, -precision.X), canGoOutsideRoom);
            else if (panel3D.Camera.RotationY < (Math.PI * (5.0 / 4.0)))
                EditorActions.MoveObjectRelative(instance, new Vector3(-pos.X, pos.Y, -pos.Z), new Vector3(-precision.X, precision.Y, -precision.Z), canGoOutsideRoom);
            else if (panel3D.Camera.RotationY < (Math.PI * (7.0 / 4.0)))
                EditorActions.MoveObjectRelative(instance, new Vector3(-pos.Z, pos.Y, pos.X), new Vector3(-precision.Z, precision.Y, precision.X), canGoOutsideRoom);
            else
                EditorActions.MoveObjectRelative(instance, pos, precision, canGoOutsideRoom);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && (components != null))
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

            if (obj is Editor.SelectedSectorsChangedEvent)
            {
                //bool validSectorSelection = _editor.SelectedSectors.Valid;

                //butTextureFloor.Enabled = validSectorSelection;
                //butTextureCeiling.Enabled = validSectorSelection;
                //butTextureWalls.Enabled = validSectorSelection;
            }

            if (obj is Editor.ModeChangedEvent)
            {
                EditorMode mode = ((Editor.ModeChangedEvent)obj).Current;
                butCenterCamera.Enabled = mode != EditorMode.Map2D;
            }

            // Update editor mode
            if (obj is Editor.ModeChangedEvent)
            {
                EditorMode mode = ((Editor.ModeChangedEvent)obj).Current;
                but2D.Checked = mode == EditorMode.Map2D;
                but3D.Checked = mode == EditorMode.Geometry;
                butLightingMode.Checked = mode == EditorMode.Lighting;
                butFaceEdit.Checked = mode == EditorMode.FaceEdit;

                panel2DMap.Visible = mode == EditorMode.Map2D;
                panel3D.Visible = (mode == EditorMode.FaceEdit) || (mode == EditorMode.Geometry) || (mode == EditorMode.Lighting);
                
                butTextureFloor.Enabled = mode == EditorMode.FaceEdit;
                butTextureCeiling.Enabled = mode == EditorMode.FaceEdit;
                butTextureWalls.Enabled = mode == EditorMode.FaceEdit;
            }

            // Update flipmap toolbar button
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomPropertiesChangedEvent))
            {
                butFlipMap.Enabled = _editor.SelectedRoom.Flipped;
                butFlipMap.Checked = _editor.SelectedRoom.AlternateBaseRoom != null;
            }

            // Update texture properties
            if (obj is Editor.SelectedTexturesChangedEvent)
            {
                var e = (Editor.SelectedTexturesChangedEvent)obj;
                butAdditiveBlending.Checked = e.Current.BlendMode == BlendMode.Additive;
                butDoubleSided.Checked = e.Current.DoubleSided;
            }

            // Update portal opacity controls
            if ((obj is Editor.ObjectChangedEvent) ||
               (obj is Editor.SelectedObjectChangedEvent))
            {
                var portal = _editor.SelectedObject as PortalInstance;
                butOpacityNone.Enabled = portal != null;
                butOpacitySolidFaces.Enabled = portal != null;
                butOpacityTraversableFaces.Enabled = portal != null;

                butOpacityNone.Checked = portal == null ? false : portal.Opacity == PortalOpacity.None;
                butOpacitySolidFaces.Checked = portal == null ? false : portal.Opacity == PortalOpacity.SolidFaces;
                butOpacityTraversableFaces.Checked = portal == null ? false : portal.Opacity == PortalOpacity.TraversableFaces;
            }
        }

        private void but3D_Click(object sender, EventArgs e)
        {
            EditorActions.SwitchMode(EditorMode.Geometry);
        }

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
            EditorActions.TexturizeAllFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedTexture);
        }

        private void butTextureCeiling_Click(object sender, EventArgs e)
        {
            EditorActions.TexturizeAllCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedTexture);
        }

        private void butTextureWalls_Click(object sender, EventArgs e)
        {
            EditorActions.TexturizeAllWalls(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedTexture);
        }

        private void butAdditiveBlending_Click(object sender, EventArgs e)
        {
            var selectedTexture = _editor.SelectedTexture;
            selectedTexture.BlendMode = butAdditiveBlending.Checked ? BlendMode.Additive : BlendMode.Normal;
            _editor.SelectedTexture = selectedTexture;
        }

        private void butDoubleSided_Click(object sender, EventArgs e)
        {
            var selectedTexture = _editor.SelectedTexture;
            selectedTexture.DoubleSided = butDoubleSided.Checked;
            _editor.SelectedTexture = selectedTexture;
        }

        private void butAddCamera_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceCamera };
        }

        private void butAddFlybyCamera_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceFlyByCamera };
        }

        private void butAddSoundSource_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceSoundSource };
        }

        private void butAddSink_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceSink };
        }

        private void butCompileLevel_Click(object sender, EventArgs e)
        {
            EditorActions.BuildLevel(false);
        }

        private void butCompileLevelAndPlay_Click(object sender, EventArgs e)
        {
            EditorActions.BuildLevelAndPlay();
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
            EditorActions.Copy(this.ParentForm);
        }

        private void butPaste_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.Paste };
        }

        private void butStamp_Click(object sender, EventArgs e)
        {
            EditorActions.Clone(this.ParentForm);
        }

        private void butAddImportedGeometry_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceImportedGeometry };
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
            panel3D.Invalidate();
        }

        private void butDrawStatics_Click(object sender, EventArgs e)
        {
            panel3D.ShowStatics = !panel3D.ShowStatics;
            butDrawStatics.Checked = panel3D.ShowStatics;
            panel3D.Invalidate();
        }

        private void butDrawImportedGeometry_Click(object sender, EventArgs e)
        {
            panel3D.ShowImportedGeometry = !panel3D.ShowImportedGeometry;
            butDrawImportedGeometry.Checked = panel3D.ShowImportedGeometry;
            panel3D.Invalidate();
        }

        private void butDrawOther_Click(object sender, EventArgs e)
        {
            panel3D.ShowOtherObjects = !panel3D.ShowOtherObjects;
            butDrawOther.Checked = panel3D.ShowOtherObjects;
            panel3D.Invalidate();
        }
    }
}
