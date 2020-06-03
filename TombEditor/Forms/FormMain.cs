using NLog;
using DarkUI.Config;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using DarkUI.Docking;
using DarkUI.Forms;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormMain : DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Editor _editor;

        // Dockable tool windows are placed on actual dock panel at runtime.

        private readonly ToolWindows.MainView MainView = new ToolWindows.MainView();
        private readonly ToolWindows.TriggerList TriggerList = new ToolWindows.TriggerList();
        private readonly ToolWindows.RoomOptions RoomOptions = new ToolWindows.RoomOptions();
        private readonly ToolWindows.ItemBrowser ItemBrowser = new ToolWindows.ItemBrowser();
        private readonly ToolWindows.ImportedGeometryBrowser ImportedGeometryBrowser = new ToolWindows.ImportedGeometryBrowser();
        private readonly ToolWindows.SectorOptions SectorOptions = new ToolWindows.SectorOptions();
        private readonly ToolWindows.Lighting Lighting = new ToolWindows.Lighting();
        private readonly ToolWindows.Palette Palette = new ToolWindows.Palette();
        private readonly ToolWindows.TexturePanel TexturePanel = new ToolWindows.TexturePanel();
        private readonly ToolWindows.ObjectList ObjectList = new ToolWindows.ObjectList();
        private readonly ToolWindows.ToolPalette ToolPalette = new ToolWindows.ToolPalette();

        // Floating tool boxes are placed on 3D view at runtime
        private readonly ToolWindows.ToolPaletteFloating ToolBox = new ToolWindows.ToolPaletteFloating();

        public FormMain(Editor editor)
        {
            InitializeComponent();
            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            // Initialize everything needed
            _editor.RaiseEvent(new Editor.InitEvent());

            Text = "Tomb Editor " + Application.ProductVersion + " - Untitled";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // Only how debug menu when a debugger is attached...
            debugToolStripMenuItem.Visible = Debugger.IsAttached;

            this.SetActualSize();

            // DockPanel message filters for drag and resize.
            Application.AddMessageFilter(dockArea.DockContentDragFilter);
            Application.AddMessageFilter(dockArea.DockResizeFilter);

            // Initialize panels
            MainView.InitializeRendering(_editor.RenderingDevice);
            ItemBrowser.InitializeRendering(_editor.RenderingDevice);
            ImportedGeometryBrowser.InitializeRendering(_editor.RenderingDevice);

            // Restore window settings and prepare UI
            Configuration.LoadWindowProperties(this, _editor.Configuration);
            LoadWindowLayout(_editor.Configuration);
            GenerateMenusRecursive(menuStrip.Items);
            UpdateUIColours();
            UpdateControls();

            // Retrieve clipboard change notifications
            ClipboardEvents.ClipboardChanged += ClipboardEvents_ClipboardChanged;
            ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

            // Add recently opened projects
            RefreshRecentProjectsList();

            // Done
            logger.Info("Tomb Editor is ready :)");
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
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Gray out menu options that do not apply
            if (obj is Editor.SelectedObjectChangedEvent ||
                obj is Editor.ModeChangedEvent ||
                obj is Editor.SelectedSectorsChangedEvent)
            {
                ObjectInstance selectedObject = _editor.SelectedObject;

                bool enableCutCopy = _editor.Mode == EditorMode.Map2D || selectedObject is PositionBasedObjectInstance || _editor.SelectedSectors.Valid;
                copyToolStripMenuItem.Enabled = enableCutCopy;
                cutToolStripMenuItem.Enabled = enableCutCopy;
                deleteToolStripMenuItem.Enabled = _editor.Mode == EditorMode.Map2D || selectedObject != null;

                stampToolStripMenuItem.Enabled = selectedObject is PositionBasedObjectInstance;
                if (obj is Editor.ModeChangedEvent)
                    ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

                bookmarkObjectToolStripMenuItem.Enabled = selectedObject != null;
                splitSectorObjectOnSelectionToolStripMenuItem.Enabled = selectedObject is SectorBasedObjectInstance && _editor.SelectedSectors.Valid;
            }

            // Disable version-specific controls
            if (obj is Editor.InitEvent ||
                obj is Editor.GameVersionChangedEvent ||
                obj is Editor.LevelChangedEvent)
            {
                addFlybyCameraToolStripMenuItem.Enabled = _editor.Level.Settings.GameVersion >= TRVersion.Game.TR4;
                addBoxVolumeToolStripMenuItem.Enabled = _editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main;
            }

            if (obj is Editor.UndoStackChangedEvent)
            {
                var stackEvent = (Editor.UndoStackChangedEvent)obj;
                undoToolStripMenuItem.Enabled = stackEvent.UndoPossible;
                redoToolStripMenuItem.Enabled = stackEvent.RedoPossible;
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
                averageCeilingToolStripMenuItem.Enabled = validSectorSelection;
                averageFloorToolStripMenuItem.Enabled = validSectorSelection;
                gridWallsIn3ToolStripMenuItem.Enabled = validSectorSelection;
                gridWallsIn5ToolStripMenuItem.Enabled = validSectorSelection;
                gridWallsIn3SquaresToolStripMenuItem.Enabled = validSectorSelection;
                gridWallsIn5SquaresToolStripMenuItem.Enabled = validSectorSelection;
                splitSectorObjectOnSelectionToolStripMenuItem.Enabled = _editor.SelectedObject is SectorBasedObjectInstance && validSectorSelection;
            }

            // Update version-specific controls
            if (obj is Editor.InitEvent ||
                obj is Editor.LevelChangedEvent ||
                obj is Editor.GameVersionChangedEvent)
            {
                bool isNG  = _editor.Level.Settings.GameVersion == TRVersion.Game.TRNG;
                bool isT5M = _editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main;

                addSphereVolumeToolStripMenuItem.Enabled    = isT5M;
                addPrismVolumeToolStripMenuItem.Enabled     = isT5M;
                addBoxVolumeToolStripMenuItem.Enabled       = isT5M;
                makeQuickItemGroupToolStripMenuItem.Enabled = isNG;
            }

            // Update compilation statistics
            if (obj is Editor.LevelCompilationCompletedEvent)
            {
                var evt = obj as Editor.LevelCompilationCompletedEvent;
                statusLastCompilation.Text = "Last level output { " + evt.InfoString + " }";
            }

            // Update autosave status
            if (obj is Editor.AutosaveEvent)
            {
                var evt = obj as Editor.AutosaveEvent;
                statusAutosave.Text = evt.Exception == null ? "Autosave OK: " + evt.Time : "Autosave failed!";
                statusAutosave.ForeColor = evt.Exception == null ? statusLastCompilation.ForeColor : Color.LightSalmon;
            }

            // Update room information on the status strip
            if (obj is Editor.SelectedRoomChangedEvent ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomGeometryChangedEvent) ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomSectorPropertiesChangedEvent) ||
                obj is Editor.RoomPropertiesChangedEvent)
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
            if (obj is Editor.SelectedRoomChangedEvent ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomGeometryChangedEvent) ||
                _editor.IsSelectedRoomEvent(obj as Editor.RoomSectorPropertiesChangedEvent) ||
                obj is Editor.SelectedSectorsChangedEvent)
            {
                var room = _editor.SelectedRoom;
                if (room == null || !_editor.SelectedSectors.Valid)
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
                        " | y = [" + (minHeight == int.MaxValue || maxHeight == int.MinValue ? "N/A" : room.Position.Y + minHeight + ", " + (room.Position.Y + maxHeight)) + "]";

                    statusStripLocalSelectionArea.Text = "Local area = " +
                        "(" + _editor.SelectedSectors.Area.X0 + ", " + _editor.SelectedSectors.Area.Y0 + ") \u2192 " +
                        "(" + _editor.SelectedSectors.Area.X1 + ", " + _editor.SelectedSectors.Area.Y1 + ")" +
                        " | y = [" + (minHeight == int.MaxValue || maxHeight == int.MinValue ? "N/A" : minHeight + ", " + maxHeight) + "]";
                }
            }

            // Update application title bar
            if (obj is Editor.LevelFileNameChangedEvent || obj is Editor.HasUnsavedChangesChangedEvent)
            {
                string LevelName = string.IsNullOrEmpty(_editor.Level.Settings.LevelFilePath) ? "Untitled" :
                    PathC.GetFileNameWithoutExtensionTry(_editor.Level.Settings.LevelFilePath);

                Text = "Tomb Editor " + Application.ProductVersion + " - " + LevelName + (_editor.HasUnsavedChanges ? "*" : "");
            }

            // Update save button
            if (obj is Editor.HasUnsavedChangesChangedEvent)
                saveLevelToolStripMenuItem.Enabled = _editor.HasUnsavedChanges;

            // Reload window layout and keyboard shortcuts if the configuration changed
            if (obj is Editor.ConfigurationChangedEvent)
            {
                var e = (Editor.ConfigurationChangedEvent)obj;
                if (e.UpdateKeyboardShortcuts)
                    GenerateMenusRecursive(menuStrip.Items, true);
                if (e.UpdateLayout)
                    LoadWindowLayout(_editor.Configuration);
                UpdateUIColours();
                UpdateControls();
            }

            // Update texture controls
            if (obj is Editor.LoadedTexturesChangedEvent)
                remapTextureToolStripMenuItem.Enabled =
                removeTexturesToolStripMenuItem.Enabled =
                unloadTexturesToolStripMenuItem.Enabled =
                reloadTexturesToolStripMenuItem.Enabled =
                importConvertTexturesToPng.Enabled = _editor.Level.Settings.Textures.Count > 0;

            // Update wad controls
            if (obj is Editor.LoadedWadsChangedEvent)
                removeWadsToolStripMenuItem.Enabled =
                reloadWadsToolStripMenuItem.Enabled = _editor.Level.Settings.Wads.Count > 0;

            if (obj is Editor.LoadedSoundsCatalogsChangedEvent)
                reloadSoundsToolStripMenuItem.Enabled = _editor.Level.Settings.SoundsCatalogs.Count > 0;

            // Update object bookmarks
            if (obj is Editor.BookmarkedObjectChanged)
            {
                var @event = (Editor.BookmarkedObjectChanged)obj;
                bookmarkRestoreObjectToolStripMenuItem.Enabled = @event.Current != null;
            }

            if (obj is Editor.LevelFileNameChangedEvent)
                RefreshRecentProjectsList();

            // Quit editor
            if (obj is Editor.EditorQuitEvent)
                Close();
        }

        private void UpdateUIColours()
        {
            // Refresh all forms if UI colours were changed
            var newButtonHighlightColour = ColorTranslator.FromHtml(_editor.Configuration.UI_FormColor_ButtonHighlight);
            if (Colors.HighlightBase != newButtonHighlightColour)
            {
                Colors.HighlightBase = newButtonHighlightColour;
                foreach (var form in Application.OpenForms)
                    if (form is DarkForm) ((DarkForm)form).Refresh();
            }
        }

        private void UpdateControls()
        {
            ShowRealTintForObjectsToolStripMenuItem.Checked = _editor.Configuration.Rendering3D_ShowRealTintForObjects;
            drawWhiteTextureLightingOnlyToolStripMenuItem.Checked = _editor.Configuration.Rendering3D_ShowLightingWhiteTextureOnly;
        }

        private void RefreshRecentProjectsList()
        {
            openRecentToolStripMenuItem.DropDownItems.Clear();

            if (Properties.Settings.Default.RecentProjects != null && Properties.Settings.Default.RecentProjects.Count > 0)
                foreach(var fileName in Properties.Settings.Default.RecentProjects)
                {
                    if (fileName == _editor.Level.Settings.LevelFilePath)   // Skip currently loaded level
                        continue;

                    var item = new ToolStripMenuItem() { Name = fileName, Text = fileName };
                    item.Click += (s, e) => EditorActions.OpenLevel(this, ((ToolStripMenuItem)s).Text);
                    openRecentToolStripMenuItem.DropDownItems.Add(item);
                }

            // Add "Clear recent files" option
            openRecentToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            var item2 = new ToolStripMenuItem() { Name = "clearRecentMenuItem", Text = "Clear recent file list" };
            item2.Click += (s, e) => { Properties.Settings.Default.RecentProjects.Clear(); RefreshRecentProjectsList(); Properties.Settings.Default.Save(); };
            openRecentToolStripMenuItem.DropDownItems.Add(item2);

            // Disable menu item, if list is empty
            openRecentToolStripMenuItem.Enabled = openRecentToolStripMenuItem.DropDownItems.Count > 2;
        }

        private void GenerateMenusRecursive(ToolStripItemCollection dropDownItems, bool onlyHotkeys = false)
        {
            foreach (object obj in dropDownItems)
            {
                ToolStripMenuItem subMenu = obj as ToolStripMenuItem;

                if (subMenu != null)
                {
                    if (subMenu.HasDropDownItems)
                        GenerateMenusRecursive(subMenu.DropDownItems, onlyHotkeys);
                    else
                    {
                        if (!string.IsNullOrEmpty(subMenu.Tag?.ToString()))
                        {
                            if (!onlyHotkeys)
                            {
                                var command = CommandHandler.GetCommand(subMenu.Tag.ToString());
                                if (command != null)
                                {
                                    subMenu.Click += (sender, e) => { command.Execute?.Invoke(new CommandArgs { Editor = _editor, Window = this }); };
                                    subMenu.Text = command.FriendlyName;
                                }
                            }

                            var hotkeysForCommand = _editor.Configuration.UI_Hotkeys[subMenu.Tag.ToString()];
                            subMenu.ShortcutKeyDisplayString = string.Join(", ", hotkeysForCommand.Select(h => h.ToString()).Where(str => !string.IsNullOrWhiteSpace(str)));
                        }
                    }
                }
            }
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
                case "ItemBrowser":
                case "ObjectBrowser": // Deprecated name
                    return ItemBrowser;
                case "ImportedGeometryBrowser":
                    return ImportedGeometryBrowser;
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

        private void ClipboardEvents_ClipboardChanged(object sender, EventArgs e)
        {
            if (_editor.Mode != EditorMode.Map2D)
                pasteToolStripMenuItem.Enabled = Clipboard.ContainsData(typeof(ObjectClipboardData).FullName) ||
                                                 Clipboard.ContainsData(typeof(SectorsClipboardData).FullName);
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

            if(!e.Cancel)
            {
                // Always save window properties on exit and resave config!
                SaveWindowLayout(_editor.Configuration);
                Configuration.SaveWindowProperties(this, _editor.Configuration);
                _editor.ConfigurationChange(false, false, false, true);
            }

            base.OnFormClosing(e);
        }

        private void LoadWindowLayout(Configuration configuration)
        {
            dockArea.RemoveContent();
            dockArea.RestoreDockPanelState(configuration.Window_Layout, FindDockContentByKey);

            floatingToolStripMenuItem.Checked = configuration.Rendering3D_ToolboxVisible;
            ToolBox.Location = configuration.Rendering3D_ToolboxPosition;
        }

        private void SaveWindowLayout(Configuration configuration)
        {
            configuration.Window_Layout = dockArea.GetDockPanelState();

            configuration.Rendering3D_ToolboxVisible = floatingToolStripMenuItem.Checked;
            configuration.Rendering3D_ToolboxPosition = ToolBox.Location;

            _editor.ConfigurationChange();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (_editor.FlyMode && e.KeyCode == Keys.Menu)
                e.Handled = true;

            var timer = new Timer { Interval = 1 };
            timer.Tick += CheckFlyModeStateTimer_Tick; // We must delay this event, otherwise it won't work
            timer.Start();
        }

        private void CheckFlyModeStateTimer_Tick(object sender, EventArgs e)
        {
            // We must disable the menuStrip in Fly Mode because the menu items are reacting to the ALT key
            menuStrip.Enabled = !_editor.FlyMode;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (_editor.FlyMode && !_editor.Configuration.UI_Hotkeys["ToggleFlyMode"].Contains(keyData))
                return base.ProcessCmdKey(ref msg, keyData);

            // Don't process reserved camera keys
            if (Hotkey.ReservedCameraKeys.Contains(keyData))
                return base.ProcessCmdKey(ref msg, keyData);

            // Don't process one-key and shift hotkeys if we're focused on control which allows text input
            if (WinFormsUtils.CurrentControlSupportsInput(this, keyData))
                return base.ProcessCmdKey(ref msg, keyData);

            CommandHandler.ExecuteHotkey(new CommandArgs
            {
                Editor = _editor,
                KeyData = keyData,
                Window = this
            });

            // Don't open menus with the alt key
            if (keyData.HasFlag(Keys.Alt))
                return true;

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (_editor.Action is IEditorActionDisableOnLostFocus)
                _editor.Action = null;
        }

        private void restoreDefaultLayoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadWindowLayout(new Configuration());
        }

        private void sectorOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(SectorOptions);
        }

        private void roomOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(RoomOptions);
        }

        private void itemBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(ItemBrowser);
        }

        private void importedGeometryBrowserToolstripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(ImportedGeometryBrowser);
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
            itemBrowserToolStripMenuItem.Checked = dockArea.ContainsContent(ItemBrowser);
            importedGeometryBrowserToolstripMenuItem.Checked = dockArea.ContainsContent(ImportedGeometryBrowser);
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

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
                _editor.Focus();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormAbout form = new FormAbout(Properties.Resources.misc_AboutScreen_800))
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
            var batchList = new BatchCompileList();
            batchList.Location = "D:\\FMAP";
            batchList.Files.Add("test1.prj2");
            batchList.Files.Add("test2.prj2");
            batchList.Files.Add("test3.prj2");
            batchList.Files.Add("test4.prj2");
            batchList.Files.Add("test5.prj2");
            BatchCompileList.SaveToXml("BATCH.xml", batchList);
        }

        private void debugAction1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //level.Load("");

            //var level = new TestLevel("e:\\trle\\data\\city130.tr4");
            //level.Load("crash");

            //level = new TombRaider3Level("e:\\tomb3\\data\\jungle.tr2");
            //level.Load("jungle");
        }

        private void debugAction2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*var tempColors = new List<int>();

            var bmp = (Bitmap)System.Drawing.Image.FromFile("Editor\\Palette.png");
            for (int y = 2; y < bmp.Height; y += 14)
            {
                for (int x = 2; x < bmp.Width; x += 14)
                {
                    var col = bmp.GetPixel(x, y);
                    if (col.A == 0)
                        continue;
                    // if (!tempColors.Contains(col.ToArgb()))
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
            }*/
        }

        private void debugAction3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Wad2.SaveToStream(_editor.Level.Wad, File.OpenWrite("E:\\test.wad2"));
            //Wad2.LoadFromStream(File.OpenRead("E:\\test.wad2"));
            //RoomGeometryExporter.ExportRoomToObj(_editor.SelectedRoom, "room.obj");
        }

        private void debugAction4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Conversions.ConvertWad2ToNewSoundFormat("test\\Caesum\\Lara\\Lara.wad2", "Converted.wad2");

            //Conversions.ConvertPrj2ToNewSoundFormat("Karnak_import.prj2", "Karnak_import_converted.prj2", "Sounds\\TR4\\Sounds.tr4.xml");

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
            /*List<string> soundNames = new List<string>();
            using (var reader = new StreamReader(File.OpenRead("H:\\Sounds.txt")))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    soundNames.Add(line.Split(':')[0]);
                }
            }*/

            XmlDocument doc = new XmlDocument();
            doc.Load("H:\\TrCatalog.xml");

            var gameNode = doc.SelectSingleNode("//*[@id='TR2']");
            var soundsNode = gameNode.ChildNodes[2];
            foreach (XmlNode node in soundsNode.ChildNodes)
            {
                int id = int.Parse(node.Attributes["id"].InnerText);
                node.Attributes.Append(doc.CreateAttribute("description"));
                node.Attributes["description"].InnerText = node.Attributes["name"].InnerText;
                node.Attributes["name"].InnerText = "SOUND_" + id;
            }

            doc.Save("H:\\TrCatalog.xml");
        }

        private void debugScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ////var script = Script.LoadFromTxt("E:\\trle\\script\\script.txt");
          // script.CompileScript("E:\\trle\\script\\");
            //Script.Test();
        }

        protected override void WndProc(ref Message message)
        {
            switch (message.Msg)
            {
                case SingleInstanceManagement.WM_COPYDATA:
                    var fileName = SingleInstanceManagement.Catch(ref message);
                    if (fileName != null && Path.GetExtension(fileName) == ".prj2")
                    {
                        SingleInstanceManagement.RestoreWindowState(this);

                        if (_editor.Level.Settings.LevelFilePath != fileName)
                        {
                            // Try to open file only if main window is opened, otherwise try to close everything.
                            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
                                if (Application.OpenForms[i].Name != Name && !(Application.OpenForms[i] is PopUpInfo))
                                    Application.OpenForms[i].Close();

                            EditorActions.OpenLevel(this, fileName);
                        }
                    }
                    break;

                case SingleInstanceManagement.WM_SHOWWINDOW:
                    SingleInstanceManagement.RestoreWindowState(this);
                    break;

                default:
                    base.WndProc(ref message);
                    break;
            }
        }
    }
}
