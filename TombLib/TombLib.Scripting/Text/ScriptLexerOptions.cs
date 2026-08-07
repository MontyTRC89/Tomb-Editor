namespace TombLib.Scripting.Text;

/// <summary>
/// Specifies language-neutral options for the <see cref="ScriptLexer"/>.
/// </summary>
public readonly struct ScriptLexerOptions
{
	/// <summary>
	/// Gets the comment delimiter, e.g. <c>";"</c> for ClassicScript or <c>"//"</c> for GameFlowScript.
	/// </summary>
	public string CommentDelimiter { get; init; }

	/// <summary>
	/// Gets the continuation marker character, e.g. <c>'>'</c> for ClassicScript.
	/// Set to <c>'\0'</c> when the language has no continuation marker.
	/// </summary>
	public char ContinuationMarker { get; init; }

	/// <summary>
	/// Gets the directive prefix character, e.g. <c>'#'</c>.
	/// Set to <c>'\0'</c> when the language has no directive prefix.
	/// </summary>
	public char DirectivePrefix { get; init; }

	/// <summary>
	/// Gets the section-header opening bracket, e.g. <c>'['</c>.
	/// Set to <c>'\0'</c> when the language has no section headers.
	/// </summary>
	public char SectionOpenBracket { get; init; }

	/// <summary>
	/// Gets the section-header closing bracket, e.g. <c>']'</c>.
	/// </summary>
	public char SectionCloseBracket { get; init; }

	/// <summary>
	/// Gets the hexadecimal value prefix character, e.g. <c>'$'</c>.
	/// Set to <c>'\0'</c> when the language has no hex prefix.
	/// </summary>
	public char HexPrefix { get; init; }

	/// <summary>
	/// Gets the character used to delimit string literals, e.g. <c>'"'</c>.
	/// </summary>
	public char StringQuote { get; init; }

	/// <summary>
	/// Returns the default options for ClassicScript: <c>;</c> comments,
	/// <c>&gt;</c> continuations, <c>#</c> directives, <c>[…]</c> sections,
	/// <c>$</c> hex values, and <c>"</c> quoted strings.
	/// </summary>
	public static ScriptLexerOptions ClassicScript => new()
	{
		CommentDelimiter = ";",
		ContinuationMarker = '>',
		DirectivePrefix = '#',
		SectionOpenBracket = '[',
		SectionCloseBracket = ']',
		HexPrefix = '$',
		StringQuote = '"'
	};
}
