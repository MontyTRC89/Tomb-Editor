using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Controls;
using DarkUI.Docking;
using DarkUI.Forms;
using NLog;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Script;
using TombLib.Utils;
using System.Collections.Generic;

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
        private readonly ToolWindows.SectorOptions SectorOptions = new ToolWindows.SectorOptions();
        private readonly ToolWindows.Lighting Lighting = new ToolWindows.Lighting();
        private readonly ToolWindows.Palette Palette = new ToolWindows.Palette();
        private readonly ToolWindows.TexturePanel TexturePanel = new ToolWindows.TexturePanel();
        private readonly ToolWindows.ObjectList ObjectList = new ToolWindows.ObjectList();
        private readonly ToolWindows.ToolPalette ToolPalette = new ToolWindows.ToolPalette();

        // Floating tool boxes are placed on 3D view at runtime
        private readonly ToolWindows.ToolPaletteFloating ToolBox = new ToolWindows.ToolPaletteFloating();
        private readonly Dictionary<ToolStripItem, string> _originalShortcutKeyDisplayStrings = new Dictionary<ToolStripItem, string>();

        public FormMain(Editor editor)
        {
            InitializeComponent();
            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            Text = "Tomb Editor " + Application.ProductVersion + " - Untitled";
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // Only how debug menu when a debugger is attached...
            debugToolStripMenuItem.Visible = Debugger.IsAttached;

            // Calculate the sizes at runtime since they actually depend on the choosen layout.
            // https://stackoverflow.com/questions/1808243/how-does-one-calculate-the-minimum-client-size-of-a-net-windows-form
            MinimumSize = Configuration.Window_SizeDefault + (Size - ClientSize);

            // DockPanel message filters for drag and resize.
            Application.AddMessageFilter(dockArea.DockContentDragFilter);
            Application.AddMessageFilter(dockArea.DockResizeFilter);

            // Initialize panels
            MainView.InitializeRendering(_editor.RenderingDevice);
            ItemBrowser.InitializeRendering(_editor.RenderingDevice);

            // Restore window settings
            LoadWindowLayout(_editor.Configuration);
            GenerateMenusRecursive(menuStrip.Items);

            // Retrieve clipboard change notifications
            ClipboardEvents.ClipboardChanged += ClipboardEvents_ClipboardChanged;
            ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

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
            if (obj is Editor.SelectedObjectChangedEvent || obj is Editor.ModeChangedEvent)
            {
                ObjectInstance selectedObject = _editor.SelectedObject;
                copyToolStripMenuItem.Enabled = _editor.Mode == EditorMode.Map2D || selectedObject is PositionBasedObjectInstance;
                stampToolStripMenuItem.Enabled = selectedObject is PositionBasedObjectInstance;
                if (obj is Editor.ModeChangedEvent)
                    ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

                deleteToolStripMenuItem.Enabled = _editor.Mode == EditorMode.Map2D || selectedObject != null;
                bookmarkObjectToolStripMenuItem.Enabled = selectedObject != null;
                splitSectorObjectOnSelectionToolStripMenuItem.Enabled = selectedObject is SectorBasedObjectInstance && _editor.SelectedSectors.Valid;
            }
            if (obj is Editor.SelectedSectorsChangedEvent)
            {
                bool validSectorSelection = _editor.SelectedSectors.Valid;
                copyToolStripMenuItem.Enabled = _editor.Mode == EditorMode.Geometry || validSectorSelection;
                if (obj is Editor.ModeChangedEvent)
                    ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);
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
                splitSectorObjectOnSelectionToolStripMenuItem.Enabled = _editor.SelectedObject is SectorBasedObjectInstance && validSectorSelection;
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
                var @event = (Editor.ConfigurationChangedEvent)obj;
                if (@event.Current.Window_Maximized != @event.Previous.Window_Maximized ||
                    @event.Current.Window_Position != @event.Previous.Window_Position ||
                    @event.Current.Window_Size != @event.Previous.Window_Size ||
                    @event.Current.Window_Layout != @event.Previous.Window_Layout)
                    LoadWindowLayout(_editor.Configuration);

                if(@event.UpdateKeyboardShortcuts)
                    GenerateMenusRecursive(menuStrip.Items, true);
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

            // Update object bookmarks
            if (obj is Editor.BookmarkedObjectChanged)
            {
                var @event = (Editor.BookmarkedObjectChanged)obj;
                bookmarkRestoreObjectToolStripMenuItem.Enabled = @event.Current != null;
            }

            // Quit editor
            if (obj is Editor.EditorQuitEvent)
                Close();
        }

        private void GarbageCollectOoriginalShortcutKeyDisplayStrings()
        {
            // Clean up old _originalShortcutKeyDisplayStrings
            foreach (var key in _originalShortcutKeyDisplayStrings.Keys.ToArray())
                if (key.IsDisposed)
                    _originalShortcutKeyDisplayStrings.Remove(key);
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

                            var hotkeysForCommand = _editor.Configuration.Window_HotkeySets[subMenu.Tag.ToString()];

                            // Store original shortcut key display strings
                            string baseShortcutKeyDisplayString;
                            if (!_originalShortcutKeyDisplayStrings.TryGetValue(subMenu, out baseShortcutKeyDisplayString))
                            {
                                _originalShortcutKeyDisplayStrings.Add(subMenu, subMenu.ShortcutKeyDisplayString);
                                baseShortcutKeyDisplayString = subMenu.ShortcutKeyDisplayString;
                            }

                            // Create new shortcut key display
                            subMenu.ShortcutKeyDisplayString = string.Join(", ",
                                new[] { baseShortcutKeyDisplayString } // Preserve existing hot keys.
                                .Concat(hotkeysForCommand.Select(h => h.ToString())) // Add new hot keys
                                .Where(str => !string.IsNullOrWhiteSpace(str))); // Only those which aren't empty
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Don't process reserved camera keys
            if (HotkeySets.ReservedCameraKeys.Contains(keyData))
                return base.ProcessCmdKey(ref msg, keyData);

            // Don't process one-key and shift hotkeys if we're focused on control which allows text input
            var activeControlType = GetFocusedControl(this).GetType().Name;
            if (!keyData.HasFlag(Keys.Control) && !keyData.HasFlag(Keys.Alt) &&
                (activeControlType == "DarkTextBox" ||
                 activeControlType == "DarkComboBox" ||
                 activeControlType == "DarkListBox" ||
                 activeControlType == "UpDownEdit"))
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

        private static Control GetFocusedControl(ContainerControl control)
        {
            return (control.ActiveControl is ContainerControl ? GetFocusedControl((ContainerControl)control.ActiveControl) : control.ActiveControl);
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            if (_editor.Action is IEditorActionDisableOnLostFocus)
                _editor.Action = null;
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

        private void itemBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolWindow_Toggle(ItemBrowser);
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

            GarbageCollectOoriginalShortcutKeyDisplayStrings();
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
            //level.Load("");
            var level = new TestLevel("E:\\trle\\data\\coastal.tr4");

            //var level = new TrLevel();
            //level.LoadLevel("Game\\data\\title.tr4", "", "");
            // level = new TombRaider4Level("D:\\Software\\Tomb-Editor\\Build\\Game\\Data\\karnak.tr4");
            // level.Load("editor");

            //level = new TombEngine.TombRaider4Level("e:\\trle\\data\\tut1.tr4");
            //level.Load("originale");
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
            /*using (var reader = new BinaryReader(File.OpenRead("Font.tr5.pc")))
            {
                var bmp = new Bitmap(256, 768);
                for (var y=0;y<768;y++)
                    for (var x=0;x<256;x++)
                    {
                        var r = reader.ReadByte();
                        var g = reader.ReadByte();
                        var b = reader.ReadByte();
                        var a = reader.ReadByte();
                        bmp.SetPixel(x, y, Color.FromArgb(a, r, g, b));
                    }
                bmp.Save("Font.Tr5.png");
            }*/

            /*for (var j = 3; j <= 5; j++)
            {
                var list = OriginalSoundsDefinitions.LoadSounds(File.OpenRead("Sounds\\TR" + j + "\\sounds.txt"));

                using (var writer = new StreamWriter(File.OpenWrite("Sounds\\TR" + j + "\\Samples.Tr" + j + ".xml")))
                {
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    writer.WriteLine("\t<Sounds>");

                    var i = 0;
                    foreach (var sound in list)
                    {
                        writer.WriteLine("\t\t<Sound Id=\"" + i + "\">");

                        writer.WriteLine("\t\t\t<Name>" + sound.Name.Replace("&", "&amp;") + "</Name>");
                        writer.WriteLine("\t\t\t<Volume>" + sound.Volume + "</Volume>");
                        writer.WriteLine("\t\t\t<Pitch>" + sound.Pitch + "</Pitch>");
                        writer.WriteLine("\t\t\t<Range>" + sound.Range + "</Range>");
                        writer.WriteLine("\t\t\t<Chance>" + sound.Chance + "</Chance>");
                        writer.WriteLine("\t\t\t<L>" + sound.FlagL + "</L>");
                        writer.WriteLine("\t\t\t<N>" + sound.FlagN + "</N>");
                        writer.WriteLine("\t\t\t<P>" + sound.FlagP + "</P>");
                        writer.WriteLine("\t\t\t<R>" + sound.FlagR + "</R>");
                        writer.WriteLine("\t\t\t<V>" + sound.FlagV + "</V>");

                        writer.WriteLine("\t\t\t<Samples>");
                        foreach (var sample in sound.Samples)
                        {
                            writer.WriteLine("\t\t\t\t<Sample>" + sample.Replace("&", "&amp;") + "</Sample>");
                        }
                        writer.WriteLine("\t\t\t</Samples>");

                        writer.WriteLine("\t\t</Sound>");

                        i++;
                    }

                    writer.WriteLine("\t</Sounds>");
                    writer.WriteLine("</xml>");
                }
            }*/
        }

        private void debugScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var script = Script.LoadFromTxt("E:\\trle\\script\\script.txt");
            script.CompileScript("E:\\trle\\script\\");
            //Script.Test();
        }
    }
}
