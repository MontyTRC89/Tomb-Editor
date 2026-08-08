using System;
using System.Diagnostics;
using System.IO;
using TombLib.Scripting.GameFlowScript.Compilers;

namespace TombLib.Tests.GameFlow;

/// <summary>
/// Direct tests for <see cref="ScriptCompiler"/> process orchestration through the injected
/// <see cref="ICompilerProcessFactory"/> seam: null starts, paused and timed waits, timeout
/// termination with fallback, start configuration, and staging-directory cleanup.
/// </summary>
[TestClass]
public class ScriptCompilerProcessTests
{
	[TestMethod]
	public void RunCompileWorkflow_NullProcess_ReturnsFalseAndCleansStagingDirectory()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false),
				"tombpc.dat",
				"gameFlow.exe",
				pause: false,
				new FakeCompilerProcessFactory(_ => null));

			Assert.IsFalse(result);
			Assert.IsFalse(File.Exists(Path.Combine(gameflowDirectory, "compile.bat")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void RunCompileWorkflow_Paused_ProcessRunsToCompletionAndCopiesOutput()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			var process = new FakeCompilerProcess(
				onWaitForExit: () => File.WriteAllText(Path.Combine(gameflowDirectory, "tombpc.dat"), "compiled data"));

			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: true),
				"tombpc.dat",
				"gameFlow.exe",
				pause: true,
				new FakeCompilerProcessFactory(_ => process));

			Assert.IsTrue(result);
			Assert.IsTrue(process.WaitForExitCalled);
			Assert.IsFalse(process.TimedWaitForExitCalled);
			Assert.IsTrue(process.Disposed);
			Assert.AreEqual("compiled data", File.ReadAllText(Path.Combine(outputDirectory, "tombpc.dat")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void RunCompileWorkflow_TimedWait_ProcessExitsInTimeAndCopiesOutput()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			var process = new FakeCompilerProcess(
				onTimedWaitForExit: () =>
				{
					File.WriteAllText(Path.Combine(gameflowDirectory, "tombpc.dat"), "compiled data");
					return true;
				});

			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false),
				"tombpc.dat",
				"gameFlow.exe",
				pause: false,
				new FakeCompilerProcessFactory(_ => process));

			Assert.IsTrue(result);
			Assert.IsTrue(process.TimedWaitForExitCalled);
			Assert.IsFalse(process.WaitForExitCalled);
			Assert.IsTrue(process.Disposed);
			Assert.AreEqual("compiled data", File.ReadAllText(Path.Combine(outputDirectory, "tombpc.dat")));
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void RunCompileWorkflow_Timeout_TerminatesProcessTreeAndReturnsFalse()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			var process = new FakeCompilerProcess(onTimedWaitForExit: () => false);

			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false),
				"tombpc.dat",
				"gameFlow.exe",
				pause: false,
				new FakeCompilerProcessFactory(_ => process));

			Assert.IsFalse(result);
			Assert.AreEqual(1, process.KillEntireProcessTreeCalls);
			Assert.AreEqual(0, process.KillCalls);
			Assert.IsTrue(process.Disposed);
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void RunCompileWorkflow_TimeoutWithTreeKillFailure_FallsBackToSingleKill()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			var process = new FakeCompilerProcess(
				onTimedWaitForExit: () => false,
				onKillEntireProcessTree: () => throw new InvalidOperationException("Process has already exited"));

			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false),
				"tombpc.dat",
				"gameFlow.exe",
				pause: false,
				new FakeCompilerProcessFactory(_ => process));

			Assert.IsFalse(result);
			Assert.AreEqual(1, process.KillEntireProcessTreeCalls);
			Assert.AreEqual(1, process.KillCalls);
			Assert.IsTrue(process.Disposed);
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	[TestMethod]
	public void RunCompileWorkflow_StartsBatchWithStagingDirectory()
	{
		(string baseDirectory, string inputDirectory, string gameflowDirectory, string outputDirectory) = CreateWorkflowDirectories();

		try
		{
			ProcessStartInfo? capturedStartInfo = null;
			var process = new FakeCompilerProcess(onTimedWaitForExit: () => true);

			bool result = ScriptCompiler.RunCompileWorkflow(
				inputDirectory,
				outputDirectory,
				gameflowDirectory,
				ScriptCompiler.BuildClassicBatchContent(isTR3: false, pause: false),
				"tombpc.dat",
				"gameFlow.exe",
				pause: false,
				new FakeCompilerProcessFactory(startInfo =>
				{
					capturedStartInfo = startInfo;
					return process;
				}));

			Assert.IsFalse(result);

			Assert.IsNotNull(capturedStartInfo);
			Assert.IsNotNull(capturedStartInfo.FileName);
			Assert.IsNotNull(capturedStartInfo.WorkingDirectory);
			Assert.AreEqual(Path.Combine(gameflowDirectory, "compile.bat"), capturedStartInfo.FileName);
			Assert.AreEqual(gameflowDirectory, capturedStartInfo.WorkingDirectory);
			Assert.IsTrue(capturedStartInfo.UseShellExecute);
			Assert.IsTrue(process.TimedWaitForExitCalled);
		}
		finally
		{
			Directory.Delete(baseDirectory, recursive: true);
		}
	}

	private static (string BaseDirectory, string InputDirectory, string GameflowDirectory, string OutputDirectory) CreateWorkflowDirectories()
	{
		string baseDirectory = CreateTempDirectory();
		string inputDirectory = CreateTempDirectory(baseDirectory);
		string gameflowDirectory = CreateTempDirectory(baseDirectory);
		string outputDirectory = CreateTempDirectory(baseDirectory);

		File.WriteAllText(Path.Combine(inputDirectory, "Script.txt"), "script content");

		return (baseDirectory, inputDirectory, gameflowDirectory, outputDirectory);
	}

	private static string CreateTempDirectory(string? parent = null)
	{
		string path = Path.Combine(parent ?? Path.GetTempPath(), "ScriptCompilerProcessTests_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(path);
		return path;
	}

	private sealed class FakeCompilerProcessFactory : ICompilerProcessFactory
	{
		private readonly Func<ProcessStartInfo, ICompilerProcess?> _start;

		public FakeCompilerProcessFactory(Func<ProcessStartInfo, ICompilerProcess?> start)
			=> _start = start;

		public ICompilerProcess? Start(ProcessStartInfo startInfo)
			=> _start(startInfo);
	}

	private sealed class FakeCompilerProcess : ICompilerProcess
	{
		private readonly Action? _onWaitForExit;
		private readonly Func<bool>? _onTimedWaitForExit;
		private readonly Action? _onKillEntireProcessTree;
		private readonly Action? _onKill;

		public FakeCompilerProcess(
			Action? onWaitForExit = null,
			Func<bool>? onTimedWaitForExit = null,
			Action? onKillEntireProcessTree = null,
			Action? onKill = null)
		{
			_onWaitForExit = onWaitForExit;
			_onTimedWaitForExit = onTimedWaitForExit;
			_onKillEntireProcessTree = onKillEntireProcessTree;
			_onKill = onKill;
		}

		public bool WaitForExitCalled { get; private set; }

		public bool TimedWaitForExitCalled { get; private set; }

		public int KillEntireProcessTreeCalls { get; private set; }

		public int KillCalls { get; private set; }

		public bool Disposed { get; private set; }

		public void WaitForExit()
		{
			WaitForExitCalled = true;
			_onWaitForExit?.Invoke();
		}

		public bool WaitForExit(int timeoutMilliseconds)
		{
			TimedWaitForExitCalled = true;
			return _onTimedWaitForExit?.Invoke() ?? true;
		}

		public void KillEntireProcessTree()
		{
			KillEntireProcessTreeCalls++;
			_onKillEntireProcessTree?.Invoke();
		}

		public void Kill()
		{
			KillCalls++;
			_onKill?.Invoke();
		}

		public void Dispose()
			=> Disposed = true;
	}
}
