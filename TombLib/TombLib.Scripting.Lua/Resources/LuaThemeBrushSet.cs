using System.Windows.Media;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua.Resources;

/// <summary>
/// Bundles the frozen WPF brushes derived from a resolved Lua theme.
/// </summary>
internal sealed class LuaThemeBrushSet
{
	public LuaThemeBrushSet(
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
		ThemeName = themeName;
		EditorBackground = editorBackground;
		EditorForeground = editorForeground;
		MutedTextBrush = mutedText;
		MiscBrush = misc;
		MethodBrush = method;
		VariableBrush = variable;
		PropertyBrush = property;
		TypeBrush = type;
		KeywordBrush = keyword;
		LanguageConstantBrush = languageConstant;
		ConstantBrush = constant;
		FileBrush = file;
		SignatureParamDocForeground = signatureParamDoc;
		SignatureActiveParamForeground = signatureActiveParam;
		SignatureForeground = signatureForeground;
	}

	public string ThemeName { get; }
	public SolidColorBrush EditorBackground { get; }
	public SolidColorBrush EditorForeground { get; }
	public SolidColorBrush MutedTextBrush { get; }
	public SolidColorBrush MiscBrush { get; }
	public SolidColorBrush MethodBrush { get; }
	public SolidColorBrush VariableBrush { get; }
	public SolidColorBrush PropertyBrush { get; }
	public SolidColorBrush TypeBrush { get; }
	public SolidColorBrush KeywordBrush { get; }
	public SolidColorBrush LanguageConstantBrush { get; }
	public SolidColorBrush ConstantBrush { get; }
	public SolidColorBrush FileBrush { get; }
	public SolidColorBrush SignatureParamDocForeground { get; }
	public SolidColorBrush SignatureActiveParamForeground { get; }
	public SolidColorBrush SignatureForeground { get; }

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
