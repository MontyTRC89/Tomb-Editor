namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the outcome of synchronizing one document with the language server.
/// </summary>
/// <param name="Success">Whether the synchronization completed successfully.</param>
/// <param name="Document">The synchronized document snapshot, when one remains tracked.</param>
public readonly record struct DocumentSynchronizationResult(bool Success, DocumentSnapshot? Document);
