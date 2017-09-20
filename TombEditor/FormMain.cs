using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Geometry;
using SharpDX;
using System.IO;
using TombEngine;
using NLog;
using TombEditor.Geometry.IO;
using TombLib.Wad;
using TombLib.Utils;
using TombLib.NG;
using DarkUI.Docking;

namespace TombEditor
{
    public partial class FormMain : DarkUI.Forms.DarkForm
    {
        // Dockable tool windows are placed on actual dock panel at runtime.

        private ToolWindows.MainView      MainView;
        private ToolWindows.TriggerList   TriggerList;
        private ToolWindows.RoomOptions   RoomOptions;
        private ToolWindows.ObjectBrowser ObjectBrowser;
        private ToolWindows.SectorOptions SectorOptions;
        private ToolWindows.Lighting      Lighting;
        private ToolWindows.TexturePanel  TexturePanel;


        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private bool _pressedZorY = false;
        private Editor _editor;
        private DeviceManager _deviceManager = new DeviceManager();

        public FormMain()
        {
            InitializeComponent();

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = new Size(1212, 763) + (Size - ClientSize);
            
            // Only how debug menu when a debugger is attached...
            debugToolStripMenuItem.Visible = System.Diagnostics.Debugger.IsAttached;

            // Initialize controls
            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            _editor.Level = Level.CreateSimpleLevel();

            PopulateDockPanel();

            // Initialize color controls
            Lighting.BindParameters();

            // Initialize panels
            MainView.Initialize(_deviceManager);
            ObjectBrowser.Initialize(_deviceManager);
            TexturePanel.Initialize();
            
            // Initialize the geometry importer class
            GeometryImporterExporter.Initialize(_deviceManager);


            this.Text = "Tomb Editor " + Application.ProductVersion + " - Untitled";

            logger.Info("Tomb Editor is ready :)");
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void PopulateDockPanel()
        {
            // DockPanel message filters for drag and resize.
            Application.AddMessageFilter(dockArea.DockContentDragFilter);
            Application.AddMessageFilter(dockArea.DockResizeFilter);

            MainView = new ToolWindows.MainView();
            TriggerList = new ToolWindows.TriggerList();
            RoomOptions = new ToolWindows.RoomOptions();
            ObjectBrowser = new ToolWindows.ObjectBrowser();
            SectorOptions = new ToolWindows.SectorOptions();
            Lighting = new ToolWindows.Lighting();
            TexturePanel = new ToolWindows.TexturePanel();

            if (_editor.Configuration.Window_Layout == null)
            {
                // Add default windows to dock area.
                dockArea.AddContent(MainView);
                dockArea.AddContent(SectorOptions);
                dockArea.AddContent(ObjectBrowser);
                dockArea.AddContent(RoomOptions);
                dockArea.AddContent(TriggerList);
                dockArea.AddContent(Lighting);
                dockArea.AddContent(TexturePanel);
            }
            else
            {
                dockArea.RestoreDockPanelState(_editor.Configuration.Window_Layout, FindDockContentByKey);
            }
        }

        private DarkDockContent FindDockContentByKey(string key)
        {
            switch (key)
            {
                case "MainView"     : return MainView;
                case "TriggerList"  : return TriggerList;
                case "Lighting"     : return Lighting;
                case "ObjectBrowser": return ObjectBrowser;
                case "RoomOptions"  : return RoomOptions;
                case "SectorOptions": return SectorOptions;
                case "TexturePanel" : return TexturePanel;
                default: logger.Warn("Unknown tool window '" + key + "' in configuration."); return null;
            }
        }

            private void EditorEventRaised(IEditorEvent obj)
        {
            // Update room information on the status strip
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.RoomGeometryChangedEvent))
            {
                var room = _editor.SelectedRoom;
                if (room == null)
                    statusStripSelectedRoom.Text = "Selected room: None";
                else
                    statusStripSelectedRoom.Text = "Selected room: " +
                        "Name = " + room + " | " +
                        "X = " + room.Position.X + " | " +
                        "Y = " + room.Position.Y + " | " +
                        "Z = " + room.Position.Z + " | " +
                        "Floor = " + (room.Position.Y + room.GetLowestCorner()) + " | " +
                         "Ceiling = " + (room.Position.Y + room.GetHighestCorner());
            }

