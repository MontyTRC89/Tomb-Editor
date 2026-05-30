using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TombEditor.Controls;
using TombEditor.Controls.Panel3D;
using TombEditor.Features.DockableViews.ContentBrowser;
using TombEditor.Features.DockableViews.ImportedGeometryBrowser;
using TombEditor.Features.DockableViews.ItemBrowser;
using TombEditor.Features.DockableViews.LightingPanel;
using TombEditor.Features.DockableViews.ObjectList;
using TombEditor.Features.DockableViews.PalettePanel;
using TombEditor.Features.DockableViews.RoomOptionsPanel;
using TombEditor.Features.DockableViews.RoomsPanel;
using TombEditor.Features.DockableViews.SectorOptionsPanel;
using TombEditor.Features.DockableViews.TexturePanel;
using TombEditor.Features.DockableViews.TriggerList;
using TombEditor.Features.Panel3D.ObjectBrush;
using TombEditor.Features.Panel3D.ToolPalette;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.LevelData;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor;

public partial class MainWindow : Window
{

	private readonly Editor _editor;
	private readonly Panel3D _panel3D;
	private readonly Panel2DMap _panel2DMap;
	private readonly ToolPaletteFloating _toolPalette;
	private readonly ObjectBrushToolbox _objectBrushToolbox;

	private readonly IMessageService _messageService;
	private readonly ILocalizationService _localizationService;

	// XML snapshot of the dock layout as declared in MainWindow.xaml, captured the
	// first time the window finishes loading. Switching to "Default" deserializes
	// this back so the user can always recover the baseline arrangement.
	private string _defaultDockState;

	public MainWindow(Editor editor)
	{
		_editor = editor;
		_messageService = ServiceLocator.ResolveService<IMessageService>();
		_localizationService = ServiceLocator.ResolveService<ILocalizationService>();
		InitializeComponent();

		// Host the WinForms Panel3D inside the AvalonDock document. Rendering is initialized
		// here (same call path FormMain uses) so the device is ready before the first paint.
		// The 3D pipeline will be rewritten as part of the separate Vulkan/OpenGL refactor;
		// keeping the existing WinForms control hosted via WindowsFormsHost avoids doing that
		// work twice.
		_panel3D = new Panel3D();
		_panel3D.InitializeRendering(
			_editor.RenderingDevice,
			_editor.Configuration.Rendering3D_Antialias,
			(ObjectRenderingQuality)_editor.Configuration.Rendering3D_ObjectQuality);

		panel3DHost.Child = _panel3D;

		// The 2D map shares the document area with the 3D view as a sibling tab. The user can
		// switch via the toolbar (Switch2DMode brings the tab forward) or by clicking the tab
		// header directly.
		_panel2DMap = new Panel2DMap();
		panel2DMapHost.Child = _panel2DMap;

		// The imported geometry preview needs the rendering device too; it owns its WindowsFormsHost
		// internally and exposes InitializeRendering to mirror the FormMain init path.
		importedGeometryBrowserView.InitializeRendering(_editor.RenderingDevice);
		itemBrowserView.InitializeRendering(_editor.RenderingDevice);

		// ContentBrowser has its own ViewModel and editor-event bridge (kept on the WinForms
		// ContentBrowser wrapper that FormMain still uses). Activate the bridge in WPF here.
		contentBrowserView.AttachToEditor(_editor);

		// Floating toolboxes are WinForms children of the WinForms Panel3D — they "live"
		// inside the 3D viewport (snap to edges, drag, etc. via DarkFloatingToolbox). The
		// tool palette is shown unconditionally; the object-brush toolbox only appears in
		// ObjectPlacement mode, mirroring FormMain.cs.
		_toolPalette = new ToolPaletteFloating
		{
			Location = _editor.Configuration.Window_Layout.ToolboxPosition,
		};
		_objectBrushToolbox = new ObjectBrushToolbox();

		_panel3D.Controls.Add(_toolPalette);

		_editor.EditorEventRaised += OnEditorEventRaised;

		// Apply the current configuration to the freshly-created Panel3D so toolbar toggles
		// (DrawAllRooms, DrawPortals, etc.) reflect the persisted state on first paint.
		ApplyConfigurationToPanel3D(_editor.Configuration);

		flybyTimelineView.Initialize();
		ApplyFlybyTimelineVisibility();

		// Capture the XAML-declared dock arrangement as the "Default" layout, then
		// restore the user's previously-selected custom layout (if any). Loaded fires
		// once the visual tree is realized, which is when AvalonDock's layout root
		// is in its initial state.
		Loaded += OnLoaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		Loaded -= OnLoaded;

		_defaultDockState = SerializeDockState();

		var activeName = _editor.Configuration.Window_ActiveLayoutName;
		if (string.IsNullOrEmpty(activeName))
			return;

		var active = _editor.Configuration.Window_CustomLayouts.FirstOrDefault(l => l.Name == activeName);
		if (active is null || string.IsNullOrEmpty(active.AvalonDockState))
			return;

		LoadDockState(active.AvalonDockState);
	}

