using System.Text.Json.Serialization;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the typed top-level workspace edit response used by rename.
/// </summary>
public readonly record struct WorkspaceEditResponse
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WorkspaceEditResponse"/> struct.
	/// </summary>
	/// <param name="changes">The simple URI-to-edit map returned by the server.</param>
	/// <param name="documentChanges">The structured document-change payload returned by the server.</param>
	[JsonConstructor]
	public WorkspaceEditResponse(
		IReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>? changes,
		IReadOnlyList<WorkspaceDocumentChangePayload>? documentChanges)
	{
		Changes = WorkspaceEditPayloadCloner.CloneChangeMap(changes);
		DocumentChanges = WorkspaceEditPayloadCloner.CloneDocumentChanges(documentChanges);
	}

	/// <summary>
	/// Gets the simple URI-to-edit map returned by the server.
	/// The returned dictionary and nested edit lists are defensive read-only snapshots.
	/// </summary>
	[JsonPropertyName("changes")]
	public IReadOnlyDictionary<string, IReadOnlyList<TextEditPayload>?>? Changes { get; }

	/// <summary>
	/// Gets the structured document-change payload returned by the server.
	/// The returned list is a defensive read-only snapshot.
	/// </summary>
	[JsonPropertyName("documentChanges")]
	public IReadOnlyList<WorkspaceDocumentChangePayload>? DocumentChanges { get; }
}
