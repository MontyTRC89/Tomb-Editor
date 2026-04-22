#nullable enable

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

internal readonly record struct LuaLanguageServerStartupFailure(string Message, bool IsPersistent);
