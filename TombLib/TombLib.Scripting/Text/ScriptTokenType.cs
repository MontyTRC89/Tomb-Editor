namespace TombLib.Scripting.Text;

/// <summary>
/// Enumerates the structural token types produced by <see cref="ScriptLexer"/>.
/// These describe syntactic form, not semantic meaning.
/// </summary>
public enum ScriptTokenType
{
	/// <summary>Unrecognized character sequence.</summary>
	Unknown,

	/// <summary>A line comment including the delimiter and all text through end of line.</summary>
	Comment,

	/// <summary>A section header delimited by brackets, e.g. <c>[Level]</c>.</summary>
	SectionHeader,

	/// <summary>A directive keyword starting with a prefix character, e.g. <c>#INCLUDE</c>.</summary>
	Directive,

	/// <summary>A general identifier (word).</summary>
	Identifier,

	/// <summary>
	/// An identifier that is composed entirely of uppercase letters, digits and underscores
	/// and contains at least one underscore, e.g. <c>CUST_BEHAVIOR</c>.
	/// </summary>
	MnemonicLikeIdentifier,

	/// <summary>A hexadecimal literal prefixed with <c>$</c>, e.g. <c>$1A2B</c>.</summary>
	HexValue,

	/// <summary>A sequence of decimal digits.</summary>
	DecimalValue,

	/// <summary>A double-quoted string literal, including the quotes.</summary>
	StringLiteral,

	/// <summary>A line-continuation marker (<c>&gt;</c>) appearing at the end of a code line.</summary>
	ContinuationMarker,

	/// <summary>The <c>=</c> character.</summary>
	Equals,

	/// <summary>The <c>,</c> character.</summary>
	Comma,

	/// <summary>The <c>+</c> character.</summary>
	Plus,

	/// <summary>The <c>-</c> character.</summary>
	Minus,

	/// <summary>The <c>*</c> character.</summary>
	Asterisk,

	/// <summary>The <c>/</c> character.</summary>
	Slash,

	/// <summary>The <c>(</c> character.</summary>
	OpenParen,

	/// <summary>The <c>)</c> character.</summary>
	CloseParen,

	/// <summary>A run of spaces or tabs.</summary>
	Whitespace
}
