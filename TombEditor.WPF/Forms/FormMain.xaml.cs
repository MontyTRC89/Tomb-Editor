using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using DarkUI.WPF.CustomControls;
using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Xml;
using TombEditor.WPF.ToolWindows;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using WinForms = System.Windows.Forms;

namespace TombEditor.WPF.Forms;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class FormMain : Window
{
	public FormMain()
	{
		InitializeComponent();
		DataContext = this;

		_editor = Editor.Instance;
		_editor.EditorEventRaised += EditorEventRaised;

		// Initialize everything needed
		_editor.RaiseEvent(new Editor.InitEvent());

		Title = "Tomb Editor " + WinForms.Application.ProductVersion + " - Untitled";
		//Icon = System.Drawing.Icon.ExtractAssociatedIcon(WinForms.Application.ExecutablePath);

		// Only show debug menu when a debugger is attached...
		//debugToolStripMenuItem.Visible = Debugger.IsAttached;

		//this.SetActualSize();

		// DockPanel message filters for drag and resize.
		//Application.AddMessageFilter(dockArea.DockContentDragFilter);
		//Application.AddMessageFilter(dockArea.DockResizeFilter);

		// Initialize panels
		//GetWindow<MainView>().InitializeRendering(_editor.RenderingDevice);
		//GetWindow<ItemBrowser>().InitializeRendering(_editor.RenderingDevice);
		//GetWindow<ImportedGeometryBrowser>().InitializeRendering(_editor.RenderingDevice);

		// Restore window settings and prepare UI
		//Configuration.LoadWindowProperties(this, _editor.Configuration);
		//LoadWindowLayout(_editor.Configuration);
		GenerateMenusRecursive(menuStrip.Items);
		UpdateUIColours();
		UpdateControls();

		// Retrieve clipboard change notifications
		//ClipboardEvents.ClipboardChanged += ClipboardEvents_ClipboardChanged;
		//ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

		// Add recently opened projects
		RefreshRecentProjectsList();

		// HACK: Select all text in search textbox when user clicks on it
		//tbSearchMenu.MouseDown += (object sender, MouseEventArgs e) =>
		//{
		//	if (tbSearchMenu.SelectionLength == 0)
		//		tbSearchMenu.SelectAll();
		//};

		// Done
		logger.Info("Tomb Editor is ready :)");
	}

	public WinForms.NativeWindow NativeWindow
	{
		get
		{
			IntPtr hwnd = new WindowInteropHelper(this).Handle;
			return WinForms.NativeWindow.FromHandle(hwnd);
		}
	}

	private void OnShowEditorOptions(object sender, RoutedEventArgs e)
	{
		var window = new FormOptions(Editor.Instance);
		window.ShowDialog();
	}

	private static readonly Logger logger = LogManager.GetCurrentClassLogger();
	private readonly Editor _editor;

	// Floating tool boxes are placed on 3D view at runtime
	private readonly ToolPaletteFloating ToolBox = new();

