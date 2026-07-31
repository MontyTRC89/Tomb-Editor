namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Describes a language-server startup failure that should be surfaced to the UI.
/// </summary>
/// <param name="Message">The user-facing failure message.</param>
/// <param name="IsPersistent">Whether IntelliSense is disabled until the host application restarts.</param>
public readonly record struct LanguageServerStartupFailure(string Message, bool IsPersistent);
