using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.Objects;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class SyntaxHighlighting : IHighlightingDefinition
	{
		private static readonly IReadOnlyList<HighlightingColor> EmptyHighlightingColors = Array.Empty<HighlightingColor>();
		private static readonly IDictionary<string, string> EmptyProperties = new Dictionary<string, string>();

		private readonly ColorScheme _scheme;
		private readonly HighlightingRuleSet _mainRuleSet;

		public SyntaxHighlighting(ColorScheme scheme)
		{
			_scheme = scheme ?? new ColorScheme();
			_mainRuleSet = CreateMainRuleSet();
		}

		public HighlightingRuleSet MainRuleSet => _mainRuleSet;

		public string Name => "Lua Rules";

		public IEnumerable<HighlightingColor> NamedHighlightingColors => EmptyHighlightingColors;
		public IDictionary<string, string> Properties => EmptyProperties;

		public HighlightingColor? GetNamedColor(string name)
			=> null;

		public HighlightingRuleSet? GetNamedRuleSet(string name)
			=> string.Equals(name, Name, StringComparison.Ordinal) ? _mainRuleSet : null;

		private HighlightingRuleSet CreateMainRuleSet()
		{
			var ruleSet = new HighlightingRuleSet
			{
				Name = Name
			};

			ruleSet.Rules.Add(CreateRule(Patterns.Comments, _scheme.Comments));
			ruleSet.Rules.Add(CreateRule(Patterns.Values, _scheme.Values, RegexOptions.IgnoreCase));
			ruleSet.Rules.Add(CreateRule(Patterns.Statements, _scheme.Statements, RegexOptions.IgnoreCase));
			ruleSet.Rules.Add(CreateRule(Patterns.Operators, _scheme.Operators, RegexOptions.IgnoreCase));
			ruleSet.Rules.Add(CreateRule(Patterns.SpecialOperators, _scheme.SpecialOperators, RegexOptions.IgnoreCase));

			return ruleSet;
		}

		private static HighlightingRule CreateRule(string pattern, HighlightingObject highlighting, RegexOptions options = RegexOptions.None)
		{
			highlighting ??= new HighlightingObject();

			return new HighlightingRule
			{
				Regex = new Regex(pattern, RegexOptions.Compiled | options),
				Color = new HighlightingColor
				{
					Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(highlighting.HtmlColor)),
					FontWeight = highlighting.IsBold ? FontWeights.Bold : FontWeights.Normal,
					FontStyle = highlighting.IsItalic ? FontStyles.Italic : FontStyles.Normal
				}
			};
		}
	}
}
