using System.Collections.Generic;
using TombLib.Scripting.Highlighting;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaTheme
	{
		public string Name { get; set; } = string.Empty;
		public List<string> Aliases { get; set; } = new List<string>();
		public string EditorBackground { get; set; } = LuaBuiltInThemes.DefaultEditorBackground;
		public string EditorForeground { get; set; } = LuaBuiltInThemes.DefaultEditorForeground;
		public TextMateTokenTheme TextMateTheme { get; set; } = new TextMateTokenTheme();
		public LuaThemeSemanticColors SemanticColors { get; set; } = new LuaThemeSemanticColors();

		public LuaTheme Normalize(string fallbackName)
		{
			if (string.IsNullOrWhiteSpace(Name))
				Name = fallbackName;

			if (string.IsNullOrWhiteSpace(EditorBackground))
				EditorBackground = LuaBuiltInThemes.DefaultEditorBackground;

			if (string.IsNullOrWhiteSpace(EditorForeground))
				EditorForeground = LuaBuiltInThemes.DefaultEditorForeground;

			Aliases ??= new List<string>();
			TextMateTheme ??= new TextMateTokenTheme();
			SemanticColors ??= new LuaThemeSemanticColors();
			SemanticColors.Normalize();

			return this;
		}
	}

	public sealed class LuaThemeSemanticColors
	{
		public string MutedText { get; set; } = LuaBuiltInThemes.DefaultMutedText;
		public string Misc { get; set; } = LuaBuiltInThemes.DefaultMisc;
		public string Method { get; set; } = LuaBuiltInThemes.DefaultMethod;
		public string Variable { get; set; } = LuaBuiltInThemes.DefaultVariable;
		public string Property { get; set; } = LuaBuiltInThemes.DefaultProperty;
		public string Type { get; set; } = LuaBuiltInThemes.DefaultType;
		public string Keyword { get; set; } = LuaBuiltInThemes.DefaultKeyword;
		public string LanguageConstant { get; set; } = LuaBuiltInThemes.DefaultLanguageConstant;
		public string Constant { get; set; } = LuaBuiltInThemes.DefaultConstant;
		public string File { get; set; } = LuaBuiltInThemes.DefaultFile;
		public string SignatureParameterDocumentation { get; set; } = LuaBuiltInThemes.DefaultSignatureParameterDocumentation;
		public string SignatureActiveParameter { get; set; } = LuaBuiltInThemes.DefaultSignatureActiveParameter;
		public string SignatureText { get; set; } = LuaBuiltInThemes.DefaultSignatureText;

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
}