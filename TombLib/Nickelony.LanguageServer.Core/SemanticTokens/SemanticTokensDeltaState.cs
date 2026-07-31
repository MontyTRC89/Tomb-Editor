namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Represents the cached semantic tokens delta state used for incremental refresh requests.
/// </summary>
/// <param name="PreviousResultId">The previously cached result identifier returned by the server.</param>
/// <param name="PreviousData">The previously cached semantic token integer stream.</param>
public readonly record struct SemanticTokensDeltaState(string? PreviousResultId, int[]? PreviousData);
