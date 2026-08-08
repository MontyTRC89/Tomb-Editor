using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using TombLib.Scripting.IO;

namespace TombLib.Scripting.ClassicScript.Compilers;

/// <summary>
/// Compiles ClassicScript using the external NG compiler.
/// </summary>
public static class NGCompiler
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Ensures the libraries required by the NG compiler are registered.
	/// </summary>
	/// <returns><c>true</c> when the required libraries are available; otherwise, <c>false</c>.</returns>
	public static bool AreLibrariesRegistered()
	{
		bool requiredFilesExist = File.Exists(ClassicScriptCompilerPaths.Default.MscomctlSystemFile)
			&& File.Exists(ClassicScriptCompilerPaths.Default.Richtx32SystemFile)
			&& File.Exists(ClassicScriptCompilerPaths.Default.PicFormat32SystemFile)
			&& File.Exists(ClassicScriptCompilerPaths.Default.Comdlg32SystemFile);

		if (!requiredFilesExist)
		{
			try
			{
				var process = new ProcessStartInfo
				{
					FileName = ClassicScriptCompilerPaths.Default.LibraryRegistrationExecutable,
					UseShellExecute = true
				};

				Process.Start(process)?.WaitForExit();
			}
			catch (Exception exception)
			{
				Log.Warn(exception, "Failed to register the required libraries.");
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Compiles the script at the given path with the NG compiler.
	/// </summary>
	/// <param name="projectScriptPath">The path of the project script directory.</param>
	/// <param name="projectEnginePath">The path of the project engine directory.</param>
	/// <param name="newIncludeMethod">Whether the new include merging method is used.</param>
	/// <returns><c>true</c> when the compilation produced a data file.</returns>
	public static bool Compile(string projectScriptPath, string projectEnginePath, bool newIncludeMethod = false)
	{
		ThrowIfLibrariesNotRegistered(AreLibrariesRegistered());

		CopyFilesToVGEScriptDirectory(projectScriptPath, ClassicScriptCompilerPaths.Default.VGEScriptDirectory);

		if (newIncludeMethod)
			MergeIncludes();

		var process = new ProcessStartInfo
		{
			FileName = ClassicScriptCompilerPaths.Default.NGCExecutable,
			Arguments = $"\"{ClassicScriptCompilerPaths.Default.VGEScriptDirectory}\\Script.txt\" -Log -NoMsgBox -NoWait -Concise",
			UseShellExecute = true
		};

		Process.Start(process)?.WaitForExit();

		FixLogs(projectEnginePath, out bool containsError);
		CopyCompiledFilesToProject(projectEnginePath);

		return !containsError;
	}

	private static void CopyFilesToVGEScriptDirectory(string projectScriptPath, string vgeScriptPath)
		=> ScriptDirectoryCopier.CopyScriptDirectory(projectScriptPath, vgeScriptPath, clearTarget: true, CompilerFileCopy.CopyTextFormatted);

	/// <summary>
	/// Throws when the libraries required by the NG compiler are not registered.
	/// </summary>
	/// <param name="librariesRegistered">Whether the required libraries are registered.</param>
	/// <exception cref="InvalidOperationException">The required libraries are not registered.</exception>
	internal static void ThrowIfLibrariesNotRegistered(bool librariesRegistered)
	{
		if (!librariesRegistered)
			throw new InvalidOperationException("The required libraries are not registered.");
	}

	private static void MergeIncludes()
	{
		string vgeScriptFilePath = Path.Combine(ClassicScriptCompilerPaths.Default.VGEScriptDirectory, "Script.txt");

		string[] lines = File.ReadAllLines(vgeScriptFilePath);

		// The visited set tracks the include path currently being expanded so recursive
		// includes cannot loop. It is scoped to this merge call rather than stored statically.
		var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { vgeScriptFilePath };
		lines = ReplaceIncludesWithFileContents(lines, visitedFiles, ClassicScriptCompilerPaths.Default.VGEScriptDirectory);

		string newFileContent = string.Join(Environment.NewLine, lines);
		File.WriteAllText(vgeScriptFilePath, newFileContent, Encoding.GetEncoding(1252));
	}

	/// <summary>
	/// Recursively expands <c>#include</c> directives in the given lines, resolving relative
	/// include paths against <paramref name="includeBaseDirectory"/> and guarding against include
	/// cycles through the visited set.
	/// </summary>
	/// <returns>The lines with all include directives replaced by their file contents.</returns>
	internal static string[] ReplaceIncludesWithFileContents(string[] lines, HashSet<string> visitedFiles, string includeBaseDirectory)
	{
		var newLines = new List<string>();

		foreach (string line in lines)
		{
			if (line.TrimStart().StartsWith("#include", StringComparison.OrdinalIgnoreCase))
				try
				{
					string partialIncludePath = line.Split('"')[1].Trim();
					string includedFilePath = Path.Combine(includeBaseDirectory, partialIncludePath);

					if (File.Exists(includedFilePath) && visitedFiles.Add(includedFilePath))
					{
						newLines.Add("; // // // // <" + partialIncludePath.ToUpper() + "> // // // //");

						string[] includeLines = File.ReadAllLines(includedFilePath);
						includeLines = ReplaceIncludesWithFileContents(includeLines, visitedFiles, includeBaseDirectory);

						newLines.AddRange(includeLines);

						newLines.Add("; // // // // </" + partialIncludePath.ToUpper() + "> // // // //");

						visitedFiles.Remove(includedFilePath);
					}

					continue;
				}
				catch (Exception exception)
				{
					Log.Warn(exception, "Failed to merge include line '{Line}'.", line);
				}

			newLines.Add(line);
		}

		return newLines.ToArray();
	}

	private static void FixLogs(string projectEnginePath, out bool containsError)
	{
		string logFilePath = Path.Combine(ClassicScriptCompilerPaths.Default.VGEDirectory, "LastCompilerLog.txt");
		string? newFileContent = FixLogFile(logFilePath, projectEnginePath, ClassicScriptCompilerPaths.Default.VGEDirectory);

		if (newFileContent is null)
		{
			containsError = false;
			return;
		}

		containsError = newFileContent.Contains("ERROR:");
	}

	internal static string? FixLogFile(string logFilePath, string projectEnginePath, string vgeDirectory)
	{
		if (!File.Exists(logFilePath))
			return null;

		// Replace the VGE paths in the log file with the current project ones
		string newFileContent = File.ReadAllText(logFilePath)
			.Replace(vgeDirectory, projectEnginePath)
			.Replace("ERROR: unknonw ", "ERROR: unknown ");

		File.WriteAllText(logFilePath, newFileContent);
		return newFileContent;
	}

	private static void CopyCompiledFilesToProject(string projectEnginePath)
	{
		// Copy the compiled files from the Virtual Game Engine folder to the current project folder
		string compiledScriptFilePath = Path.Combine(ClassicScriptCompilerPaths.Default.VGEDirectory, "Script.dat");
		string compiledEnglishFilePath = Path.Combine(ClassicScriptCompilerPaths.Default.VGEDirectory, "English.dat");

		if (File.Exists(compiledScriptFilePath))
			File.Copy(compiledScriptFilePath, Path.Combine(projectEnginePath, "Script.dat"), true);

		if (File.Exists(compiledEnglishFilePath))
			File.Copy(compiledEnglishFilePath, Path.Combine(projectEnginePath, "English.dat"), true);
	}
}
