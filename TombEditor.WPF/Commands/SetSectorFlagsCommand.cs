using NLog;
using TombEditor.WPF.Extensions;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal class SetSectorFlagsCommand : UnconditionalEditorCommand
{
	private readonly BlockFlags _flags;

	public SetSectorFlagsCommand(Editor editor, BlockFlags flags, Logger? logger = null) : base(editor, logger)
		=> _flags = flags;

	public override void Execute(object? parameter)
	{
		if (!Editor.IsValidRoomAndSectorSelection)
			return;

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

		room.ToggleBlockFlag(area, _flags);
	}
}
