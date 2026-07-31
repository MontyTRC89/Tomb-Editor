namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Lightweight value-type carrier for an in-flight document synchronization request.
/// </summary>
/// <param name="Kind">The synchronization action to perform.</param>
/// <param name="Document">The document snapshot to synchronize.</param>
/// <param name="ChangeRange">The incremental change range, when the synchronization is incremental.</param>
public readonly record struct DocumentSynchronizationRequest(
	DocumentSynchronizationKind Kind,
	DocumentSnapshot Document,
	DocumentChangeRange? ChangeRange = null);
