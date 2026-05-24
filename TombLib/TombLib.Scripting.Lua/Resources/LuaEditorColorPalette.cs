using System.Windows.Media;
using TombLib.Scripting.Lua.Themes;
using TombLib.Scripting.UI.Resources;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.Lua.Resources;

/// <summary>
/// Builds the frozen brush palette used by the Lua editor from a resolved theme definition.
/// </summary>
internal static class LuaEditorColorPalette
{
	/// <summary>
	/// Creates the editor brush set for the supplied Lua theme.
	/// </summary>
	/// <param name="theme">The theme to materialize into brushes.</param>
	/// <returns>A frozen brush set ready for use by the editor UI.</returns>
	public static LuaThemeBrushSet Create(LuaTheme theme)
	{
		LuaTheme effectiveTheme = (theme ?? new LuaTheme()).Normalize(ConfigurationDefaults.SelectedThemeName);
		LuaThemeSemanticColors semanticColors = effectiveTheme.SemanticColors;

		return new LuaThemeBrushSet(
			effectiveTheme.Name,
			CreateBrush(effectiveTheme.EditorBackground, LuaBuiltInThemes.DefaultEditorBackground),
			CreateBrush(effectiveTheme.EditorForeground, LuaBuiltInThemes.DefaultEditorForeground),
			CreateBrush(semanticColors.MutedText, LuaBuiltInThemes.DefaultMutedText),
			CreateBrush(semanticColors.Misc, LuaBuiltInThemes.DefaultMisc),
			CreateBrush(semanticColors.Method, LuaBuiltInThemes.DefaultMethod),
			CreateBrush(semanticColors.Variable, LuaBuiltInThemes.DefaultVariable),
			CreateBrush(semanticColors.Property, LuaBuiltInThemes.DefaultProperty),
			CreateBrush(semanticColors.Type, LuaBuiltInThemes.DefaultType),
			CreateBrush(semanticColors.Keyword, LuaBuiltInThemes.DefaultKeyword),
			CreateBrush(semanticColors.LanguageConstant, LuaBuiltInThemes.DefaultLanguageConstant),
			CreateBrush(semanticColors.Constant, LuaBuiltInThemes.DefaultConstant),
			CreateBrush(semanticColors.File, LuaBuiltInThemes.DefaultFile),
			CreateBrush(semanticColors.SignatureParameterDocumentation, LuaBuiltInThemes.DefaultSignatureParameterDocumentation),
			CreateBrush(semanticColors.SignatureActiveParameter, LuaBuiltInThemes.DefaultSignatureActiveParameter),
			CreateBrush(semanticColors.SignatureText, ColorToString(TextEditorColorPalette.ToolTipForeground.Color)));
	}

	private static SolidColorBrush CreateBrush(string colorValue, string fallbackColorValue)
	{
		try
		{
			string effectiveColorValue = string.IsNullOrWhiteSpace(colorValue)
				? fallbackColorValue
				: colorValue;

			return CreateFrozenBrush(effectiveColorValue);
		}
		catch
		{
			return CreateFrozenBrush(fallbackColorValue);
		}
	}

	private static string ColorToString(Color color)
		=> $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
