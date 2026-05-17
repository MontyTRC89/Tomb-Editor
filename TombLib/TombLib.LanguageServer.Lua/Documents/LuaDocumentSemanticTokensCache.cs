using TombLib.Scripting.Lua.Objects;

namespace TombLib.LanguageServer.Lua;

internal sealed class LuaDocumentSemanticTokensCache
{
	public IReadOnlyList<LuaSemanticToken> Tokens { get; private set; } = [];

	public int Version { get; private set; }

	public int[]? PreviousData { get; private set; }

	public string? PreviousResultId { get; private set; }

	public void Clear()
	{
		Tokens = [];
		Version = 0;
		PreviousData = null;
		PreviousResultId = null;
	}

	public SemanticTokensDeltaState GetDeltaState()
		=> new(PreviousResultId, PreviousData);

	public void InvalidateServerSynchronization()
	{
		Version = 0;
		PreviousData = null;
		PreviousResultId = null;
	}

	public void StoreDeltaState(string? resultId, int[]? data)
	{
		PreviousResultId = resultId;
		PreviousData = data;
	}

	public bool TryStore(int version, IReadOnlyList<LuaSemanticToken> semanticTokens)
	{
		if (IsStaleVersion(Version, version))
			return false;

		if (version > 0)
			Version = version;

		Tokens = semanticTokens ?? [];
		return true;
	}

	private static bool IsStaleVersion(int currentVersion, int incomingVersion)
		=> incomingVersion > 0 && currentVersion > 0 && incomingVersion < currentVersion;
}
