using System.Windows.Media;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Resources;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.Lua.Resources
{
	internal static class LuaEditorColorPalette
	{
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

	internal sealed class LuaThemeBrushSet
	{
		public LuaThemeBrushSet(
			string themeName,
			SolidColorBrush editorBackground,
			SolidColorBrush editorForeground,
			SolidColorBrush mutedText,
			SolidColorBrush misc,
			SolidColorBrush method,
			SolidColorBrush variable,
			SolidColorBrush property,
			SolidColorBrush type,
			SolidColorBrush keyword,
			SolidColorBrush languageConstant,
			SolidColorBrush constant,
			SolidColorBrush file,
			SolidColorBrush signatureParamDoc,
			SolidColorBrush signatureActiveParam,
			SolidColorBrush signatureForeground)
		{
			ThemeName = themeName;
			EditorBackground = editorBackground;
			EditorForeground = editorForeground;
			MutedTextBrush = mutedText;
			MiscBrush = misc;
			MethodBrush = method;
			VariableBrush = variable;
			PropertyBrush = property;
			TypeBrush = type;
			KeywordBrush = keyword;
			LanguageConstantBrush = languageConstant;
			ConstantBrush = constant;
			FileBrush = file;
			SignatureParamDocForeground = signatureParamDoc;
			SignatureActiveParamForeground = signatureActiveParam;
			SignatureForeground = signatureForeground;
		}

		public string ThemeName { get; }
		public SolidColorBrush EditorBackground { get; }
		public SolidColorBrush EditorForeground { get; }
		public SolidColorBrush MutedTextBrush { get; }
		public SolidColorBrush MiscBrush { get; }
		public SolidColorBrush MethodBrush { get; }
		public SolidColorBrush VariableBrush { get; }
		public SolidColorBrush PropertyBrush { get; }
		public SolidColorBrush TypeBrush { get; }
		public SolidColorBrush KeywordBrush { get; }
		public SolidColorBrush LanguageConstantBrush { get; }
		public SolidColorBrush ConstantBrush { get; }
		public SolidColorBrush FileBrush { get; }
		public SolidColorBrush SignatureParamDocForeground { get; }
		public SolidColorBrush SignatureActiveParamForeground { get; }
		public SolidColorBrush SignatureForeground { get; }

		public Brush GetCompletionItemBrush(Objects.LuaCompletionIconKind kind)
		{
			return kind switch
			{
				Objects.LuaCompletionIconKind.Variable => VariableBrush,
				Objects.LuaCompletionIconKind.Field => PropertyBrush,
				Objects.LuaCompletionIconKind.Method => MethodBrush,
				Objects.LuaCompletionIconKind.Property => PropertyBrush,
				Objects.LuaCompletionIconKind.Class => TypeBrush,
				Objects.LuaCompletionIconKind.Keyword => KeywordBrush,
				Objects.LuaCompletionIconKind.Constant => ConstantBrush,
				Objects.LuaCompletionIconKind.Parameter => VariableBrush,
				Objects.LuaCompletionIconKind.Namespace => TypeBrush,
				Objects.LuaCompletionIconKind.File => FileBrush,
				Objects.LuaCompletionIconKind.Folder => FileBrush,
				_ => MiscBrush
			};
		}
	}
}