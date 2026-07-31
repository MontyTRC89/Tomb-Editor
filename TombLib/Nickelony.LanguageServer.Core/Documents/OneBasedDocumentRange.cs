namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents a one-based document range derived from an LSP protocol range payload.
/// </summary>
/// <param name="StartLineNumber">The one-based start line number.</param>
/// <param name="StartColumnNumber">The one-based start column number.</param>
/// <param name="EndLineNumber">The one-based end line number.</param>
/// <param name="EndColumnNumber">The one-based end column number.</param>
public readonly record struct OneBasedDocumentRange(
	int StartLineNumber,
	int StartColumnNumber,
	int EndLineNumber,
	int EndColumnNumber);
