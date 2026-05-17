using TombLib.Scripting.Objects;

namespace TombLib.LanguageServer.Lua;

internal sealed class LuaDocumentDiagnosticsCache
{
	public IReadOnlyList<TextEditorDiagnostic> Diagnostics { get; private set; } = [];

	public int Version { get; private set; }

	public void Clear()
	{
		Diagnostics = [];
		Version = 0;
	}

	public bool TryStore(LuaPublishedDiagnostics publishedDiagnostics)
	{
		if (IsStaleVersion(Version, publishedDiagnostics.Version))
			return false;

		if (publishedDiagnostics.Version > 0)
			Version = publishedDiagnostics.Version;

		Diagnostics = publishedDiagnostics.Diagnostics;
		return true;
	}

	private static bool IsStaleVersion(int currentVersion, int incomingVersion)
		=> incomingVersion > 0 && currentVersion > 0 && incomingVersion < currentVersion;
}
