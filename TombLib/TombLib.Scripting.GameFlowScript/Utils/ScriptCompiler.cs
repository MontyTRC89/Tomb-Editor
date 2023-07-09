using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TombLib.Scripting.GameFlowScript.Utils
{
	public static class ScriptCompiler
	{
		public static bool ClassicCompile(string inputDirectory, string outputDirectory, bool isTR3, bool pause = true)
		{
			string gameflowDirectory = DefaultPaths.GameFlow2Directory;
			CopyFilesToGFScriptDirectory(inputDirectory, gameflowDirectory);

			string batchFilePath = Path.Combine(gameflowDirectory, "compile.bat");
			string batchFileContent = $"gameflow -Game " + (isTR3 ? 3 : 2) + "\n" + (pause ? "@pause" : string.Empty);

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

			string compiledScriptFilePath = Path.Combine(gameflowDirectory, "tombpc.dat");
			bool success = false;

			if (File.Exists(compiledScriptFilePath))
			{
				File.Copy(compiledScriptFilePath, Path.Combine(outputDirectory, "tombpc.dat"), true);
				success = true;
			}

			var gfScriptDirectory = new DirectoryInfo(gameflowDirectory);

			foreach (FileSystemInfo fileSystemInfo in gfScriptDirectory.EnumerateFileSystemInfos()
				.Where(x => !x.Name.Equals("gameFlow.exe", StringComparison.OrdinalIgnoreCase)))
			{
				if (fileSystemInfo is DirectoryInfo dir)
					dir.Delete(true);
				else
					fileSystemInfo.Delete();
			}

			return success;
		}

		public static bool CompileTR3Version2Plus(string inputDirectory, string outputDirectory, bool pause = true)
		{
			string gameflowDirectory = DefaultPaths.GameFlow3Directory;
			CopyFilesToGFScriptDirectory(inputDirectory, gameflowDirectory);

			string batchFilePath = Path.Combine(gameflowDirectory, "compile.bat");
			string batchFileContent = $"TRGameFlow Script.txt\n" + (pause ? "@pause" : string.Empty);

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

			string compiledScriptFilePath = Path.Combine(gameflowDirectory, "Script.dat");
			bool success = false;

			if (File.Exists(compiledScriptFilePath))
			{
				File.Copy(compiledScriptFilePath, Path.Combine(outputDirectory, "tombpc.dat"), true);
				success = true;
			}

			var gfScriptDirectory = new DirectoryInfo(gameflowDirectory);

			foreach (FileSystemInfo fileSystemInfo in gfScriptDirectory.EnumerateFileSystemInfos()
				.Where(x => !x.Name.Equals("TRGameFlow.exe", StringComparison.OrdinalIgnoreCase)))
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
