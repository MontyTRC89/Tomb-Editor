#nullable enable

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Represents the cached semantic-tokens delta state used for incremental refresh requests.
/// </summary>
internal readonly record struct LuaSemanticTokensDeltaState(string? PreviousResultId, int[]? PreviousData);