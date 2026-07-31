namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a coalesced batch of workspace file changes ready to forward to the language server.
/// </summary>
public sealed class FileChangeBatch
{
	/// <summary>
	/// Initializes a new instance of the <see cref="FileChangeBatch"/> class.
	/// </summary>
	/// <param name="entries">The coalesced entries captured for the batch.</param>
	public FileChangeBatch(IEnumerable<WorkspaceFileChange> entries)
		=> Entries = Array.AsReadOnly([.. entries]);

	/// <summary>
	/// Gets the coalesced file-change entries.
	/// </summary>
	public IReadOnlyList<WorkspaceFileChange> Entries { get; }

	/// <summary>
	/// Gets the number of coalesced entries in the batch.
	/// </summary>
	public int Count => Entries.Count;
}
