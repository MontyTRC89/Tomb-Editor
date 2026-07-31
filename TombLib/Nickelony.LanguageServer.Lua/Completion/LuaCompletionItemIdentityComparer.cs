namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Compares parsed completion items using the case-sensitive identity rules required by Lua identifiers.
/// </summary>
internal sealed class LuaCompletionItemIdentityComparer : IEqualityComparer<LuaCompletionItemIdentity>
{
	/// <summary>
	/// Gets the shared comparer instance.
	/// </summary>
	internal static LuaCompletionItemIdentityComparer Instance { get; } = new();

	public bool Equals(LuaCompletionItemIdentity x, LuaCompletionItemIdentity y)
		=> StringComparer.Ordinal.Equals(x.Label, y.Label)
		&& StringComparer.Ordinal.Equals(x.InsertText, y.InsertText)
		&& StringComparer.Ordinal.Equals(x.FilterText, y.FilterText)
		&& StringComparer.Ordinal.Equals(x.Detail, y.Detail)
		&& StringComparer.Ordinal.Equals(x.Description, y.Description)
		&& x.Kind == y.Kind
		&& x.TextEdit.Equals(y.TextEdit);

	public int GetHashCode(LuaCompletionItemIdentity value)
	{
		return HashCode.Combine(
			StringComparer.Ordinal.GetHashCode(value.Label),
			StringComparer.Ordinal.GetHashCode(value.InsertText),
			StringComparer.Ordinal.GetHashCode(value.FilterText),
			StringComparer.Ordinal.GetHashCode(value.Detail),
			StringComparer.Ordinal.GetHashCode(value.Description),
			value.Kind,
			value.TextEdit);
	}
}
