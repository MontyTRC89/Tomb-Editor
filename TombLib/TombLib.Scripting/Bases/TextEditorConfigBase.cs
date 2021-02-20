using TombLib.Scripting.Resources;

namespace TombLib.Scripting.Bases
{
	public abstract class TextEditorConfigBase : ConfigurationBase
	{
		public abstract override string DefaultPath { get; }

		#region Properties

		public double FontSize { get; set; } = TextEditorBaseDefaults.FontSize;
		public string FontFamily { get; set; } = TextEditorBaseDefaults.FontFamily;

		public int UndoStackSize { get; set; } = TextEditorBaseDefaults.UndoStackSize;

		public bool AutocompleteEnabled { get; set; } = TextEditorBaseDefaults.AutocompleteEnabled;
		public bool LiveErrorUnderlining { get; set; } = TextEditorBaseDefaults.LiveErrorUnderlining;

		public bool AutoCloseParentheses { get; set; } = TextEditorBaseDefaults.AutoCloseParentheses;
		public bool AutoCloseBraces { get; set; } = TextEditorBaseDefaults.AutoCloseBraces;
		public bool AutoCloseBrackets { get; set; } = TextEditorBaseDefaults.AutoCloseBrackets;
		public bool AutoCloseQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseQuotes;

		public bool WordWrapping { get; set; } = TextEditorBaseDefaults.WordWrapping;

		public bool HighlightCurrentLine { get; set; } = TextEditorBaseDefaults.HighlightCurrentLine;

		public bool ShowLineNumbers { get; set; } = TextEditorBaseDefaults.ShowLineNumbers;

		public bool ShowVisualSpaces { get; set; } = TextEditorBaseDefaults.ShowVisualSpaces;
		public bool ShowVisualTabs { get; set; } = TextEditorBaseDefaults.ShowVisualTabs;

		#endregion Properties

		#region Virtual methods

		public virtual void ResetToDefaultSettings()
		{
			FontSize = TextEditorBaseDefaults.FontSize;
			FontFamily = TextEditorBaseDefaults.FontFamily;

			UndoStackSize = TextEditorBaseDefaults.UndoStackSize;

			AutocompleteEnabled = TextEditorBaseDefaults.AutocompleteEnabled;
			LiveErrorUnderlining = TextEditorBaseDefaults.LiveErrorUnderlining;

			AutoCloseParentheses = TextEditorBaseDefaults.AutoCloseParentheses;
			AutoCloseBraces = TextEditorBaseDefaults.AutoCloseBraces;
			AutoCloseBrackets = TextEditorBaseDefaults.AutoCloseBrackets;
			AutoCloseQuotes = TextEditorBaseDefaults.AutoCloseQuotes;

			WordWrapping = TextEditorBaseDefaults.WordWrapping;

			ShowLineNumbers = TextEditorBaseDefaults.ShowLineNumbers;

			ShowVisualSpaces = TextEditorBaseDefaults.ShowVisualSpaces;
			ShowVisualTabs = TextEditorBaseDefaults.ShowVisualTabs;
		}

		#endregion Virtual methods
	}
}