	private void OnEditorEventRaised(IEditorEvent obj)
	{
		OnEditorEventForFloatingToolboxes(obj);

		// Toolbar toggle commands flip a flag on Editor.Configuration and then raise
		// ConfigurationChangedEvent (see CommandHandler entries like "DrawAllRooms"). Mirror
		// MainView.RefreshControls(...) so the hosted Panel3D actually picks up the change.
		if (obj is Editor.ConfigurationChangedEvent || obj is Editor.InitEvent)
		{
			ApplyConfigurationToPanel3D(_editor.Configuration);
			ApplyFlybyTimelineVisibility();
		}

		if (obj is Editor.ToolWindowToggleEvent toggle && _anchorableIdByType.TryGetValue(toggle.ContentType, out var id))
			ToggleAnchorable(id);

		// Bring the matching document tab forward when the editor mode switches between
		// 3D-style modes and Map2D. Avoids requiring the user to also click the tab header.
		if (obj is Editor.ModeChangedEvent)
		{
			if (_editor.Mode == EditorMode.Map2D)
				map2DDocument.IsActive = true;
			else
				mainViewDocument.IsActive = true;
		}

		if (obj is Editor.SwitchLayoutEvent layoutEvent)
			ApplyLayoutByIndex(layoutEvent.LayoutIndex);
	}

	private void OnEditorEventForFloatingToolboxes(IEditorEvent obj)
	{
		if (obj is Editor.ToolChangedEvent or Editor.ModeChangedEvent or Editor.InitEvent)
		{
			bool showBrush = _editor.Mode == EditorMode.ObjectPlacement;

			if (showBrush && _objectBrushToolbox.Parent is null)
			{
				_panel3D.Controls.Add(_objectBrushToolbox);
				_objectBrushToolbox.Location = _editor.Configuration.Window_Layout.ObjectBrushToolboxPosition;
			}
			else if (!showBrush && _objectBrushToolbox.Parent is not null)
			{
				_editor.Configuration.Window_Layout.ObjectBrushToolboxPosition = _objectBrushToolbox.Location;
				_panel3D.Controls.Remove(_objectBrushToolbox);
			}
		}
	}

