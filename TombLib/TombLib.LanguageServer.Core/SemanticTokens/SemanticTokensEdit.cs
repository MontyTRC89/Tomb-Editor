namespace TombLib.LanguageServer.Core;

/// <summary>
/// Represents a single edit against a cached semantic token integer stream.
/// </summary>
/// <param name="Start">The zero-based start index within the cached token data.</param>
/// <param name="DeleteCount">The number of integers to remove starting at <paramref name="Start"/>.</param>
/// <param name="Data">The replacement integer payload to insert.</param>
public readonly record struct SemanticTokensEdit(int Start, int DeleteCount, int[] Data);
