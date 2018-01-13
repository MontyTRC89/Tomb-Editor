using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;
using System.Numerics;
using System.IO;
using TombEngine;
using NLog;
using TombLib.Wad;
using TombLib.Utils;
using TombLib.NG;
using DarkUI.Docking;
using DarkUI.Forms;
using TombLib.GeometryIO.Importers;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib;

namespace TombEditor
{
    public partial class FormMain : DarkForm
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
        private ToolWindows.ObjectList ObjectList = new ToolWindows.ObjectList();
        private ToolWindows.ToolPalette ToolPalette = new ToolWindows.ToolPalette();

        // Floating tool boxes are placed on 3D view at runtime
        private ToolWindows.ToolPaletteFloating ToolBox = new ToolWindows.ToolPaletteFloating();

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private bool _pressedMoveCameraKey = false;
        private Editor _editor;
        private DeviceManager _deviceManager = DeviceManager.DefaultDeviceManager;

        public FormMain(Editor editor)
        {
            InitializeComponent();
            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            Text = "Tomb Editor " + Application.ProductVersion + " - Untitled";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

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

            // Retrieve clipboard change notifications
            ClipboardEvents.ClipboardChanged += ClipboardEvents_ClipboardChanged;
            ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            ClipboardEvents.ClipboardChanged -= ClipboardEvents_ClipboardChanged;
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
                case "ObjectList":
                    return ObjectList;
                case "ToolPalette":
                    return ToolPalette;
                default:
                    logger.Warn("Unknown tool window '" + key + "' in configuration.");
                    return null;
            }
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Gray out menu options that do not apply
            if (obj is Editor.SelectedObjectChangedEvent || obj is Editor.ModeChangedEvent)
            {
                ObjectInstance selectedObject = _editor.SelectedObject;
                copyToolStripMenuItem.Enabled = _editor.Mode == EditorMode.Map2D || selectedObject is PositionBasedObjectInstance;
                stampToolStripMenuItem.Enabled = selectedObject is PositionBasedObjectInstance;
                if (obj is Editor.ModeChangedEvent)
                    ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

                deleteToolStripMenuItem.Enabled = _editor.Mode == EditorMode.Map2D || selectedObject != null;
                rotateToolStripMenuItem.Enabled = selectedObject is IRotateableY;
            }
            if (obj is Editor.SelectedSectorsChangedEvent)
            {
                bool validSectorSelection = _editor.SelectedSectors.Valid;
                transformToolStripMenuItem.Enabled = validSectorSelection;
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

            if (obj is Editor.LoadedWadsChangedEvent)
            {
                if (_editor.Level.Wad != null)
                    soundManagerToolStripMenuItem.Enabled = !_editor.Level.Wad.IsImported;
                else
                    soundManagerToolStripMenuItem.Enabled = false;
            }

            // Update compilation statistics
            if (obj is Editor.LevelCompilationCompletedEvent)
            {
                var evt = obj as Editor.LevelCompilationCompletedEvent;
                statusLastCompilation.Text = "Last level output { " + evt.InfoString + " }";
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
                        "(" + (room.Position.X + _editor.SelectedSectors.Area.X0) + ", " + (room.Position.Z + _editor.SelectedSectors.Area.Y0) + ") \u2192 " +
                        "(" + (room.Position.X + _editor.SelectedSectors.Area.X1) + ", " + (room.Position.Z + _editor.SelectedSectors.Area.Y1) + ")" +
                        " | y = [" + ((minHeight == int.MaxValue || maxHeight == int.MinValue) ? "N/A" : ((room.Position.Y + minHeight) + ", " + (room.Position.Y + maxHeight))) + "]";

                    statusStripLocalSelectionArea.Text = "Local area = " +
                        "(" + _editor.SelectedSectors.Area.X0 + ", " + _editor.SelectedSectors.Area.Y0 + ") \u2192 " +
                        "(" + _editor.SelectedSectors.Area.X1 + ", " + _editor.SelectedSectors.Area.Y1 + ")" +
                        " | y = [" + ((minHeight == int.MaxValue || maxHeight == int.MinValue) ? "N/A" : (minHeight + ", " + maxHeight)) + "]";
                }
            }

