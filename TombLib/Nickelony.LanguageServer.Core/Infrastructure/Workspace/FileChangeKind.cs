namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Identifies the file-system change kind reported to the language server.
/// </summary>
public enum FileChangeKind
{
	/// <summary>
	/// A file or directory was created.
	/// </summary>
	Created = 1,

	/// <summary>
	/// A file or directory changed in place.
	/// </summary>
	Changed = 2,

	/// <summary>
	/// A file or directory was deleted.
	/// </summary>
	Deleted = 3
}