            // Update selection information of the status strip
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.SelectedSectorsChangedEvent))
            {
                var room = _editor.SelectedRoom;
                if ((room == null) || !_editor.SelectedSectors.Valid)
                    statusStripSelectionArea.Text = "Selected area: None";
                else
                    statusStripSelectionArea.Text = "Selected area: " +
                        "X₀ = " + (room.Position.X + _editor.SelectedSectors.Area.X) + " | " +
                        "Z₀ = " + (room.Position.Z + _editor.SelectedSectors.Area.Y) + " | " +
                        "X₁ = " + (room.Position.X + _editor.SelectedSectors.Area.Right) + " | " +
                        "Z₁ = " + (room.Position.Z + _editor.SelectedSectors.Area.Bottom);
            }

            // Update application title
            if (obj is Editor.LevelFileNameChanged)
            {
                string LevelName = string.IsNullOrEmpty(_editor.Level.Settings.LevelFilePath) ? "Untitled" :
                    Path.GetFileNameWithoutExtension(_editor.Level.Settings.LevelFilePath);
                Text = "Tomb Editor " + Application.ProductVersion.ToString() + " - " + LevelName;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _editor.Configuration.Window_Layout = dockArea.GetDockPanelState();

            base.OnClosed(e);
            _editor.Configuration.SaveTry();
        }



 



        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Your project will be lost. Do you really want to create a new project?",
                    "New project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;
            _editor.Level = Level.CreateSimpleLevel();
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

                case Keys.C: // Copy
                    if (e.Control)
                        EditorActions.Copy(this);
                    break;

                case Keys.V: // Paste
                    if (e.Control)
                        EditorActions.Paste();
                    break;

                case Keys.B: // Stamp
                    if (e.Control)
                        EditorActions.Clone(this);
                    break;

                case Keys.Delete: // Delete object
                    if (_editor.SelectedRoom == null)
                        return;
                    if (_editor.SelectedObject != null)
                        EditorActions.DeleteObjectWithWarning(_editor.SelectedRoom, _editor.SelectedObject, this);
                    break;

                case Keys.T: // Add trigger
                    if ((_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
                    return;

                case Keys.O: // Show options dialog
                    if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                        EditorActions.EditObject(_editor.SelectedRoom, _editor.SelectedObject, this);
                    break;

                case Keys.Left:
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject, EditorActions.RotationAxis.Y, -1);
                    break;

                case Keys.Right: 
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject, EditorActions.RotationAxis.Y, 1);
                    break;

                case Keys.Up:
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject, EditorActions.RotationAxis.X, 1);
                    break;

                case Keys.Down:
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject, EditorActions.RotationAxis.X, -1);
                    break;

                case Keys.Q:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 0, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.A:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 0, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.W:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 1, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.S:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 1, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.E:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 2, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.D:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 2, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.R: // Rotate object
                    if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                        EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject, EditorActions.RotationAxis.Y, e.Shift ? 5.0f : 45.0f);
                    else if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 3, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.F:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 3, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.Y: // Set camera relocation mode (Z on american keyboards, Y on german keyboards)
                    _pressedZorY = true;
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalFloorCorner, 0, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.H:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalFloorCorner, 0, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.U:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalCeilingCorner, 1, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.J:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalCeilingCorner, 1, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.OemMinus: // US keyboard key in documentation
                case Keys.Oemplus:
                case Keys.Oem3: // US keyboard key for a texture triangle rotation
                case Keys.Oem5: // German keyboard key for a texture triangle rotation
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
            var settings = _editor.Level.Settings;
            string path = ResourceLoader.BrowseTextureFile(settings, settings.TextureFilePath, this);
            if (settings.TextureFilePath == path)
                return;

            settings.TextureFilePath = path;
            _editor.LoadedTexturesChange();
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
            EditorActions.TextureFloor();
        }

        private void textureCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TextureCeiling();
        }

        private void textureWallsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.TextureWalls();
        }

        private void importConvertTextureToPng_Click(object sender, EventArgs e)
        {
            if ((_editor.Level == null) || (_editor.Level.Settings.Textures.Count == 0))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Currently there is no texture loaded to convert it.", "No texture loaded");
                return;
            }

            LevelTexture texture = _editor.Level.Settings.Textures[0];
            if (texture.ImageLoadException != null)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("The texture that should be converted to *.png could not be loaded. " + texture.ImageLoadException.Message, "Error");
                return;
            }
            
            string currentTexturePath = _editor.Level.Settings.MakeAbsolute(texture.Path);
            string pngFilePath = Path.Combine(Path.GetDirectoryName(currentTexturePath), Path.GetFileNameWithoutExtension(currentTexturePath) + ".png");

            if (File.Exists(pngFilePath))
            {
                if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                        "There is already a file at \"" + pngFilePath + "\". Continue and overwrite the file?",
                        "File exist already", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                    return;
            }
            texture.Image.Save(pngFilePath);

            DarkUI.Forms.DarkMessageBox.ShowInformation("TGA texture map was converted to PNG without errors and saved at \"" + pngFilePath + "\".", "Success");
            texture.SetPath(_editor.Level.Settings, pngFilePath);
            _editor.LoadedTexturesChange();
        }

        private void loadWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = _editor.Level.Settings;
            string path = ResourceLoader.BrowseObjectFile(settings, settings.WadFilePath, this);
            if (path == settings.WadFilePath)
                return;

            settings.WadFilePath = path;
            _editor.Level.ReloadWad();
            _editor.LoadedWadsChange(_editor.Level.Wad);
        }

        private void unloadWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Level.Settings.WadFilePath = null;
            _editor.Level.ReloadWad();
            _editor.LoadedWadsChange(null);
        }
        
        private void reloadWadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Level.ReloadWad();
            _editor.LoadedWadsChange(null);
        }

        private void butCropRoom_Click(object sender, EventArgs e)
        {
        }

        private void addCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.AddCamera();
        }

        private void addFlybyCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.AddFlybyCamera();
        }

        private void addSinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.AddSink();
        }

        private void addSoundSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.AddSoundSource();
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Your project will be lost. Do you really want to open an existing project?",
                    "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            if (openFileDialogPRJ2.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                _editor.Level = Prj2Loader.LoadFromPrj2(openFileDialogPRJ2.FileName, new ProgressReporterSimple(this));
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Unable to open \"" + openFileDialogPRJ2.FileName + "\"");
                DarkUI.Forms.DarkMessageBox.ShowError(
                    "There was an error while opening project file. File may be in use or may be corrupted. Exception: " + exc, "Error");
            }
        }
        
        private void importTRLEPRJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Choose actions
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Your project will be lost. Do you really want to open an existing project?",
                    "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            if (openFileDialogPRJ.ShowDialog(this) != DialogResult.OK)
                return;
            string fileName = openFileDialogPRJ.FileName;

            // Start import process
            Level newLevel = null;
            try
            {
                using (var form = new FormOperationDialog("Import PRJ", false, (progressReporter) =>
                    newLevel = PrjLoader.LoadFromPrj(fileName, progressReporter)))
                {
                    if (form.ShowDialog(this) != DialogResult.OK || newLevel == null)
                        return;
                    _editor.Level = newLevel;
                    newLevel = null;
                }
            }
            finally
            {
                newLevel?.Dispose();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_editor.Level.Settings.LevelFilePath))
            {
                saveFileDialogPRJ2.InitialDirectory = Path.GetDirectoryName(_editor.Level.Settings.LevelFilePath);
                saveFileDialogPRJ2.FileName = Path.GetFileName(_editor.Level.Settings.LevelFilePath);
            }
            if (saveFileDialogPRJ2.ShowDialog(this) != DialogResult.OK)
                return;
            
            try
            {
                Prj2Writer.SaveToPrj2(saveFileDialogPRJ2.FileName, _editor.Level);
                _editor.Level.Settings.LevelFilePath = saveFileDialogPRJ2.FileName;
                _editor.LevelFileNameChange();
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Unable to save to \"" + saveFileDialogPRJ2.FileName + "\".");
                DarkUI.Forms.DarkMessageBox.ShowError("There was an error while saving project file. Exception: " + exc, "Error");
            }
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
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void smoothRandomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void smoothRandomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void smoothRandomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void sharpRandomFloorUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void sharpRandomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void sharpRandomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void sharpRandomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void butFlattenFloor_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butFlattenCeiling_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void flattenFloorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void flattenCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void gridWallsIn3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (EditorActions.CheckForRoomAndBlockSelection())
                EditorActions.GridWalls3(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void gridWallsIn5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection())
                return;
            EditorActions.GridWalls5(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butDeleteTrigger_Click(object sender, EventArgs e)
        {
            if ((_editor.SelectedRoom == null) || !(_editor.SelectedObject is TriggerInstance))
                return;
            EditorActions.DeleteObject(_editor.SelectedRoom, _editor.SelectedObject);
        }

        private void butEditTrigger_Click(object sender, EventArgs e)
        {
            if ((_editor.SelectedRoom == null) || !(_editor.SelectedObject is TriggerInstance))
                return;
            EditorActions.EditObject(_editor.SelectedRoom, _editor.SelectedObject, this);
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
            if (!EditorActions.CheckForRoomAndBlockSelection())
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

        
        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAsToolStripMenuItem_Click(sender, e);
        }

        private void cropRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCropRoom_Click(null, null);
        }

        private void splitRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RoomOptions.Split_Room();
        }

        private void copyRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RoomOptions.Copy_Room();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.Copy(this);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.Paste();
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
            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Your project will be lost. Do you really want to exit?",
                    "Exit", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            Close();
        }
    
        private void levelSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormLevelSettings form = new FormLevelSettings(_editor))
                form.ShowDialog(this);
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
            //GeometryImporterExporter.ExportRoomToObj(_editor.SelectedRoom, "room.obj");
        }

        private void debugAction4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //   _editor.Level.)
            GeometryImporterExporter.LoadModel("low-poly-wooden-door.obj");
            RoomGeometryInstance instance = new RoomGeometryInstance(); // 0, _editor.SelectedRoom);
              instance.Model = GeometryImporterExporter.Models["low-poly-wooden-door.obj"];
              instance.Position = new Vector3(4096, 512, 4096);
            _editor.SelectedRoom.AddObject(_editor.Level, instance);

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

        private void darkButton16_Click(object sender, EventArgs e)
        {

        }
    }
}
