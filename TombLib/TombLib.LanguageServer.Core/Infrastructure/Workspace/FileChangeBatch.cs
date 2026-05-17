namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents a coalesced batch of workspace file changes ready to forward to the language server.
/// </summary>
public sealed class FileChangeBatch
{
	private readonly IReadOnlyList<WorkspaceFileChange> _entries;

	/// <summary>
	/// Initializes a new instance of the <see cref="FileChangeBatch"/> class.
	/// </summary>
	/// <param name="entries">The coalesced entries captured for the batch.</param>
	public FileChangeBatch(IEnumerable<WorkspaceFileChange> entries)
	{
		ArgumentNullException.ThrowIfNull(entries);
		_entries = Array.AsReadOnly([.. entries]);
	}

	/// <summary>
	/// Gets the number of coalesced entries in the batch.
	/// </summary>
	public int Count => _entries.Count;

	/// <summary>
	/// Gets the coalesced file-change entries.
	/// </summary>
	public IReadOnlyList<WorkspaceFileChange> Entries => _entries;
}
