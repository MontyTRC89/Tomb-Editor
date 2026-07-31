namespace Nickelony.LanguageServer.Core;

public sealed partial class WorkspaceFileChangeForwarder
{
	/// <summary>
	/// Reports whether disposal has already been requested.
	/// </summary>
	/// <returns><see langword="true"/> when new forwarding work should stop; otherwise, <see langword="false"/>.</returns>
	private bool IsDisposeRequested()
	{
		lock (_disposeSyncRoot)
			return _disposeRequested;
	}

	/// <summary>
	/// Releases the owned synchronization gate once no forwarding operations remain active.
	/// </summary>
	public void Dispose()
	{
		bool shouldDisposeForwardingGate = false;

		lock (_disposeSyncRoot)
		{
			if (_disposeRequested)
				return;

			_disposeRequested = true;
			shouldDisposeForwardingGate = TryMarkDisposedUnderLock();
		}

		if (shouldDisposeForwardingGate)
			_forwardingGate.Dispose();
	}

	/// <summary>
	/// Marks one forwarding operation active unless disposal has already started.
	/// </summary>
	/// <returns><see langword="true"/> when the caller may proceed; otherwise, <see langword="false"/>.</returns>
	private bool TryEnterOperation()
	{
		lock (_disposeSyncRoot)
		{
			if (_disposeRequested)
				return false;

			_activeOperationCount++;
			return true;
		}
	}

	private void ExitOperation()
	{
		bool shouldDisposeForwardingGate = false;

		lock (_disposeSyncRoot)
		{
			_activeOperationCount--;
			shouldDisposeForwardingGate = TryMarkDisposedUnderLock();
		}

		if (shouldDisposeForwardingGate)
			_forwardingGate.Dispose();
	}

	/// <summary>
	/// Marks the forwarder disposed once disposal was requested and no forwarding operations remain active.
	/// The caller must hold <see cref="_disposeSyncRoot"/>.
	/// </summary>
	/// <returns><see langword="true"/> when the forwarding gate should be disposed now.</returns>
	private bool TryMarkDisposedUnderLock()
	{
		if (!_disposeRequested || _disposed || _activeOperationCount != 0)
			return false;

		_disposed = true;
		return true;
	}
}
