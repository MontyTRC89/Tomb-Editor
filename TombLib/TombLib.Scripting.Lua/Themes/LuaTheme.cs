using System.Collections.Generic;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.UI.Highlighting;

namespace TombLib.Scripting.Lua.Themes;

/// <summary>
/// Describes a Lua editor theme, including editor colors, semantic colors, and TextMate token colors.
/// </summary>
public sealed class LuaTheme
{
	/// <summary>
	/// Gets or sets the display name of the theme.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets additional names that can be used to resolve this theme.
	/// </summary>
	public List<string> Aliases { get; set; } = [];

	/// <summary>
	/// Gets or sets the editor background color.
	/// </summary>
	public string EditorBackground { get; set; } = LuaBuiltInThemes.DefaultEditorBackground;

	/// <summary>
	/// Gets or sets the editor foreground color.
	/// </summary>
	public string EditorForeground { get; set; } = LuaBuiltInThemes.DefaultEditorForeground;

	/// <summary>
	/// Gets or sets the TextMate token theme used for syntax highlighting.
	/// </summary>
	public TextMateTokenTheme TextMateTheme { get; set; } = new TextMateTokenTheme();

	/// <summary>
	/// Gets or sets the semantic color palette used for Lua editor features.
	/// </summary>
	public LuaThemeSemanticColors SemanticColors { get; set; } = new LuaThemeSemanticColors();

	/// <summary>
	/// Normalizes missing values so the theme can be used safely at runtime.
	/// </summary>
	/// <param name="fallbackName">The theme name to use when no explicit name was provided.</param>
	/// <returns>The current theme instance.</returns>
	public LuaTheme Normalize(string fallbackName)
	{
		if (string.IsNullOrWhiteSpace(Name))
			Name = fallbackName;

		if (string.IsNullOrWhiteSpace(EditorBackground))
			EditorBackground = LuaBuiltInThemes.DefaultEditorBackground;

		if (string.IsNullOrWhiteSpace(EditorForeground))
			EditorForeground = LuaBuiltInThemes.DefaultEditorForeground;

		Aliases ??= [];
		TextMateTheme ??= new TextMateTokenTheme();
		SemanticColors ??= new LuaThemeSemanticColors();
		SemanticColors.Normalize();

		return this;
	}
}

/// <summary>
/// Defines the semantic color slots used by the Lua editor UI.
/// </summary>
public sealed class LuaThemeSemanticColors
{
	/// <summary>
	/// Gets or sets the muted text color used for secondary completion details.
	/// </summary>
	public string MutedText { get; set; } = LuaBuiltInThemes.DefaultMutedText;

	/// <summary>
	/// Gets or sets the fallback color used for uncategorized symbols.
	/// </summary>
	public string Misc { get; set; } = LuaBuiltInThemes.DefaultMisc;

	/// <summary>
	/// Gets or sets the color used for methods and functions.
	/// </summary>
	public string Method { get; set; } = LuaBuiltInThemes.DefaultMethod;

	/// <summary>
	/// Gets or sets the color used for variables and parameters.
	/// </summary>
	public string Variable { get; set; } = LuaBuiltInThemes.DefaultVariable;

	/// <summary>
	/// Gets or sets the color used for properties and fields.
	/// </summary>
	public string Property { get; set; } = LuaBuiltInThemes.DefaultProperty;

	/// <summary>
	/// Gets or sets the color used for types and namespaces.
	/// </summary>
	public string Type { get; set; } = LuaBuiltInThemes.DefaultType;

	/// <summary>
	/// Gets or sets the color used for keywords.
	/// </summary>
	public string Keyword { get; set; } = LuaBuiltInThemes.DefaultKeyword;

	/// <summary>
	/// Gets or sets the color used for language-defined constants.
	/// </summary>
	public string LanguageConstant { get; set; } = LuaBuiltInThemes.DefaultLanguageConstant;

	/// <summary>
	/// Gets or sets the color used for user-defined constants and enum members.
	/// </summary>
	public string Constant { get; set; } = LuaBuiltInThemes.DefaultConstant;

	/// <summary>
	/// Gets or sets the color used for file and folder completion items.
	/// </summary>
	public string File { get; set; } = LuaBuiltInThemes.DefaultFile;

	/// <summary>
	/// Gets or sets the color used for signature-help documentation text.
	/// </summary>
	public string SignatureParameterDocumentation { get; set; } = LuaBuiltInThemes.DefaultSignatureParameterDocumentation;

	/// <summary>
	/// Gets or sets the color used to emphasize the active signature parameter.
	/// </summary>
	public string SignatureActiveParameter { get; set; } = LuaBuiltInThemes.DefaultSignatureActiveParameter;

	/// <summary>
	/// Gets or sets the base color used for signature labels.
	/// </summary>
	public string SignatureText { get; set; } = LuaBuiltInThemes.DefaultSignatureText;

	/// <summary>
	/// Replaces missing or whitespace-only color values with built-in defaults.
	/// </summary>
	public void Normalize()
	{
		MutedText = NormalizeValue(MutedText, LuaBuiltInThemes.DefaultMutedText);
		Misc = NormalizeValue(Misc, LuaBuiltInThemes.DefaultMisc);
		Method = NormalizeValue(Method, LuaBuiltInThemes.DefaultMethod);
		Variable = NormalizeValue(Variable, LuaBuiltInThemes.DefaultVariable);
		Property = NormalizeValue(Property, LuaBuiltInThemes.DefaultProperty);
		Type = NormalizeValue(Type, LuaBuiltInThemes.DefaultType);
		Keyword = NormalizeValue(Keyword, LuaBuiltInThemes.DefaultKeyword);
		LanguageConstant = NormalizeValue(LanguageConstant, LuaBuiltInThemes.DefaultLanguageConstant);
		Constant = NormalizeValue(Constant, LuaBuiltInThemes.DefaultConstant);
		File = NormalizeValue(File, LuaBuiltInThemes.DefaultFile);
		SignatureParameterDocumentation = NormalizeValue(SignatureParameterDocumentation, LuaBuiltInThemes.DefaultSignatureParameterDocumentation);
		SignatureActiveParameter = NormalizeValue(SignatureActiveParameter, LuaBuiltInThemes.DefaultSignatureActiveParameter);
		SignatureText = NormalizeValue(SignatureText, LuaBuiltInThemes.DefaultSignatureText);
	}

	private static string NormalizeValue(string value, string fallbackValue)
	{
		if (string.IsNullOrWhiteSpace(value))
			return fallbackValue;

		return value;
	}
}
