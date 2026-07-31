namespace Nickelony.LanguageServer.Lua;

/// <summary>
/// Represents a single semantic token produced by the Lua language service.
/// </summary>
public sealed class LuaSemanticToken
{
	/// <summary>
	/// Initializes a new instance of the <see cref="LuaSemanticToken"/> class.
	/// </summary>
	/// <param name="line">The zero-based line index.</param>
	/// <param name="character">The zero-based character index within the line.</param>
	/// <param name="length">The token length in characters.</param>
	/// <param name="type">The semantic token type.</param>
	/// <param name="modifiers">The semantic token modifiers.</param>
	public LuaSemanticToken(int line, int character, int length, string type, IReadOnlyList<string> modifiers)
	{
		Line = Math.Max(0, line);
		Character = Math.Max(0, character);
		Length = Math.Max(0, length);
		Type = type;
		Modifiers = modifiers;
	}

	/// <summary>
	/// Gets the zero-based line index containing the token.
	/// </summary>
	public int Line { get; }

	/// <summary>
	/// Gets the zero-based character index of the token within its line.
	/// </summary>
	public int Character { get; }

	/// <summary>
	/// Gets the token length in characters.
	/// </summary>
	public int Length { get; }

	/// <summary>
	/// Gets the semantic token type.
	/// </summary>
	public string Type { get; }

	/// <summary>
	/// Gets the semantic token modifiers.
	/// </summary>
	public IReadOnlyList<string> Modifiers { get; }

	/// <summary>
	/// Determines whether the token has the specified modifier.
	/// </summary>
	/// <param name="modifier">The modifier name to check.</param>
	/// <returns><see langword="true"/> if the modifier is present; otherwise, <see langword="false"/>.</returns>
	public bool HasModifier(string modifier)
	{
		if (string.IsNullOrWhiteSpace(modifier) || Modifiers.Count == 0)
			return false;

		for (int i = 0; i < Modifiers.Count; i++)
		{
			if (string.Equals(Modifiers[i], modifier, StringComparison.Ordinal))
				return true;
		}

		return false;
	}
}
