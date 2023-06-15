using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TombLib.Scripting.GameFlowScript.Utils
{
	public static class ScriptCompiler
	{
		public static bool Compile(string projectScriptPath, string projectEnginePath, bool isTR3, bool showLogs)
		{
			string gameflowExecutablePath = isTR3 ? DefaultPaths.GameFlow3Executable : DefaultPaths.GameFlow2Executable;
			string gameflowDirectory = isTR3 ? DefaultPaths.GameFlow3Directory : DefaultPaths.GameFlow2Directory;

			CopyFilesToGFScriptDirectory(projectScriptPath, gameflowDirectory);

			string batchFilePath = Path.Combine(gameflowDirectory, "compile.bat");

			string batchFileContent = isTR3
				? $"TRGameFlow Script.txt\n" +
					(showLogs ? "@pause" : string.Empty)
				: $"gameflow -Game 2\n" +
					(showLogs ? "@pause" : string.Empty);

			File.WriteAllText(batchFilePath, batchFileContent);

			var startInfo = new ProcessStartInfo
			{
				FileName = batchFilePath,
				WorkingDirectory = gameflowDirectory,
				UseShellExecute = true
			};

			var process = Process.Start(startInfo);

			process.WaitForExit();
			process.Close();

			string compiledScriptFilePath = isTR3
				? Path.Combine(gameflowDirectory, "Script.dat")
				: Path.Combine(gameflowDirectory, "tombpc.dat");

			bool success = false;

			if (File.Exists(compiledScriptFilePath))
			{
				File.Copy(compiledScriptFilePath, Path.Combine(projectEnginePath, "data", "tombpc.dat"), true);
				success = true;
			}

			var gfScriptDirectory = new DirectoryInfo(gameflowDirectory);

			foreach (FileSystemInfo fileSystemInfo in gfScriptDirectory.EnumerateFileSystemInfos().Where(x
				=> !x.Name.Equals("gameFlow.exe", StringComparison.OrdinalIgnoreCase)
				&& !x.Name.Equals("TRGameFlow.exe", StringComparison.OrdinalIgnoreCase)))
			{
				if (fileSystemInfo is DirectoryInfo dir)
					dir.Delete(true);
				else
					fileSystemInfo.Delete();
			}

			return success;
		}

		private static void CopyFilesToGFScriptDirectory(string projectScriptPath, string gfScriptPath)
		{
			foreach (string dirPath in Directory.GetDirectories(projectScriptPath, "*", SearchOption.AllDirectories))
				Directory.CreateDirectory(dirPath.Replace(projectScriptPath, gfScriptPath));

			foreach (string newPath in Directory.GetFiles(projectScriptPath, "*.*", SearchOption.AllDirectories)
				.Where(x => !Path.GetExtension(x).Equals(".backup", StringComparison.OrdinalIgnoreCase)))
				File.Copy(newPath, newPath.Replace(projectScriptPath, gfScriptPath), true);
		}
	}
}