	private void ApplyConfigurationToPanel3D(Configuration settings)
	{
		// Clear stale selection when the kind of object the user has selected becomes hidden,
		// otherwise the gizmo would float over nothing — same defensive logic MainView uses.
		if (!settings.Rendering3D_ShowStatics && _panel3D.ShowStatics && _editor.SelectedObject is StaticInstance)
			_editor.SelectedObject = null;
		if (!settings.Rendering3D_ShowMoveables && _panel3D.ShowMoveables && _editor.SelectedObject is MoveableInstance)
			_editor.SelectedObject = null;
		if ((!settings.Rendering3D_ShowImportedGeometry && _panel3D.ShowImportedGeometry)
			|| (!settings.Rendering3D_DisablePickingForImportedGeometry && _panel3D.DisablePickingForImportedGeometry))
		{
			if (_editor.SelectedObject is ImportedGeometryInstance)
				_editor.SelectedObject = null;
		}
		if (!settings.Rendering3D_ShowGhostBlocks && _panel3D.ShowGhostBlocks && _editor.SelectedObject is GhostBlockInstance)
			_editor.SelectedObject = null;
		if (!settings.Rendering3D_ShowVolumes && _panel3D.ShowVolumes && _editor.SelectedObject is VolumeInstance)
			_editor.SelectedObject = null;
		if (!settings.Rendering3D_ShowOtherObjects && _panel3D.ShowOtherObjects)
		{
			if (_editor.SelectedObject is LightInstance
				or CameraInstance
				or FlybyCameraInstance
				or SinkInstance
				or SoundSourceInstance)
			{
				_editor.SelectedObject = null;
			}
		}

		_panel3D.ShowPortals = settings.Rendering3D_ShowPortals;
		_panel3D.ShowHorizon = settings.Rendering3D_ShowHorizon;
		_panel3D.ShowAllRooms = settings.Rendering3D_ShowAllRooms;
		_panel3D.ShowRoomNames = settings.Rendering3D_ShowRoomNames;
		_panel3D.ShowCardinalDirections = settings.Rendering3D_ShowCardinalDirections;
		_panel3D.ShowIllegalSlopes = settings.Rendering3D_ShowIllegalSlopes;
		_panel3D.ShowMoveables = settings.Rendering3D_ShowMoveables;
		_panel3D.ShowStatics = settings.Rendering3D_ShowStatics;
		_panel3D.ShowImportedGeometry = settings.Rendering3D_ShowImportedGeometry;
		_panel3D.ShowGhostBlocks = settings.Rendering3D_ShowGhostBlocks;
		_panel3D.ShowOtherObjects = settings.Rendering3D_ShowOtherObjects;
		_panel3D.ShowVolumes = settings.Rendering3D_ShowVolumes;
		_panel3D.ShowBoundingBoxes = settings.Rendering3D_ShowBoundingBoxes;
		_panel3D.ShowSlideDirections = settings.Rendering3D_ShowSlideDirections;
		_panel3D.ShowExtraBlendingModes = settings.Rendering3D_ShowExtraBlendingModes;
		_panel3D.HideTransparentFaces = settings.Rendering3D_HideTransparentFaces;
		_panel3D.BilinearFilter = settings.Rendering3D_BilinearFilter;
		_panel3D.DisablePickingForImportedGeometry = settings.Rendering3D_DisablePickingForImportedGeometry;
		_panel3D.DisablePickingForHiddenRooms = settings.Rendering3D_DisablePickingForHiddenRooms;
		_panel3D.ShowLightMeshes = settings.Rendering3D_ShowLightRadius;
		_panel3D.ShowLightingWhiteTextureOnly = settings.Rendering3D_ShowLightingWhiteTextureOnly;
		_panel3D.ShowRealTintForObjects = settings.Rendering3D_ShowRealTintForObjects;

		_panel3D.Invalidate();
	}

	protected override void OnClosed(System.EventArgs e)
	{
		// Persist floating toolbox positions before disposing them (matches FormMain save path).
		_editor.Configuration.Window_Layout.ToolboxPosition = _toolPalette.Location;
		if (_objectBrushToolbox.Parent is not null)
			_editor.Configuration.Window_Layout.ObjectBrushToolboxPosition = _objectBrushToolbox.Location;

		// Snapshot the user's tweaks back into the active custom layout so they survive restart.
		SaveCurrentStateToActiveLayout();

		_editor.EditorEventRaised -= OnEditorEventRaised;

		// Each migrated WPF view subscribes to Editor.EditorEventRaised in its constructor;
		// Cleanup() unsubscribes and releases ViewModels. Without this the Editor leaks event
		// handlers across MainWindow lifetimes (and ViewModels go on receiving events).
		roomsView?.Cleanup();
		roomOptionsView?.Cleanup();
		sectorOptionsView?.Cleanup();
		lightingView?.Cleanup();
		paletteView?.Cleanup();
		triggerListView?.Cleanup();
		objectListView?.Cleanup();
		importedGeometryBrowserView?.Cleanup();
		itemBrowserView?.Cleanup();
		texturePanelView?.Cleanup();
		contentBrowserView?.Cleanup();
		flybyTimelineView?.Cleanup();

		base.OnClosed(e);
	}