	private void EditorEventRaised(IEditorEvent obj)
	{
		// Gray out menu options that do not apply
		if (obj is Editor.SelectedObjectChangedEvent or
			Editor.ModeChangedEvent or
			Editor.SelectedSectorsChangedEvent)
		{
			ObjectInstance selectedObject = _editor.SelectedObject;

			bool enableCutCopy = _editor.Mode == EditorMode.Map2D || selectedObject is PositionBasedObjectInstance || _editor.SelectedSectors.Valid;
			copyToolStripMenuItem.IsEnabled = enableCutCopy;
			cutToolStripMenuItem.IsEnabled = enableCutCopy;
			deleteToolStripMenuItem.IsEnabled = _editor.Mode == EditorMode.Map2D || selectedObject != null;

			stampToolStripMenuItem.IsEnabled = selectedObject is PositionBasedObjectInstance;
			selectFloorBelowObjectToolStripMenuItem.IsEnabled = selectedObject is PositionBasedObjectInstance;

			//if (obj is Editor.ModeChangedEvent)
			//	ClipboardEvents_ClipboardChanged(this, EventArgs.Empty);

			bookmarkObjectToolStripMenuItem.IsEnabled = editObjectToolStripMenuItem.IsEnabled = selectedObject != null;
			splitSectorObjectOnSelectionToolStripMenuItem.IsEnabled = selectedObject is SectorBasedObjectInstance && _editor.SelectedSectors.Valid;
		}

		// Disable version-specific controls
		if (obj is Editor.InitEvent or
			Editor.GameVersionChangedEvent or
			Editor.LevelChangedEvent)
		{
			addSpriteToolStripMenuItem.Visibility = _editor.Level.Settings.GameVersion <= TRVersion.Game.TR2 ? Visibility.Visible : Visibility.Collapsed;

			makeQuickItemGroupToolStripMenuItem.Visibility = _editor.Level.IsNG ? Visibility.Visible : Visibility.Collapsed;

			addBoxVolumeToolStripMenuItem.Visibility = _editor.Level.IsTombEngine ? Visibility.Visible : Visibility.Collapsed;
			addSphereVolumeToolStripMenuItem.Visibility = _editor.Level.IsTombEngine ? Visibility.Visible : Visibility.Collapsed;
			generateObjectNamesToolStripMenuItem.Visibility = _editor.Level.IsTombEngine ? Visibility.Visible : Visibility.Collapsed;
			editEventSetsToolStripMenuItem.Visibility = _editor.Level.IsTombEngine ? Visibility.Visible : Visibility.Collapsed;
		}

		// Clear autosave information
		//if (obj is Editor.LevelChangedEvent ||
		//	obj is Editor.LevelFileNameChangedEvent)
		//	statusAutosave.Text = string.Empty;

		if (obj is Editor.UndoStackChangedEvent)
		{
			var stackEvent = (Editor.UndoStackChangedEvent)obj;
			undoToolStripMenuItem.IsEnabled = stackEvent.UndoPossible;
			redoToolStripMenuItem.IsEnabled = stackEvent.RedoPossible;
		}

		if (obj is Editor.SelectedSectorsChangedEvent)
		{
			bool validSectorSelection = _editor.SelectedSectors.Valid;
			smoothRandomCeilingDownToolStripMenuItem.IsEnabled = validSectorSelection;
			smoothRandomCeilingUpToolStripMenuItem.IsEnabled = validSectorSelection;
			smoothRandomFloorDownToolStripMenuItem.IsEnabled = validSectorSelection;
			smoothRandomFloorUpToolStripMenuItem.IsEnabled = validSectorSelection;
			sharpRandomCeilingDownToolStripMenuItem.IsEnabled = validSectorSelection;
			sharpRandomCeilingUpToolStripMenuItem.IsEnabled = validSectorSelection;
			sharpRandomFloorDownToolStripMenuItem.IsEnabled = validSectorSelection;
			sharpRandomFloorUpToolStripMenuItem.IsEnabled = validSectorSelection;
			averageCeilingToolStripMenuItem.IsEnabled = validSectorSelection;
			averageFloorToolStripMenuItem.IsEnabled = validSectorSelection;
			gridWallsIn3ToolStripMenuItem.IsEnabled = validSectorSelection;
			gridWallsIn5ToolStripMenuItem.IsEnabled = validSectorSelection;
			gridWallsIn3SquaresToolStripMenuItem.IsEnabled = validSectorSelection;
			gridWallsIn5SquaresToolStripMenuItem.IsEnabled = validSectorSelection;
			splitSectorObjectOnSelectionToolStripMenuItem.IsEnabled = _editor.SelectedObject is SectorBasedObjectInstance && validSectorSelection;
		}

		// Update autosave status
		if (obj is Editor.AutosaveEvent)
		{
			var evt = obj as Editor.AutosaveEvent;
			var success = evt.Exception == null;
			//statusAutosave.Text = success ? "Autosave OK: " + evt.Time : "Autosave failed!";
			//statusAutosave.ForeColor = success ? Colors.LightText : Colors.BlueHighlight;

			if (!success)
				_editor.SendMessage("Autosave failed. Error: " + evt.Exception.Message +
									"\nRestart Tomb Editor and continue from last valid version.", PopupType.Warning);
		}

		// Update room information on the status strip
		if (obj is Editor.InitEvent ||
			obj is Editor.LevelChangedEvent ||
			obj is Editor.SelectedRoomChangedEvent ||
			_editor.IsSelectedRoomEvent(obj as Editor.RoomGeometryChangedEvent) ||
			_editor.IsSelectedRoomEvent(obj as Editor.RoomPositionChangedEvent) ||
			_editor.IsSelectedRoomEvent(obj as Editor.RoomSectorPropertiesChangedEvent) ||
			obj is Editor.RoomPropertiesChangedEvent)
		{
			var room = _editor.SelectedRoom;
			//if (room == null)
			//	statusStripSelectedRoom.Text = "Selected room: None";
			//else
			//	statusStripSelectedRoom.Text = "Selected room: " +
			//		"Name = " + room + " | " +
			//		"Size = " + (room.NumXSectors - 2) + " x " + (room.NumZSectors - 2) + " | " +
			//		"Pos = (" + room.Position.X + ", " + room.Position.Y + ", " + room.Position.Z + ") | " +
			//		"Floor = " + (room.Position.Y + room.GetLowestCorner()) + " | " +
			//		"Ceiling = " + (room.Position.Y + room.GetHighestCorner());
		}

		// Update selection information of the status strip
		if (obj is Editor.SelectedRoomChangedEvent ||
			_editor.IsSelectedRoomEvent(obj as Editor.RoomGeometryChangedEvent) ||
			_editor.IsSelectedRoomEvent(obj as Editor.RoomPositionChangedEvent) ||
			_editor.IsSelectedRoomEvent(obj as Editor.RoomSectorPropertiesChangedEvent) ||
			obj is Editor.SelectedSectorsChangedEvent)
		{
			var room = _editor.SelectedRoom;
			if (room == null || !_editor.SelectedSectors.Valid)
			{
				//statusStripGlobalSelectionArea.Text = "Global area: None";
				//statusStripLocalSelectionArea.Text = "Local area: None";
			}
			else
			{
				int minHeight = room.GetLowestCorner(_editor.SelectedSectors.Area);
				int maxHeight = room.GetHighestCorner(_editor.SelectedSectors.Area);

				//statusStripGlobalSelectionArea.Text = "Global area = " +
				//	"(" + (room.Position.X + _editor.SelectedSectors.Area.X0) + ", " + (room.Position.Z + _editor.SelectedSectors.Area.Y0) + ") \u2192 " +
				//	"(" + (room.Position.X + _editor.SelectedSectors.Area.X1) + ", " + (room.Position.Z + _editor.SelectedSectors.Area.Y1) + ")" +
				//	" | y = [" + (minHeight == int.MaxValue || maxHeight == int.MinValue ? "N/A" : room.Position.Y + minHeight + ", " + (room.Position.Y + maxHeight)) + "]";

				//statusStripLocalSelectionArea.Text = "Local area = " +
				//	"(" + _editor.SelectedSectors.Area.X0 + ", " + _editor.SelectedSectors.Area.Y0 + ") \u2192 " +
				//	"(" + _editor.SelectedSectors.Area.X1 + ", " + _editor.SelectedSectors.Area.Y1 + ")" +
				//	" | y = [" + (minHeight == int.MaxValue || maxHeight == int.MinValue ? "N/A" : minHeight + ", " + maxHeight) + "]";
			}
		}

		// Update application title bar
		if (obj is Editor.LevelFileNameChangedEvent || obj is Editor.HasUnsavedChangesChangedEvent)
		{
			string LevelName = string.IsNullOrEmpty(_editor.Level.Settings.LevelFilePath) ? "Untitled" :
				PathC.GetFileNameWithoutExtensionTry(_editor.Level.Settings.LevelFilePath);

			Title = "Tomb Editor " + WinForms.Application.ProductVersion + " - " + LevelName + (_editor.HasUnsavedChanges ? "*" : "");
		}

		// Update save button
		if (obj is Editor.InitEvent ||
			obj is Editor.HasUnsavedChangesChangedEvent)
			saveLevelToolStripMenuItem.IsEnabled = _editor.HasUnsavedChanges;

		// Reload window layout and keyboard shortcuts if the configuration changed
		if (obj is Editor.ConfigurationChangedEvent)
		{
			var e = (Editor.ConfigurationChangedEvent)obj;
			if (e.UpdateKeyboardShortcuts)
				GenerateMenusRecursive(menuStrip.Items, true);
			//if (e.UpdateLayout)
			//	LoadWindowLayout(_editor.Configuration);
			UpdateUIColours();
			UpdateControls();
		}

		// Update texture controls
		if (obj is Editor.LoadedTexturesChangedEvent)
			remapTextureToolStripMenuItem.IsEnabled =
			removeTexturesToolStripMenuItem.IsEnabled =
			unloadTexturesToolStripMenuItem.IsEnabled =
			reloadTexturesToolStripMenuItem.IsEnabled =
			importConvertTexturesToPng.IsEnabled = _editor.Level.Settings.Textures.Count > 0;

		// Update wad controls
		if (obj is Editor.LoadedWadsChangedEvent)
			removeWadsToolStripMenuItem.IsEnabled =
			reloadWadsToolStripMenuItem.IsEnabled = _editor.Level.Settings.Wads.Count > 0;

		if (obj is Editor.LoadedSoundsCatalogsChangedEvent)
			reloadSoundsToolStripMenuItem.IsEnabled = _editor.Level.Settings.SoundCatalogs.Count > 0;

		// Update object bookmarks
		if (obj is Editor.BookmarkedObjectChanged)
		{
			var @event = (Editor.BookmarkedObjectChanged)obj;
			bookmarkRestoreObjectToolStripMenuItem.IsEnabled = @event.Current != null;
		}

		if (obj is Editor.DefaultControlActivationEvent)
		{
			if (((Editor.DefaultControlActivationEvent)obj).ContainerName == GetType().Name)
			{
				tbSearchMenu.Focus();
				tbSearchMenu.SelectAll();
			}
		}

		//if (obj is Editor.ToolWindowToggleEvent)
		//	ToolWindow_Toggle(GetWindow((obj as Editor.ToolWindowToggleEvent).ContentType.FullName) as DarkToolWindow);

		if (obj is Editor.LevelFileNameChangedEvent)
			RefreshRecentProjectsList();

		// Quit editor
		if (obj is Editor.EditorQuitEvent)
			Close();
	}

