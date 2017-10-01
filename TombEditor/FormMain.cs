﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Geometry;
using SharpDX;
using System.IO;
using TombEngine;
using NLog;
using TombLib.Wad;
using TombLib.Utils;
using TombLib.NG;
using DarkUI.Docking;
using DarkUI.Forms;

namespace TombEditor
{
    public partial class FormMain : DarkUI.Forms.DarkForm
    {
        // Dockable tool windows are placed on actual dock panel at runtime.

        private ToolWindows.MainView MainView = new ToolWindows.MainView();
        private ToolWindows.TriggerList TriggerList = new ToolWindows.TriggerList();
        private ToolWindows.RoomOptions RoomOptions = new ToolWindows.RoomOptions();
        private ToolWindows.ObjectBrowser ObjectBrowser = new ToolWindows.ObjectBrowser();
        private ToolWindows.SectorOptions SectorOptions = new ToolWindows.SectorOptions();
        private ToolWindows.Lighting Lighting = new ToolWindows.Lighting();
        private ToolWindows.Palette Palette = new ToolWindows.Palette();
        private ToolWindows.TexturePanel TexturePanel = new ToolWindows.TexturePanel();

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private bool _pressedZorY = false;
        private Editor _editor;
        private DeviceManager _deviceManager = DeviceManager.DefaultDeviceManager;

