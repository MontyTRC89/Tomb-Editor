#nullable enable

using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Represents the outcome of synchronizing one document with the Lua language server.
/// </summary>
internal readonly record struct LuaDocumentSynchronizationResult(bool Success, LuaDocumentSnapshot? Document);