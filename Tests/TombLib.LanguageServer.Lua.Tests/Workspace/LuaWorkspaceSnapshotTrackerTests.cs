namespace Nickelony.LanguageServer.Lua.Tests;

[TestClass]
public class LuaWorkspaceSnapshotTrackerTests
{
	[TestMethod]
	public void BuildDeltaBatch_ReportsCreatedChangedAndDeletedPaths()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWorkspaceSnapshot_" + Guid.NewGuid().ToString("N"));
		string scriptsDirectoryPath = Path.Combine(workspaceRoot, "Scripts");
		string changedFilePath = Path.Combine(scriptsDirectoryPath, "changed.lua");
		string deletedFilePath = Path.Combine(scriptsDirectoryPath, "deleted.lua");
		string createdFilePath = Path.Combine(scriptsDirectoryPath, "created.lua");

		try
		{
			Directory.CreateDirectory(scriptsDirectoryPath);
			File.WriteAllText(changedFilePath, "return 1");
			File.WriteAllText(deletedFilePath, "return 2");

			var tracker = CreateTracker(workspaceRoot);
			tracker.CaptureTrackedSnapshot();
			Dictionary<string, LuaWorkspaceSnapshotEntry> previousSnapshot = tracker.CloneTrackedSnapshot();

			File.WriteAllText(changedFilePath, "return 123456");
			File.Delete(deletedFilePath);
			File.WriteAllText(createdFilePath, "return 3");

			Dictionary<string, LuaWorkspaceSnapshotEntry> currentSnapshot = tracker.ReplaceTrackedSnapshotWithCurrent();
			FileChangeBatch batch = LuaWorkspaceSnapshotTracker.BuildDeltaBatch(previousSnapshot, currentSnapshot);

			string normalizedChangedFilePath = LanguageServerPathHelper.NormalizeLocalPath(changedFilePath);
			string normalizedDeletedFilePath = LanguageServerPathHelper.NormalizeLocalPath(deletedFilePath);
			string normalizedCreatedFilePath = LanguageServerPathHelper.NormalizeLocalPath(createdFilePath);

			Assert.AreEqual(3, batch.Count);
			Assert.AreEqual(FileChangeKind.Changed, GetChange(batch, normalizedChangedFilePath).Kind);
			Assert.AreEqual(FileChangeKind.Deleted, GetChange(batch, normalizedDeletedFilePath).Kind);
			Assert.AreEqual(FileChangeKind.Created, GetChange(batch, normalizedCreatedFilePath).Kind);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public void BuildDeltaBatch_DoesNotReportDeletionWhenTrackedPathStillExistsButCurrentSnapshotMissesIt()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWorkspaceSnapshotMissingCurrent_" + Guid.NewGuid().ToString("N"));
		string scriptsDirectoryPath = Path.Combine(workspaceRoot, "Scripts");
		string existingFilePath = Path.Combine(scriptsDirectoryPath, "existing.lua");

		try
		{
			Directory.CreateDirectory(scriptsDirectoryPath);
			File.WriteAllText(existingFilePath, "return 1");

			var tracker = CreateTracker(workspaceRoot);
			tracker.CaptureTrackedSnapshot();
			Dictionary<string, LuaWorkspaceSnapshotEntry> previousSnapshot = tracker.CloneTrackedSnapshot();
			var currentSnapshot = new Dictionary<string, LuaWorkspaceSnapshotEntry>(StringComparer.OrdinalIgnoreCase);

			FileChangeBatch batch = LuaWorkspaceSnapshotTracker.BuildDeltaBatch(previousSnapshot, currentSnapshot);

			Assert.AreEqual(0, batch.Count);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public void BuildDeltaBatch_ReportsChangedPathWhenContentChangesWithoutLengthOrTimestampChange()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWorkspaceSnapshotFingerprint_" + Guid.NewGuid().ToString("N"));
		string scriptsDirectoryPath = Path.Combine(workspaceRoot, "Scripts");
		string changedFilePath = Path.Combine(scriptsDirectoryPath, "changed.lua");

		try
		{
			Directory.CreateDirectory(scriptsDirectoryPath);
			File.WriteAllText(changedFilePath, "return 1");
			DateTime baselineWriteTime = File.GetLastWriteTimeUtc(changedFilePath);

			var tracker = CreateTracker(workspaceRoot);
			tracker.CaptureTrackedSnapshot();
			Dictionary<string, LuaWorkspaceSnapshotEntry> previousSnapshot = tracker.CloneTrackedSnapshot();

			File.WriteAllText(changedFilePath, "return 2");
			File.SetLastWriteTimeUtc(changedFilePath, baselineWriteTime);

			Dictionary<string, LuaWorkspaceSnapshotEntry> currentSnapshot = tracker.ReplaceTrackedSnapshotWithCurrent();
			FileChangeBatch batch = LuaWorkspaceSnapshotTracker.BuildDeltaBatch(previousSnapshot, currentSnapshot);

			string normalizedChangedFilePath = LanguageServerPathHelper.NormalizeLocalPath(changedFilePath);

			Assert.AreEqual(1, batch.Count);
			Assert.AreEqual(FileChangeKind.Changed, GetChange(batch, normalizedChangedFilePath).Kind);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	[TestMethod]
	public void ApplyChanges_UpdatesTrackedSnapshotForSubsequentRecoveryDiff()
	{
		string workspaceRoot = Path.Combine(Path.GetTempPath(), "LuaWorkspaceSnapshotApply_" + Guid.NewGuid().ToString("N"));
		string scriptsDirectoryPath = Path.Combine(workspaceRoot, "Scripts");
		string createdFilePath = Path.Combine(scriptsDirectoryPath, "created.lua");

		try
		{
			Directory.CreateDirectory(scriptsDirectoryPath);

			var tracker = CreateTracker(workspaceRoot);
			tracker.CaptureTrackedSnapshot();

			File.WriteAllText(createdFilePath, "return 1");
			string normalizedCreatedFilePath = LanguageServerPathHelper.NormalizeLocalPath(createdFilePath);

			tracker.ApplyChanges(
			[
				new WorkspaceFileChange(normalizedCreatedFilePath, FileChangeKind.Created)
			]);

			Dictionary<string, LuaWorkspaceSnapshotEntry> previousSnapshot = tracker.CloneTrackedSnapshot();
			File.Delete(createdFilePath);

			Dictionary<string, LuaWorkspaceSnapshotEntry> currentSnapshot = tracker.ReplaceTrackedSnapshotWithCurrent();
			FileChangeBatch batch = LuaWorkspaceSnapshotTracker.BuildDeltaBatch(previousSnapshot, currentSnapshot);

			Assert.AreEqual(1, batch.Count);
			Assert.AreEqual(FileChangeKind.Deleted, batch.Entries[0].Kind);
			Assert.AreEqual(normalizedCreatedFilePath, batch.Entries[0].Path);
		}
		finally
		{
			if (Directory.Exists(workspaceRoot))
				Directory.Delete(workspaceRoot, recursive: true);
		}
	}

	private static LuaWorkspaceSnapshotTracker CreateTracker(string workspaceRootDirectoryPath) => new(
		LanguageServerPathHelper.NormalizeLocalPath(workspaceRootDirectoryPath),
		[
			new WorkspaceWatchSpecification("*.lua", IncludeSubdirectories: true),
			new WorkspaceWatchSpecification(".API", IncludeSubdirectories: false),
			new WorkspaceWatchSpecification(".luarc.*", IncludeSubdirectories: false)
		]);

	private static WorkspaceFileChange GetChange(FileChangeBatch batch, string filePath)
		=> batch.Entries.First(change => string.Equals(change.Path, filePath, StringComparison.OrdinalIgnoreCase));
}
