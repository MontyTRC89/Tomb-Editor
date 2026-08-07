using System.Diagnostics;
using System.IO;
using TombLib.Scripting.IO;

namespace TombLib.Scripting.GameFlowScript.Compilers;

public static class ScriptCompiler
{
	public static bool ClassicCompile(string inputDirectory, string outputDirectory, bool isTR3, bool pause = true)
	{
		string gameflowDirectory = DefaultPaths.GameFlow2Directory;
		ScriptDirectoryCopier.CopyScriptDirectory(inputDirectory, gameflowDirectory, clearTarget: false);

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

		process?.WaitForExit();
		process?.Close();

		string compiledScriptFilePath = Path.Combine(gameflowDirectory, "tombpc.dat");
		bool success = false;

		if (File.Exists(compiledScriptFilePath))
		{
			File.Copy(compiledScriptFilePath, Path.Combine(outputDirectory, "tombpc.dat"), true);
			success = true;
		}

		ScriptDirectoryCopier.ClearDirectoryExcept(gameflowDirectory, name => name.Equals("gameFlow.exe", StringComparison.OrdinalIgnoreCase));

		return success;
	}

	public static bool CompileTR3Version2Plus(string inputDirectory, string outputDirectory, bool pause = true)
	{
		string gameflowDirectory = DefaultPaths.GameFlow3Directory;
		ScriptDirectoryCopier.CopyScriptDirectory(inputDirectory, gameflowDirectory, clearTarget: false);

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

		process?.WaitForExit();
		process?.Close();

		string compiledScriptFilePath = Path.Combine(gameflowDirectory, "Script.dat");
		bool success = false;

		if (File.Exists(compiledScriptFilePath))
		{
			File.Copy(compiledScriptFilePath, Path.Combine(outputDirectory, "tombpc.dat"), true);
			success = true;
		}

		ScriptDirectoryCopier.ClearDirectoryExcept(gameflowDirectory, name => name.Equals("TRGameFlow.exe", StringComparison.OrdinalIgnoreCase));

		return success;
	}
}