        public FormMain(Editor editor)
        {
            InitializeComponent();
            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            Text = "Tomb Editor " + Application.ProductVersion + " - Untitled";

            // Only how debug menu when a debugger is attached...
            debugToolStripMenuItem.Visible = System.Diagnostics.Debugger.IsAttached;

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = Configuration.Window_SizeDefault + (Size - ClientSize);

            // Hook window added/removed events.
            dockArea.ContentAdded += ToolWindow_Added;
            dockArea.ContentRemoved += ToolWindow_Removed;

            // DockPanel message filters for drag and resize.
            Application.AddMessageFilter(dockArea.DockContentDragFilter);
            Application.AddMessageFilter(dockArea.DockResizeFilter);

            // Initialize panels
            MainView.Initialize(_deviceManager);
            ObjectBrowser.Initialize(_deviceManager);

            // Restore window settings
            LoadWindowLayout(_editor.Configuration);

            logger.Info("Tomb Editor is ready :)");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Application.RemoveMessageFilter(dockArea.DockContentDragFilter);
                Application.RemoveMessageFilter(dockArea.DockResizeFilter);
                _editor.EditorEventRaised -= EditorEventRaised;
            }
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private DarkDockContent FindDockContentByKey(string key)
        {
            switch (key)
            {
                case "MainView":
                    return MainView;
                case "TriggerList":
                    return TriggerList;
                case "Lighting":
                    return Lighting;
                case "Palette":
                    return Palette;
                case "ObjectBrowser":
                    return ObjectBrowser;
                case "RoomOptions":
                    return RoomOptions;
                case "SectorOptions":
                    return SectorOptions;
                case "TexturePanel":
                    return TexturePanel;
                default:
                    logger.Warn("Unknown tool window '" + key + "' in configuration.");
                    return null;
            }
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Gray out menu options that do not apply
            if (obj is Editor.SelectedObjectChangedEvent)
            {
                ObjectInstance selectedObject = _editor.SelectedObject;
                copyToolStripMenuItem.Enabled = selectedObject is PositionBasedObjectInstance;
                stampToolStripMenuItem.Enabled = selectedObject is PositionBasedObjectInstance;
                deleteToolStripMenuItem.Enabled = selectedObject != null;
                editToolStripMenuItem.Enabled = selectedObject != null;
                rotateToolStripMenuItem.Enabled = selectedObject is IRotateableY;
            }
            if (obj is Editor.SelectedSectorsChangedEvent)
            {
                bool validSectorSelection = _editor.SelectedSectors.Valid;
                smoothRandomCeilingDownToolStripMenuItem.Enabled = validSectorSelection;
                smoothRandomCeilingUpToolStripMenuItem.Enabled = validSectorSelection;
                smoothRandomFloorDownToolStripMenuItem.Enabled = validSectorSelection;
                smoothRandomFloorUpToolStripMenuItem.Enabled = validSectorSelection;
                sharpRandomCeilingDownToolStripMenuItem.Enabled = validSectorSelection;
                sharpRandomCeilingUpToolStripMenuItem.Enabled = validSectorSelection;
                sharpRandomFloorDownToolStripMenuItem.Enabled = validSectorSelection;
                sharpRandomFloorUpToolStripMenuItem.Enabled = validSectorSelection;
                flattenCeilingToolStripMenuItem.Enabled = validSectorSelection;
                flattenFloorToolStripMenuItem.Enabled = validSectorSelection;
                gridWallsIn3ToolStripMenuItem.Enabled = validSectorSelection;
                gridWallsIn5ToolStripMenuItem.Enabled = validSectorSelection;
            }

            // Update room information on the status strip
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomGeometryChangedEvent) ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomSectorPropertiesChangedEvent) ||
                (obj is Editor.RoomPropertiesChangedEvent))
            {
                var room = _editor.SelectedRoom;
                if (room == null)
                    statusStripSelectedRoom.Text = "Selected room: None";
                else
                    statusStripSelectedRoom.Text = "Selected room: " +
                        "Name = " + room + " | " +
                        "Pos = (" + room.Position.X + ", " + room.Position.Y + ", " + room.Position.Z + ") | " +
                        "Floor = " + (room.Position.Y + room.GetLowestCorner()) + " | " +
                         "Ceiling = " + (room.Position.Y + room.GetHighestCorner());
            }

            // Update selection information of the status strip
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomGeometryChangedEvent) ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomSectorPropertiesChangedEvent) ||
                (obj is Editor.SelectedSectorsChangedEvent))
            {
                var room = _editor.SelectedRoom;
                if ((room == null) || !_editor.SelectedSectors.Valid)
                {
                    statusStripGlobalSelectionArea.Text = "Global area: None";
                    statusStripLocalSelectionArea.Text = "Local area: None";
                }
                else
                {
                    int minHeight = room.GetLowestCorner(_editor.SelectedSectors.Area);
                    int maxHeight = room.GetHighestCorner(_editor.SelectedSectors.Area);

                    statusStripGlobalSelectionArea.Text = "Global area = " +
                        "(" + (room.Position.X + _editor.SelectedSectors.Area.X) + ", " + (room.Position.Z + _editor.SelectedSectors.Area.Y) + ") \u2192 " +
                        "(" + (room.Position.X + _editor.SelectedSectors.Area.Right) + ", " + (room.Position.Z + _editor.SelectedSectors.Area.Bottom) + ")" +
                        " | y = [" + (room.Position.Y + minHeight) + ", " + (room.Position.Y + maxHeight) + "]";
                    statusStripLocalSelectionArea.Text = "Local area = " +
                        "(" + _editor.SelectedSectors.Area.X + ", " + _editor.SelectedSectors.Area.Y + ") \u2192 " +
                        "(" + _editor.SelectedSectors.Area.Right + ", " + _editor.SelectedSectors.Area.Bottom + ")" +
                        " | y = [" + minHeight + ", " + maxHeight + "]";
                }
            }

            // Update application title
            if (obj is Editor.LevelFileNameChanged)
            {
                string LevelName = string.IsNullOrEmpty(_editor.Level.Settings.LevelFilePath) ? "Untitled" :
                    Path.GetFileNameWithoutExtension(_editor.Level.Settings.LevelFilePath);
                Text = "Tomb Editor " + Application.ProductVersion.ToString() + " - " + LevelName;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            switch (e.CloseReason)
            {
                case CloseReason.None:
                case CloseReason.UserClosing:
                    if (DarkMessageBox.Show(this, "Your level will be lost. Do you really want to exit?",
                            "Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        e.Cancel = true;
                    break;
            }
            base.OnFormClosing(e);
        }

        private void LoadWindowLayout(Configuration configuration)
        {
            dockArea.RemoveContent();
            dockArea.RestoreDockPanelState(configuration.Window_Layout, FindDockContentByKey);

            Size = configuration.Window_Size;
            Location = configuration.Window_Position;
            WindowState = configuration.Window_Maximized ? FormWindowState.Maximized : FormWindowState.Normal;
        }

        private void SaveWindowLayout(Configuration configuration)
        {
            configuration.Window_Layout = dockArea.GetDockPanelState();

            configuration.Window_Size = Size;
            configuration.Window_Position = Location;
            configuration.Window_Maximized = WindowState == FormWindowState.Maximized;

            _editor.ConfigurationChange();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Stamp

            switch (e.KeyCode)
            {
                case Keys.Z: // Set camera relocation mode (Z on american keyboards, Y on german keyboards)
                    _pressedZorY = true;
                    break;

                case Keys.Escape: // End any action
                    _editor.Action = EditorAction.None;
                    _editor.SelectedSectors = SectorSelection.None;
                    _editor.SelectedObject = null;
                    break;

                case Keys.T: // Add trigger
                    if (e.Modifiers == Keys.None && _editor.SelectedSectors.Valid)
                        EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
                    return;

                case Keys.P: // Add portal
                    if (e.Modifiers == Keys.None && _editor.SelectedSectors.Valid)
                        try
                        {
                            EditorActions.AddPortal(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
                        }
                        catch (Exception exc)
                        {
                            logger.Error(exc, "Unable to create portal");
                            DarkMessageBox.Show(this, exc.Message, "Unable to create portal", MessageBoxIcon.Error);
                        }
                    return;

                case Keys.O: // Show options dialog
                    if (e.Modifiers == Keys.None && (_editor.SelectedObject != null))
                        EditorActions.EditObject(_editor.SelectedObject, this);
                    break;

                case Keys.Left: // Rotate objects with cones
                    if (e.Modifiers == Keys.Shift && (_editor.SelectedObject != null))
                        EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, -1);
                    else if (e.Modifiers == Keys.Control && (_editor.SelectedObject is PositionBasedObjectInstance))
                        EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(-1024, 0, 0), new Vector3(), true);
                    break;
                case Keys.Right: // Rotate objects with cones
                    if (e.Modifiers == Keys.Shift && (_editor.SelectedObject != null))
                        EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, 1);
                    else if (e.Modifiers == Keys.Control && (_editor.SelectedObject is PositionBasedObjectInstance))
                        EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(1024, 0, 0), new Vector3(), true);
                    break;

                case Keys.Up:// Rotate objects with cones
                    if (e.Modifiers == Keys.Shift && (_editor.SelectedObject != null))
                        EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.X, 1);
                    else if (e.Modifiers == Keys.Control && (_editor.SelectedObject is PositionBasedObjectInstance))
                        EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, 0, 1024), new Vector3(), true);
                    break;

                case Keys.Down:// Rotate objects with cones
                    if (e.Modifiers == Keys.Shift && (_editor.SelectedObject != null))
                        EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.X, -1);
                    else if (e.Modifiers == Keys.Control && (_editor.SelectedObject is PositionBasedObjectInstance))
                        EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, 0, -1024), new Vector3(), true);
                    break;

                case Keys.Q:
                    if (e.Modifiers == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 0, (short)(e.Shift ? 4 : 1), e.Control);
                    else if (e.Modifiers == Keys.Control && (_editor.SelectedObject is PositionBasedObjectInstance))
                        EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, 256, 0), new Vector3(), true);
                    break;

                case Keys.A:
                    if (e.Modifiers == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 0, (short)-(e.Shift ? 4 : 1), e.Control);
                    else if (e.Modifiers == Keys.Control && (_editor.SelectedObject is PositionBasedObjectInstance))
                        EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, -256, 0), new Vector3(), true);
                    break;

                case Keys.W:
                    if (e.Modifiers == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 1, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.S:
                    if (e.Modifiers == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 1, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.E:
                    if (e.Modifiers == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 2, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.D:
                    if (e.Modifiers == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 2, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.R: // Rotate object
                    if (e.Modifiers == Keys.None)
                        if ((_editor.SelectedObject != null))
                            EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, e.Shift ? 5.0f : 45.0f);
                        else if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                            EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 3, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.F:
                    if (e.Modifiers == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 3, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.Y: // Set camera relocation mode (Z on american keyboards, Y on german keyboards)
                    _pressedZorY = true;
                    if (e.Modifiers == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalFloorCorner, 0, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.H:
                    if (e.Modifiers == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalFloorCorner, 0, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.U:
                    if (e.Modifiers == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalCeilingCorner, 1, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.J:
                    if (ModifierKeys == Keys.None && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalCeilingCorner, 1, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.OemMinus: // US keyboard key in documentation
                case Keys.Oemplus:
                case Keys.Oem3: // US keyboard key for a texture triangle rotation
                case Keys.Oem5: // German keyboard key for a texture triangle rotation
                    if (ModifierKeys == Keys.None)
                        EditorActions.RotateSelectedTexture();
                    break;
            }

            // Set camera relocation mode based on previous inputs
            if (e.Alt && _pressedZorY)
            {
                EditorAction action = _editor.Action;
                action.RelocateCameraActive = true;
                _editor.Action = action;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if ((e.KeyCode == Keys.Menu) || (e.KeyCode == Keys.Y) || (e.KeyCode == Keys.Z))
            {
                EditorAction action = _editor.Action;
                action.RelocateCameraActive = false;
                _editor.Action = action;
            }
            if ((e.KeyCode == Keys.Y) || (e.KeyCode == Keys.Z))
                _pressedZorY = false;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            EditorAction action = _editor.Action;
            action.RelocateCameraActive = false;
            _editor.Action = action;
            _pressedZorY = false;
        }

        private void loadTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.LoadTextures(this);
        }

        private void unloadTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var texture in _editor.Level.Settings.Textures)
                texture.SetPath(_editor.Level.Settings, "");
            _editor.LoadedTexturesChange();
        }

        private void reloadTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Level.ReloadLevelTextures();
            _editor.LoadedTexturesChange();
        }

        private void textureFloorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TexturizeAllFloor(_editor.SelectedRoom, _editor.SelectedTexture);
        }

        private void textureCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TexturizeAllCeiling(_editor.SelectedRoom, _editor.SelectedTexture);
        }

        private void textureWallsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TexturizeAllWalls(_editor.SelectedRoom, _editor.SelectedTexture);
        }

        private void importConvertTextureToPng_Click(object sender, EventArgs e)
        {
            if ((_editor.Level == null) || (_editor.Level.Settings.Textures.Count == 0))
            {
                DarkMessageBox.Show(this, "Currently there is no texture loaded to convert it.", "No texture loaded", MessageBoxIcon.Error);
                return;
            }

            LevelTexture texture = _editor.Level.Settings.Textures[0];
            if (texture.ImageLoadException != null)
            {
                DarkMessageBox.Show(this, "The texture that should be converted to *.png could not be loaded. " + texture.ImageLoadException.Message, "Error", MessageBoxIcon.Error);
                return;
            }

            string currentTexturePath = _editor.Level.Settings.MakeAbsolute(texture.Path);
            string pngFilePath = Path.Combine(Path.GetDirectoryName(currentTexturePath), Path.GetFileNameWithoutExtension(currentTexturePath) + ".png");

            if (File.Exists(pngFilePath))
            {
                if (DarkMessageBox.Show(this,
                        "There is already a file at \"" + pngFilePath + "\". Continue and overwrite the file?",
                        "File exist already", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
            }
            texture.Image.Save(pngFilePath);

            DarkMessageBox.Show(this, "TGA texture map was converted to PNG without errors and saved at \"" + pngFilePath + "\".", "Success", MessageBoxIcon.Information);
            texture.SetPath(_editor.Level.Settings, pngFilePath);
            _editor.LoadedTexturesChange();
        }

        private void loadWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.LoadWad(this);
        }

        private void unloadWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.UnloadWad();
        }

        private void reloadWadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.ReloadWad();
        }

        private void addCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceCamera };
        }

        private void addImportedGeometryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceImportedGeometry };
        }

        private void addFlybyCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceFlyByCamera };
        }

        private void addSinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceSink };
        }

        private void addSoundSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceSoundSource };
        }

        private void addPortalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedSectors.Valid)
                try
                {
                    EditorActions.AddPortal(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Unable to create portal");
                    DarkMessageBox.Show(this, exc.Message, "Unable to create portal", MessageBoxIcon.Error);
                }
        }

        private void addTriggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedSectors.Valid)
                EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
        }

        private void newLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(this,
                    "Your level will be lost. Do you really want to create a new level?",
                    "New level", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _editor.Level = Level.CreateSimpleLevel();
        }

        private void saveLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.SaveLevel(this, false);
        }

        private void openLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.OpenLevel(this);
        }

        private void importTRLEPRJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.OpenLevelPrj(this);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.SaveLevel(this, true);
        }

        private void buildLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.BuildLevel(false);
        }

        private void buildLevelPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.BuildLevelAndPlay();
        }

        private void animationRangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.ShowAnimationRangesDialog(this);
        }

        private void textureSoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.ShowTextureSoundsDialog(this);
        }

        private void smoothRandomFloorUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SmoothRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void smoothRandomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SmoothRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void smoothRandomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SmoothRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void smoothRandomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SmoothRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void sharpRandomFloorUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SharpRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void sharpRandomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SharpRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void sharpRandomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SharpRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void sharpRandomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SharpRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void butFlattenFloor_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.FlattenFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butFlattenCeiling_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.FlattenCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void flattenFloorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.FlattenFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void flattenCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.FlattenCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void gridWallsIn3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EditorActions.CheckForRoomAndBlockSelection(this))
                EditorActions.GridWalls3(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void gridWallsIn5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.GridWalls5(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void findObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ObjectBrowser.FindItem();
        }

        private void resetFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ObjectBrowser.ResetSearch();
        }

        private void moveLaraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;

            // Search for first Lara and remove her
            MoveableInstance lara;
            foreach (Room room in _editor.Level.Rooms.Where(room => room != null))
                foreach (var instance in room.Objects)
                {
                    lara = instance as MoveableInstance;
                    if ((lara != null) && (lara.WadObjectId == 0))
                    {
                        room.RemoveObject(_editor.Level, instance);
                        goto FoundLara;
                    }
                }
            lara = new MoveableInstance { WadObjectId = 0 }; // Lara
            FoundLara:

            // Add lara to current sector
            {
                var room = _editor.SelectedRoom;
                var block = room.GetBlock(_editor.SelectedSectors.Start);
                lara.Position = new Vector3(_editor.SelectedSectors.Start.X * 1024 + 512, block.FloorMax * 256, _editor.SelectedSectors.Start.Y * 1024 + 512);
                room.AddObject(_editor.Level, lara);
                _editor.ObjectChange(lara);
            }
        }

        private void cropRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.CropRoom(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
        }

        private void splitRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.SplitRoom(this);
        }

        private void copyRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.CopyRoom(this);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.Copy(this);
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedObject != null)
                EditorActions.DeleteObjectWithWarning(_editor.SelectedObject, this);
        }

        private void editToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedObject != null)
                EditorActions.EditObject(_editor.SelectedObject, this);
        }

        private void rotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedObject != null)
                EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, 45);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.Paste };
        }

        private void stampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.Clone(this);
        }

        private void newRoomUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.CreateRoomAboveOrBelow(_editor.SelectedRoom, (room) => room.GetHighestCorner(), 12);
        }

        private void newRoomDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.CreateRoomAboveOrBelow(_editor.SelectedRoom, (room) => room.GetLowestCorner() - 12, 12);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void levelSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormLevelSettings form = new FormLevelSettings(_editor))
                form.ShowDialog(this);
        }

        private void saveCurrentLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveWindowLayout(_editor.Configuration);
        }

        private void restoreDefaultLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadWindowLayout(new Configuration());
        }

        private void reloadLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadWindowLayout(_editor.Configuration);
        }

        private void sectorOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(SectorOptions);
        }

        private void roomOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(RoomOptions);
        }

        private void objectBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(ObjectBrowser);
        }

        private void triggerListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(TriggerList);
        }

        private void lightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(Lighting);
        }

        private void paletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(Palette);
        }

        private void texturePanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(TexturePanel);
        }

        private void ToolWindow_Toggle(DarkToolWindow toolWindow)
        {
            if (toolWindow.DockPanel == null)
                dockArea.AddContent(toolWindow);
            else
                dockArea.RemoveContent(toolWindow);
        }

        private void ToolWindow_BuildMenu()
        {
            sectorOptionsToolStripMenuItem.Checked = dockArea.ContainsContent(SectorOptions);
            roomOptionsToolStripMenuItem.Checked = dockArea.ContainsContent(RoomOptions);
            objectBrowserToolStripMenuItem.Checked = dockArea.ContainsContent(ObjectBrowser);
            triggerListToolStripMenuItem.Checked = dockArea.ContainsContent(TriggerList);
            lightingToolStripMenuItem.Checked = dockArea.ContainsContent(Lighting);
            paletteToolStripMenuItem.Checked = dockArea.ContainsContent(Palette);
            texturePanelToolStripMenuItem.Checked = dockArea.ContainsContent(TexturePanel);
        }

        private void ToolWindow_Added(object sender, DockContentEventArgs e)
        {
            if (dockArea.Contains(e.Content))
                ToolWindow_BuildMenu();
        }

        private void ToolWindow_Removed(object sender, DockContentEventArgs e)
        {
            if (!dockArea.Contains(e.Content))
                ToolWindow_BuildMenu();
        }

        // Only for debugging purposes...

        private void debugAction0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //level.Load("");
            var level = new TombRaider4Level("e:\\trle\\data\\tut1.tr4");
            level.Load("originale");

            level = new TombRaider4Level("E:\\Vecchi\\Tomb-Editor\\Build\\Game\\Data\\tut1.tr4");
            level.Load("editor");

            //level = new TombEngine.TombRaider4Level("e:\\trle\\data\\tut1.tr4");
            //level.Load("originale");
        }

        private void debugAction1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //level.Load("");

            var level = new TombRaider3Level("e:\\tomb3\\data\\crash.tr2");
            level.Load("crash");

            level = new TombRaider3Level("e:\\tomb3\\data\\jungle.tr2");
            level.Load("jungle");
        }

        private void debugAction2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tempColors = new List<int>();

            var bmp = (Bitmap)System.Drawing.Image.FromFile("Editor\\Palette.png");
            for (int y = 2; y < bmp.Height; y += 14)
            {
                for (int x = 2; x < bmp.Width; x += 14)
                {
                    var col = bmp.GetPixel(x, y);
                    if (col.A == 0)
                        continue;
                    /* if (!tempColors.Contains(col.ToArgb()))*/
                    tempColors.Add(col.ToArgb());
                }
            }
            File.Delete("Editor\\Palette.bin");
            using (var writer = new BinaryWriter(new FileStream("Editor\\Palette.bin", FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                foreach (int c in tempColors)
                {
                    var col2 = System.Drawing.Color.FromArgb(c);
                    writer.Write(col2.R);
                    writer.Write(col2.G);
                    writer.Write(col2.B);
                }
            }
        }

        private void debugAction3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Wad2.SaveToStream(_editor.Level.Wad, File.OpenWrite("E:\\test.wad2"));
            Wad2.LoadFromStream(File.OpenRead("E:\\test.wad2"));
            //RoomGeometryExporter.ExportRoomToObj(_editor.SelectedRoom, "room.obj");
        }

        private void debugAction4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //   _editor.Level.)
            /*RoomGeometryExporter.LoadModel("low-poly-wooden-door.obj");
            ImportedGeometryInstance instance = new ImportedGeometryInstance();
              instance.Model = GeometryImporterExporter.Models["low-poly-wooden-door.obj"];
              instance.Position = new Vector3(4096, 512, 4096);
            _editor.SelectedRoom.AddObject(_editor.Level, instance);*/

            /*  GeometryImporterExporter.LoadModel("room.obj", 1.0f);
              RoomGeometryInstance instance = new RoomGeometryInstance();
              instance.Model = GeometryImporterExporter.Models["room.obj"];
              instance.Position = new Vector3(4096, 512, 4096);
              _editor.SelectedRoom.AddObject(_editor.Level, instance);*/
        }

        private void debugAction5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NGTriggersDefinitions.LoadTriggers(File.OpenRead("NG\\NG_Constants.txt"));
        }
    }
}
