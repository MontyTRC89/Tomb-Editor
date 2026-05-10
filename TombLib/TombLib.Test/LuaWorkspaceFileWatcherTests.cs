using System.Runtime.ExceptionServices;
using TombIDE.ScriptingStudio.Services.LuaIntellisense;

namespace TombLib.Test;

[TestClass]
public class LuaWorkspaceFileWatcherTests
{
	[TestMethod]
	public async Task Dispose_DuringActiveDispatch_DoesNotFaultDispatch()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWatcherDispose_" + Guid.NewGuid().ToString("N"));
		var dispatchStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowDispatchToFinish = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

		try
		{
			Directory.CreateDirectory(workspaceRoot);

			using var watcher = new LuaWorkspaceFileWatcher(workspaceRoot, async (_, _) =>
			{
				dispatchStarted.TrySetResult(true);
				await allowDispatchToFinish.Task.ConfigureAwait(false);
			});

			watcher.QueueChangeForTest(Path.Combine(workspaceRoot, "test.lua"), FileChangeKind.Changed);
			Task dispatchTask = watcher.DispatchPendingChangesForTestAsync();

			Task completedTask = await Task.WhenAny(dispatchStarted.Task, Task.Delay(TimeSpan.FromSeconds(1))).ConfigureAwait(false);
			Assert.AreSame(dispatchStarted.Task, completedTask);

			watcher.Dispose();
			allowDispatchToFinish.TrySetResult(true);

			await dispatchTask.ConfigureAwait(false);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}
}