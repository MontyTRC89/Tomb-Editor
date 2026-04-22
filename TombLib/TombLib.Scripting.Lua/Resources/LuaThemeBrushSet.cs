using System.Windows.Media;
using TombLib.Scripting.Lua.Objects;

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
	public string ThemeName { get; } = themeName;
	public SolidColorBrush EditorBackground { get; } = editorBackground;
	public SolidColorBrush EditorForeground { get; } = editorForeground;
	public SolidColorBrush MutedTextBrush { get; } = mutedText;
	public SolidColorBrush MiscBrush { get; } = misc;
	public SolidColorBrush MethodBrush { get; } = method;
	public SolidColorBrush VariableBrush { get; } = variable;
	public SolidColorBrush PropertyBrush { get; } = property;
	public SolidColorBrush TypeBrush { get; } = type;
	public SolidColorBrush KeywordBrush { get; } = keyword;
	public SolidColorBrush LanguageConstantBrush { get; } = languageConstant;
	public SolidColorBrush ConstantBrush { get; } = constant;
	public SolidColorBrush FileBrush { get; } = file;
	public SolidColorBrush SignatureParamDocForeground { get; } = signatureParamDoc;
	public SolidColorBrush SignatureActiveParamForeground { get; } = signatureActiveParam;
	public SolidColorBrush SignatureForeground { get; } = signatureForeground;

	/// <summary>
	/// Gets the brush used to render the specified completion item kind.
	/// </summary>
	/// <param name="kind">The completion item icon kind.</param>
	/// <returns>The brush associated with that kind.</returns>
	public Brush GetCompletionItemBrush(LuaCompletionIconKind kind) => kind switch
	{
		LuaCompletionIconKind.Variable => VariableBrush,
		LuaCompletionIconKind.Field => PropertyBrush,
		LuaCompletionIconKind.Method => MethodBrush,
		LuaCompletionIconKind.Property => PropertyBrush,
		LuaCompletionIconKind.Class => TypeBrush,
		LuaCompletionIconKind.Keyword => KeywordBrush,
		LuaCompletionIconKind.Constant => ConstantBrush,
		LuaCompletionIconKind.Parameter => VariableBrush,
		LuaCompletionIconKind.Namespace => TypeBrush,
		LuaCompletionIconKind.File => FileBrush,
		LuaCompletionIconKind.Folder => FileBrush,
		_ => MiscBrush
	};
}
