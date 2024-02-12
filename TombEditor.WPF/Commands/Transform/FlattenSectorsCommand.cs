using NLog;
using System.ComponentModel;

namespace TombEditor.WPF.Commands;

/// <summary>
/// Flattens the selected sectors.
/// </summary>
internal sealed class FlattenSectorsCommand(INotifyPropertyChanged caller, Editor editor, bool ceiling, Logger? logger = null) : SmartBuildGeometryCommand(caller, editor, logger)
{
	public override void Execute(object? parameter)
	{
		if (Editor.SelectedRoom != null && Editor.SelectedSectors.ValidOrNone)
			EditorActions.FlattenRoomArea(Editor.SelectedRoom, Editor.SelectedSectors.Valid ? Editor.SelectedSectors.Area : Editor.SelectedRoom.LocalArea.Inflate(-1), null, ceiling, false, true);
	}
}
