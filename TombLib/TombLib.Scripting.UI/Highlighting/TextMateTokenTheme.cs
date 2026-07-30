using System.Collections.Generic;

namespace TombLib.Scripting.UI.Highlighting
{
	public sealed class TextMateTokenTheme
	{
		public List<TextMateTokenThemeRule> Rules { get; set; } = new List<TextMateTokenThemeRule>();
	}

	public sealed class TextMateTokenThemeRule
	{
		public string Scope { get; set; } = string.Empty;
		public string Foreground { get; set; } = string.Empty;
		public string FontStyle { get; set; } = string.Empty;
	}
}
