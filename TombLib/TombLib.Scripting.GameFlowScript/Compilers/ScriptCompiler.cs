using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using TombLib.Scripting.IO;

namespace TombLib.Scripting.GameFlowScript.Compilers;

/// <summary>
/// Compiles GameFlow scripts by driving the external GameFlow compiler.
/// </summary>
public static class ScriptCompiler
{
	private const int ProcessTimeoutMilliseconds = 300000;

	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	/// <summary>
	/// Compiles a classic GameFlow script and copies the resulting data file to the output directory.
	/// </summary>
	/// <param name="inputDirectory">The directory that contains the script files.</param>
	/// <param name="outputDirectory">The directory that receives the compiled data file.</param>
	/// <param name="isTR3">Whether the script targets Tomb Raider 3.</param>
	/// <param name="pause">Whether the compiler batch should pause when it finishes.</param>
	/// <returns><c>true</c> if the compiled data file was produced and copied; otherwise, <c>false</c>.</returns>
	public static bool ClassicCompile(string inputDirectory, string outputDirectory, bool isTR3, bool pause = true)
		=> RunCompileWorkflow(
			inputDirectory,
			outputDirectory,
			DefaultPaths.GameFlow2Directory,
			BuildClassicBatchContent(isTR3, pause),
			"tombpc.dat",
			"gameFlow.exe",
			pause);

	/// <summary>
	/// Compiles a TR3 version 2+ GameFlow script and copies the resulting data file to the output directory.
	/// </summary>
	/// <param name="inputDirectory">The directory that contains the script files.</param>
	/// <param name="outputDirectory">The directory that receives the compiled data file.</param>
	/// <param name="pause">Whether the compiler batch should pause when it finishes.</param>
	/// <returns><c>true</c> if the compiled data file was produced and copied; otherwise, <c>false</c>.</returns>
	public static bool CompileTR3Version2Plus(string inputDirectory, string outputDirectory, bool pause = true)
		=> RunCompileWorkflow(
			inputDirectory,
			outputDirectory,
			DefaultPaths.GameFlow3Directory,
			BuildTR3Version2PlusBatchContent(pause),
			"Script.dat",
			"TRGameFlow.exe",
			pause);

	/// <summary>
	/// Builds the compiler batch content for a classic GameFlow compile.
	/// </summary>
	/// <param name="isTR3">Whether the script targets Tomb Raider 3.</param>
	/// <param name="pause">Whether the batch should pause when it finishes.</param>
	/// <returns>The batch file content.</returns>
	internal static string BuildClassicBatchContent(bool isTR3, bool pause)
		=> "gameflow -Game " + (isTR3 ? 3 : 2) + "\n" + (pause ? "@pause" : string.Empty);

	/// <summary>
	/// Builds the compiler batch content for a TR3 version 2+ compile.
	/// </summary>
	/// <param name="pause">Whether the batch should pause when it finishes.</param>
	/// <returns>The batch file content.</returns>
	internal static string BuildTR3Version2PlusBatchContent(bool pause)
		=> "TRGameFlow Script.txt\n" + (pause ? "@pause" : string.Empty);

	/// <summary>
	/// Detects and copies the compiled data file into the output directory.
	/// </summary>
	/// <param name="gameflowDirectory">The directory that contains the compiled data file.</param>
	/// <param name="outputDirectory">The directory that receives the copied data file.</param>
	/// <param name="compiledScriptFileName">The name of the compiled data file.</param>
	/// <returns><c>true</c> when the compiled data file existed and was copied; otherwise, <c>false</c>.</returns>
	internal static bool FinalizeCompileResult(string gameflowDirectory, string outputDirectory, string compiledScriptFileName)
	{
		string compiledScriptFilePath = Path.Combine(gameflowDirectory, compiledScriptFileName);

		if (!File.Exists(compiledScriptFilePath))
			return false;

		File.Copy(compiledScriptFilePath, Path.Combine(outputDirectory, "tombpc.dat"), true);
		return true;
	}

	private static bool RunCompileWorkflow(
		string inputDirectory,
		string outputDirectory,
		string gameflowDirectory,
		string batchFileContent,
		string compiledScriptFileName,
		string keptExecutableName,
		bool pause)
	{
		ScriptDirectoryCopier.CopyScriptDirectory(inputDirectory, gameflowDirectory, clearTarget: false);

		string batchFilePath = Path.Combine(gameflowDirectory, "compile.bat");
		File.WriteAllText(batchFilePath, batchFileContent);

		var startInfo = new ProcessStartInfo
		{
			FileName = batchFilePath,
			WorkingDirectory = gameflowDirectory,
			UseShellExecute = true
		};

		Process? process = null;

		try
		{
			process = Process.Start(startInfo);

			if (process is null)
				return false;

			// In interactive (paused) mode the batch waits for the user to dismiss the compiler
			// window, so it runs to completion; otherwise a runaway compiler is bounded by a
			// timeout and the whole process tree is killed so no child compiler keeps running.
			if (pause)
				process.WaitForExit();
			else if (!process.WaitForExit(ProcessTimeoutMilliseconds))
			{
				TerminateProcessTree(process);
				return false;
			}

			return FinalizeCompileResult(gameflowDirectory, outputDirectory, compiledScriptFileName);
		}
		finally
		{
			process?.Dispose();
			ScriptDirectoryCopier.ClearDirectoryExcept(gameflowDirectory, name => name.Equals(keptExecutableName, StringComparison.OrdinalIgnoreCase));
		}
	}

	private static void TerminateProcessTree(Process process)
	{
		try
		{
			process.Kill(entireProcessTree: true);
		}
		catch (Exception exception) when (exception is InvalidOperationException or NotSupportedException or Win32Exception)
		{
			// Whole-tree termination is not always available for shell-launched batch processes.
			// Fall back to killing the batch wrapper alone so the workflow still cannot hang.
			Log.Warn(exception, "Could not terminate the whole compiler process tree; killing the batch process only.");

			try
			{
				process.Kill();
			}
			catch (Exception fallbackException) when (fallbackException is InvalidOperationException or Win32Exception)
			{
				// The process already exited; there is nothing left to terminate.
			}
		}

		try
		{
			process.WaitForExit();
		}
		catch (InvalidOperationException)
		{
			// The process exited before its termination state could be observed.
		}
	}
}
