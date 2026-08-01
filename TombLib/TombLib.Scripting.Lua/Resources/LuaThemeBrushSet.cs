using System.Windows.Media;
using Nickelony.LanguageServer.Abstractions.Completion;

namespace TombLib.Scripting.Lua.Resources;

/// <summary>
/// Bundles the frozen WPF brushes derived from a resolved Lua theme.
/// </summary>
internal sealed class LuaThemeBrushSet(
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
	/// <summary>
	/// Gets the canonical name of the theme that produced this brush set.
	/// </summary>
	public string ThemeName { get; } = themeName;

	/// <summary>
	/// Gets the editor background brush.
	/// </summary>
	public SolidColorBrush EditorBackground { get; } = editorBackground;

	/// <summary>
	/// Gets the editor foreground brush.
	/// </summary>
	public SolidColorBrush EditorForeground { get; } = editorForeground;

	/// <summary>
	/// Gets the muted text brush used for secondary completion details and similar metadata.
	/// </summary>
	public SolidColorBrush MutedTextBrush { get; } = mutedText;

	/// <summary>
	/// Gets the fallback brush used for uncategorized symbols.
	/// </summary>
	public SolidColorBrush MiscBrush { get; } = misc;

	/// <summary>
	/// Gets the brush used for methods and functions.
	/// </summary>
	public SolidColorBrush MethodBrush { get; } = method;

	/// <summary>
	/// Gets the brush used for variables and parameters.
	/// </summary>
	public SolidColorBrush VariableBrush { get; } = variable;

	/// <summary>
	/// Gets the brush used for properties and fields.
	/// </summary>
	public SolidColorBrush PropertyBrush { get; } = property;

	/// <summary>
	/// Gets the brush used for types and namespaces.
	/// </summary>
	public SolidColorBrush TypeBrush { get; } = type;

	/// <summary>
	/// Gets the brush used for keywords.
	/// </summary>
	public SolidColorBrush KeywordBrush { get; } = keyword;

	/// <summary>
	/// Gets the brush used for language-defined constants.
	/// </summary>
	public SolidColorBrush LanguageConstantBrush { get; } = languageConstant;

	/// <summary>
	/// Gets the brush used for user-defined constants and enum members.
	/// </summary>
	public SolidColorBrush ConstantBrush { get; } = constant;

	/// <summary>
	/// Gets the brush used for file and folder completion items.
	/// </summary>
	public SolidColorBrush FileBrush { get; } = file;

	/// <summary>
	/// Gets the brush used for signature-help parameter documentation.
	/// </summary>
	public SolidColorBrush SignatureParamDocForeground { get; } = signatureParamDoc;

	/// <summary>
	/// Gets the brush used to emphasize the active signature parameter.
	/// </summary>
	public SolidColorBrush SignatureActiveParamForeground { get; } = signatureActiveParam;

	/// <summary>
	/// Gets the base brush used for signature labels.
	/// </summary>
	public SolidColorBrush SignatureForeground { get; } = signatureForeground;

	/// <summary>
	/// Gets the brush used to render the specified completion item kind.
	/// </summary>
	/// <param name="kind">The completion item icon kind.</param>
	/// <returns>The brush associated with that kind.</returns>
	public Brush GetCompletionItemBrush(TextCompletionItemKind kind) => kind switch
	{
		TextCompletionItemKind.Variable => VariableBrush,
		TextCompletionItemKind.Field => PropertyBrush,
		TextCompletionItemKind.Method => MethodBrush,
		TextCompletionItemKind.Property => PropertyBrush,
		TextCompletionItemKind.Class => TypeBrush,
		TextCompletionItemKind.Keyword => KeywordBrush,
		TextCompletionItemKind.Constant => ConstantBrush,
		TextCompletionItemKind.Parameter => VariableBrush,
		TextCompletionItemKind.Namespace => TypeBrush,
		TextCompletionItemKind.File => FileBrush,
		TextCompletionItemKind.Folder => FileBrush,
		_ => MiscBrush
	};
}
