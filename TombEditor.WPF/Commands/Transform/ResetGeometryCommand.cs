using NLog;
using System.ComponentModel;

namespace TombEditor.WPF.Commands;

/// <summary>
/// Flattens both floor and ceiling.
/// </summary>
internal sealed class ResetGeometryCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : SmartBuildGeometryCommand(caller, editor, logger)
{
	public override void Execute(object? parameter)
	{
		if (Editor.SelectedRoom != null && Editor.SelectedSectors.ValidOrNone)
		{
			EditorActions.FlattenRoomArea(Editor.SelectedRoom, Editor.SelectedSectors.Valid ? Editor.SelectedSectors.Area : Editor.SelectedRoom.LocalArea.Inflate(-1), null, false, true, false);
			EditorActions.FlattenRoomArea(Editor.SelectedRoom, Editor.SelectedSectors.Valid ? Editor.SelectedSectors.Area : Editor.SelectedRoom.LocalArea.Inflate(-1), null, true, true, true, true);
		}
	}
}
