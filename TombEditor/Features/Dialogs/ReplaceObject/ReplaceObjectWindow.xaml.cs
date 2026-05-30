using System.Windows;
using System.Windows.Input;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ReplaceObject;

public partial class ReplaceObjectWindow : Window
{
	public ReplaceObjectWindow()
	{
		InitializeComponent();
		Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormReplaceObject");
		Closed += (_, _) =>
		{
			if (DataContext is ReplaceObjectWindowViewModel vm)
				vm.Dispose();
		};
	}

	private ReplaceObjectWindowViewModel? Vm => DataContext as ReplaceObjectWindowViewModel;

	private void OnDragEnter(object sender, DragEventArgs e)
	{
		e.Effects = TryGetWadObject(e) is not null ? DragDropEffects.Copy : DragDropEffects.None;
		e.Handled = true;
	}

	private void OnDrop(object sender, DragEventArgs e)
	{
		var item = TryGetWadObject(e);
		if (item is null || Vm is null)
			return;

		PositionBasedObjectInstance? instance = item switch
		{
			WadStatic ws => new StaticInstance { WadObjectId = ws.Id },
			WadMoveable wm => new MoveableInstance { WadObjectId = wm.Id },
			ImportedGeometry ig => new ImportedGeometryInstance { Model = ig },
			_ => null
		};

		if (instance is null)
			return;

		var dropPos = e.GetPosition(this);
		var sourceTop = tbSource.TransformToAncestor(this).Transform(new Point(0, 0)).Y;
		var destTop = tbDest.TransformToAncestor(this).Transform(new Point(0, 0)).Y;
		var midpoint = (sourceTop + destTop) / 2;

		Vm.ToggleItem(instance, dropPos.Y >= midpoint
			? ReplaceObjectWindowViewModel.ObjectSelectionType.Destination
			: ReplaceObjectWindowViewModel.ObjectSelectionType.Source);

		e.Handled = true;
	}

	private static IWadObject? TryGetWadObject(DragEventArgs e)
	{
		var formats = e.Data.GetFormats();
		if (formats.Length == 0)
			return null;
		return e.Data.GetData(formats[0]) as IWadObject;
	}

	private void OnSourceColorClick(object sender, MouseButtonEventArgs e) => EditLightColor(asSource: true);
	private void OnDestColorClick(object sender, MouseButtonEventArgs e) => EditLightColor(asSource: false);

	private void EditLightColor(bool asSource)
	{
		if (Vm is null)
			return;

		var light = (asSource ? Vm.SourceObject : Vm.DestObject) as LightInstance;
		if (light is null)
			return;

		var editor = Editor.Instance;
		var owner = new WindowOwner(new System.Windows.Interop.WindowInteropHelper(this).Handle);
		using (var colorDialog = new TombLib.Controls.RealtimeColorDialog(editor.Configuration.UI_ColorScheme))
		{
			colorDialog.Color = new System.Numerics.Vector4(light.Color * 0.5f, 1.0f).ToWinFormsColor();
			colorDialog.FullOpen = true;
			if (colorDialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK)
				return;

			light.Color = colorDialog.Color.ToFloat3Color() * 2.0f;
			Vm.RefreshUI();
		}
	}

	private sealed class WindowOwner : System.Windows.Forms.IWin32Window
	{
		public WindowOwner(System.IntPtr h) { Handle = h; }
		public System.IntPtr Handle { get; }
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