	#region Tool window toggle

	// ToggleToolWindow(Type) → ContentId mapping. Keep in sync with the LayoutAnchorable ContentIds in XAML.
	private static readonly Dictionary<Type, string> _anchorableIdByType = new()
	{
		[typeof(ObjectList)]              = "objectList",
		[typeof(SectorOptions)]           = "sectorOptions",
		[typeof(RoomOptions)]             = "roomOptions",
		[typeof(TriggerList)]             = "triggerList",
		[typeof(TexturePanel)]            = "texturePanel",
		[typeof(ItemBrowser)]             = "itemBrowser",
		[typeof(ImportedGeometryBrowser)] = "importedGeometryBrowser",
		[typeof(ContentBrowser)]          = "contentBrowser",
		[typeof(Lighting)]                = "lighting",
		[typeof(Palette)]                 = "palette",
	};

	private void ToggleAnchorable(string contentId)
	{
		if (string.IsNullOrEmpty(contentId))
			return;
		if (FindAnchorable(contentId) is { } anchorable)
		{
			if (anchorable.IsHidden)
				anchorable.Show();
			else
				anchorable.Hide();
		}
	}

	private LayoutAnchorable FindAnchorable(string contentId)
		=> dockManager.Layout.Descendents().OfType<LayoutAnchorable>().FirstOrDefault(a => a.ContentId == contentId);

	private void ApplyFlybyTimelineVisibility()
	{
		bool shouldBeVisible = _editor.Configuration.Window_Layout.ShowFlybyTimeline;
		if (shouldBeVisible && flybyTimelineAnchorable.IsHidden)
			flybyTimelineAnchorable.Show();
		else if (!shouldBeVisible && !flybyTimelineAnchorable.IsHidden)
			flybyTimelineAnchorable.Hide();
	}

	#endregion

	private void AboutMenu_Click(object sender, RoutedEventArgs e)
	{
		using var form = new FormAbout(Properties.Resources.misc_AboutScreen_800);
		form.ShowDialog(this.GetWin32Window());
	}

	private void WindowMenu_SubmenuOpened(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuItem windowMenu)
			return;

