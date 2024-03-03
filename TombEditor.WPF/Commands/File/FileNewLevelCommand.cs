using NLog;
using System.ComponentModel;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class FileNewLevelCommand : UnconditionalEditorCommand
{
	public FileNewLevelCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller, editor, logger)
	{ }

	public override void Execute(object? parameter)
	{
		if (!Editor.ContinueOnFileDrop(Caller, "New level"))
			return;

		Editor.Level = Level.CreateSimpleLevel(Editor.Configuration.Editor_DefaultProjectGameVersion);

		// Make border wall grids, as in dxtre3d
		if (Editor.Configuration.Editor_GridNewRoom)
		{
			new GridWallsCommand(Caller, Editor, Editor.Level.Rooms[0], Editor.Level.Rooms[0].LocalArea, false, true, false, Logger)
				.Execute(null);
		}
	}
}
