using System.Windows;
using System.Windows.Input;
using TombEditor.ViewModels;
using TombLib.LevelData;
using TombLib.Wad;

namespace TombEditor.Views;

public partial class ReplaceObjectWindow : Window
{
	public ReplaceObjectWindow()
	{
		InitializeComponent();
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

		// Decide source vs dest based on which row the user dropped over.
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

	private void OnSourceColorClick(object sender, MouseButtonEventArgs e)
		=> EditLightColor(asSource: true);

	private void OnDestColorClick(object sender, MouseButtonEventArgs e)
		=> EditLightColor(asSource: false);

	private void EditLightColor(bool asSource)
	{
		// Color editing on the floating swatches is a niche feature; punt until the
		// WinForms RealtimeColorDialog has a WPF replacement bound to LightInstance.Color.
		// Users can still change the light color the usual way (re-select a light with the
		// desired color in the level) and use it as source/destination here.
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