	private void UpdateUIColours()
	{
		//// Refresh all forms if UI colours were changed
		//var newButtonHighlightColour = ColorTranslator.FromHtml(_editor.Configuration.UI_FormColor_ButtonHighlight);
		//if (Colors.HighlightBase != newButtonHighlightColour)
		//{
		//	Colors.HighlightBase = newButtonHighlightColour;
		//	foreach (var form in Application.OpenForms)
		//		if (form is DarkForm) ((DarkForm)form).Refresh();
		//}

		//// HACK: MenuStrip textbox has no means of updating other way.
		//tbSearchMenu.BackColor = Colors.GreyBackground;
	}

	private void UpdateControls()
	{
		showRealTintForObjectsToolStripMenuItem.IsChecked = _editor.Configuration.Rendering3D_ShowRealTintForObjects;
		drawWhiteTextureLightingOnlyToolStripMenuItem.IsChecked = _editor.Configuration.Rendering3D_ShowLightingWhiteTextureOnly;
		statisticsToolStripMenuItem.IsChecked = _editor.Configuration.UI_ShowStats;
	}

	private void RefreshRecentProjectsList()
	{
		openRecentToolStripMenuItem.Items.Clear();

		if (Properties.Settings.Default.RecentProjects != null && Properties.Settings.Default.RecentProjects.Count > 0)
			foreach (var fileName in Properties.Settings.Default.RecentProjects)
			{
				if (fileName == _editor.Level.Settings.LevelFilePath) // Skip currently loaded level
					continue;

				if (!File.Exists(fileName)) // Skip nonexistent levels
					continue;

				var item = new MenuItem() { Name = fileName, Header = fileName };
				item.Click += (s, e) => EditorActions.OpenLevel(NativeWindow, ((MenuItem)s).Header.ToString());
				openRecentToolStripMenuItem.Items.Add(item);
			}

		// Add "Clear recent files" option
		openRecentToolStripMenuItem.Items.Add(new Separator());
		var item2 = new MenuItem() { Name = "clearRecentMenuItem", Header = "Clear recent file list" };
		item2.Click += (s, e) => { Properties.Settings.Default.RecentProjects.Clear(); RefreshRecentProjectsList(); Properties.Settings.Default.Save(); };
		openRecentToolStripMenuItem.Items.Add(item2);

		// Disable menu item, if list is empty
		openRecentToolStripMenuItem.IsEnabled = openRecentToolStripMenuItem.Items.Count > 2;
	}

