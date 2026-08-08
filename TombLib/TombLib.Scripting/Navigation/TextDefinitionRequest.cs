namespace TombLib.Scripting.Navigation;

/// <summary>
/// Describes a definition lookup request against an immutable document snapshot.
/// </summary>
public sealed record TextDefinitionRequest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextDefinitionRequest"/> class.
	/// </summary>
	/// <param name="documentText">The current document snapshot text.</param>
	/// <param name="symbolName">The target symbol or object name.</param>
	/// <param name="identifier">An optional language-specific identifier that disambiguates the target.</param>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="documentText"/> or <paramref name="symbolName"/> is null.
	/// </exception>
	public TextDefinitionRequest(string documentText, string symbolName, object? identifier = null)
	{
		ArgumentNullException.ThrowIfNull(documentText);
		ArgumentNullException.ThrowIfNull(symbolName);

		DocumentText = documentText;
		SymbolName = symbolName;
		Identifier = identifier;
	}

	/// <summary>
	/// Gets the current document snapshot text.
	/// </summary>
	public string DocumentText { get; }

	/// <summary>
	/// Gets the target symbol or object name.
	/// </summary>
	public string SymbolName { get; }

	/// <summary>
	/// Gets the optional language-specific discriminator forwarded from the hover provider.
	/// This is an intentional, isolated escape hatch: providers pattern-match a known
	/// language-specific type (such as <c>ObjectType</c>) and must return no result for
	/// unsupported identifier types.
	/// </summary>
	public object? Identifier { get; }
}
