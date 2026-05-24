using System.Collections.Generic;

namespace TombLib.Scripting.UI.Highlighting
{
	public static class LuaBuiltInTextMateThemeDefaults
	{
		public const string DefaultEditorBackground = "#2D2D2D";
		public const string DefaultEditorForeground = "#CCCCCC";

		public const string DefaultMutedText = "#999999";
		public const string DefaultMisc = "#CCCCCC";
		public const string DefaultMethod = "#FFCC66";
		public const string DefaultVariable = "#E6C8FF";
		public const string DefaultProperty = "#D7B8FF";
		public const string DefaultType = "#66CCCC";
		public const string DefaultKeyword = "#6699CC";
		public const string DefaultLanguageConstant = "#66CCCC";
		public const string DefaultConstant = "#99CC99";
		public const string DefaultFile = "#F99157";
		public const string DefaultSignatureParameterDocumentation = "#999999";
		public const string DefaultSignatureActiveParameter = "#FFCC66";
		public const string DefaultSignatureText = "#CCCCCC";

		public static TextMateTokenTheme CreateDefaultTextMateTheme()
		{
			return new TextMateTokenTheme
			{
				Rules = new List<TextMateTokenThemeRule>
				{
					new TextMateTokenThemeRule { Scope = "comment", Foreground = DefaultMutedText },
					new TextMateTokenThemeRule { Scope = "string", Foreground = DefaultFile },
					new TextMateTokenThemeRule { Scope = "constant.numeric", Foreground = DefaultConstant },
					new TextMateTokenThemeRule { Scope = "constant.character.escape", Foreground = DefaultFile },
					new TextMateTokenThemeRule { Scope = "constant.language", Foreground = DefaultLanguageConstant, FontStyle = "bold" },
					new TextMateTokenThemeRule { Scope = "keyword, storage", Foreground = DefaultKeyword, FontStyle = "bold" },
					new TextMateTokenThemeRule { Scope = "keyword.control.goto, keyword.control.return, keyword.control.break", Foreground = "#CC99CC" },
					new TextMateTokenThemeRule { Scope = "keyword.control.local", Foreground = DefaultFile, FontStyle = "bold" },
					new TextMateTokenThemeRule { Scope = "entity.name.class, support.class, support.type, support.variable, variable.language.self", Foreground = DefaultType, FontStyle = "bold" },
					new TextMateTokenThemeRule { Scope = "entity.other.attribute, support.type.property-name", Foreground = DefaultProperty },
					new TextMateTokenThemeRule { Scope = "variable.parameter, variable.other.object", Foreground = DefaultVariable },
					new TextMateTokenThemeRule { Scope = "entity.name.function, support.function, support.function.library, support.function.any-method", Foreground = DefaultMethod, FontStyle = "bold" },
					new TextMateTokenThemeRule { Scope = "keyword.operator, punctuation", Foreground = DefaultMisc }
				}
			};
		}
	}
}