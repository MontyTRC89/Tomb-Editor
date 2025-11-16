using System.Collections.Generic;
using System.IO;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

public sealed class TR1XArchiveService : GameArchiveServiceBase
{
	public override bool SupportsGameVersion(IGameProject project)
		=> project.GameVersion is TRVersion.Game.TR1 or TRVersion.Game.TR1X; // TR1X was previously known as TR1

	protected override IReadOnlyList<string> GetImportantFolders(string engineDirectory) => new List<string>
	{
		Path.Combine(engineDirectory, "music"),
		Path.Combine(engineDirectory, "cfg"),
		Path.Combine(engineDirectory, "data"),
		Path.Combine(engineDirectory, "shaders")
	};

	protected override IReadOnlyList<string> GetImportantFiles(string engineDirectory) => new List<string>(GetCommonFiles(engineDirectory))
	{
		Path.Combine(engineDirectory, "TR1X.exe"),
		Path.Combine(engineDirectory, "Tomb1Main.exe"),
		Path.Combine(engineDirectory, "TR1X_ConfigTool.exe"),
		Path.Combine(engineDirectory, "Tomb1Main_ConfigTool.exe")
	};
}
