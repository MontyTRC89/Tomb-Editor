namespace TombLib.Scripting.Lua.LanguageServer;

/// <summary>
/// Represents the outcome of synchronizing one document with the Lua language server.
/// </summary>
public readonly record struct LuaDocumentSynchronizationResult(bool Success, LuaDocumentSnapshot? Document);
