using System.Collections.Generic;

namespace TombLib.Scripting.UI.Highlighting;

/// <summary>
/// Describes a TextMate token theme as a collection of token rules.
/// </summary>
public sealed class TextMateTokenTheme
{
	/// <summary>
	/// Gets or sets the token rules of the theme.
	/// </summary>
	public List<TextMateTokenThemeRule> Rules { get; set; } = new List<TextMateTokenThemeRule>();
}

/// <summary>
/// Describes the style applied to a TextMate token scope.
/// </summary>
public sealed class TextMateTokenThemeRule
{
	/// <summary>
	/// Gets or sets the TextMate scope selector of the rule.
	/// </summary>
	public string Scope { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the foreground color of the rule.
	/// </summary>
	public string Foreground { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the font style of the rule, such as <c>bold</c> or <c>italic</c>.
	/// </summary>
	public string FontStyle { get; set; } = string.Empty;
}
