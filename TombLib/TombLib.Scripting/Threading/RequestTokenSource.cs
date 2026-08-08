namespace TombLib.Scripting.Threading;

/// <summary>
/// Provides monotonically-increasing request tokens used to invalidate stale asynchronous results.
/// </summary>
public sealed class RequestTokenSource
{
	private int _currentToken;

	/// <summary>
	/// Begins a new request and returns its token.
	/// </summary>
	public int Begin()
		=> Interlocked.Increment(ref _currentToken);

	/// <summary>
	/// Invalidates all outstanding requests so their tokens are no longer current.
	/// </summary>
	public void Invalidate()
		=> Interlocked.Increment(ref _currentToken);

	/// <summary>
	/// Determines whether the supplied token belongs to the most recent request.
	/// </summary>
	public bool IsCurrent(int token)
		=> token == Volatile.Read(ref _currentToken);
}
