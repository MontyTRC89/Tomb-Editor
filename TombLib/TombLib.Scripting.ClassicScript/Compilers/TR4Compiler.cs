using System.Diagnostics;
using System.IO;
using TombLib.Scripting.IO;

namespace TombLib.Scripting.ClassicScript.Compilers;

/// <summary>
/// Compiles ClassicScript using the external TR4 compiler.
/// </summary>
public static class TR4Compiler
{
	/// <summary>
	/// Compiles the script at the given path with the TR4 compiler.
	/// </summary>
	/// <param name="projectScriptPath">The path of the project script directory.</param>
	/// <param name="projectEnginePath">The path of the project engine directory.</param>
	/// <returns>The compiler log content.</returns>
	public static string Compile(string projectScriptPath, string projectEnginePath)
	{
		if (DefaultPaths.TR4ScriptCompilerDirectory.Contains('\''))
			throw new Exception("The path to the TR4 script compiler contains an invalid character.\n' is not a valid path character for this operation.");

		ScriptDirectoryCopier.CopyScriptDirectory(projectScriptPath, DefaultPaths.TR4ScriptCompilerDirectory, clearTarget: false, CompilerFileCopy.CopyTextFormatted);

		var startInfo = new ProcessStartInfo
		{
			FileName = DefaultPaths.DOSBoxExecutable,
			WorkingDirectory = DefaultPaths.DOSDirectory,
			Arguments =
				$"-c \"mount C '{DefaultPaths.TR4ScriptCompilerDirectory}'\" " +
				"-c \"C:\" " +
				"-c \"script script.txt >> logs.txt\" " +
				"-c \"exit\" " +
				"-noconsole",
			UseShellExecute = true
		};

		Process.Start(startInfo)?.WaitForExit();

		string logFilePath = Path.Combine(DefaultPaths.TR4ScriptCompilerDirectory, "logs.txt");
		string logFileContent = File.ReadAllText(logFilePath);

		string compiledScriptFilePath = Path.Combine(DefaultPaths.TR4ScriptCompilerDirectory, "Script.dat");
		string compiledEnglishFilePath = Path.Combine(DefaultPaths.TR4ScriptCompilerDirectory, "English.dat");

		if (File.Exists(compiledScriptFilePath))
			File.Copy(compiledScriptFilePath, Path.Combine(projectEnginePath, "Script.dat"), true);

		if (File.Exists(compiledEnglishFilePath))
			File.Copy(compiledEnglishFilePath, Path.Combine(projectEnginePath, "English.dat"), true);

		ScriptDirectoryCopier.ClearDirectoryExcept(
			DefaultPaths.TR4ScriptCompilerDirectory,
			name => name.Equals("SCRIPT.EXE", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("DOS4GW.EXE", StringComparison.OrdinalIgnoreCase));

		return logFileContent;
	}
}
