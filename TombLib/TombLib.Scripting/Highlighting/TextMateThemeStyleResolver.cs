using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using TextMateSharp.Themes;
using TextMateFontStyle = TextMateSharp.Themes.FontStyle;

namespace TombLib.Scripting.Highlighting
{
	internal sealed class TextMateThemeStyleResolver
	{
		private static readonly TextDecorationCollection UnderlineDecorations = CreateTextDecorations(TextDecorations.Underline);
		private static readonly TextDecorationCollection StrikethroughDecorations = CreateTextDecorations(TextDecorations.Strikethrough);

		private readonly Theme _theme;
		private readonly Dictionary<string, TextMateHighlightingStyle> _cache = new Dictionary<string, TextMateHighlightingStyle>(StringComparer.Ordinal);

		public TextMateThemeStyleResolver(Theme theme)
			=> _theme = theme ?? throw new ArgumentNullException(nameof(theme));

		public TextMateHighlightingStyle Resolve(IList<string> scopes)
		{
			if (scopes is null || scopes.Count == 0)
				return TextMateHighlightingStyle.Empty;

			string cacheKey = string.Join(" ", scopes);

			if (_cache.TryGetValue(cacheKey, out TextMateHighlightingStyle cachedStyle))
				return cachedStyle;

			int foreground = 0;
			TextMateFontStyle fontStyle = TextMateFontStyle.NotSet;
			TextDecorationCollection textDecorations = null;

			foreach (ThemeTrieElementRule rule in _theme.Match(scopes))
			{
				if (foreground == 0 && rule.foreground > 0)
					foreground = rule.foreground;

				if (fontStyle == TextMateFontStyle.NotSet && rule.fontStyle != TextMateFontStyle.NotSet)
					fontStyle = rule.fontStyle;

				if (foreground > 0 && fontStyle != TextMateFontStyle.NotSet)
					break;
			}

			if (fontStyle == TextMateFontStyle.NotSet)
				fontStyle = TextMateFontStyle.None;

			if ((fontStyle & TextMateFontStyle.Underline) == TextMateFontStyle.Underline
				&& (fontStyle & TextMateFontStyle.Strikethrough) == TextMateFontStyle.Strikethrough)
			{
				textDecorations = new TextDecorationCollection();

				foreach (TextDecoration decoration in UnderlineDecorations)
					textDecorations.Add(decoration);

				foreach (TextDecoration decoration in StrikethroughDecorations)
					textDecorations.Add(decoration);

				textDecorations.Freeze();
			}
			else if ((fontStyle & TextMateFontStyle.Underline) == TextMateFontStyle.Underline)
			{
				textDecorations = UnderlineDecorations;
			}
			else if ((fontStyle & TextMateFontStyle.Strikethrough) == TextMateFontStyle.Strikethrough)
			{
				textDecorations = StrikethroughDecorations;
			}

			Brush brush = foreground > 0
				? CreateFrozenBrush(_theme.GetColor(foreground))
				: null;

			TextMateHighlightingStyle style = new TextMateHighlightingStyle(
				brush,
				(fontStyle & TextMateFontStyle.Bold) == TextMateFontStyle.Bold,
				(fontStyle & TextMateFontStyle.Italic) == TextMateFontStyle.Italic,
				textDecorations);

			_cache[cacheKey] = style;
			return style;
		}

		private static Brush CreateFrozenBrush(string colorValue)
		{
			if (string.IsNullOrWhiteSpace(colorValue))
				return null;

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
	}
}