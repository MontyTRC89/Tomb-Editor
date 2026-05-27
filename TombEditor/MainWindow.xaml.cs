using System.Windows;
using TombEditor.Controls;
using TombEditor.Controls.Panel3D;
using TombEditor.Features.Panel3D.ObjectBrush;
using TombEditor.Features.Panel3D.ToolPalette;
using TombLib.Controls;
using TombLib.LevelData;

namespace TombEditor;

public partial class MainWindow : Window
{
	private readonly Editor _editor;
	private readonly Panel3D _panel3D;
	private readonly Panel2DMap _panel2DMap;
	private readonly ToolPaletteFloating _toolPalette;
	private readonly ObjectBrushToolbox _objectBrushToolbox;

	public MainWindow(Editor editor)
	{
		_editor = editor;
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
	}

	private void OnEditorEventRaised(IEditorEvent obj)
	{
		OnEditorEventForFloatingToolboxes(obj);

		// Toolbar toggle commands flip a flag on Editor.Configuration and then raise
		// ConfigurationChangedEvent (see CommandHandler entries like "DrawAllRooms"). Mirror
		// MainView.RefreshControls(...) so the hosted Panel3D actually picks up the change.
		if (obj is Editor.ConfigurationChangedEvent || obj is Editor.InitEvent)
			ApplyConfigurationToPanel3D(_editor.Configuration);

		// Bring the matching document tab forward when the editor mode switches between
		// 3D-style modes and Map2D. Avoids requiring the user to also click the tab header.
		if (obj is Editor.ModeChangedEvent)
		{
			if (_editor.Mode == EditorMode.Map2D)
				map2DDocument.IsActive = true;
			else
				mainViewDocument.IsActive = true;
		}
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

		_editor.EditorEventRaised -= OnEditorEventRaised;

		// Each migrated WPF view subscribes to Editor.EditorEventRaised in its constructor;
		// Cleanup() unsubscribes and releases ViewModels. Without this the Editor leaks event
		// handlers across MainWindow lifetimes (and ViewModels go on receiving events).
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

		base.OnClosed(e);
	}
}
