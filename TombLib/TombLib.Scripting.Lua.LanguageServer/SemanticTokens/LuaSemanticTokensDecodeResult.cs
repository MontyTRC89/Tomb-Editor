using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents the decoded semantic tokens payload and whether the caller should retry with a full refresh.
/// </summary>
public readonly record struct LuaSemanticTokensDecodeResult(
	IReadOnlyList<LuaSemanticToken> Tokens,
	int[]? Data,
	string? ResultId,
	bool RetryWithFullRefresh);
