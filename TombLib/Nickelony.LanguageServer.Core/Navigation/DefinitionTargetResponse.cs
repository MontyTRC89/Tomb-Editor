namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents one concrete definition target returned by a language server.
/// </summary>
/// <param name="Uri">The target document URI.</param>
/// <param name="LineNumber">The one-based target line number.</param>
/// <param name="ColumnNumber">The one-based target column number.</param>
public readonly record struct DefinitionTargetResponse(string? Uri, int LineNumber, int ColumnNumber);
