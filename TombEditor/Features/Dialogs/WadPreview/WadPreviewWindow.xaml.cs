#nullable enable

using System;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using TombLib.Controls;
using TombLib.Rendering;
using TombLib.Wad;

namespace TombEditor.Features.Dialogs.WadPreview;

/// <summary>
/// Borderless pop-up that previews a <see cref="Wad2"/>: a WAD tree on the left
/// for selection, a 3D item preview on the right. Mirrors the WinForms
/// <c>FormPreviewWad</c>: both inner controls remain WinForms (the 3D pipeline
/// is hosted via <c>WindowsFormsHost</c> until the Vulkan/OpenGL refactor).
/// </summary>
public partial class WadPreviewWindow : Window
{
	private readonly WadTreeView _wadTree;
	private readonly PanelRenderingItemPreview _panelItem;

	public WadPreviewWindow(RenderingDevice device, Editor editor)
	{
		InitializeComponent();

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
			MultiSelect = false
		};
		_wadTree.SelectedWadObjectIdsChanged += OnTreeSelectionChanged;

		wadTreeHost.Child = _wadTree;
		panelItemHost.Child = _panelItem;

		DataContextChanged += OnDataContextChanged;

		// Close when focus leaves the window AND the cursor is outside its bounds —
		// preserves the "hover preview" feel of the WinForms version.
		Deactivated += OnDeactivated;
	}

	private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue is WadPreviewWindowViewModel vm)
		{
			_wadTree.Wad = vm.Wad;
			_wadTree.SelectFirst();
		}
	}

	private void OnTreeSelectionChanged(object? sender, EventArgs e)
	{
		if (DataContext is not WadPreviewWindowViewModel vm)
			return;

		vm.SelectObject(_wadTree.SelectedWadObjectIds.FirstOrDefault());

		_panelItem.CurrentObject = vm.ObjectToRender;
		_panelItem.ResetCamera();
	}

	private void OnDeactivated(object? sender, EventArgs e)
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
}
