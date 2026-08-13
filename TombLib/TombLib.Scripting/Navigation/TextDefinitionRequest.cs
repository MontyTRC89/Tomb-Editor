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
	/// <param name="identifier">An optional language-specific discriminator that disambiguates the target.</param>
	public TextDefinitionRequest(string documentText, string symbolName, TextDefinitionDiscriminator? identifier = null)
	{
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
	/// Gets the optional language-specific discriminator forwarded from the hover or outline provider.
	/// Providers that do not recognize the discriminator must return no result.
	/// </summary>
	public TextDefinitionDiscriminator? Identifier { get; }
}
