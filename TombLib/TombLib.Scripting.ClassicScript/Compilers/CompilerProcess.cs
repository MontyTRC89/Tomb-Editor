using System;
using System.Diagnostics;

namespace TombLib.Scripting.ClassicScript.Compilers;

/// <summary>
/// Abstracts the external process driven by a ClassicScript compiler.
/// </summary>
internal interface ICompilerProcess : IDisposable
{
	/// <summary>
	/// Blocks until the process exits.
	/// </summary>
	void WaitForExit();
}

/// <summary>
/// Creates processes for ClassicScript compiler workflows.
/// </summary>
internal interface ICompilerProcessFactory
{
	/// <summary>
	/// Starts the process described by the supplied configuration.
	/// </summary>
	/// <param name="startInfo">The process start configuration.</param>
	/// <returns>The started process, or <see langword="null"/> when no process was started.</returns>
	ICompilerProcess? Start(ProcessStartInfo startInfo);
}

/// <summary>
/// Adapts <see cref="Process"/> to the compiler process seam.
/// </summary>
internal sealed class ProcessCompilerProcess : ICompilerProcess
{
	private readonly Process _process;

	public ProcessCompilerProcess(Process process)
		=> _process = process;

	public void WaitForExit()
		=> _process.WaitForExit();

	public void Dispose()
		=> _process.Dispose();
}

/// <summary>
/// Creates compiler process adapters from the real <see cref="Process"/> API.
/// </summary>
internal sealed class ProcessCompilerProcessFactory : ICompilerProcessFactory
{
	public static ICompilerProcessFactory Instance { get; } = new ProcessCompilerProcessFactory();

	private ProcessCompilerProcessFactory()
	{
	}

	public ICompilerProcess? Start(ProcessStartInfo startInfo)
	{
		Process? process = Process.Start(startInfo);

		return process is null
			? null
			: new ProcessCompilerProcess(process);
	}
}