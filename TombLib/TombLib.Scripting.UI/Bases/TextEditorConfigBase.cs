using System.Xml.Serialization;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Bases
{
	public abstract class TextEditorConfigBase : ConfigurationBase
	{
		public abstract override string DefaultPath { get; }

		#region Properties

		public double FontSize { get; set; } = TextEditorBaseDefaults.FontSize;
		public string FontFamily { get; set; } = TextEditorBaseDefaults.FontFamily;

		public int UndoStackSize { get; set; } = TextEditorBaseDefaults.UndoStackSize;

		public bool IntellisenseEnabled { get; set; } = TextEditorBaseDefaults.IntellisenseEnabled;
		public bool AutocompleteEnabled { get; set; } = TextEditorBaseDefaults.AutocompleteEnabled;
		public bool LiveErrorUnderlining { get; set; } = TextEditorBaseDefaults.LiveErrorUnderlining;
		public bool SignatureHelpPopupsEnabled { get; set; } = TextEditorBaseDefaults.SignatureHelpPopupsEnabled;

		public bool AutoCloseParentheses { get; set; } = TextEditorBaseDefaults.AutoCloseParentheses;
		public bool AutoCloseBraces { get; set; } = TextEditorBaseDefaults.AutoCloseBraces;
		public bool AutoCloseBrackets { get; set; } = TextEditorBaseDefaults.AutoCloseBrackets;
		public bool AutoCloseDoubleQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseDoubleQuotes;
		public bool AutoCloseSingleQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseSingleQuotes;

		[XmlIgnore]
		public bool AutoCloseQuotes
		{
			get => AutoCloseDoubleQuotes && AutoCloseSingleQuotes;
			set
			{
				AutoCloseDoubleQuotes = value;
				AutoCloseSingleQuotes = value;
			}
		}

		[XmlElement("AutoCloseQuotes")]
		public bool LegacyAutoCloseQuotes
		{
			get => AutoCloseQuotes;
			set => AutoCloseQuotes = value;
		}

		[XmlIgnore]
		public bool LegacyAutoCloseQuotesSpecified
		{
			get => false;
			set { }
		}

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

			IntellisenseEnabled = TextEditorBaseDefaults.IntellisenseEnabled;
			AutocompleteEnabled = TextEditorBaseDefaults.AutocompleteEnabled;
			LiveErrorUnderlining = TextEditorBaseDefaults.LiveErrorUnderlining;
			SignatureHelpPopupsEnabled = TextEditorBaseDefaults.SignatureHelpPopupsEnabled;

			AutoCloseParentheses = TextEditorBaseDefaults.AutoCloseParentheses;
			AutoCloseBraces = TextEditorBaseDefaults.AutoCloseBraces;
			AutoCloseBrackets = TextEditorBaseDefaults.AutoCloseBrackets;
			AutoCloseDoubleQuotes = TextEditorBaseDefaults.AutoCloseDoubleQuotes;
			AutoCloseSingleQuotes = TextEditorBaseDefaults.AutoCloseSingleQuotes;

			WordWrapping = TextEditorBaseDefaults.WordWrapping;

			ShowLineNumbers = TextEditorBaseDefaults.ShowLineNumbers;

			ShowVisualSpaces = TextEditorBaseDefaults.ShowVisualSpaces;
			ShowVisualTabs = TextEditorBaseDefaults.ShowVisualTabs;
		}

		#endregion Virtual methods
	}
}