	private void GenerateMenusRecursive(ItemCollection dropDownItems, bool onlyHotkeys = false)
	{
		foreach (object obj in dropDownItems)
		{
			MenuItem subMenu = obj as MenuItem;

			if (subMenu != null)
			{
				if (subMenu.HasItems)
					GenerateMenusRecursive(subMenu.Items, onlyHotkeys);
				else
				{
					if (!string.IsNullOrEmpty(subMenu.Tag?.ToString()))
					{
						if (!onlyHotkeys)
						{
							var command = CommandHandler.GetCommand(subMenu.Tag.ToString());
							if (command != null)
							{
								subMenu.Click += (sender, e) => { command.Execute?.Invoke(new CommandArgs { Editor = _editor, Window = NativeWindow }); };
								subMenu.Header = command.Type == CommandType.Windows ? command.Name.Replace("Show", string.Empty).SplitCamelcase() : command.FriendlyName;
							}
						}

						var hotkeysForCommand = _editor.Configuration.UI_Hotkeys[subMenu.Tag.ToString()];
						subMenu.InputGestureText = string.Join(", ", hotkeysForCommand.Select(h => h.ToString()).Where(str => !string.IsNullOrWhiteSpace(str)));
					}
				}
			}
		}
	}

	private void OnLayoutRootPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		var activeContent = ((LayoutRoot)sender).ActiveContent;

