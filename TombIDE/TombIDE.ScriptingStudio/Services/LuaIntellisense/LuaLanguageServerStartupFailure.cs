#nullable enable

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Describes a Lua language server startup failure that should be surfaced to the UI.
/// </summary>
/// <param name="Message">The user-facing failure message.</param>
/// <param name="IsPersistent">Whether IntelliSense is disabled until TombIDE restarts.</param>
internal readonly record struct LuaLanguageServerStartupFailure(string Message, bool IsPersistent);
