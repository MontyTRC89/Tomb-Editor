using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using TombLib.Scripting.UI.Documents;

namespace TombLib.Tests;

[TestClass]
public class ContentChangedWorkerDisposalTests
{
	[TestMethod]
	public void Dispose_IsIdempotent_SetsDisposedState()
	{
		WPFTestHelper.RunInSta(() =>
		{
			var worker = new ContentChangedWorker();

			worker.Dispose();
			worker.Dispose();

			Assert.IsTrue(GetDisposed(worker));
			Assert.IsFalse(worker.IsBusy);
		});
	}

	[TestMethod]
	public void PublicOperations_AfterDisposal_NoOpAndReturnSafeDefaults()
	{
		WPFTestHelper.RunInSta(() =>
		{
			string directory = CreateTempDirectory();
			string filePath = Path.Combine(directory, "document.txt");

			try
			{
				var worker = new ContentChangedWorker { FilePath = filePath, CreateBackupFiles = true };
				worker.Dispose();

				worker.Run("content");
				worker.SetPersistedContent("content");
				worker.CreateBackupFile("content");
				worker.DeleteBackupFile();
				worker.FilePath = Path.Combine(directory, "other.txt");
				worker.CreateBackupFiles = false;

				// Query members return safe defaults instead of touching the file system.
				Assert.IsFalse(worker.HasChanges("content"));
				Assert.IsFalse(worker.IsBusy);
				Assert.IsFalse(File.Exists(filePath + ".backup"));
			}
			finally
			{
				Directory.Delete(directory, true);
			}
		});
	}

	[TestMethod]
	public void BackupSynchronization_AfterDisposal_DoesNotWriteBackupFile()
	{
		WPFTestHelper.RunInSta(() =>
		{
			string directory = CreateTempDirectory();
			string filePath = Path.Combine(directory, "document.txt");

			try
			{
				var worker = new ContentChangedWorker { FilePath = filePath };
				worker.Dispose();

				// Simulate an in-flight backup synchronization completing after disposal.
				var task = (Task?)WPFTestHelper.InvokeInstanceMethod(
					worker,
					"SynchronizeBackupStateAsync",
					new[] { typeof(string), typeof(string), typeof(bool), typeof(int), typeof(int) },
					filePath,
					"content that differs",
					true,
					1,
					1);

				Assert.IsNotNull(task);
				task.GetAwaiter().GetResult();

				Assert.IsFalse(File.Exists(filePath + ".backup"));
			}
			finally
			{
				Directory.Delete(directory, true);
			}
		});
	}

	[TestMethod]
	public void Dispose_DuringInFlightRequest_InvalidatesAndLeavesNoBackupFile()
	{
		WPFTestHelper.RunInSta(() =>
		{
			string directory = CreateTempDirectory();
			string filePath = Path.Combine(directory, "document.txt");

			try
			{
				var worker = new ContentChangedWorker { FilePath = filePath, CreateBackupFiles = true };
				worker.Run("content that differs");

				var processingTask = (Task?)WPFTestHelper.GetPrivateFieldValue(worker, "_processingTask");
				Assert.IsNotNull(processingTask);

				worker.Dispose();

				// Disposal invalidates the pending request and clears the busy flag.
				Assert.IsFalse(worker.IsBusy);
				Assert.IsFalse(GetPendingRequest(worker));

				PumpDispatcher();
				Assert.IsTrue(processingTask.Wait(TimeSpan.FromSeconds(5)), "In-flight task did not settle after disposal.");

				// The in-flight pass must not leave a backup file behind after disposal.
				Assert.IsFalse(File.Exists(filePath + ".backup"));
			}
			finally
			{
				Directory.Delete(directory, true);
			}
		});
	}

	[TestMethod]
	public void ContentPersistenceCoordinator_AfterDisposal_DoesNotRunWorker()
	{
		WPFTestHelper.RunInSta(() =>
		{
			string directory = CreateTempDirectory();
			string filePath = Path.Combine(directory, "document.txt");

			try
			{
				var coordinator = new ContentPersistenceCoordinator(() => "content", () => false);

				coordinator.FilePath = filePath;
				coordinator.SetPersistedContent("baseline");
				coordinator.Dispose();

				Assert.IsFalse(coordinator.HandleContentChanged());
				Assert.IsFalse(coordinator.RunContentChangedCheck());
				Assert.IsFalse(coordinator.HasChanges("different"));
				coordinator.SetPersistedContent("more");
				coordinator.FilePath = Path.Combine(directory, "other.txt");
				coordinator.CreateBackupFiles = false;

				Assert.IsFalse(File.Exists(filePath + ".backup"));
			}
			finally
			{
				Directory.Delete(directory, true);
			}
		});
	}

	private static string CreateTempDirectory()
	{
		string directory = Path.Combine(Path.GetTempPath(), "TombLibContentChangedWorker_" + Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(directory);
		return directory;
	}

	private static void PumpDispatcher()
	{
		var frame = new DispatcherFrame();
		Dispatcher.CurrentDispatcher.BeginInvoke(
			DispatcherPriority.Background,
			new Action(() => frame.Continue = false));
		Dispatcher.PushFrame(frame);
	}

	private static bool GetPendingRequest(ContentChangedWorker worker)
		=> (bool)(WPFTestHelper.GetPrivateFieldValue(worker, "_hasPendingRequest") ?? false);

	private static bool GetDisposed(ContentChangedWorker worker)
		=> (bool)(WPFTestHelper.GetPrivateFieldValue(worker, "_isDisposed") ?? false);
}
