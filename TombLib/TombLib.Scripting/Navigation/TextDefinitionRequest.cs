namespace TombLib.Scripting.Navigation;

/// <summary>
/// Describes a definition lookup request against the current document.
/// </summary>
public sealed class TextDefinitionRequest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TextDefinitionRequest"/> class.
	/// </summary>
	/// <param name="documentText">The current document content.</param>
	/// <param name="symbolName">The target symbol or object name.</param>
	/// <param name="identifier">An optional language-specific identifier that disambiguates the target.</param>
	public TextDefinitionRequest(string documentText, string symbolName, object? identifier = null)
	{
		DocumentText = documentText;
		SymbolName = symbolName;
		Identifier = identifier;
	}

	/// <summary>
	/// Gets the current document content.
	/// </summary>
	public string DocumentText { get; }

	/// <summary>
	/// Gets the target symbol or object name.
	/// </summary>
	public string SymbolName { get; }

	/// <summary>
	/// Gets the optional language-specific identifier.
	/// </summary>
	public object? Identifier { get; }
}
