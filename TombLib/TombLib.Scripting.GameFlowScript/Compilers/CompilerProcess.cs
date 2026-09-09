using System;
using System.Diagnostics;

namespace TombLib.Scripting.GameFlowScript.Compilers;

/// <summary>
/// Abstracts the external process a compiler drives so process-level orchestration can be tested
/// without launching a real process.
/// </summary>
internal interface ICompilerProcess : IDisposable
{
	/// <summary>
	/// Blocks until the process exits.
	/// </summary>
	void WaitForExit();

	/// <summary>
	/// Waits up to the given timeout and reports whether the process exited in time.
	/// </summary>
	/// <param name="timeoutMilliseconds">The timeout in milliseconds.</param>
	/// <returns><c>true</c> when the process exited within the timeout; otherwise, <c>false</c>.</returns>
	bool WaitForExit(int timeoutMilliseconds);

	/// <summary>
	/// Kills the process and its child process tree.
	/// </summary>
	void KillEntireProcessTree();

	/// <summary>
	/// Kills the process without its child processes.
	/// </summary>
	void Kill();
}

/// <summary>
/// Creates <see cref="ICompilerProcess"/> instances for a compiler workflow.
/// </summary>
internal interface ICompilerProcessFactory
{
	/// <summary>
	/// Starts the process described by the supplied start configuration.
	/// </summary>
	/// <param name="startInfo">The process start configuration.</param>
	/// <returns>The started process, or <c>null</c> when no process could be started.</returns>
	ICompilerProcess? Start(ProcessStartInfo startInfo);
}

/// <summary>
/// Adapts <see cref="Process"/> to <see cref="ICompilerProcess"/>.
/// </summary>
internal sealed class ProcessCompilerProcess : ICompilerProcess
{
	private readonly Process _process;

	/// <summary>
	/// Initializes a new instance of the <see cref="ProcessCompilerProcess"/> class.
	/// </summary>
	/// <param name="process">The wrapped process.</param>
	public ProcessCompilerProcess(Process process) => _process = process;

	public void WaitForExit() => _process.WaitForExit();

	public bool WaitForExit(int timeoutMilliseconds) => _process.WaitForExit(timeoutMilliseconds);

	public void KillEntireProcessTree() => _process.Kill(entireProcessTree: true);

	public void Kill() => _process.Kill();

	public void Dispose() => _process.Dispose();
}

/// <summary>
/// Creates <see cref="ProcessCompilerProcess"/> instances from the real <see cref="Process"/> API.
/// </summary>
internal sealed class ProcessCompilerProcessFactory : ICompilerProcessFactory
{
	/// <summary>
	/// Gets the shared factory instance.
	/// </summary>
	public static ICompilerProcessFactory Instance { get; } = new ProcessCompilerProcessFactory();

	private ProcessCompilerProcessFactory()
	{ }

	public ICompilerProcess? Start(ProcessStartInfo startInfo)
	{
		Process? process = Process.Start(startInfo);

		return process is null
			? null
			: new ProcessCompilerProcess(process);
	}
}
