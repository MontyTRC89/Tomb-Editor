namespace TombLib.LanguageServer.Core;

/// <summary>
/// Forwards workspace file changes when the owner allows it, buffers recoverable delivery failures for replay,
/// and reports unexpected dropped batches with forwarding context.
/// </summary>
public sealed partial class WorkspaceFileChangeForwarder : IDisposable
{
	// Forwarding prerequisites.
	private readonly Func<bool> _canForwardAccessor;
	private readonly Func<bool> _isDisposedAccessor;
	private readonly Func<CancellationToken, Task<bool>> _ensureStartedAsync;
	private readonly Action _markTransportUnavailable;
	private readonly Action<WorkspaceFileForwardingFailure>? _logForwardingFailure;
	private readonly bool _bufferChangesWhileForwardingDisabled;

	// Forwarding and disposal lifecycle state. _disposeRequested blocks new work immediately,
	// while _disposed tracks when the forwarding gate has been released permanently.
	private readonly WorkspaceChangeAccumulator _deferredChanges = new();
	private readonly SemaphoreSlim _forwardingGate = new(1, 1);
	private readonly object _disposeSyncRoot = new();
	private int _activeOperationCount;
	private bool _disposeRequested;
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="WorkspaceFileChangeForwarder"/> class.
	/// </summary>
	/// <param name="canForwardAccessor">Reports whether forwarding attempts are currently allowed.</param>
	/// <param name="isDisposedAccessor">Reports whether the owner has been disposed.</param>
	/// <param name="ensureStartedAsync">Starts or validates the underlying transport before forwarding.</param>
	/// <param name="markTransportUnavailable">Marks the current transport as unavailable after recoverable forwarding failures.</param>
	/// <param name="logForwardingFailure">Logs forwarding failures together with batch context.</param>
	/// <param name="bufferChangesWhileForwardingDisabled">Whether changes should be buffered instead of dropped while forwarding is temporarily disallowed.</param>
	public WorkspaceFileChangeForwarder(
		Func<bool> canForwardAccessor,
		Func<bool> isDisposedAccessor,
		Func<CancellationToken, Task<bool>> ensureStartedAsync,
		Action markTransportUnavailable,
		Action<WorkspaceFileForwardingFailure>? logForwardingFailure = null,
		bool bufferChangesWhileForwardingDisabled = true)
	{
		ArgumentNullException.ThrowIfNull(canForwardAccessor);
		ArgumentNullException.ThrowIfNull(isDisposedAccessor);
		ArgumentNullException.ThrowIfNull(ensureStartedAsync);
		ArgumentNullException.ThrowIfNull(markTransportUnavailable);

		_canForwardAccessor = canForwardAccessor;
		_isDisposedAccessor = isDisposedAccessor;
		_ensureStartedAsync = ensureStartedAsync;
		_markTransportUnavailable = markTransportUnavailable;
		_logForwardingFailure = logForwardingFailure;
		_bufferChangesWhileForwardingDisabled = bufferChangesWhileForwardingDisabled;
	}
}
