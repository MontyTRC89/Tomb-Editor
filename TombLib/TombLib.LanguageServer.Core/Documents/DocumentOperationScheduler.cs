namespace TombLib.LanguageServer.Core;

/// <summary>
/// Coordinates globally ordered operations, per-document ordered operations, and document updates where
/// only the most recently queued update per document remains active while older pending updates are canceled.
/// </summary>
public sealed partial class DocumentOperationScheduler
{
	// Queue tails per scheduling policy. All dictionaries are keyed by normalized document path so
	// global, per-document, latest-update, and exclusive-barrier policies can advance independently.
	private readonly object _syncRoot = new();
	private readonly Dictionary<string, Task> _queuedPerDocumentOperations = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, Task> _queuedLatestUpdateOperations = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, QueuedUpdateRegistration> _queuedDocumentUpdates = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<string, Task> _queuedPerDocumentBarriers = new(StringComparer.OrdinalIgnoreCase);
	private Task _queuedGlobalOperation = Task.CompletedTask;

	/// <summary>
	/// Awaits a queued operation while suppressing its failure so later operations can continue.
	/// </summary>
	/// <param name="previousOperation">The previously scheduled operation.</param>
	/// <returns>A task that completes after the previous operation settles.</returns>
	private static async Task WaitForQueuedOperationAsync(Task previousOperation)
	{
		try
		{
			await previousOperation.ConfigureAwait(false);
		}
		catch
		{ }
	}

	/// <summary>
	/// Awaits one or two queued operations while suppressing failures so later operations can continue.
	/// </summary>
	private static async Task WaitForQueuedOperationsAsync(Task firstOperation, Task secondOperation)
	{
		await WaitForQueuedOperationAsync(firstOperation).ConfigureAwait(false);

		if (ReferenceEquals(secondOperation, Task.CompletedTask) || ReferenceEquals(secondOperation, firstOperation))
			return;

		await WaitForQueuedOperationAsync(secondOperation).ConfigureAwait(false);
	}
}
