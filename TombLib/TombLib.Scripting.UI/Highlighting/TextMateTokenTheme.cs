using System;
using System.Collections.Generic;

namespace TombLib.Scripting.UI.Highlighting;

/// <summary>
/// Describes a TextMate token theme as a collection of token rules.
/// </summary>
public sealed class TextMateTokenTheme
{
	private IReadOnlyList<TextMateTokenThemeRule> _rules = [];

	/// <summary>
	/// Gets or initializes the token rules of the theme.
	/// The assigned collection is copied into owned read-only storage; later caller mutations cannot leak in.
	/// </summary>
	public IReadOnlyList<TextMateTokenThemeRule> Rules
	{
		get => _rules;
		init => _rules = value is null ? [] : Array.AsReadOnly([.. value]);
	}
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
