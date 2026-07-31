namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Captures the current synchronized state of a tracked document.
/// </summary>
public sealed class DocumentSnapshot
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DocumentSnapshot"/> class.
	/// </summary>
	/// <param name="filePath">The normalized local file path.</param>
	/// <param name="uri">The corresponding file URI sent to the language server.</param>
	/// <param name="content">The current document text.</param>
	/// <param name="version">The local synchronization version.</param>
	public DocumentSnapshot(string filePath, string uri, string? content, int version)
	{
		FilePath = filePath;
		Uri = uri;
		Content = content ?? string.Empty;
		Version = version;
	}

	/// <summary>
	/// Gets the normalized local file path.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the file URI sent to the language server for this document.
	/// </summary>
	public string Uri { get; }

	/// <summary>
	/// Gets the current document text.
	/// </summary>
	public string Content { get; }

	/// <summary>
	/// Gets the local synchronization version.
	/// </summary>
	public int Version { get; }
}
