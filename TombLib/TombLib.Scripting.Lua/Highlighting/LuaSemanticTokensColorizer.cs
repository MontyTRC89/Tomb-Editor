using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua.Highlighting;

/// <summary>
/// Applies Lua semantic-token styling on top of the editor's baseline syntax highlighting.
/// </summary>
internal sealed class LuaSemanticTokensColorizer : DocumentColorizingTransformer
{
	private static readonly TextDecorationCollection DeprecatedDecorations = CreateTextDecorations(TextDecorations.Strikethrough);

	private static readonly IReadOnlyDictionary<int, IReadOnlyList<StyledSemanticToken>> EmptyTokensByLine =
		new Dictionary<int, IReadOnlyList<StyledSemanticToken>>();

	private LuaThemeBrushSet _themeBrushSet;
	private readonly TextView _textView;

	// Tokens are pre-styled at SetTokens time so that ColorizeLine, which runs on every redraw and
	// for every visible line, can avoid re-resolving brushes and modifier flags per token.
	private IReadOnlyList<LuaSemanticToken> _rawTokens = [];

	private IReadOnlyDictionary<int, IReadOnlyList<StyledSemanticToken>> _tokensByLine = EmptyTokensByLine;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaSemanticTokensColorizer"/> class.
	/// </summary>
	/// <param name="textView">The text view that will be redrawn when semantic styles change.</param>
	/// <param name="themeBrushSet">The active Lua theme brush set.</param>
	public LuaSemanticTokensColorizer(TextView textView, LuaThemeBrushSet themeBrushSet)
	{
		_textView = textView;
		_themeBrushSet = themeBrushSet;
	}

	/// <summary>
	/// Rebuilds the styled semantic-token cache for a new theme and redraws the text view.
	/// </summary>
	/// <param name="themeBrushSet">The new active theme brush set.</param>
	public void UpdateTheme(LuaThemeBrushSet themeBrushSet)
	{
		_themeBrushSet = themeBrushSet;
		_tokensByLine = BuildStyledMap(_rawTokens, _themeBrushSet);
		_textView.Redraw();
	}

	/// <summary>
	/// Replaces the semantic tokens currently applied to the text view.
	/// </summary>
	/// <param name="tokens">The semantic tokens to render.</param>
	public void SetTokens(IReadOnlyList<LuaSemanticToken> tokens)
	{
		if (tokens is null || tokens.Count == 0)
		{
			ClearTokens();
			return;
		}

		_rawTokens = tokens;
		_tokensByLine = BuildStyledMap(tokens, _themeBrushSet);
		_textView.Redraw();
	}

	/// <summary>
	/// Removes all semantic-token styling from the text view.
	/// </summary>
	public void ClearTokens()
	{
		if (_tokensByLine.Count == 0 && _rawTokens.Count == 0)
			return;

		_rawTokens = [];
		_tokensByLine = EmptyTokensByLine;
		_textView.Redraw();
	}

	protected override void ColorizeLine(DocumentLine line)
	{
		if (!_tokensByLine.TryGetValue(line.LineNumber - 1, out IReadOnlyList<StyledSemanticToken>? tokens))
			return;

		int lineLength = line.Length;

		for (int i = 0; i < tokens.Count; i++)
		{
			StyledSemanticToken styled = tokens[i];
			int startIndex = Math.Max(0, Math.Min(styled.Character, lineLength));
			int endIndex = Math.Max(startIndex, Math.Min(styled.Character + styled.Length, lineLength));

			if (endIndex <= startIndex)
				continue;

			LuaSemanticTokenStyle style = styled.Style;
			ChangeLinePart(line.Offset + startIndex, line.Offset + endIndex, element => ApplyStyle(element, style));
		}
	}

