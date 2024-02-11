using NLog;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class SetSectorFlagsCommand : RoomGeometryCommand
{
	private readonly BlockFlags _flags;

	public SetSectorFlagsCommand(INotifyPropertyChanged caller, Editor editor, BlockFlags flags, Logger? logger = null) : base(caller, editor, logger)
		=> _flags = flags;

	public override void Execute(object? parameter)
	{
		if (!CheckForRoomAndBlockSelection())
			return;

		// Check if flag is supported in the current engine version
		switch (_flags)
		{
			case BlockFlags.Beetle:
			case BlockFlags.Monkey:
			case BlockFlags.TriggerTriggerer:
				if (!VersionCheck(Editor.Level.Settings.GameVersion >= TRVersion.Game.TR3, "This flag"))
					return;
				break;

			case BlockFlags.ClimbNegativeX:
			case BlockFlags.ClimbNegativeZ:
			case BlockFlags.ClimbPositiveX:
			case BlockFlags.ClimbPositiveZ:
				if (!VersionCheck(Editor.Level.Settings.GameVersion >= TRVersion.Game.TR2, "Climbing"))
					return;
				break;
		}

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		ToggleBlockFlag(room, area, _flags);
	}
}
