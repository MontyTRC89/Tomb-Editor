using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua.Objects
{
	public sealed class SyntaxHighlighting : IHighlightingDefinition
	{
		private readonly ColorScheme _scheme;

		#region Construction

		public SyntaxHighlighting(ColorScheme scheme)
			=> _scheme = scheme;

		#endregion Construction

		#region Rules

		public HighlightingRuleSet MainRuleSet
		{
			get
			{
				var ruleSet = new HighlightingRuleSet();

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.Comments),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Comments.HtmlColor)),
						FontWeight = _scheme.Comments.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Comments.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.Values, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Values.HtmlColor)),
						FontWeight = _scheme.Values.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Values.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.Statements, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Statements.HtmlColor)),
						FontWeight = _scheme.Statements.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Statements.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.Operators, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Operators.HtmlColor)),
						FontWeight = _scheme.Operators.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Operators.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.SpecialOperators, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.SpecialOperators.HtmlColor)),
						FontWeight = _scheme.SpecialOperators.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.SpecialOperators.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Name = "Lua Rules";
				return ruleSet;
			}
		}

		#endregion Rules

		#region Other

		public string Name => "Lua Rules";

		public IEnumerable<HighlightingColor> NamedHighlightingColors => throw new NotImplementedException();
		public IDictionary<string, string> Properties => throw new NotImplementedException();

		public HighlightingColor GetNamedColor(string name)
			=> throw new NotImplementedException();

		public HighlightingRuleSet GetNamedRuleSet(string name)
			=> throw new NotImplementedException();

		#endregion Other
	}
}
