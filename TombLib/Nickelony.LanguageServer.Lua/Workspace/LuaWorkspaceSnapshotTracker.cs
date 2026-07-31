using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Tracks the subset of workspace paths mirrored to LuaLS so watcher recovery can reconcile missed file changes.
/// </summary>
internal sealed class LuaWorkspaceSnapshotTracker
{
	private readonly ILogger _logger;

	private readonly string _workspaceRootDirectoryPath;
	private readonly IReadOnlyList<WorkspaceWatchSpecification> _watchSpecifications;
	private readonly object _snapshotSyncRoot = new();
	private Dictionary<string, LuaWorkspaceSnapshotEntry> _trackedSnapshot = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaWorkspaceSnapshotTracker"/> class.
	/// </summary>
	/// <param name="workspaceRootDirectoryPath">The normalized workspace root directory.</param>
	/// <param name="watchSpecifications">The file patterns that should participate in the tracked snapshot.</param>
	/// <param name="logger">The logger instance, or <see langword="null"/> for a no-op logger.</param>
	internal LuaWorkspaceSnapshotTracker(string workspaceRootDirectoryPath, IReadOnlyList<WorkspaceWatchSpecification> watchSpecifications, ILogger? logger = null)
	{
		_logger = logger ?? NullLogger.Instance;

		_workspaceRootDirectoryPath = workspaceRootDirectoryPath;
		_watchSpecifications = watchSpecifications;
	}

	/// <summary>
	/// Replaces the tracked snapshot with a fresh capture of the current workspace state.
	/// </summary>
	internal void CaptureTrackedSnapshot()
	{
		Dictionary<string, LuaWorkspaceSnapshotEntry> snapshot = CaptureSnapshot();

		lock (_snapshotSyncRoot)
			_trackedSnapshot = snapshot;
	}

	/// <summary>
	/// Creates a stable clone of the currently tracked workspace snapshot.
	/// </summary>
	/// <returns>The cloned snapshot.</returns>
	internal Dictionary<string, LuaWorkspaceSnapshotEntry> CloneTrackedSnapshot()
	{
		lock (_snapshotSyncRoot)
			return CloneSnapshot(_trackedSnapshot);
	}

	/// <summary>
	/// Captures the current workspace state, replaces the tracked snapshot, and returns the fresh snapshot.
	/// </summary>
	/// <returns>The newly captured snapshot.</returns>
	internal Dictionary<string, LuaWorkspaceSnapshotEntry> ReplaceTrackedSnapshotWithCurrent()
	{
		Dictionary<string, LuaWorkspaceSnapshotEntry> currentSnapshot = CaptureSnapshot();

		lock (_snapshotSyncRoot)
			_trackedSnapshot = currentSnapshot;

		return currentSnapshot;
	}

	/// <summary>
	/// Applies a set of already forwarded file changes to the tracked snapshot.
	/// </summary>
	/// <param name="changes">The normalized forwarded file changes.</param>
	internal void ApplyChanges(IReadOnlyList<WorkspaceFileChange> changes)
	{
		lock (_snapshotSyncRoot)
		{
			for (int i = 0; i < changes.Count; i++)
			{
				WorkspaceFileChange change = changes[i];

				if (change.Kind == FileChangeKind.Deleted)
				{
					_trackedSnapshot.Remove(change.Path);
					continue;
				}

				if (TryCreateSnapshotEntry(change.Path, out LuaWorkspaceSnapshotEntry entry, _logger))
					_trackedSnapshot[change.Path] = entry;
				else if (TryDeterminePathMissing(change.Path, out bool isMissing, _logger) && isMissing)
					_trackedSnapshot.Remove(change.Path);
			}
		}
	}

	/// <summary>
	/// Builds a normalized delta batch between two captured workspace snapshots.
	/// </summary>
	/// <param name="previousSnapshot">The baseline snapshot captured before watcher disruption.</param>
	/// <param name="currentSnapshot">The replacement snapshot captured after watcher recovery.</param>
	/// <returns>The sorted workspace change batch needed to reconcile the snapshots.</returns>
	internal static FileChangeBatch BuildDeltaBatch(
		IReadOnlyDictionary<string, LuaWorkspaceSnapshotEntry> previousSnapshot,
		IReadOnlyDictionary<string, LuaWorkspaceSnapshotEntry> currentSnapshot)
	{
		var changes = new List<WorkspaceFileChange>();

		foreach ((string path, LuaWorkspaceSnapshotEntry previousEntry) in previousSnapshot)
		{
			if (!currentSnapshot.TryGetValue(path, out LuaWorkspaceSnapshotEntry currentEntry))
			{
				if (TryDeterminePathMissing(path, out bool isMissing) && isMissing)
					changes.Add(new WorkspaceFileChange(path, FileChangeKind.Deleted));

				continue;
			}

			if (!currentEntry.Equals(previousEntry))
				changes.Add(new WorkspaceFileChange(path, FileChangeKind.Changed));
		}

		foreach ((string path, LuaWorkspaceSnapshotEntry _) in currentSnapshot)
		{
			if (!previousSnapshot.ContainsKey(path))
				changes.Add(new WorkspaceFileChange(path, FileChangeKind.Created));
		}

		changes.Sort(static (left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.Path, right.Path));
		return new FileChangeBatch(changes);
	}

