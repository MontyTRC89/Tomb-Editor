using System.Collections.Generic;
using System.IO;
using TombIDE.Shared.NewStructure;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.ArchiveCreation;

public sealed class TR2XArchiveService : GameArchiveServiceBase
{
	public override bool SupportsGameVersion(IGameProject project)
		=> project.GameVersion == TRVersion.Game.TR2X;

	protected override IReadOnlyList<string> GetImportantFolders(string engineDirectory) => new List<string>
	{
		Path.Combine(engineDirectory, "music"),
		Path.Combine(engineDirectory, "cfg"),
		Path.Combine(engineDirectory, "data"),
		Path.Combine(engineDirectory, "shaders")
	};

	protected override IReadOnlyList<string> GetImportantFiles(string engineDirectory) => new List<string>(GetCommonFiles(engineDirectory))
	{
		Path.Combine(engineDirectory, "TR2X.exe"),
		Path.Combine(engineDirectory, "TR2X_ConfigTool.exe")
	};
}
