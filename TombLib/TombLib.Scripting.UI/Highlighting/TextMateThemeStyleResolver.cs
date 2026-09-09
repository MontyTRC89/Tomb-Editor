using NLog;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.UI.Highlighting;

internal sealed class TextMateThemeStyleResolver
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly TextDecorationCollection UnderlineDecorations = CreateTextDecorations(TextDecorations.Underline);
	private static readonly TextDecorationCollection StrikethroughDecorations = CreateTextDecorations(TextDecorations.Strikethrough);

	private readonly List<ParsedThemeRule> _rules;
	private readonly Dictionary<string, TextMateHighlightingStyle> _cache = new(StringComparer.Ordinal);

	public TextMateThemeStyleResolver(TextMateTokenTheme theme)
	{
		ArgumentNullException.ThrowIfNull(theme);

		_rules = CreateRules(theme);
	}

	public TextMateHighlightingStyle Resolve(IList<string> scopes)
	{
		if (scopes is null || scopes.Count == 0)
			return TextMateHighlightingStyle.Empty;

		string cacheKey = string.Join(" ", scopes);

		if (_cache.TryGetValue(cacheKey, out TextMateHighlightingStyle? cachedStyle) && cachedStyle is not null)
			return cachedStyle;

		Brush? foreground = null;
		bool? isBold = null;
		bool? isItalic = null;
		bool? isUnderline = null;
		bool? isStrikethrough = null;

		var matches = new List<RuleMatch>();

		for (int i = 0; i < _rules.Count; i++)
		{
			int score = _rules[i].GetMatchScore(scopes);

			if (score >= 0)
				matches.Add(new RuleMatch(_rules[i], score));
		}

		matches.Sort((left, right) => right.Score.CompareTo(left.Score));

		for (int i = 0; i < matches.Count; i++)
		{
			ParsedThemeRule rule = matches[i].Rule;

			if (foreground is null && rule.Foreground is not null)
				foreground = rule.Foreground;

			if (!isBold.HasValue && rule.IsBold.HasValue)
				isBold = rule.IsBold.Value;

			if (!isItalic.HasValue && rule.IsItalic.HasValue)
				isItalic = rule.IsItalic.Value;

			if (!isUnderline.HasValue && rule.IsUnderline.HasValue)
				isUnderline = rule.IsUnderline.Value;

			if (!isStrikethrough.HasValue && rule.IsStrikethrough.HasValue)
				isStrikethrough = rule.IsStrikethrough.Value;

			if (foreground is not null
				&& isBold.HasValue
				&& isItalic.HasValue
				&& isUnderline.HasValue
				&& isStrikethrough.HasValue)
			{
				break;
			}
		}

		TextDecorationCollection? textDecorations = CreateTextDecorations(isUnderline ?? false, isStrikethrough ?? false);

		TextMateHighlightingStyle style = new(
			foreground,
			isBold ?? false,
			isItalic ?? false,
			textDecorations);

		_cache[cacheKey] = style;
		return style;
	}

	private static List<ParsedThemeRule> CreateRules(TextMateTokenTheme theme)
	{
		var rules = new List<ParsedThemeRule>();

		if (theme.Rules is null)
			return rules;

		for (int i = 0; i < theme.Rules.Count; i++)
		{
			TextMateTokenThemeRule rawRule = theme.Rules[i];

			if (rawRule is null || string.IsNullOrWhiteSpace(rawRule.Scope))
				continue;

			string[] selectors = rawRule.Scope.Split(',');

			for (int selectorIndex = 0; selectorIndex < selectors.Length; selectorIndex++)
				selectors[selectorIndex] = selectors[selectorIndex].Trim();

			Brush? foreground = null;

			if (!string.IsNullOrWhiteSpace(rawRule.Foreground))
			{
				try
				{
					foreground = CreateFrozenBrush(rawRule.Foreground);
				}
				catch (Exception exception)
				{
					Log.Warn(exception, "Invalid foreground color '{Color}' in TextMate theme rule; the rule is skipped.", rawRule.Foreground);
					foreground = null;
				}
			}

			ParseFontStyle(rawRule.FontStyle, out bool? isBold, out bool? isItalic, out bool? isUnderline, out bool? isStrikethrough);

			rules.Add(new ParsedThemeRule(selectors, foreground, isBold, isItalic, isUnderline, isStrikethrough));
		}

		return rules;
	}

	private static void ParseFontStyle(string fontStyleValue, out bool? isBold, out bool? isItalic, out bool? isUnderline, out bool? isStrikethrough)
	{
		isBold = null;
		isItalic = null;
		isUnderline = null;
		isStrikethrough = null;

		if (string.IsNullOrWhiteSpace(fontStyleValue))
			return;

		string[] parts = fontStyleValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < parts.Length; i++)
		{
			switch (parts[i].Trim())
			{
				case "bold":
					isBold = true;
					break;

				case "italic":
					isItalic = true;
					break;

				case "underline":
					isUnderline = true;
					break;

				case "strikethrough":
					isStrikethrough = true;
					break;

				case "none":
					isBold = false;
					isItalic = false;
					isUnderline = false;
					isStrikethrough = false;
					break;
			}
		}
	}

	private static TextDecorationCollection? CreateTextDecorations(bool isUnderline, bool isStrikethrough)
	{
		if (isUnderline && isStrikethrough)
		{
			var decorations = new TextDecorationCollection();

			foreach (TextDecoration decoration in UnderlineDecorations)
				decorations.Add(decoration);

			foreach (TextDecoration decoration in StrikethroughDecorations)
				decorations.Add(decoration);

			decorations.Freeze();
			return decorations;
		}

		if (isUnderline)
			return UnderlineDecorations;

		if (isStrikethrough)
			return StrikethroughDecorations;

		return null;
	}

	private static TextDecorationCollection CreateTextDecorations(TextDecorationCollection source)
	{
		var clone = source.Clone();
		clone.Freeze();
		return clone;
	}

	private sealed class ParsedThemeRule
	{
		private readonly string[] _selectors;

		public ParsedThemeRule(string[] selectors, Brush? foreground, bool? isBold, bool? isItalic, bool? isUnderline, bool? isStrikethrough)
		{
			_selectors = selectors;
			Foreground = foreground;
			IsBold = isBold;
			IsItalic = isItalic;
			IsUnderline = isUnderline;
			IsStrikethrough = isStrikethrough;
		}

		public Brush? Foreground { get; }
		public bool? IsBold { get; }
		public bool? IsItalic { get; }
		public bool? IsUnderline { get; }
		public bool? IsStrikethrough { get; }

		public int GetMatchScore(IList<string> scopes)
		{
			int bestScore = -1;

			for (int selectorIndex = 0; selectorIndex < _selectors.Length; selectorIndex++)
			{
				string selector = _selectors[selectorIndex];

				if (string.IsNullOrWhiteSpace(selector))
					continue;

				for (int scopeIndex = 0; scopeIndex < scopes.Count; scopeIndex++)
				{
					string scope = scopes[scopeIndex];

					if (!MatchesScope(selector, scope))
						continue;

					int selectorDepth = selector.Split('.').Length;
					int score = (selectorDepth * 1000) + (selector.Length * 10) + scope.Length;

					if (score > bestScore)
						bestScore = score;
				}
			}

			return bestScore;
		}

		private static bool MatchesScope(string selector, string scope)
		{
			if (string.Equals(scope, selector, StringComparison.Ordinal))
				return true;

			return scope.StartsWith(selector + ".", StringComparison.Ordinal);
		}
	}

	private readonly struct RuleMatch
	{
		public RuleMatch(ParsedThemeRule rule, int score)
		{
			Rule = rule;
			Score = score;
		}

		public ParsedThemeRule Rule { get; }
		public int Score { get; }
	}
}
