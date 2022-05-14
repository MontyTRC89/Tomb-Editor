namespace TombLib.Scripting.Resources
{
	public struct TextEditorBaseDefaults
	{
		public const double FontSize = 16d;
		public const string FontFamily = "Consolas";

		public const int UndoStackSize = 256;

		public const bool AutocompleteEnabled = true;
		public const bool LiveErrorUnderlining = true;

		public const bool AutoCloseParentheses = true;
		public const bool AutoCloseBraces = true;
		public const bool AutoCloseBrackets = true;
		public const bool AutoCloseQuotes = false;

		public const bool WordWrapping = false;

		public const bool HighlightCurrentLine = true;

		public const bool ShowLineNumbers = true;

		public const bool ShowVisualSpaces = false;
		public const bool ShowVisualTabs = true;
	}
}
