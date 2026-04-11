using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua.Highlighting
{
	internal sealed class LuaSemanticTokensColorizer : DocumentColorizingTransformer
	{
		private static readonly Brush DefaultLibraryBrush = CreateFrozenBrush("#4EC9B0");
		private static readonly Brush GlobalVariableBrush = CreateFrozenBrush("#4FC1FF");
		private static readonly Brush TypeBrush = CreateFrozenBrush("#4EC9B0");
		private static readonly Brush FunctionBrush = CreateFrozenBrush("#DCDCAA");
		private static readonly Brush ParameterBrush = CreateFrozenBrush("#9CDCFE");
		private static readonly Brush PropertyBrush = CreateFrozenBrush("#9CDCFE");
		private static readonly Brush DecoratorBrush = CreateFrozenBrush("#C586C0");
		private static readonly Brush MacroBrush = CreateFrozenBrush("#C586C0");
		private static readonly Brush EnumMemberBrush = CreateFrozenBrush("#B5CEA8");
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

				if (!groupedTokens.TryGetValue(token.Line, out List<LuaSemanticToken> lineTokens))
				{
					lineTokens = new List<LuaSemanticToken>();
					groupedTokens[token.Line] = lineTokens;
				}

				lineTokens.Add(token);
			}

			var frozenMap = new Dictionary<int, IReadOnlyList<LuaSemanticToken>>(groupedTokens.Count);

			foreach (KeyValuePair<int, List<LuaSemanticToken>> pair in groupedTokens)
				frozenMap[pair.Key] = pair.Value;

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
			if (!_tokensByLine.TryGetValue(line.LineNumber - 1, out IReadOnlyList<LuaSemanticToken> tokens))
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
			Brush foreground = token.Type switch
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
				null,
				token.HasModifier("deprecated") ? DeprecatedDecorations : null);
		}

		private static Brush ResolveVariableBrush(LuaSemanticToken token)
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

		private static Brush CreateFrozenBrush(string colorValue)
		{
			var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorValue));
			brush.Freeze();
			return brush;
		}

		private static TextDecorationCollection CreateTextDecorations(TextDecorationCollection source)
		{
			var clone = source.Clone();
			clone.Freeze();
			return clone;
		}

		private readonly struct LuaSemanticTokenStyle
		{
			public LuaSemanticTokenStyle(Brush foreground, bool isBold, Brush background, TextDecorationCollection textDecorations)
			{
				Foreground = foreground;
				IsBold = isBold;
				Background = background;
				TextDecorations = textDecorations;
			}

			public Brush Foreground { get; }
			public bool IsBold { get; }
			public Brush Background { get; }
			public TextDecorationCollection TextDecorations { get; }

			public bool HasFormatting => Foreground is not null || Background is not null || IsBold || TextDecorations is not null;
		}
	}
}