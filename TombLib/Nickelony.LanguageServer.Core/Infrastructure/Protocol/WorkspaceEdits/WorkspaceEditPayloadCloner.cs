using System.Collections.ObjectModel;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Clones workspace-edit payload collections into defensive read-only snapshots.
/// </summary>
internal static class WorkspaceEditPayloadCloner
{
	/// <summary>
	/// Clones the simple URI-to-edit map into a detached read-only snapshot.
	/// </summary>
	/// <param name="changes">The change map to clone.</param>
	/// <returns>The cloned change map, or <see langword="null"/> when no change map was provided.</returns>
	public static IReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>? CloneChangeMap(
		IReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>? changes)
	{
		if (changes is null)
			return null;

		var clonedChanges = new Dictionary<string, IReadOnlyList<TextEditPayload>?>(changes.Count, StringComparer.Ordinal);

		foreach ((string uri, IReadOnlyList<TextEditPayload>? edits) in changes)
			clonedChanges[uri] = CloneEditList(edits);

		return new ReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>(clonedChanges);
	}

	/// <summary>
	/// Clones the structured document-change list into a detached read-only snapshot.
	/// </summary>
	/// <param name="documentChanges">The document changes to clone.</param>
	/// <returns>The cloned document-change list, or <see langword="null"/> when no document changes were provided.</returns>
	public static IReadOnlyList<WorkspaceDocumentChangePayload>? CloneDocumentChanges(IReadOnlyList<WorkspaceDocumentChangePayload>? documentChanges)
		=> documentChanges is null ? null : Array.AsReadOnly([.. documentChanges]);

	/// <summary>
	/// Clones a text-edit list into a detached read-only snapshot.
	/// </summary>
	/// <param name="edits">The text edits to clone.</param>
	/// <returns>The cloned text-edit list, or <see langword="null"/> when no edits were provided.</returns>
	public static IReadOnlyList<TextEditPayload>? CloneEditList(IReadOnlyList<TextEditPayload>? edits)
		=> edits is null ? null : Array.AsReadOnly([.. edits]);
}