		foreach (var item in windowMenu.Items.OfType<MenuItem>())
		{
			string commandName = EditorMenu.GetCommand(item);
			if (string.IsNullOrEmpty(commandName))
				continue;

			CommandObj cmd;
			try { cmd = CommandHandler.GetCommand(commandName); }
			catch { continue; }
			if (cmd is null || cmd.Type != CommandType.Windows)
				continue;

			item.IsCheckable = true;

			if (commandName == "ShowFlybyTimeline")
				item.IsChecked = _editor.Configuration.Window_Layout.ShowFlybyTimeline;
			else if (cmd.Execute is { } && _anchorableIdByType.Values.Contains(NormalizedContentIdFor(commandName)))
				item.IsChecked = FindAnchorable(NormalizedContentIdFor(commandName)) is { IsHidden: false };
		}
	}

	private static string NormalizedContentIdFor(string showCommandName)
	{
		// "ShowItemBrowser" → "itemBrowser" — first letter lowercased after stripping "Show".
		var name = showCommandName.StartsWith("Show") ? showCommandName.Substring(4) : showCommandName;
		if (string.IsNullOrEmpty(name))
			return string.Empty;
		return char.ToLowerInvariant(name[0]) + name.Substring(1);
	}

	private void CustomizeToolbar_Click(object sender, RoutedEventArgs e)
	{
		// The WPF toolbar's button set is currently hardcoded in XAML, so the
		// reordering committed via this dialog only affects the WinForms shell
		// (UI_ToolbarButtons in Configuration). Still surfaces the dialog so
		// users can manage the persisted button list for the legacy shell.
		var allCommands = CommandHandler.Commands.Select(c => c.Name).ToList();
		var vm = new Features.Dialogs.ToolBarLayout.ToolBarLayoutWindowViewModel(_editor, allCommands);
		var dialog = new Features.Dialogs.ToolBarLayout.ToolBarLayoutWindow
		{
			DataContext = vm,
			Owner = this
		};
		dialog.ShowDialog();
	}

	#region Layout management

	private string SerializeDockState()
	{
		var serializer = new XmlLayoutSerializer(dockManager);
		using var sw = new StringWriter();
		serializer.Serialize(sw);
		return sw.ToString();
	}

	private void LoadDockState(string xml)
	{
		if (string.IsNullOrEmpty(xml))
			return;

		var serializer = new XmlLayoutSerializer(dockManager);
		serializer.LayoutSerializationCallback += OnLayoutSerializationCallback;
		try
		{
			using var sr = new StringReader(xml);
			serializer.Deserialize(sr);
		}
		finally
		{
			serializer.LayoutSerializationCallback -= OnLayoutSerializationCallback;
		}
	}

	private void OnLayoutSerializationCallback(object sender, LayoutSerializationCallbackEventArgs e)
	{
		// Re-attach the existing view instances by ContentId. Any ContentId not listed
		// here is something the XML knows about but the current build no longer ships —
		// cancel so AvalonDock drops it instead of creating a phantom dock entry.
		e.Content = e.Model.ContentId switch
		{
			"rooms" => roomsView,
			"objectList" => objectListView,
			"sectorOptions" => sectorOptionsView,
			"roomOptions" => roomOptionsView,
			"triggerList" => triggerListView,
			"texturePanel" => texturePanelView,
			"itemBrowser" => itemBrowserView,
			"importedGeometryBrowser" => importedGeometryBrowserView,
			"contentBrowser" => contentBrowserView,
			"lighting" => lightingView,
			"palette" => paletteView,
			"mainView" => panel3DHost,
			"map2DView" => panel2DMapHost,
			_ => null
		};

		if (e.Content is null)
			e.Cancel = true;
	}

	private void ApplyLayoutByIndex(int index)
	{
		var config = _editor.Configuration;

		// Save tweaks to whatever layout was active before switching.
		SaveCurrentStateToActiveLayout();

		if (index < 0)
		{
			config.Window_ActiveLayoutName = string.Empty;
			LoadDockState(_defaultDockState);
			return;
		}

		if (index >= config.Window_CustomLayouts.Count)
			return;

		var target = config.Window_CustomLayouts[index];
		config.Window_ActiveLayoutName = target.Name;
		LoadDockState(string.IsNullOrEmpty(target.AvalonDockState) ? _defaultDockState : target.AvalonDockState);
	}

	private void SaveCurrentStateToActiveLayout()
	{
		var config = _editor.Configuration;
		if (string.IsNullOrEmpty(config.Window_ActiveLayoutName))
			return;

		var active = config.Window_CustomLayouts.FirstOrDefault(l => l.Name == config.Window_ActiveLayoutName);
		if (active is null)
			return;

		active.AvalonDockState = SerializeDockState();
	}

	private void LayoutsMenu_SubmenuOpened(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuItem menuItem)
			return;

		menuItem.Items.Clear();

		var config = _editor.Configuration;

		var defaultItem = new MenuItem
		{
			Header = _localizationService["~TombEditor.MainWindow.Layouts_Default"],
			IsChecked = string.IsNullOrEmpty(config.Window_ActiveLayoutName)
		};
		defaultItem.Click += (_, _) => _editor.SwitchLayout(-1);
		menuItem.Items.Add(defaultItem);

		if (config.Window_CustomLayouts.Count > 0)
		{
			menuItem.Items.Add(new Separator());

			for (int i = 0; i < config.Window_CustomLayouts.Count; i++)
			{
				var layout = config.Window_CustomLayouts[i];
				var item = new MenuItem
				{
					Header = layout.Name,
					IsChecked = layout.Name == config.Window_ActiveLayoutName
				};

				if (i < Configuration.MaxWindowLayouts)
				{
					var hotkeyName = "SwitchLayout" + (i + 1);
					if (config.UI_Hotkeys.Any(h => h.Key == hotkeyName))
					{
						item.InputGestureText = string.Join(", ",
							config.UI_Hotkeys[hotkeyName]
								.Select(h => h.ToString())
								.Where(s => !string.IsNullOrWhiteSpace(s)));
					}
				}

				int layoutIndex = i;
				item.Click += (_, _) => _editor.SwitchLayout(layoutIndex);
				menuItem.Items.Add(item);
			}
		}

		menuItem.Items.Add(new Separator());

		var saveAsItem = new MenuItem { Header = _localizationService["~TombEditor.MainWindow.Layouts_SaveAs"] };
		saveAsItem.Click += (_, _) => Layout_SaveAs();
		menuItem.Items.Add(saveAsItem);

		var deleteItem = new MenuItem
		{
			Header = _localizationService["~TombEditor.MainWindow.Layouts_Delete"],
			IsEnabled = !string.IsNullOrEmpty(config.Window_ActiveLayoutName)
		};
		deleteItem.Click += (_, _) => Layout_Delete();
		menuItem.Items.Add(deleteItem);
	}

	private void Layout_SaveAs()
	{
		var config = _editor.Configuration;

		var vm = new InputBoxWindowViewModel(
			title: _localizationService["~TombEditor.MainWindow.Layouts_SaveAsTitle"],
			label: _localizationService["~TombEditor.MainWindow.Layouts_SaveAsLabel"],
			invalidNames: config.Window_CustomLayouts.Select(l => l.Name).ToArray());

		// IDialogService.ShowDialog requires the owner ViewModel's DataContext to be
		// registered against a Window, which MainWindow can't satisfy without
		// breaking child bindings (it can't be its own DataContext). Construct the
		// dialog manually and mirror the auto-close behaviour MvvmDialogs gives us
		// by watching DialogResult on the VM.
		var win = new InputBoxWindow
		{
			DataContext = vm,
			Owner = this,
			WindowStartupLocation = WindowStartupLocation.CenterOwner
		};
		void onVmChanged(object _, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == nameof(vm.DialogResult) && vm.DialogResult is not null)
				win.Close();
		}
		vm.PropertyChanged += onVmChanged;
		try { win.ShowDialog(); }
		finally { vm.PropertyChanged -= onVmChanged; }

		if (vm.DialogResult is not true)
			return;

		var name = vm.Value.Trim();
		if (string.IsNullOrEmpty(name))
			return;

		// Belt-and-braces — InputBoxWindowViewModel already rejects exact matches,
		// but it's case-sensitive, so dedupe again here to stop "Foo" vs "foo".
		if (config.Window_CustomLayouts.Any(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
		{
			_messageService.ShowError(_localizationService["~TombEditor.MainWindow.Layouts_NameAlreadyExists"]);
			return;
		}

		config.Window_CustomLayouts.Add(new NamedLayout
		{
			Name = name,
			AvalonDockState = SerializeDockState()
		});
		config.Window_ActiveLayoutName = name;
	}

	private void Layout_Delete()
	{
		var config = _editor.Configuration;
		var layout = config.Window_CustomLayouts.FirstOrDefault(l => l.Name == config.Window_ActiveLayoutName);
		if (layout is null)
			return;

		config.Window_CustomLayouts.Remove(layout);
		_editor.SwitchLayout(-1);
	}

	#endregion
}
