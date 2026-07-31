namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Stores the neutral mirrored state for a tracked language-server document.
/// This type is safe for concurrent reads and mutations of its core tracked document fields.
/// Derived types should still synchronize any additional mutable state they introduce.
/// </summary>
public abstract class TrackedDocumentState
{
	private readonly object _stateSyncRoot = new();

	private string _filePath;
	private string _uri;
	private string _content;
	private int _version;
	private bool _isOpen;
	private long _lastAccessStamp;

	/// <summary>
	/// Initializes a new instance of the <see cref="TrackedDocumentState"/> class.
	/// </summary>
	/// <param name="filePath">The normalized tracked file path.</param>
	/// <param name="uri">The file URI mirrored to the language server.</param>
	/// <param name="content">The latest synchronized document content.</param>
	/// <param name="version">The tracked document version.</param>
	/// <param name="isOpen">Whether the server currently considers the document open.</param>
	/// <param name="openReferenceCount">The initial number of open-editor references.</param>
	/// <param name="requestReferenceCount">The initial number of request-owned references.</param>
	/// <param name="lastAccessStamp">The access stamp used for request-only eviction ordering.</param>
	public TrackedDocumentState(
		string filePath,
		string uri,
		string content,
		int version,
		bool isOpen,
		int openReferenceCount,
		int requestReferenceCount,
		long lastAccessStamp)
	{
		_filePath = filePath;
		_uri = uri;
		_content = content;
		_version = version;
		_isOpen = isOpen;
		_lastAccessStamp = lastAccessStamp;

		References = new DocumentReferenceTracker(openReferenceCount, requestReferenceCount);
	}

	/// <summary>
	/// Gets the normalized file path.
	/// </summary>
	public string FilePath
	{
		get
		{
			lock (_stateSyncRoot)
				return _filePath;
		}
	}

	/// <summary>
	/// Gets the file URI mirrored to the server.
	/// </summary>
	public string Uri
	{
		get
		{
			lock (_stateSyncRoot)
				return _uri;
		}
	}

	/// <summary>
	/// Gets the latest synchronized content.
	/// </summary>
	public string Content
	{
		get
		{
			lock (_stateSyncRoot)
				return _content;
		}
	}

	/// <summary>
	/// Gets the current tracked version.
	/// </summary>
	public int Version
	{
		get
		{
			lock (_stateSyncRoot)
				return _version;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the server currently considers the document open.
	/// </summary>
	public bool IsOpen
	{
		get
		{
			lock (_stateSyncRoot)
				return _isOpen;
		}
	}

	/// <summary>
	/// Gets the access stamp used for request-only eviction ordering.
	/// </summary>
	public long LastAccessStamp
	{
		get
		{
			lock (_stateSyncRoot)
				return _lastAccessStamp;
		}
	}

	/// <summary>
	/// Gets the active ownership references for the document.
	/// </summary>
	public DocumentReferenceTracker References { get; }

	/// <summary>
	/// Creates a stable snapshot of the current tracked document state.
	/// </summary>
	/// <returns>The current document snapshot.</returns>
	public DocumentSnapshot CreateSnapshot()
	{
		lock (_stateSyncRoot)
			return new DocumentSnapshot(_filePath, _uri, _content, _version);
	}

	/// <summary>
	/// Updates the request-only eviction ordering stamp.
	/// </summary>
	/// <param name="lastAccessStamp">The new access stamp.</param>
	protected void SetLastAccessStamp(long lastAccessStamp)
	{
		lock (_stateSyncRoot)
			_lastAccessStamp = lastAccessStamp;
	}

	/// <summary>
	/// Reopens the tracked server document with fresh content and a new version.
	/// </summary>
	/// <param name="content">The reopened content.</param>
	protected void ReopenDocument(string content)
	{
		lock (_stateSyncRoot)
		{
			_content = content;
			_version++;
			_isOpen = true;
		}
	}

	/// <summary>
	/// Replaces the tracked content and advances the version.
	/// </summary>
	/// <param name="content">The replacement content.</param>
	/// <returns>The previous content snapshot.</returns>
	protected string ReplaceContent(string content)
	{
		lock (_stateSyncRoot)
		{
			string previousContent = _content;
			_content = content;
			_version++;

			return previousContent;
		}
	}

	/// <summary>
	/// Replaces the tracked path and URI after a rename.
	/// </summary>
	/// <param name="filePath">The normalized replacement file path.</param>
	/// <param name="uri">The replacement file URI.</param>
	protected void RenameDocument(string filePath, string uri)
	{
		lock (_stateSyncRoot)
		{
			_filePath = filePath;
			_uri = uri;
		}
	}

	/// <summary>
	/// Marks the mirrored server document as closed while keeping the cached state alive.
	/// </summary>
	protected void MarkDocumentClosed()
	{
		lock (_stateSyncRoot)
			_isOpen = false;
	}
}
