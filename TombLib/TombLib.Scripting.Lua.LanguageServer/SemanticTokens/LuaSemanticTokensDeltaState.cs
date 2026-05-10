namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents the cached semantic-tokens delta state used for incremental refresh requests.
/// </summary>
public readonly record struct LuaSemanticTokensDeltaState(string? PreviousResultId, int[]? PreviousData);
