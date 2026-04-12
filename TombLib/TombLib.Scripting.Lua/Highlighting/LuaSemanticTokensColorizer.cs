using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua.Highlighting
{
	internal sealed class LuaSemanticTokensColorizer : DocumentColorizingTransformer
	{
		private static readonly Brush DefaultLibraryBrush = LuaEditorColorPalette.TypeBrush;
		private static readonly Brush GlobalVariableBrush = LuaEditorColorPalette.PropertyBrush;
		private static readonly Brush TypeBrush = LuaEditorColorPalette.TypeBrush;
		private static readonly Brush FunctionBrush = LuaEditorColorPalette.MethodBrush;
		private static readonly Brush ParameterBrush = LuaEditorColorPalette.VariableBrush;
		private static readonly Brush PropertyBrush = LuaEditorColorPalette.VariableBrush;
		private static readonly Brush DecoratorBrush = LuaEditorColorPalette.KeywordBrush;
		private static readonly Brush MacroBrush = LuaEditorColorPalette.KeywordBrush;
		private static readonly Brush EnumMemberBrush = LuaEditorColorPalette.ConstantBrush;
		private static readonly TextDecorationCollection DeprecatedDecorations = CreateTextDecorations(TextDecorations.Strikethrough);
		private static readonly IReadOnlyDictionary<int, IReadOnlyList<LuaSemanticToken>> EmptyTokensByLine =
			new Dictionary<int, IReadOnlyList<LuaSemanticToken>>();

		private readonly TextView _textView;
		private IReadOnlyDictionary<int, IReadOnlyList<LuaSemanticToken>> _tokensByLine = EmptyTokensByLine;

		public LuaSemanticTokensColorizer(TextView textView)
			=> _textView = textView ?? throw new ArgumentNullException(nameof(textView));

		public void SetTokens(IReadOnlyList<LuaSemanticToken> tokens)
		{
			if (tokens is null || tokens.Count == 0)
			{
				ClearTokens();
				return;
			}

			var groupedTokens = new Dictionary<int, List<LuaSemanticToken>>();

			for (int i = 0; i < tokens.Count; i++)
			{
				LuaSemanticToken token = tokens[i];

				if (!groupedTokens.TryGetValue(token.Line, out List<LuaSemanticToken>? lineTokens) || lineTokens is null)
				{
					lineTokens = new List<LuaSemanticToken>();
					groupedTokens[token.Line] = lineTokens;
				}

				lineTokens.Add(token);
			}

			var frozenMap = new Dictionary<int, IReadOnlyList<LuaSemanticToken>>(groupedTokens.Count);

			foreach (KeyValuePair<int, List<LuaSemanticToken>> pair in groupedTokens)
			{
				pair.Value.Sort((left, right) =>
				{
					int characterComparison = left.Character.CompareTo(right.Character);

					if (characterComparison != 0)
						return characterComparison;

					return left.Length.CompareTo(right.Length);
				});

				frozenMap[pair.Key] = pair.Value;
			}

			_tokensByLine = frozenMap;
			_textView.Redraw();
		}

		public void ClearTokens()
		{
			if (_tokensByLine.Count == 0)
				return;

			_tokensByLine = EmptyTokensByLine;
			_textView.Redraw();
		}

		protected override void ColorizeLine(DocumentLine line)
		{
			if (!_tokensByLine.TryGetValue(line.LineNumber - 1, out IReadOnlyList<LuaSemanticToken>? tokens) || tokens is null)
				return;

			int lineLength = line.Length;

			for (int i = 0; i < tokens.Count; i++)
			{
				LuaSemanticToken token = tokens[i];
				LuaSemanticTokenStyle style = ResolveStyle(token);

				if (!style.HasFormatting)
					continue;

				int startIndex = Math.Max(0, Math.Min(token.Character, lineLength));
				int endIndex = Math.Max(startIndex, Math.Min(token.Character + token.Length, lineLength));

				if (endIndex <= startIndex)
					continue;

				ChangeLinePart(line.Offset + startIndex, line.Offset + endIndex, element => ApplyStyle(element, style));
			}
		}

		private static LuaSemanticTokenStyle ResolveStyle(LuaSemanticToken token)
		{
			Brush? foreground = token.Type switch
			{
				"namespace" => token.HasModifier("defaultLibrary") ? DefaultLibraryBrush : TypeBrush,
				"type" => TypeBrush,
				"class" => TypeBrush,
				"enum" => TypeBrush,
				"interface" => TypeBrush,
				"struct" => TypeBrush,
				"typeParameter" => TypeBrush,
				"function" => token.HasModifier("defaultLibrary") ? DefaultLibraryBrush : FunctionBrush,
				"method" => token.HasModifier("defaultLibrary") ? DefaultLibraryBrush : FunctionBrush,
				"parameter" => ParameterBrush,
				"property" => PropertyBrush,
				"event" => PropertyBrush,
				"enumMember" => EnumMemberBrush,
				"decorator" => DecoratorBrush,
				"macro" => MacroBrush,
				"variable" => ResolveVariableBrush(token),
				_ => null
			};

			return new LuaSemanticTokenStyle(
				foreground,
				token.HasModifier("declaration") && (token.Type == "function" || token.Type == "method"),
				token.HasModifier("deprecated") ? DeprecatedDecorations : null);
		}

		private static Brush? ResolveVariableBrush(LuaSemanticToken token)
		{
			if (token.HasModifier("defaultLibrary"))
				return DefaultLibraryBrush;

			if (token.HasModifier("global"))
				return GlobalVariableBrush;

			return null;
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

		private readonly struct LuaSemanticTokenStyle
		{
			public LuaSemanticTokenStyle(Brush? foreground, bool isBold, TextDecorationCollection? textDecorations)
			{
				Foreground = foreground;
				IsBold = isBold;
				TextDecorations = textDecorations;
			}

			public Brush? Foreground { get; }
			public bool IsBold { get; }
			public TextDecorationCollection? TextDecorations { get; }

			public bool HasFormatting => Foreground is not null || IsBold || TextDecorations is not null;
		}
	}
}