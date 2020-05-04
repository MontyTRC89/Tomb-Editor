namespace TombLib.Scripting.TextEditors.Configuration
{
	public abstract class TextEditorConfigurationBase : ConfigurationBase
	{
		public abstract override string DefaultPath { get; }

		public double FontSize { get; set; } = 16d;
		public string FontFamily { get; set; } = "Consolas";

		public int UndoStackSize { get; set; } = 256;

		public bool AutocompleteEnabled { get; set; } = true;
		public bool LiveErrorUnderlining { get; set; } = true;

		public bool AutoCloseParentheses { get; set; } = true;
		public bool AutoCloseBraces { get; set; } = true;
		public bool AutoCloseBrackets { get; set; } = true;
		public bool AutoCloseQuotes { get; set; } = true;

		public bool WordWrapping { get; set; } = false;

		public bool ShowLineNumbers { get; set; } = true;

		public bool ShowVisualSpaces { get; set; } = false;
		public bool ShowVisualTabs { get; set; } = true;

		public bool ShowDefinitionToolTips { get; set; } = true;
	}
}
