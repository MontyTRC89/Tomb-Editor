using TombLib.Scripting.Objects;

namespace TombLib.LanguageServer.Lua;

/// <summary>
/// Caches the latest non-stale diagnostics payload published for a tracked Lua document.
/// </summary>
internal sealed class LuaDiagnosticsCache
{
	/// <summary>
	/// Gets the currently cached diagnostics.
	/// </summary>
	public IReadOnlyList<TextEditorDiagnostic> Diagnostics { get; private set; } = [];

	/// <summary>
	/// Gets the synchronized document version associated with the cached diagnostics.
	/// </summary>
	public int Version { get; private set; }

	/// <summary>
	/// Clears the cached diagnostics state.
	/// </summary>
	public void Clear()
	{
		Diagnostics = [];
		Version = 0;
	}

	/// <summary>
	/// Stores a diagnostics payload when its version is not stale relative to the current cache.
	/// </summary>
	/// <param name="publishedDiagnostics">The diagnostics payload to cache.</param>
	/// <returns><see langword="true"/> when the payload was stored; otherwise, <see langword="false"/>.</returns>
	public bool TryStore(LuaPublishedDiagnostics publishedDiagnostics)
	{
		if (!LuaDocumentVersionHelper.TryAccept(Version, publishedDiagnostics.Version, out int acceptedVersion))
			return false;

		Version = acceptedVersion;

		Diagnostics = publishedDiagnostics.Diagnostics;
		return true;
	}
}