		if (e.PropertyName == "ActiveContent")
		{

		}
	}

	private MenuItem FindAndExpandMenu(ItemCollection dropDownItems, string searchRequest, MenuItem parent = null)
	{
		if (string.IsNullOrEmpty(searchRequest))
			return null;

		MenuItem result = null;

		foreach (object obj in dropDownItems)
		{
			MenuItem subMenu = obj as MenuItem;

			if (subMenu == null)
				continue;

			if (subMenu.HasItems)
			{
				result = FindAndExpandMenu(subMenu.Items, searchRequest, subMenu);
				if (result != null)
					break;
			}
			else
			{

				if (subMenu.IsEnabled && subMenu.Header.ToString().IndexOf(searchRequest, 0, StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					subMenu.Focus();
					result = parent;
					break;
				}
			}
		}

		if (result != null)
		{
			//result.ShowDropDown();
			return parent;
		}

		return null;
	}

	//private void ClipboardEvents_ClipboardChanged(object sender, EventArgs e)
	//{
	//	if (_editor.Mode != EditorMode.Map2D)
	//		pasteToolStripMenuItem.IsEnabled = Clipboard.ContainsData(typeof(ObjectClipboardData).FullName) ||
	//										 Clipboard.ContainsData(typeof(SectorsClipboardData).FullName);
	//	else
	//		pasteToolStripMenuItem.IsEnabled = Clipboard.ContainsData(typeof(RoomClipboardData).FullName);
	//}

	//protected override void OnFormClosing(FormClosingEventArgs e)
	//{
	//	switch (e.CloseReason)
	//	{
	//		case CloseReason.None:
	//		case CloseReason.UserClosing:
	//			if (!EditorActions.ContinueOnFileDrop(this, "Exit"))
	//				e.Cancel = true;
	//			break;
	//	}

	//	if (!e.Cancel)
	//	{
	//		// Always save window properties on exit and resave config!
	//		SaveWindowLayout(_editor.Configuration);
	//		Configuration.SaveWindowProperties(this, _editor.Configuration);
	//		_editor.ConfigurationChange(false, false, false, true);
	//	}

	//	base.OnFormClosing(e);
	//}

	//private void LoadWindowLayout(Configuration configuration)
	//{
	//	dockArea.RemoveContent();
	//	dockArea.RestoreDockPanelState(configuration.Window_Layout, GetWindow);

	//	floatingToolStripMenuItem.Checked = configuration.Rendering3D_ToolboxVisible;
	//	ToolBox.Location = configuration.Rendering3D_ToolboxPosition;
	//}

	//private void SaveWindowLayout(Configuration configuration)
	//{
	//	configuration.Window_Layout = dockArea.GetDockPanelState();

	//	configuration.Rendering3D_ToolboxVisible = floatingToolStripMenuItem.Checked;
	//	configuration.Rendering3D_ToolboxPosition = ToolBox.Location;
	//}

	//protected override bool ProcessDialogKey(Keys keyData)
	//{
	//	// Snatch ALT key processing for the fly mode to disable menus
	//	if (_editor.FlyMode && keyData.HasFlag(Keys.Alt))
	//		return true;
	//	else
	//		return base.ProcessDialogKey(keyData);
	//}

	//protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	//{
	//	// Disable all hotkeys in fly mode except ToggleFlyMode
	//	if (_editor.FlyMode && !_editor.Configuration.UI_Hotkeys["ToggleFlyMode"].Contains(keyData))
	//		return base.ProcessCmdKey(ref msg, keyData);

	//	// Don't process reserved camera keys
	//	if (WinFormsUtils.DirectionalCameraKeys.Contains(keyData))
	//		return base.ProcessCmdKey(ref msg, keyData);

	//	// Don't process one-key and shift hotkeys if we're focused on control which allows text input
	//	if (WinFormsUtils.CurrentControlSupportsInput(this, keyData))
	//		return base.ProcessCmdKey(ref msg, keyData);

	//	CommandHandler.ExecuteHotkey(new CommandArgs
	//	{
	//		Editor = _editor,
	//		KeyData = keyData,
	//		Window = this
	//	});

	//	// Don't open menus with the alt key
	//	if (keyData.HasFlag(Keys.Alt))
	//		return true;

	//	return base.ProcessCmdKey(ref msg, keyData);
	//}

	//private void restoreDefaultLayoutToolStripMenuItem_Click(object sender, EventArgs e)
	//{
	//	LoadWindowLayout(new Configuration());
	//}

	//private void ToolWindow_Toggle(DarkToolWindow toolWindow)
	//{
	//	if (toolWindow == null)
	//		return;

	//	if (toolWindow.DockPanel == null)
	//		dockArea.AddContent(toolWindow);
	//	else
	//		dockArea.RemoveContent(toolWindow);
	//}

	//private void ToolWindow_BuildMenu()
	//{
	//	sectorOptionsToolStripMenuItem.Checked = dockArea.ContainsContent(GetWindow<SectorOptions>());
	//	roomOptionsToolStripMenuItem.Checked = dockArea.ContainsContent(GetWindow<RoomOptions>());
	//	itemBrowserToolStripMenuItem.Checked = dockArea.ContainsContent(GetWindow<ItemBrowser>());
	//	importedGeometryBrowserToolstripMenuItem.Checked = dockArea.ContainsContent(GetWindow<ImportedGeometryBrowser>());
	//	triggerListToolStripMenuItem.Checked = dockArea.ContainsContent(GetWindow<TriggerList>());
	//	objectListToolStripMenuItem.Checked = dockArea.ContainsContent(GetWindow<ObjectList>());
	//	lightingToolStripMenuItem.Checked = dockArea.ContainsContent(GetWindow<Lighting>());
	//	paletteToolStripMenuItem.Checked = dockArea.ContainsContent(GetWindow<Palette>());
	//	texturePanelToolStripMenuItem.Checked = dockArea.ContainsContent(GetWindow<TexturePanel>());
	//	dockableToolStripMenuItem.Checked = dockArea.ContainsContent(GetWindow<ToolPalette>());
	//}

	//private void ToolWindow_Added(object sender, DockContentEventArgs e)
	//{
	//	if (dockArea.Contains(e.Content))
	//		ToolWindow_BuildMenu();
	//}

	//private void ToolWindow_Removed(object sender, DockContentEventArgs e)
	//{
	//	if (!dockArea.Contains(e.Content))
	//		ToolWindow_BuildMenu();
	//}

	//protected override void OnDragEnter(DragEventArgs e)
	//{
	//	base.OnDragEnter(e);

	//	if (EditorActions.DragDropFileSupported(e))
	//		e.Effect = DragDropEffects.Move;
	//	else
	//		e.Effect = DragDropEffects.None;
	//}

	//protected override void OnDragDrop(DragEventArgs e)
	//{
	//	base.OnDragDrop(e);
	//	EditorActions.DragDropCommonFiles(e, this);
	//}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);
		//_editor.Focus();
	}

	//private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
	//{
	//	using (FormAbout form = new FormAbout(Properties.Resources.misc_AboutScreen_800))
	//		form.ShowDialog(this);
	//}

	//private void floatingToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
	//{
	//	if (floatingToolStripMenuItem.Checked)
	//		GetWindow<MainView>().AddToolbox(ToolBox);
	//	else
	//		GetWindow<MainView>().RemoveToolbox(ToolBox);
	//}

	// Only for debugging purposes...

	private void debugAction0ToolStripMenuItem_Click(object sender, EventArgs e)
	{
		//EditorActions.ConvertLevelToTombEngine(this);
	}

	private void debugAction1ToolStripMenuItem_Click(object sender, EventArgs e)
	{

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

	//protected override void WndProc(ref Message message)
	//{
	//	switch (message.Msg)
	//	{
	//		case SingleInstanceManagement.WM_COPYDATA:
	//			var fileName = SingleInstanceManagement.Catch(ref message);
	//			if (fileName != null && Path.GetExtension(fileName) == ".prj2")
	//			{
	//				SingleInstanceManagement.RestoreWindowState(this);

	//				if (_editor.Level.Settings.LevelFilePath != fileName)
	//				{
	//					// Try to open file only if main window is opened, otherwise try to close everything.
	//					for (int i = Application.OpenForms.Count - 1; i >= 0; i--)
	//						if (Application.OpenForms[i].Name != Name && !(Application.OpenForms[i] is PopUpInfo))
	//							Application.OpenForms[i].Close();

	//					EditorActions.OpenLevel(this, fileName);
	//				}
	//			}
	//			break;

	//		case SingleInstanceManagement.WM_SHOWWINDOW:
	//			SingleInstanceManagement.RestoreWindowState(this);
	//			break;

	//		default:
	//			base.WndProc(ref message);
	//			break;
	//	}
	//}

	//private void tbSearchMenu_KeyDown(object sender, KeyEventArgs e)
	//{
	//	if (e.KeyCode == Keys.Return)
	//		FindAndExpandMenu(menuStrip.Items, tbSearchMenu.Text);
	//}

	//private void butFindMenu_Click(object sender, EventArgs e)
	//{
	//	FindAndExpandMenu(menuStrip.Items, tbSearchMenu.Text);
	//}
}
