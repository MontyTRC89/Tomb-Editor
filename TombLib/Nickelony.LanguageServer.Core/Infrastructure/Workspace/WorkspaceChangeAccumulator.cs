using System.Collections.Concurrent;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Accumulates workspace file changes, coalesces repeated updates per path, preserves delete/create replacement
/// semantics, and preserves the first pending occurrence order.
/// </summary>
internal sealed class WorkspaceChangeAccumulator
{
	/// <summary>
	/// Stores the latest coalesced change for each normalized path.
	/// </summary>
	private readonly ConcurrentDictionary<string, BufferedWorkspaceChange> _changes = new(LanguageServerPathHelper.LocalPathComparer);

	/// <summary>
	/// Produces deterministic insertion order for buffered paths.
	/// </summary>
	private long _nextSequence;

	/// <summary>
	/// Stores the current coalesced change kind together with the order in which the path first became pending.
	/// </summary>
	/// <param name="Sequence">The first pending occurrence order for the path.</param>
	/// <param name="PrimaryKind">The first effective coalesced change kind.</param>
	/// <param name="SecondaryKind">The second effective coalesced change kind when a delete/create pair must be preserved.</param>
	private readonly record struct BufferedWorkspaceChange(long Sequence, FileChangeKind PrimaryKind, FileChangeKind? SecondaryKind = null);

	/// <summary>
	/// Gets a value indicating whether the accumulator currently holds no pending changes.
	/// </summary>
	public bool IsEmpty => _changes.IsEmpty;

	/// <summary>
	/// Adds or merges a single workspace file change into the accumulator.
	/// </summary>
	/// <param name="path">The normalized file path.</param>
	/// <param name="kind">The incoming change kind.</param>
	public void Add(string path, FileChangeKind kind)
	{
		if (string.IsNullOrEmpty(path))
			return;

		long newSequence = Interlocked.Increment(ref _nextSequence);

		while (true)
		{
			if (!_changes.TryGetValue(path, out BufferedWorkspaceChange existingChange))
			{
				if (_changes.TryAdd(path, new BufferedWorkspaceChange(newSequence, kind)))
					return;

				continue;
			}

			if (!TryCombine(existingChange, kind, out BufferedWorkspaceChange combinedChange))
			{
				if (_changes.TryRemove(new KeyValuePair<string, BufferedWorkspaceChange>(path, existingChange)))
					return;

				continue;
			}

			if (_changes.TryUpdate(path, combinedChange, existingChange))
				return;
		}
	}

	/// <summary>
	/// Adds or merges a sequence of workspace file changes into the accumulator.
	/// </summary>
	/// <param name="changes">The changes to merge.</param>
	public void AddRange(IReadOnlyList<WorkspaceFileChange> changes)
	{
		for (int i = 0; i < changes.Count; i++)
			Add(changes[i].Path, changes[i].Kind);
	}

	/// <summary>
	/// Drains all pending changes into a batch ready for forwarding.
	/// </summary>
	/// <returns>The drained file-change batch.</returns>
	public FileChangeBatch DrainBatch()
	{
		List<WorkspaceFileChange> changes = DrainChanges();
		return new FileChangeBatch(changes);
	}

	/// <summary>
	/// Drains all pending changes as normalized path/kind values.
	/// </summary>
	/// <returns>The drained list of coalesced changes.</returns>
	public List<WorkspaceFileChange> DrainChanges()
	{
		var drainedChanges = new List<BufferedWorkspaceChangeEntry>();

		foreach (KeyValuePair<string, BufferedWorkspaceChange> entry in _changes)
		{
			if (_changes.TryRemove(entry.Key, out BufferedWorkspaceChange change))
				drainedChanges.Add(new BufferedWorkspaceChangeEntry(entry.Key, change.Sequence, change.PrimaryKind, change.SecondaryKind));
		}

		drainedChanges.Sort(static (left, right) => left.Sequence.CompareTo(right.Sequence));

		var changes = new List<WorkspaceFileChange>(drainedChanges.Count);

		for (int i = 0; i < drainedChanges.Count; i++)
		{
			changes.Add(new WorkspaceFileChange(drainedChanges[i].Path, drainedChanges[i].PrimaryKind));

			if (drainedChanges[i].SecondaryKind is FileChangeKind secondaryKind)
				changes.Add(new WorkspaceFileChange(drainedChanges[i].Path, secondaryKind));
		}

		return changes;
	}

	/// <summary>
	/// Stores one drained buffered change together with its preserved ordering metadata.
	/// </summary>
	/// <param name="Path">The normalized file path.</param>
	/// <param name="Sequence">The first pending occurrence order for the path.</param>
	/// <param name="PrimaryKind">The first effective coalesced change kind.</param>
	/// <param name="SecondaryKind">The second effective coalesced change kind when a delete/create pair must be preserved.</param>
	private readonly record struct BufferedWorkspaceChangeEntry(string Path, long Sequence, FileChangeKind PrimaryKind, FileChangeKind? SecondaryKind);

	/// <summary>
	/// Combines an existing and incoming change into the effective change that should be forwarded.
	/// </summary>
	/// <param name="existing">The buffered change already stored for the path.</param>
	/// <param name="incoming">The new incoming change kind.</param>
	/// <param name="combined">Receives the coalesced effective change state when one remains.</param>
	/// <returns><see langword="true"/> when a coalesced change should remain buffered; otherwise, <see langword="false"/>.</returns>
	private static bool TryCombine(BufferedWorkspaceChange existing, FileChangeKind incoming, out BufferedWorkspaceChange combined)
	{
		if (existing.SecondaryKind is FileChangeKind secondaryKind)
			return TryCombineDeleteCreatePair(existing, secondaryKind, incoming, out combined);

		switch (existing.PrimaryKind)
		{
			case FileChangeKind.Created:
				if (incoming == FileChangeKind.Deleted)
				{
					combined = default;
					return false;
				}

				combined = existing;
				return true;

			case FileChangeKind.Changed:
				combined = incoming switch
				{
					FileChangeKind.Created => existing with { PrimaryKind = FileChangeKind.Created },
					FileChangeKind.Deleted => existing with { PrimaryKind = FileChangeKind.Deleted },
					_ => existing
				};

				return true;

			case FileChangeKind.Deleted:
				combined = incoming == FileChangeKind.Created
					? existing with { SecondaryKind = FileChangeKind.Created }
					: existing;

				return true;

			default:
				combined = existing;
				return true;
		}
	}

	private static bool TryCombineDeleteCreatePair(
		BufferedWorkspaceChange existing,
		FileChangeKind secondaryKind,
		FileChangeKind incoming,
		out BufferedWorkspaceChange combined)
	{
		if (existing.PrimaryKind != FileChangeKind.Deleted || secondaryKind != FileChangeKind.Created)
		{
			combined = existing;
			return true;
		}

		combined = incoming == FileChangeKind.Deleted
			? existing with { SecondaryKind = null }
			: existing;

		return true;
	}
}
