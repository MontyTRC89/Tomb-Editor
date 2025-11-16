using DarkUI.Forms;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster.Services.LevelCompile;

public sealed class LevelCompileService : ILevelCompileService
{
	public void RebuildAllLevels(IGameProject project, IWin32Window owner)
	{
		LevelProject[] levels = project.GetAllValidLevelProjects();

		if (levels.Length == 0)
		{
			DarkMessageBox.Show(owner,
				"There are no levels in the current project.",
				"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

			return;
		}

		var batchList = new BatchCompileList();

		foreach (ILevelProject level in levels)
		{
			string prj2Path;

			if (level.TargetPrj2FileName is null)
				prj2Path = Path.Combine(level.DirectoryPath, level.GetMostRecentlyModifiedPrj2FileName());
			else
				prj2Path = Path.Combine(level.DirectoryPath, level.TargetPrj2FileName);

			batchList.Files.Add(prj2Path);
		}

		string batchListFilePath = Path.Combine(Path.GetTempPath(), "tide_batch.xml");
		BatchCompileList.SaveToXml(batchListFilePath, batchList);

		var startInfo = new ProcessStartInfo
		{
			FileName = Path.Combine(DefaultPaths.ProgramDirectory, "TombEditor.exe"),
			Arguments = "\"" + batchListFilePath + "\"",
			WorkingDirectory = DefaultPaths.ProgramDirectory,
			UseShellExecute = true
		};

		Process.Start(startInfo);
	}

	public void RebuildLevel(ILevelProject level)
	{
		var batchList = new BatchCompileList();

		string prj2Path;

		if (level.TargetPrj2FileName is null)
			prj2Path = Path.Combine(level.DirectoryPath, level.GetMostRecentlyModifiedPrj2FileName());
		else
			prj2Path = Path.Combine(level.DirectoryPath, level.TargetPrj2FileName);

		batchList.Files.Add(prj2Path);

		string batchListFilePath = Path.Combine(Path.GetTempPath(), "tide_batch.xml");
		BatchCompileList.SaveToXml(batchListFilePath, batchList);

		var startInfo = new ProcessStartInfo
		{
			FileName = Path.Combine(DefaultPaths.ProgramDirectory, "TombEditor.exe"),
			Arguments = "\"" + batchListFilePath + "\"",
			WorkingDirectory = DefaultPaths.ProgramDirectory,
			UseShellExecute = true
		};

		Process.Start(startInfo);
	}
}
