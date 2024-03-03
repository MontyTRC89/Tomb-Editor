using NLog;
using System.ComponentModel;
using System.Numerics;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class GenericDirectionalControlCommand : UnconditionalEditorCommand
{
	private readonly BlockVertical _surface;
	private readonly int _increment;
	private readonly bool _smooth;
	private readonly bool _oppositeDiagonal;

	public GenericDirectionalControlCommand(INotifyPropertyChanged caller, Editor editor,
		BlockVertical surface, int increment, bool smooth, bool oppositeDiagonal, Logger? logger = null)
		: base(caller, editor, logger)
	{
		_surface = surface;
		_increment = increment;
		_smooth = smooth;
		_oppositeDiagonal = oppositeDiagonal;
	}

	public override void Execute(object? parameter)
	{
		if (Editor.LastSelection == LastSelectionType.Block && Editor.Mode == EditorMode.Geometry && Editor.SelectedSectors.Valid)
		{
			Editor.EditSectorGeometry(Editor.SelectedRoom, Editor.SelectedSectors.Area, Editor.SelectedSectors.Arrow, _surface, _increment, _smooth, _oppositeDiagonal);
		}
		else if (Editor.LastSelection == LastSelectionType.SpatialObject && (_surface == BlockVertical.Floor || _surface == BlockVertical.Ceiling) && !_oppositeDiagonal && !_smooth)
		{
			if (Editor.SelectedObject is PositionBasedObjectInstance @object && _surface == BlockVertical.Floor)
			{
				EditorActions.MoveObjectRelative(@object, new Vector3(0, _increment, 0), new Vector3(), true);
			}
			else if (Editor.SelectedObject is GhostBlockInstance ghostBlock)
			{
				ghostBlock.Move(_increment, _surface == BlockVertical.Floor);
				Editor.RoomSectorPropertiesChange(Editor.SelectedRoom);
			}
		}
	}
}
