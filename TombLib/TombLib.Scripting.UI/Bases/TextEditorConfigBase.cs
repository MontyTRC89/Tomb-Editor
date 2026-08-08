using System.Xml.Serialization;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.UI.Bases;

/// <summary>
/// Provides the shared configuration base for text editors.
/// </summary>
public abstract class TextEditorConfigBase : ConfigurationBase
{
	/// <inheritdoc/>
	public abstract override string DefaultPath { get; }

	// Properties

	/// <summary>
	/// Gets or sets the font size of the editor.
	/// </summary>
	public double FontSize { get; set; } = TextEditorBaseDefaults.FontSize;

	/// <summary>
	/// Gets or sets the font family of the editor.
	/// </summary>
	public string FontFamily { get; set; } = TextEditorBaseDefaults.FontFamily;

	/// <summary>
	/// Gets or sets the undo stack size of the editor.
	/// </summary>
	public int UndoStackSize { get; set; } = TextEditorBaseDefaults.UndoStackSize;

	/// <summary>
	/// Gets or sets whether IntelliSense is enabled in the editor.
	/// </summary>
	public bool IntelliSenseEnabled { get; set; } = TextEditorBaseDefaults.IntelliSenseEnabled;

	/// <summary>
	/// Gets or sets whether completion is enabled in the editor.
	/// </summary>
	public bool CompletionEnabled { get; set; } = TextEditorBaseDefaults.CompletionEnabled;

	/// <summary>
	/// Gets or sets whether live error underlining is enabled in the editor.
	/// </summary>
	public bool LiveErrorUnderlining { get; set; } = TextEditorBaseDefaults.LiveErrorUnderlining;

	/// <summary>
	/// Gets or sets whether signature help popups are enabled in the editor.
	/// </summary>
	public bool SignatureHelpPopupsEnabled { get; set; } = TextEditorBaseDefaults.SignatureHelpPopupsEnabled;

	/// <summary>
	/// Gets or sets whether parentheses are auto-closed in the editor.
	/// </summary>
	public bool AutoCloseParentheses { get; set; } = TextEditorBaseDefaults.AutoCloseParentheses;

	/// <summary>
	/// Gets or sets whether braces are auto-closed in the editor.
	/// </summary>
	public bool AutoCloseBraces { get; set; } = TextEditorBaseDefaults.AutoCloseBraces;

	/// <summary>
	/// Gets or sets whether brackets are auto-closed in the editor.
	/// </summary>
	public bool AutoCloseBrackets { get; set; } = TextEditorBaseDefaults.AutoCloseBrackets;

	/// <summary>
	/// Gets or sets whether double quotes are auto-closed in the editor.
	/// </summary>
	public bool AutoCloseDoubleQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseDoubleQuotes;

	/// <summary>
	/// Gets or sets whether single quotes are auto-closed in the editor.
	/// </summary>
	public bool AutoCloseSingleQuotes { get; set; } = TextEditorBaseDefaults.AutoCloseSingleQuotes;

	/// <summary>
	/// Gets or sets whether both quote kinds are auto-closed in the editor.
	/// </summary>
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

	/// <summary>
	/// Backs the legacy <c>AutoCloseQuotes</c> serialized element.
	/// </summary>
	[XmlElement("AutoCloseQuotes")]
	public bool LegacyAutoCloseQuotes
	{
		get => AutoCloseQuotes;
		set => AutoCloseQuotes = value;
	}

	/// <summary>
	/// Controls whether the legacy <c>AutoCloseQuotes</c> element is serialized.
	/// </summary>
	[XmlIgnore]
	public bool LegacyAutoCloseQuotesSpecified
	{
		get => false;
		set { }
	}

	/// <summary>
	/// Gets or sets whether word wrapping is enabled in the editor.
	/// </summary>
	public bool WordWrapping { get; set; } = TextEditorBaseDefaults.WordWrapping;

	/// <summary>
	/// Gets or sets whether the current line is highlighted in the editor.
	/// </summary>
	public bool HighlightCurrentLine { get; set; } = TextEditorBaseDefaults.HighlightCurrentLine;

	/// <summary>
	/// Gets or sets whether line numbers are shown in the editor.
	/// </summary>
	public bool ShowLineNumbers { get; set; } = TextEditorBaseDefaults.ShowLineNumbers;

	/// <summary>
	/// Gets or sets whether spaces are shown visually in the editor.
	/// </summary>
	public bool ShowVisualSpaces { get; set; } = TextEditorBaseDefaults.ShowVisualSpaces;

	/// <summary>
	/// Gets or sets whether tabs are shown visually in the editor.
	/// </summary>
	public bool ShowVisualTabs { get; set; } = TextEditorBaseDefaults.ShowVisualTabs;
}
