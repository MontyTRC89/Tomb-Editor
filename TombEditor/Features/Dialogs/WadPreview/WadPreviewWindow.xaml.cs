using System;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using TombLib.Controls;
using TombLib.Rendering;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace TombEditor.Features.Dialogs.WadPreview;

/// <summary>
/// Borderless pop-up that previews a <see cref="Wad2"/>: a WAD tree on the left
/// for selection, a 3D item preview on the right. Mirrors the WinForms
/// <c>FormPreviewWad</c>: both inner controls remain WinForms (the 3D pipeline
/// is hosted via <c>WindowsFormsHost</c> until the Vulkan/OpenGL refactor).
/// </summary>
public partial class WadPreviewWindow : Window
{
	private readonly Wad2 _wad;
	private readonly WadTreeView _wadTree;
	private readonly PanelRenderingItemPreview _panelItem;

	public WadPreviewWindow(Wad2 wad, RenderingDevice device, Editor editor)
	{
		InitializeComponent();
		_wad = wad;

		_panelItem = new PanelRenderingItemPreview
		{
			Editor = editor,
			BorderStyle = BorderStyle.FixedSingle,
			AnimatePreview = editor.Configuration.RenderingItem_Animate,
			Dock = DockStyle.Fill
		};
		_panelItem.InitializeRendering(device, editor.Configuration.RenderingItem_Antialias);

		_wadTree = new WadTreeView
		{
			Dock = DockStyle.Fill,
			Wad = wad,
			MultiSelect = false
		};
		_wadTree.SelectedWadObjectIdsChanged += OnTreeSelectionChanged;
		_wadTree.SelectFirst();

		wadTreeHost.Child = _wadTree;
		panelItemHost.Child = _panelItem;

		// Close when focus leaves the window AND the cursor is outside its bounds —
		// preserves the "hover preview" feel of the WinForms version.
		Deactivated += OnDeactivated;
	}

	private void OnTreeSelectionChanged(object sender, EventArgs e)
	{
		var selectedObjectId = _wadTree.SelectedWadObjectIds.FirstOrDefault();
		var selectedObject = selectedObjectId == null ? null : _wad.TryGet(selectedObjectId);

		if (selectedObject is WadMoveable moveable)
		{
			var skinId = new WadMoveableId(
				TrCatalog.GetMoveableSkin(_panelItem.Editor.Level.Settings.GameVersion, moveable.Id.TypeId));
			var skin = _panelItem.Editor.Level.Settings.WadTryGetMoveable(skinId);

			_panelItem.CurrentObject = (skin != null && skin != moveable)
				? moveable.ReplaceDummyMeshes(skin)
				: moveable;
		}
		else
			_panelItem.CurrentObject = selectedObject;

		_panelItem.ResetCamera();
	}

	private void OnDeactivated(object sender, EventArgs e)
	{
		var cursor = System.Windows.Forms.Cursor.Position;
		var bounds = new System.Drawing.Rectangle((int)Left, (int)Top, (int)Width, (int)Height);
		if (!bounds.Contains(cursor))
			Close();
	}

	/// <summary>Convenience helper for WinForms callers — sets the WPF owner from an HWND.</summary>
	public void SetOwnerFromHwnd(IntPtr hwnd)
	{
		if (hwnd == IntPtr.Zero)
			return;

		new WindowInteropHelper(this).Owner = hwnd;
	}

	public sealed class PanelRenderingItemPreview : PanelItemPreview
	{
		public Editor Editor { get; set; }

		protected override Vector4 ClearColor => Editor.Configuration.UI_ColorScheme.Color3DBackground;
		public override float FieldOfView => Editor.Configuration.RenderingItem_FieldOfView;
		public override float NavigationSpeedMouseWheelZoom => Editor.Configuration.RenderingItem_NavigationSpeedMouseWheelZoom;
		public override float NavigationSpeedMouseZoom => Editor.Configuration.RenderingItem_NavigationSpeedMouseZoom;
		public override float NavigationSpeedMouseTranslate => Editor.Configuration.RenderingItem_NavigationSpeedMouseTranslate;
		public override float NavigationSpeedMouseRotate => Editor.Configuration.RenderingItem_NavigationSpeedMouseRotate;
		public override bool ReadOnly => true;
	}
}
