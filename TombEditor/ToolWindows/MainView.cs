using DarkUI.Controls;
using DarkUI.Docking;
using System;
using System.Numerics;
using System.Windows.Forms;
using TombEditor.Controls;
using TombLib;
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

            RefreshControls(_editor.Configuration);
            GenerateToolStripCommands(toolStrip.Items);
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

        public void MoveRoomRelative(Room room, VectorInt3 pos)
        {
            if (panel3D.Camera.RotationY < Math.PI * (1.0 / 4.0))
                EditorActions.MoveRooms(pos, room.Versions);
            else if (panel3D.Camera.RotationY < Math.PI * (3.0 / 4.0))
                EditorActions.MoveRooms(new VectorInt3(pos.Z, pos.Y, -pos.X), room.Versions); // valid
            else if (panel3D.Camera.RotationY < Math.PI * (5.0 / 4.0))
                EditorActions.MoveRooms(new VectorInt3(-pos.X, pos.Y, -pos.Z), room.Versions); // valid
            else if (panel3D.Camera.RotationY < Math.PI * (7.0 / 4.0))
                EditorActions.MoveRooms(new VectorInt3(-pos.Z, pos.Y, pos.X), room.Versions);
            else
                EditorActions.MoveRooms(pos, room.Versions);
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
            if (obj is Editor.ConfigurationChangedEvent)
            {
                if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    GenerateToolStripCommands(toolStrip.Items, true);

                RefreshControls(((Editor.ConfigurationChangedEvent)obj).Current);
            }
            
            // Gray out menu options that do not apply
            if (obj is Editor.SelectedObjectChangedEvent)
            {
                ObjectInstance selectedObject = _editor.SelectedObject;
                butCopy.Enabled = selectedObject is PositionBasedObjectInstance;
                butStamp.Enabled = selectedObject is PositionBasedObjectInstance;
            }

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
                ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

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
                butFlipMap.Enabled = _editor.SelectedRoom.Alternated;
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
                    case PopupType.None:
                        popup.ShowSimple(this, msg.Message);
                        break;
                    case PopupType.Info:
                        popup.ShowInfo(this, msg.Message);
                        break;
                    case PopupType.Warning:
                        popup.ShowWarning(this, msg.Message);
                        break;
                    case PopupType.Error:
                        popup.ShowError(this, msg.Message);
                        break;
                }
            }
        }

        private void ClipboardEvents_ClipboardChanged(object sender, EventArgs e)
        {
            butPaste.Enabled = _editor.Mode != EditorMode.Map2D && Clipboard.ContainsData(typeof(ObjectClipboardData).FullName);
        }
        
        private void RefreshControls(Configuration settings)
        {
            if (!settings.Rendering3D_ShowStatics && panel3D.ShowStatics)
                if (_editor.SelectedObject is StaticInstance) _editor.SelectedObject = null;

            if (!settings.Rendering3D_ShowMoveables && panel3D.ShowMoveables)
                if (_editor.SelectedObject is MoveableInstance) _editor.SelectedObject = null;

            if ((!settings.Rendering3D_ShowImportedGeometry && panel3D.ShowImportedGeometry) ||
                (!settings.Rendering3D_DisablePickingForImportedGeometry && panel3D.DisablePickingForImportedGeometry))
                if (_editor.SelectedObject is ImportedGeometryInstance) _editor.SelectedObject = null;

            if (!settings.Rendering3D_ShowOtherObjects && panel3D.ShowOtherObjects)
                if (_editor.SelectedObject is LightInstance ||
                    _editor.SelectedObject is CameraInstance ||
                    _editor.SelectedObject is FlybyCameraInstance ||
                    _editor.SelectedObject is SinkInstance ||
                    _editor.SelectedObject is SoundSourceInstance)
                    _editor.SelectedObject = null;

            panel3D.ShowPortals = butDrawPortals.Checked = settings.Rendering3D_ShowPortals;
            panel3D.ShowHorizon = butDrawHorizon.Checked = settings.Rendering3D_ShowHorizon;
            panel3D.ShowAllRooms = butDrawAllRooms.Checked = settings.Rendering3D_ShowAllRooms;
            panel3D.ShowRoomNames = butDrawRoomNames.Checked = settings.Rendering3D_ShowRoomNames;
            panel3D.ShowCardinalDirections = butDrawCardinalDirections.Checked = settings.Rendering3D_ShowCardinalDirections;
            panel3D.ShowIllegalSlopes = butDrawIllegalSlopes.Checked = settings.Rendering3D_ShowIllegalSlopes;
            panel3D.ShowMoveables = butDrawMoveables.Checked = settings.Rendering3D_ShowMoveables;
            panel3D.ShowStatics = butDrawStatics.Checked = settings.Rendering3D_ShowStatics;
            panel3D.ShowImportedGeometry = butDrawImportedGeometry.Checked = settings.Rendering3D_ShowImportedGeometry;
            panel3D.ShowOtherObjects = butDrawOther.Checked = settings.Rendering3D_ShowOtherObjects;
            panel3D.ShowSlideDirections = butDrawSlideDirections.Checked = settings.Rendering3D_ShowSlideDirections;
            panel3D.ShowExtraBlendingModes = butDrawExtraBlendingModes.Checked = settings.Rendering3D_ShowExtraBlendingModes;
            panel3D.DisablePickingForImportedGeometry = butDisableGeometryPicking.Checked = settings.Rendering3D_DisablePickingForImportedGeometry;

            panel3D.Invalidate();
        }

        private void GenerateToolStripCommands(ToolStripItemCollection items, bool onlyLabels = false)
        {
            foreach (object obj in items)
            {
                ToolStripDropDownButton subMenu = obj as ToolStripDropDownButton;

                if (subMenu != null && subMenu.HasDropDownItems)
                    GenerateToolStripCommands(subMenu.DropDownItems, onlyLabels);
                else
                {
                    var control = obj as ToolStripItem;
                    if (!string.IsNullOrEmpty(control.Tag?.ToString()))
                    {
                        var command = CommandHandler.GetCommand(control.Tag.ToString());
                        if (command != null)
                        {
                            if (!onlyLabels)
                                control.Click += (sender, e) => { command.Execute?.Invoke(new CommandArgs { Editor = _editor, Window = this }); };

                            var hotkeyLabel = string.Join(", ", _editor.Configuration.UI_Hotkeys[control.Tag.ToString()]);
                            var label = command.FriendlyName + (string.IsNullOrEmpty(hotkeyLabel) ? "" : " (" + hotkeyLabel + ")");

                            if (control is ToolStripMenuItem)
                                control.Text = label;
                            else
                                control.ToolTipText = label;
                        }
                    }
                }
            }
        }
    }
}
