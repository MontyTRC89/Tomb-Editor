namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Tracks editor-owned and request-owned references for a mirrored document.
/// This type is safe for concurrent callers.
/// </summary>
public sealed class DocumentReferenceTracker
{
	private int _openReferenceCount;
	private int _requestReferenceCount;

	/// <summary>
	/// Initializes a new instance of the <see cref="DocumentReferenceTracker"/> class.
	/// </summary>
	/// <param name="openReferenceCount">The initial number of open-editor references.</param>
	/// <param name="requestReferenceCount">The initial number of request-owned references.</param>
	public DocumentReferenceTracker(int openReferenceCount = 0, int requestReferenceCount = 0)
	{
		_openReferenceCount = Math.Max(0, openReferenceCount);
		_requestReferenceCount = Math.Max(0, requestReferenceCount);
	}

	/// <summary>
	/// Gets the number of open-editor references.
	/// </summary>
	public int OpenReferenceCount => Volatile.Read(ref _openReferenceCount);

	/// <summary>
	/// Gets the number of request-owned references.
	/// </summary>
	public int RequestReferenceCount => Volatile.Read(ref _requestReferenceCount);

	/// <summary>
	/// Gets a value indicating whether at least one open-editor reference is active.
	/// </summary>
	public bool HasOpenReferences => OpenReferenceCount > 0;

	/// <summary>
	/// Gets a value indicating whether no references remain.
	/// </summary>
	public bool IsIdle => OpenReferenceCount == 0 && RequestReferenceCount == 0;

	/// <summary>
	/// Adds one open-editor reference.
	/// </summary>
	public void AcquireOpen()
		=> Interlocked.Increment(ref _openReferenceCount);

	/// <summary>
	/// Adds one request-owned reference.
	/// </summary>
	public void AcquireRequest()
		=> Interlocked.Increment(ref _requestReferenceCount);

	/// <summary>
	/// Releases one open-editor reference.
	/// </summary>
	public void ReleaseOpen()
		=> DecrementIfPositive(ref _openReferenceCount);

	/// <summary>
	/// Releases one request-owned reference.
	/// </summary>
	public void ReleaseRequest()
		=> DecrementIfPositive(ref _requestReferenceCount);

	/// <summary>
	/// Decrements a reference count when it is still positive.
	/// </summary>
	/// <param name="referenceCount">The reference count field to update.</param>
	private static void DecrementIfPositive(ref int referenceCount)
	{
		while (true)
		{
			int currentValue = Volatile.Read(ref referenceCount);

			if (currentValue == 0)
				return;

			if (Interlocked.CompareExchange(ref referenceCount, currentValue - 1, currentValue) == currentValue)
				return;
		}
	}
}
