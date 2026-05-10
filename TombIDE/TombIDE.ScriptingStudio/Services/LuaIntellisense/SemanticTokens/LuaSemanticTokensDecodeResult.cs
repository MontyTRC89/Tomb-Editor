#nullable enable

using System.Collections.Generic;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Represents the decoded semantic tokens payload and whether the caller should retry with a full refresh.
/// </summary>
internal readonly record struct LuaSemanticTokensDecodeResult(
	IReadOnlyList<LuaSemanticToken> Tokens,
	int[]? Data,
	string? ResultId,
	bool RetryWithFullRefresh);