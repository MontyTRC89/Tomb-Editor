using System.Diagnostics;
using System.IO;
using TombLib.Scripting.IO;

namespace TombLib.Scripting.GameFlowScript.Compilers;

/// <summary>
/// Compiles GameFlow scripts by driving the external GameFlow compiler.
/// </summary>
public static class ScriptCompiler
{
	/// <summary>
	/// Compiles a classic GameFlow script and copies the resulting data file to the output directory.
	/// </summary>
	/// <param name="inputDirectory">The directory that contains the script files.</param>
	/// <param name="outputDirectory">The directory that receives the compiled data file.</param>
	/// <param name="isTR3">Whether the script targets Tomb Raider 3.</param>
	/// <param name="pause">Whether the compiler batch should pause when it finishes.</param>
	/// <returns><c>true</c> if the compiled data file was produced and copied; otherwise, <c>false</c>.</returns>
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

	/// <summary>
	/// Compiles a TR3 version 2+ GameFlow script and copies the resulting data file to the output directory.
	/// </summary>
	/// <param name="inputDirectory">The directory that contains the script files.</param>
	/// <param name="outputDirectory">The directory that receives the compiled data file.</param>
	/// <param name="pause">Whether the compiler batch should pause when it finishes.</param>
	/// <returns><c>true</c> if the compiled data file was produced and copied; otherwise, <c>false</c>.</returns>
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
