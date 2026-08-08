using System;
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
	/// Throws when the TR4 script compiler directory contains a character that is invalid for
	/// the DOSBox mount command.
	/// </summary>
	/// <param name="compilerDirectory">The path to the TR4 script compiler directory.</param>
	/// <exception cref="ArgumentException">The path contains an invalid path character.</exception>
	internal static void ThrowIfInvalidCompilerPath(string compilerDirectory)
	{
		if (compilerDirectory.Contains('\''))
			throw new ArgumentException(
				"The path to the TR4 script compiler contains an invalid path character. " +
				$"' is not a valid path character for this operation: '{compilerDirectory}'.",
				nameof(compilerDirectory));
	}

	/// <summary>
	/// Compiles the script at the given path with the TR4 compiler.
	/// </summary>
	/// <param name="projectScriptPath">The path of the project script directory.</param>
	/// <param name="projectEnginePath">The path of the project engine directory.</param>
	/// <returns>The compiler log content.</returns>
	public static string Compile(string projectScriptPath, string projectEnginePath)
	{
		ThrowIfInvalidCompilerPath(ClassicScriptCompilerPaths.Default.TR4ScriptCompilerDirectory);

		ScriptDirectoryCopier.CopyScriptDirectory(projectScriptPath, ClassicScriptCompilerPaths.Default.TR4ScriptCompilerDirectory, clearTarget: false, CompilerFileCopy.CopyTextFormatted);

		var startInfo = new ProcessStartInfo
		{
			FileName = ClassicScriptCompilerPaths.Default.DOSBoxExecutable,
			WorkingDirectory = ClassicScriptCompilerPaths.Default.DOSDirectory,
			Arguments =
				$"-c \"mount C '{ClassicScriptCompilerPaths.Default.TR4ScriptCompilerDirectory}'\" " +
				"-c \"C:\" " +
				"-c \"script script.txt >> logs.txt\" " +
				"-c \"exit\" " +
				"-noconsole",
			UseShellExecute = true
		};

		Process.Start(startInfo)?.WaitForExit();

		string logFilePath = Path.Combine(ClassicScriptCompilerPaths.Default.TR4ScriptCompilerDirectory, "logs.txt");
		string logFileContent = File.ReadAllText(logFilePath);

		string compiledScriptFilePath = Path.Combine(ClassicScriptCompilerPaths.Default.TR4ScriptCompilerDirectory, "Script.dat");
		string compiledEnglishFilePath = Path.Combine(ClassicScriptCompilerPaths.Default.TR4ScriptCompilerDirectory, "English.dat");

		if (File.Exists(compiledScriptFilePath))
			File.Copy(compiledScriptFilePath, Path.Combine(projectEnginePath, "Script.dat"), true);

		if (File.Exists(compiledEnglishFilePath))
			File.Copy(compiledEnglishFilePath, Path.Combine(projectEnginePath, "English.dat"), true);

		ScriptDirectoryCopier.ClearDirectoryExcept(
			ClassicScriptCompilerPaths.Default.TR4ScriptCompilerDirectory,
			name => name.Equals("SCRIPT.EXE", StringComparison.OrdinalIgnoreCase)
				|| name.Equals("DOS4GW.EXE", StringComparison.OrdinalIgnoreCase));

		return logFileContent;
	}
}