            // Update application title bar
            if ((obj is Editor.LevelFileNameChangedEvent) || (obj is Editor.HasUnsavedChangesChangedEvent))
            {
                string LevelName = string.IsNullOrEmpty(_editor.Level.Settings.LevelFilePath) ? "Untitled" :
                    FileSystemUtils.GetFileNameWithoutExtensionTry(_editor.Level.Settings.LevelFilePath);

                Text = "Tomb Editor " + Application.ProductVersion + " - " + LevelName + (_editor.HasUnsavedChanges ? "*" : "");
            }

            // Update save button
            if (obj is Editor.HasUnsavedChangesChangedEvent)
                saveLevelToolStripMenuItem.Enabled = _editor.HasUnsavedChanges;

            // Reload window layout if the configuration changed
            if (obj is Editor.ConfigurationChangedEvent)
            {
                var @event = (Editor.ConfigurationChangedEvent)obj;
                if ((@event.Current.Window_Maximized != @event.Previous.Window_Maximized) ||
                    (@event.Current.Window_Position != @event.Previous.Window_Position) ||
                    (@event.Current.Window_Size != @event.Previous.Window_Size) ||
                    (@event.Current.Window_Layout != @event.Previous.Window_Layout))
                    LoadWindowLayout(_editor.Configuration);
            }
        }

        private void ClipboardEvents_ClipboardChanged(object sender, EventArgs e)
        {
            if (_editor.Mode != EditorMode.Map2D)
                pasteToolStripMenuItem.Enabled = Clipboard.ContainsData(typeof(ObjectClipboardData).FullName);
            else
                pasteToolStripMenuItem.Enabled = Clipboard.ContainsData(typeof(RoomClipboardData).FullName);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            switch (e.CloseReason)
            {
                case CloseReason.None:
                case CloseReason.UserClosing:
                    if (!EditorActions.ContinueOnFileDrop(this, "Exit"))
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

            floatingToolStripMenuItem.Checked = configuration.Rendering3D_ToolboxVisible;
            ToolBox.Location = configuration.Rendering3D_ToolboxPosition;
        }

        private void SaveWindowLayout(Configuration configuration)
        {
            configuration.Window_Layout = dockArea.GetDockPanelState();

            configuration.Window_Size = Size;
            configuration.Window_Position = Location;
            configuration.Window_Maximized = WindowState == FormWindowState.Maximized;

            configuration.Rendering3D_ToolboxVisible = floatingToolStripMenuItem.Checked;
            configuration.Rendering3D_ToolboxPosition = ToolBox.Location;

            _editor.ConfigurationChange();
        }


        protected override bool ProcessDialogKey(Keys keyData)
        {
            bool focused = IsFocused(MainView) || IsFocused(TexturePanel);

            Keys modifierKeys = keyData & (Keys.Alt | Keys.Shift | Keys.Control);
            bool shift = keyData.HasFlag(Keys.Shift);
            bool alt = keyData.HasFlag(Keys.Alt);

            switch (keyData & ~(Keys.Alt | Keys.Shift | Keys.Control))
            {
                case Keys.Escape: // End any action
                    _editor.Action = null;
                    _editor.SelectedSectors = SectorSelection.None;
                    _editor.SelectedObject = null;
                    _editor.SelectedRooms = new Room[] { _editor.SelectedRoom };
                    break;

                case Keys.F1: // 2D map mode
                    if (modifierKeys == Keys.None)
                        EditorActions.SwitchMode(EditorMode.Map2D);
                    break;

                case Keys.F2: // 3D geometry mode
                    if (modifierKeys == Keys.None)
                        EditorActions.SwitchMode(EditorMode.Geometry);
                    break;

                case Keys.F3: // 3D face texturing mode
                    if (modifierKeys == Keys.None)
                        EditorActions.SwitchMode(EditorMode.FaceEdit);
                    break;

                case Keys.F4: // 3D lighting mode
                    if (modifierKeys == Keys.None)
                        EditorActions.SwitchMode(EditorMode.Lighting);
                    break;

                case Keys.F6: // Reset 3D camera
                    if (modifierKeys == Keys.None)
                        _editor.ResetCamera();
                    break;

                case Keys.T: // Add trigger
                    if (modifierKeys == Keys.None && _editor.SelectedSectors.Valid && focused)
                        EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
                    break;

                case Keys.P: // Add portal
                    if (modifierKeys == Keys.None && _editor.SelectedSectors.Valid && focused)
                        try
                        {
                            EditorActions.AddPortal(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
                        }
                        catch (Exception exc)
                        {
                            logger.Error(exc, "Unable to create portal");
                            DarkMessageBox.Show(this, exc.Message, "Unable to create portal", MessageBoxIcon.Error);
                        }
                    break;

                case Keys.O: // Show options dialog
                    if (modifierKeys == Keys.None && (_editor.SelectedObject != null) && focused)
                        EditorActions.EditObject(_editor.SelectedObject, this);
                    break;

                case Keys.NumPad1: // Switch additive blending
                case Keys.D1:
                    if (modifierKeys == Keys.Shift)
                    {
                        var texture = _editor.SelectedTexture;
                        if (texture.BlendMode == BlendMode.Additive)
                            texture.BlendMode = BlendMode.Normal;
                        else
                            texture.BlendMode = BlendMode.Additive;
                        _editor.SelectedTexture = texture;
                    }
                    break;

                case Keys.NumPad2: // Switch double sided
                case Keys.D2:
                    if (modifierKeys == Keys.Shift)
                    {
                        var texture = _editor.SelectedTexture;
                        texture.DoubleSided = !texture.DoubleSided;
                        _editor.SelectedTexture = texture;
                    }
                    break;

                case Keys.NumPad3: // Switch to an invisible texture
                case Keys.D3:
                    if (modifierKeys == Keys.Shift)
                    {
                        var texture = _editor.SelectedTexture;
                        texture.Texture = TextureInvisible.Instance;
                        _editor.SelectedTexture = texture;
                    }
                    break;

                case Keys.Left: // Rotate objects with cones
                    if (modifierKeys == Keys.Shift && (_editor.SelectedObject != null) && focused)
                        EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, -1);
                    else if (modifierKeys == Keys.Control && (_editor.SelectedObject is PositionBasedObjectInstance) && focused)
                        MainView.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(1024, 0, 0), new Vector3(), true);
                    break;
                case Keys.Right: // Rotate objects with cones
                    if (modifierKeys == Keys.Shift && (_editor.SelectedObject != null) && focused)
                        EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, 1);
                    else if (modifierKeys == Keys.Control && (_editor.SelectedObject is PositionBasedObjectInstance) && focused)
                        MainView.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(-1024, 0, 0), new Vector3(), true);
                    break;

                case Keys.Up:// Rotate objects with cones
                    if (modifierKeys == Keys.Shift && (_editor.SelectedObject != null) && focused)
                        EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.X, 1);
                    else if (modifierKeys == Keys.Control && (_editor.SelectedObject is PositionBasedObjectInstance) && focused)
                        MainView.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, 0, -1024), new Vector3(), true);
                    break;

                case Keys.Down:// Rotate objects with cones
                    if (modifierKeys == Keys.Shift && (_editor.SelectedObject != null) && focused)
                        EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.X, -1);
                    else if (modifierKeys == Keys.Control && (_editor.SelectedObject is PositionBasedObjectInstance) && focused)
                        MainView.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, 0, 1024), new Vector3(), true);
                    break;

                case Keys.Q:
                    int qDirection = KeyboardLayoutDetector.KeyboardLayout == KeyboardLayout.Azerty ? -1 : 1;
                    if (!modifierKeys.HasFlag(Keys.Control) && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 0, (short)(qDirection * (shift ? 4 : 1)), alt);
                    else if (_editor.SelectedObject is PositionBasedObjectInstance && focused)
                        EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, qDirection * 256, 0), new Vector3(), true);
                    break;

                case Keys.A:
                    int aDirection = KeyboardLayoutDetector.KeyboardLayout == KeyboardLayout.Azerty ? 1 : -1;
                    if (!modifierKeys.HasFlag(Keys.Control) && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 0, (short)(aDirection * (shift ? 4 : 1)), alt);
                    else if (_editor.SelectedObject is PositionBasedObjectInstance && focused)
                        EditorActions.MoveObjectRelative((PositionBasedObjectInstance)_editor.SelectedObject, new Vector3(0, -256, 0), new Vector3(), true);
                    break;

                case Keys.W:
                    switch (KeyboardLayoutDetector.KeyboardLayout)
                    {
                        case KeyboardLayout.Azerty:
                            _pressedMoveCameraKey = true;
                            break;
                        default:
                            if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused && modifierKeys != Keys.Control)
                                EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 1, (short)(shift ? 4 : 1), alt);
                            break;
                    }
                    break;

                case Keys.S:
                    if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused && modifierKeys != Keys.Control)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 1, (short)-(shift ? 4 : 1), alt);
                    break;

                case Keys.E:
                    if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 2, (short)(shift ? 4 : 1), alt);
                    break;

                case Keys.D:
                    if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 2, (short)-(shift ? 4 : 1), alt);
                    break;

                case Keys.R: // Rotate object
                    if (!modifierKeys.HasFlag(Keys.Control) && _editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 3, (short)(shift ? 4 : 1), alt);
                    else if (_editor.SelectedObject != null && focused)
                        EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, shift ? 5.0f : 45.0f);
                    break;

                case Keys.F:
                    if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 3, (short)-(shift ? 4 : 1), alt);
                    break;

                case Keys.Y:
                    switch (KeyboardLayoutDetector.KeyboardLayout)
                    {
                        case KeyboardLayout.Qwertz:
                            _pressedMoveCameraKey = true;
                            break;
                        default:
                            if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                                EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.EntireFace, 0, (short)(shift ? 4 : 1), alt, true);
                            break;
                    }
                    break;

                case Keys.Z:
                    switch (KeyboardLayoutDetector.KeyboardLayout)
                    {
                        case KeyboardLayout.Qwertz:
                            if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                                EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.EntireFace, 0, (short)(shift ? 4 : 1), alt, true);
                            break;
                        case KeyboardLayout.Azerty:
                            if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused && modifierKeys != Keys.Control)
                                EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 1, (short)(shift ? 4 : 1), alt);
                            break;
                        default:
                            _pressedMoveCameraKey = true;
                            break;
                    }
                    break;

                case Keys.H:
                    if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.EntireFace, 0, (short)-(shift ? 4 : 1), alt, true);
                    break;
                case Keys.U:
                    if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.EntireFace, 1, (short)(shift ? 4 : 1), alt, true);
                    break;
                case Keys.J:
                    if (_editor.Mode == EditorMode.Geometry && _editor.SelectedSectors.Valid && focused)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.EntireFace, 1, (short)-(shift ? 4 : 1), alt, true);
                    break;

                case Keys.OemMinus: // US keyboard key in documentation
                case Keys.Oemplus:
                case Keys.Oem3: // US keyboard key for a texture triangle rotation
                case Keys.Oem5: // German keyboard key for a texture triangle rotation
                    if (ModifierKeys == Keys.None)
                        EditorActions.RotateSelectedTexture();
                    else if (ModifierKeys.HasFlag(Keys.Shift))
                        EditorActions.MirrorSelectedTexture();
                    break;
            }

            // Set camera relocation mode based on previous inputs
            if (alt && _pressedMoveCameraKey)
            {
                _editor.Action = new EditorActionRelocateCamera();
            }

            // Don't open menus with the alt key
            if (alt)
                return true;

            return base.ProcessDialogKey(keyData);
        }

        private static bool IsFocused(Control control)
        {
            if (control.Focused)
                return true;
            foreach (Control controlInner in control.Controls)
                if (IsFocused(controlInner))
                    return true;
            return false;
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            // Get camera move key
            bool keyCodeIsMoveCameraKey;
            switch (KeyboardLayoutDetector.KeyboardLayout)
            {
                case KeyboardLayout.Qwertz:
                    keyCodeIsMoveCameraKey = e.KeyCode == Keys.Y;
                    break;
                case KeyboardLayout.Azerty:
                    keyCodeIsMoveCameraKey = e.KeyCode == Keys.W;
                    break;
                default:
                    keyCodeIsMoveCameraKey = e.KeyCode == Keys.Z;
                    break;
            }

            // Check camera move key state
            if ((e.KeyCode == Keys.Menu) || keyCodeIsMoveCameraKey)
                if (_editor.Action is EditorActionRelocateCamera)
                    _editor.Action = null;
            if (keyCodeIsMoveCameraKey)
                _pressedMoveCameraKey = false;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            _pressedMoveCameraKey = false;
            if (_editor.Action is IEditorActionDisableOnLostFocus)
                _editor.Action = null;
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
            EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Floor);
        }

        private void textureCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Ceiling);
        }

        private void textureWallsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TexturizeAll(_editor.SelectedRoom, _editor.SelectedSectors, _editor.SelectedTexture, BlockFaceType.Wall);
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
            _editor.Action = new EditorActionPlace(false, (l, r) => new CameraInstance());
        }

        private void addImportedGeometryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorActionPlace(false, (l, r) => new ImportedGeometryInstance());
        }

        private void addFlybyCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorActionPlace(false, (l, r) => new FlybyCameraInstance());
        }

        private void addSinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorActionPlace(false, (l, r) => new SinkInstance());
        }

        private void addSoundSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorActionPlace(false, (l, r) => new SoundSourceInstance());
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
            if (!EditorActions.ContinueOnFileDrop(this, "New level"))
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
            EditorActions.BuildLevel(false, this);
        }

        private void buildLevelPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.BuildLevelAndPlay(this);
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
                EditorActions.GridWalls(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void gridWallsIn5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.GridWalls(_editor.SelectedRoom, _editor.SelectedSectors.Area, true);
        }

        private void wholeRoomUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.MoveRooms(new VectorInt3(0, 1, 0), _editor.SelectedRoom.Versions);
        }

        private void wholeRoomDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.MoveRooms(new VectorInt3(0, -1, 0), _editor.SelectedRoom.Versions);
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

            EditorActions.MoveLara(this, _editor.SelectedSectors.Start);
        }

        private void duplicateRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.DuplicateRooms(this);
        }

        private void deleteRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.Mode == EditorMode.Map2D)
                EditorActions.DeleteRooms(_editor.SelectedRooms, this);
            else
                EditorActions.DeleteRooms(new Room[] { _editor.SelectedRoom }, this);
        }

        private void selectConnectedRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.SelectConnectedRooms();
        }

        private void rotateRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = -1 }, this);
        }

        private void rotateRoomsCountercockwiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TransformRooms(new RectTransformation { QuadrantRotation = 1 }, this);
        }

        private void mirrorRoomsOnXAxisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TransformRooms(new RectTransformation { MirrorX = true }, this);
        }

        private void mirrorRoomsOnZAxisToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TransformRooms(new RectTransformation { MirrorX = true, QuadrantRotation = 2 }, this);
        }

        private void cropRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.CropRoom(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
        }

        private void splitRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.SplitRoom(this);
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.Mode == EditorMode.Map2D)
                _editor.SelectRooms(_editor.Level.Rooms.Where(room => room != null));
            else
                _editor.SelectedSectors = new SectorSelection { Area = _editor.SelectedRoom.LocalArea };
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.Mode == EditorMode.Map2D)
                EditorActions.DeleteRooms(_editor.SelectedRooms, this);
            else
            {
                if (_editor.SelectedObject != null)
                    EditorActions.DeleteObjectWithWarning(_editor.SelectedObject, this);
            }
        }

        private void editObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedObject != null)
                EditorActions.EditObject(_editor.SelectedObject, this);
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormSearch searchForm = new FormSearch(_editor);
            searchForm.Show(this); // Also disposes: https://social.msdn.microsoft.com/Forums/windows/en-US/5cbf16a9-1721-4861-b7c0-ea20cf328d48/any-difference-between-formclose-and-formdispose?forum=winformsdesigner
        }

        private void rotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedObject != null)
                EditorActions.RotateObject(_editor.SelectedObject, EditorActions.RotationAxis.Y, 45);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.Mode != EditorMode.Map2D)
                EditorActions.TryCopyObject(_editor.SelectedObject, this);
            else
            {
                /*try
                {
                    var data = new RoomClipboardData(_editor);
                    var serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                    serializer.Serialize(new MemoryStream(), data);
                    DarkMessageBox.Show(this, "Working");
                }
                catch (Exception exc)
                {
                    DarkMessageBox.Show(this, "Broken: " + exc.Message);
                }*/
                Clipboard.SetDataObject(new RoomClipboardData(_editor), true);
            }
        }

        private void stampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TryStampObject(_editor.SelectedObject, this);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_editor.Mode != EditorMode.Map2D)
            {
                var data = Clipboard.GetDataObject().GetData(typeof(ObjectClipboardData)) as ObjectClipboardData;
                if (data == null)
                    MessageBox.Show("Clipboard contains no object data.");
                else
                    _editor.Action = new EditorActionPlace(false, (level, room) => data.MergeGetSingleObject(_editor));
            }
            else
            {
                var roomClipboardData = Clipboard.GetDataObject().GetData(typeof(RoomClipboardData)) as RoomClipboardData;
                if (roomClipboardData == null)
                    MessageBox.Show("Clipboard contains no room data.");
                else
                    roomClipboardData.MergeInto(_editor, new VectorInt2());
            }
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

        private void objectListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(ObjectList);
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
            objectListToolStripMenuItem.Checked = dockArea.ContainsContent(ObjectList);
            lightingToolStripMenuItem.Checked = dockArea.ContainsContent(Lighting);
            paletteToolStripMenuItem.Checked = dockArea.ContainsContent(Palette);
            texturePanelToolStripMenuItem.Checked = dockArea.ContainsContent(TexturePanel);
            dockableToolStripMenuItem.Checked = dockArea.ContainsContent(ToolPalette);
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

        private void ToolBox_Show(bool show)
        {
            if (show)
                MainView.AddToolbox(ToolBox);
            else
                MainView.RemoveToolbox(ToolBox);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

            if (EditorActions.DragDropFileSupported(e))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            base.OnDragDrop(e);
            EditorActions.DragDropCommonFiles(e, this);
        }

        private void exportRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.ExportCurrentRoom(this);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout form = new FormAbout())
                form.ShowDialog(this);
        }

        private void dockableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(ToolPalette);
        }

        private void floatingToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ToolBox_Show(floatingToolStripMenuItem.Checked);
        }

        // Only for debugging purposes...

        private void debugAction0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //level.Load("");
            var level = new TombRaider4Level("D:\\Software\\Tomb-Editor\\Build\\Game\\Data\\karnak.tr4");
            level.Load("originale");

           // level = new TombRaider4Level("D:\\Software\\Tomb-Editor\\Build\\Game\\Data\\karnak.tr4");
           // level.Load("editor");

            //level = new TombEngine.TombRaider4Level("e:\\trle\\data\\tut1.tr4");
            //level.Load("originale");
        }

        private void debugAction1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //level.Load("");

            var level = new TombRaider4Level("e:\\trle\\data\\city130.tr4");
            level.Load("crash");

            //level = new TombRaider3Level("e:\\tomb3\\data\\jungle.tr2");
            //level.Load("jungle");
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
            //Wad2.SaveToStream(_editor.Level.Wad, File.OpenWrite("E:\\test.wad2"));
            //Wad2.LoadFromStream(File.OpenRead("E:\\test.wad2"));
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
              _editor.SelectedRoom.AddObject(_editor.Leel, instance);*/
        }

        private void debugAction5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var room in _editor.Level.Rooms)
            {
                for (var x=0;x<room.NumXSectors;x++)
                {
                    for (var z=0;z<room.NumZSectors;z++)
                    {
                        foreach (BlockFace faceId in Enum.GetValues(typeof(BlockFace)))
                        {
                            var face = room.Blocks[x, z].GetFaceTexture(faceId);
                            //if (face.)
                        }
                    }
                }
            }
        }

        private void soundManagerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormSoundEditor(_editor.Level.Wad, true))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    if (DarkMessageBox.Show(this, "Do you want to save changes to original Wad2 file?", "Save changes",
                                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        Wad2.SaveToFile(_editor.Level.Wad, _editor.Level.Wad.FileName);
                    else
                        EditorActions.ReloadWad();

                }
                else
                    EditorActions.ReloadWad();
            }
        }

        private void applyCurrentAmbientLightToAllRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(this,"Do you really want to apply the ambient light of the current room to all rooms?",
                                    "Apply ambient light", MessageBoxButtons.YesNo, MessageBoxIcon.Question)== DialogResult.Yes)
            {
                EditorActions.ApplyCurrentAmbientLightToAllRooms();
                MessageBox.Show(this, "Ambient light was applied to all rooms", "Apply ambient light",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void importRoomsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.ImportRooms(this);
        }
    }
}
