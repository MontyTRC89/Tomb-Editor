using System.Collections.Generic;
using TombLib.Scripting.Highlighting;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class LuaTheme
	{
		public string Name { get; set; } = string.Empty;
		public List<string> Aliases { get; set; } = new List<string>();
		public string EditorBackground { get; set; } = "#202020";
		public string EditorForeground { get; set; } = "Gainsboro";
		public TextMateTokenTheme TextMateTheme { get; set; } = new TextMateTokenTheme();
		public LuaThemeSemanticColors SemanticColors { get; set; } = new LuaThemeSemanticColors();

		public LuaTheme Normalize(string fallbackName)
		{
			if (string.IsNullOrWhiteSpace(Name))
				Name = fallbackName;

			if (string.IsNullOrWhiteSpace(EditorBackground))
				EditorBackground = "#202020";

			if (string.IsNullOrWhiteSpace(EditorForeground))
				EditorForeground = "Gainsboro";

			Aliases ??= new List<string>();
			TextMateTheme ??= new TextMateTokenTheme();
			SemanticColors ??= new LuaThemeSemanticColors();
			SemanticColors.Normalize();

			return this;
		}
	}

	public sealed class LuaThemeSemanticColors
	{
		public string MutedText { get; set; } = "#8C8C8C";
		public string Misc { get; set; } = "#C8C8C8";
		public string Method { get; set; } = "#DCDCAA";
		public string Variable { get; set; } = "#9CDCFE";
		public string Property { get; set; } = "#4FC1FF";
		public string Type { get; set; } = "#4EC9B0";
		public string Keyword { get; set; } = "#C586C0";
		public string Constant { get; set; } = "#B5CEA8";
		public string File { get; set; } = "#D7BA7D";
		public string SignatureParameterDocumentation { get; set; } = "#B4B4B4";
		public string SignatureActiveParameter { get; set; } = "#56B4EB";
		public string SignatureText { get; set; } = "#D4D4D4";

		public void Normalize()
		{
			MutedText = NormalizeValue(MutedText, "#8C8C8C");
			Misc = NormalizeValue(Misc, "#C8C8C8");
			Method = NormalizeValue(Method, "#DCDCAA");
			Variable = NormalizeValue(Variable, "#9CDCFE");
			Property = NormalizeValue(Property, "#4FC1FF");
			Type = NormalizeValue(Type, "#4EC9B0");
			Keyword = NormalizeValue(Keyword, "#C586C0");
			Constant = NormalizeValue(Constant, "#B5CEA8");
			File = NormalizeValue(File, "#D7BA7D");
			SignatureParameterDocumentation = NormalizeValue(SignatureParameterDocumentation, "#B4B4B4");
			SignatureActiveParameter = NormalizeValue(SignatureActiveParameter, "#56B4EB");
			SignatureText = NormalizeValue(SignatureText, "#D4D4D4");
		}

		private static string NormalizeValue(string value, string fallbackValue)
		{
			if (string.IsNullOrWhiteSpace(value))
				return fallbackValue;

			return value;
		}
	}
}