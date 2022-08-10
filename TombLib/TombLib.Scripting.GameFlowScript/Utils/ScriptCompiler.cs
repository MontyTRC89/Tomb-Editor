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
			CopyFilesToGFScriptDirectory(projectScriptPath, DefaultPaths.GameFlowDirectory);

			string batchFilePath = Path.Combine(DefaultPaths.GameFlowDirectory, "compile.bat");

			string batchFileContent =
				"gameflow -Game " + (isTR3 ? "3" : "2") + "\n" +
				(showLogs ? "@pause" : string.Empty);

			File.WriteAllText(batchFilePath, batchFileContent);

			var startInfo = new ProcessStartInfo
			{
				FileName = batchFilePath,
				WorkingDirectory = DefaultPaths.GameFlowDirectory
			};

			var process = Process.Start(startInfo);

			process.WaitForExit();
			process.Close();

			string compiledScriptFilePath = Path.Combine(DefaultPaths.GameFlowDirectory, "tombpc.dat");

			bool success = false;

			if (File.Exists(compiledScriptFilePath))
			{
				File.Copy(compiledScriptFilePath, Path.Combine(projectEnginePath, "data", "tombpc.dat"), true);
				success = true;
			}

			var gfScriptDirectory = new DirectoryInfo(DefaultPaths.GameFlowDirectory);

			foreach (FileSystemInfo fileSystemInfo in gfScriptDirectory.EnumerateFileSystemInfos().Where(x
				=> !x.Name.Equals("gameflow.exe", StringComparison.OrdinalIgnoreCase)))
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
