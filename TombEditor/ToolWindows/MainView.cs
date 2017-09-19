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
using TombLib.Utils;


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

        public void Initialize3D(DeviceManager _deviceManager)
        {
            panel3D.InitializePanel(_deviceManager);

            // Update 3D view
            but3D_Click(null, null);
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
            }

            // Update flipmap toolbar button
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.RoomPropertiesChangedEvent))
            {
                Room room = ((IEditorRoomChangedEvent)obj).Room;
                butFlipMap.Checked = room.Flipped && (room.AlternateRoom == null);
            }

            // Update texture properties
            if (obj is Editor.SelectedTexturesChangedEvent)
            {
                var e = (Editor.SelectedTexturesChangedEvent)obj;
                butAdditiveBlending.Checked = e.Current.BlendMode == BlendMode.Additive;
                butDoubleSided.Checked = e.Current.DoubleSided;
                butInvisible.Checked = e.Current.Texture == TextureInvisible.Instance;
            }

            // Update portal opacity controls
            if ((obj is Editor.ObjectChangedEvent) ||
               (obj is Editor.SelectedObjectChangedEvent))
            {
                var portal = _editor.SelectedObject as Portal;
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
            _editor.Mode = EditorMode.Geometry;
            _editor.Action = EditorAction.None;
        }

        private void but2D_Click(object sender, EventArgs e)
        {
            _editor.Mode = EditorMode.Map2D;
            _editor.Action = EditorAction.None;
        }

        private void butFaceEdit_Click(object sender, EventArgs e)
        {
            _editor.Mode = EditorMode.FaceEdit;
            _editor.Action = EditorAction.None;
        }

        private void butLightingMode_Click(object sender, EventArgs e)
        {
            _editor.Mode = EditorMode.Lighting;
            _editor.Action = EditorAction.None;
        }

        private void butCenterCamera_Click(object sender, EventArgs e)
        {
            _editor.CenterCamera();
        }

        private void butDrawPortals_Click(object sender, EventArgs e)
        {
            panel3D.DrawPortals = !panel3D.DrawPortals;
            butDrawPortals.Checked = panel3D.DrawPortals;
            panel3D.Invalidate();
        }

        private void butOpacityNone_Click(object sender, EventArgs e)
        {
            SetPortalOpacity(PortalOpacity.None);
        }

        private void butOpacitySolidFaces_Click(object sender, EventArgs e)
        {
            SetPortalOpacity(PortalOpacity.SolidFaces);
        }

        private void butOpacityTraversableFaces_Click(object sender, EventArgs e)
        {
            SetPortalOpacity(PortalOpacity.TraversableFaces);
        }

        private void butTextureFloor_Click(object sender, EventArgs e)
        {
            EditorActions.TextureFloor();
        }

        private void butTextureCeiling_Click(object sender, EventArgs e)
        {
        }

        private void butTextureWalls_Click(object sender, EventArgs e)
        {
            EditorActions.TextureWalls();
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

        private void butInvisible_Click(object sender, EventArgs e)
        {
            var selectedTexture = _editor.SelectedTexture;
            selectedTexture.Texture = TextureInvisible.Instance;
            _editor.SelectedTexture = selectedTexture;
        }

        private void SetPortalOpacity(PortalOpacity opacity)
        {
            var portal = _editor.SelectedObject as Portal;
            if ((_editor.SelectedRoom == null) || (portal == null))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have to select a portal first",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }
            EditorActions.SetPortalOpacity(_editor.SelectedRoom, portal, opacity);
        }

        private void butAddCamera_Click(object sender, EventArgs e)
        {
            EditorActions.AddCamera();
        }

        private void butAddFlybyCamera_Click(object sender, EventArgs e)
        {
            EditorActions.AddFlybyCamera();
        }

        private void butAddSoundSource_Click(object sender, EventArgs e)
        {
            EditorActions.AddSoundSource();
        }

        private void butAddSink_Click(object sender, EventArgs e)
        {
            EditorActions.AddSink();
        }

        private void butCompileLevel_Click(object sender, EventArgs e)
        {
            EditorActions.BuildLevel(false);
        }

        private void butCompileLevelAndPlay_Click(object sender, EventArgs e)
        {
            
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
            EditorActions.Paste();
        }

        private void butClone_Click(object sender, EventArgs e)
        {
            EditorActions.Clone(this.ParentForm);
        }
    }
}