	private static IReadOnlyDictionary<int, IReadOnlyList<StyledSemanticToken>> BuildStyledMap(
		IReadOnlyList<LuaSemanticToken> tokens, LuaThemeBrushSet themeBrushSet)
	{
		if (tokens.Count == 0)
			return EmptyTokensByLine;

		var grouped = new Dictionary<int, List<StyledSemanticToken>>();

		for (int i = 0; i < tokens.Count; i++)
		{
			LuaSemanticToken token = tokens[i];
			LuaSemanticTokenStyle style = ResolveStyle(token, themeBrushSet);

			if (!style.HasFormatting)
				continue;

			if (!grouped.TryGetValue(token.Line, out List<StyledSemanticToken>? lineTokens))
			{
				lineTokens = [];
				grouped[token.Line] = lineTokens;
			}

			lineTokens.Add(new StyledSemanticToken(token.Character, token.Length, style));
		}

		var frozen = new Dictionary<int, IReadOnlyList<StyledSemanticToken>>(grouped.Count);

		foreach (KeyValuePair<int, List<StyledSemanticToken>> pair in grouped)
		{
			pair.Value.Sort(static (left, right) =>
			{
				int characterComparison = left.Character.CompareTo(right.Character);
				return characterComparison != 0 ? characterComparison : left.Length.CompareTo(right.Length);
			});

			frozen[pair.Key] = pair.Value;
		}

		return frozen;
	}

	private static LuaSemanticTokenStyle ResolveStyle(LuaSemanticToken token, LuaThemeBrushSet brushSet)
	{
		Brush? foreground = token.Type switch
		{
			"namespace" => brushSet.TypeBrush,
			"type" => brushSet.TypeBrush,
			"class" => brushSet.TypeBrush,
			"enum" => brushSet.TypeBrush,
			"interface" => brushSet.TypeBrush,
			"struct" => brushSet.TypeBrush,
			"typeParameter" => brushSet.TypeBrush,
			"function" => token.HasModifier("defaultLibrary") ? brushSet.TypeBrush : brushSet.MethodBrush,
			"method" => token.HasModifier("defaultLibrary") ? brushSet.TypeBrush : brushSet.MethodBrush,
			"parameter" => brushSet.VariableBrush,
			"property" => brushSet.PropertyBrush,
			"event" => brushSet.VariableBrush,
			"enumMember" => brushSet.ConstantBrush,
			"decorator" => brushSet.KeywordBrush,
			"macro" => brushSet.KeywordBrush,
			"variable" => ResolveVariableBrush(token, brushSet),
			_ => null
		};

		return new LuaSemanticTokenStyle(
			foreground,
			token.HasModifier("declaration") && (token.Type == "function" || token.Type == "method"),
			token.HasModifier("deprecated") ? DeprecatedDecorations : null);
	}

	private static SolidColorBrush? ResolveVariableBrush(LuaSemanticToken token, LuaThemeBrushSet brushSet)
	{
		if (token.HasModifier("defaultLibrary"))
			return brushSet.TypeBrush;

		if (token.HasModifier("global"))
			return brushSet.PropertyBrush;

		return brushSet.VariableBrush;
	}

	private static void ApplyStyle(VisualLineElement element, LuaSemanticTokenStyle style)
	{
		VisualLineElementTextRunProperties properties = element.TextRunProperties;

		if (style.Foreground is not null)
			properties.SetForegroundBrush(style.Foreground);

		if (style.IsBold)
		{
			Typeface typeface = properties.Typeface;
			properties.SetTypeface(new Typeface(typeface.FontFamily, typeface.Style, FontWeights.Bold, typeface.Stretch));
		}

		if (style.TextDecorations is not null)
			properties.SetTextDecorations(style.TextDecorations);
	}

	private static TextDecorationCollection CreateTextDecorations(TextDecorationCollection source)
	{
		var clone = source.Clone();
		clone.Freeze();
		return clone;
	}

	private readonly struct StyledSemanticToken(int character, int length, LuaSemanticTokenStyle style)
	{
		public int Character { get; } = character;
		public int Length { get; } = length;
		public LuaSemanticTokenStyle Style { get; } = style;
	}

	private readonly struct LuaSemanticTokenStyle(Brush? foreground, bool isBold, TextDecorationCollection? textDecorations)
	{
		public Brush? Foreground { get; } = foreground;
		public bool IsBold { get; } = isBold;
		public TextDecorationCollection? TextDecorations { get; } = textDecorations;

		public bool HasFormatting => Foreground is not null || IsBold || TextDecorations is not null;
	}
}