	private Dictionary<string, LuaWorkspaceSnapshotEntry> CaptureSnapshot()
	{
		var snapshot = new Dictionary<string, LuaWorkspaceSnapshotEntry>(StringComparer.OrdinalIgnoreCase);

		if (!Directory.Exists(_workspaceRootDirectoryPath))
			return snapshot;

		for (int i = 0; i < _watchSpecifications.Count; i++)
			CaptureSnapshotForSpecification(snapshot, _watchSpecifications[i]);

		return snapshot;
	}

	private void CaptureSnapshotForSpecification(
		Dictionary<string, LuaWorkspaceSnapshotEntry> snapshot,
		WorkspaceWatchSpecification watchSpecification)
	{
		if (watchSpecification.Filter.IndexOfAny(['*', '?']) < 0)
		{
			TryAddSnapshotPath(snapshot, Path.Combine(_workspaceRootDirectoryPath, watchSpecification.Filter));
			return;
		}

		var enumerationOptions = new EnumerationOptions
		{
			IgnoreInaccessible = true,
			RecurseSubdirectories = watchSpecification.IncludeSubdirectories,
			ReturnSpecialDirectories = false
		};

		foreach (string filePath in Directory.EnumerateFiles(_workspaceRootDirectoryPath, watchSpecification.Filter, enumerationOptions))
			TryAddSnapshotPath(snapshot, filePath);
	}

	private static void TryAddSnapshotPath(Dictionary<string, LuaWorkspaceSnapshotEntry> snapshot, string path)
	{
		if (!LanguageServerPathHelper.TryNormalizeLocalPath(path, out string normalizedPath))
			return;

		if (TryCreateSnapshotEntry(normalizedPath, out LuaWorkspaceSnapshotEntry entry))
			snapshot[normalizedPath] = entry;
	}

	private static Dictionary<string, LuaWorkspaceSnapshotEntry> CloneSnapshot(Dictionary<string, LuaWorkspaceSnapshotEntry> snapshot)
		=> new(snapshot, StringComparer.OrdinalIgnoreCase);

	private static bool TryCreateSnapshotEntry(string normalizedPath, out LuaWorkspaceSnapshotEntry entry, ILogger? logger = null)
	{
		entry = default;

		try
		{
			if (File.Exists(normalizedPath))
			{
				var fileInfo = new FileInfo(normalizedPath);

				entry = new LuaWorkspaceSnapshotEntry(
					IsDirectory: false,
					fileInfo.LastWriteTimeUtc.Ticks,
					fileInfo.Length,
					ComputeFileContentFingerprint(normalizedPath));

				return true;
			}

			if (Directory.Exists(normalizedPath))
			{
				var directoryInfo = new DirectoryInfo(normalizedPath);
				entry = new LuaWorkspaceSnapshotEntry(IsDirectory: true, directoryInfo.LastWriteTimeUtc.Ticks, 0, 0);
				return true;
			}
		}
		catch (Exception exception)
		{
			logger?.LogDebug(exception, "Failed to capture a Lua workspace snapshot entry for '{Path}'.", normalizedPath);
		}

		return false;
	}

	private static ulong ComputeFileContentFingerprint(string normalizedPath)
	{
		using FileStream stream = File.OpenRead(normalizedPath);
		byte[] hash = SHA256.HashData(stream);
		return BinaryPrimitives.ReadUInt64LittleEndian(hash.AsSpan(0, sizeof(ulong)));
	}

	private static bool TryDeterminePathMissing(string normalizedPath, out bool isMissing, ILogger? logger = null)
	{
		isMissing = false;

		try
		{
			_ = File.GetAttributes(normalizedPath);
			return true;
		}
		catch (DirectoryNotFoundException)
		{
			isMissing = true;
			return true;
		}
		catch (FileNotFoundException)
		{
			isMissing = true;
			return true;
		}
		catch (Exception exception)
		{
			logger?.LogDebug(exception, "Failed to determine whether the Lua workspace path '{Path}' is missing.", normalizedPath);
			return false;
		}
	}
}

/// <summary>
/// Represents the tracked file-system state for a single watched workspace path.
/// </summary>
/// <param name="IsDirectory">Whether the path represents a directory.</param>
/// <param name="LastWriteUtcTicks">The last-write timestamp used for change detection.</param>
/// <param name="Length">The file length used for change detection.</param>
/// <param name="ContentFingerprint">A stable file-content fingerprint used when length and timestamps alone are ambiguous.</param>
internal readonly record struct LuaWorkspaceSnapshotEntry(bool IsDirectory, long LastWriteUtcTicks, long Length, ulong ContentFingerprint);
