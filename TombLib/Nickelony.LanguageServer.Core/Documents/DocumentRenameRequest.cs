namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Describes a tracked-document path rekey that may need to be mirrored to the language server as a close/open pair.
/// </summary>
/// <param name="PreviousDocument">The prior tracked document snapshot, when one exists.</param>
/// <param name="RenamedDocument">The renamed tracked document snapshot.</param>
/// <param name="ReopenServerDocument">Whether the rename should be mirrored as a close/open document cycle.</param>
public readonly record struct DocumentRenameRequest(
	DocumentSnapshot? PreviousDocument,
	DocumentSnapshot RenamedDocument,
	bool ReopenServerDocument);
