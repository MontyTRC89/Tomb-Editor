namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Applies the shared version-acceptance rules used by Lua document caches.
/// </summary>
internal static class LuaDocumentVersionHelper
{
	/// <summary>
	/// Tries to accept an incoming document version relative to the current cached version.
	/// </summary>
	/// <param name="currentVersion">The version currently cached.</param>
	/// <param name="incomingVersion">The incoming version to evaluate.</param>
	/// <param name="acceptedVersion">The version that should remain cached after evaluation.</param>
	/// <returns><see langword="true"/> when the incoming version is acceptable; otherwise, <see langword="false"/>.</returns>
	internal static bool TryAccept(int currentVersion, int incomingVersion, out int acceptedVersion)
	{
		acceptedVersion = currentVersion;

		if (incomingVersion > 0 && currentVersion > 0 && incomingVersion < currentVersion)
			return false;

		if (incomingVersion > 0)
			acceptedVersion = incomingVersion;

		return true;
	}
}
