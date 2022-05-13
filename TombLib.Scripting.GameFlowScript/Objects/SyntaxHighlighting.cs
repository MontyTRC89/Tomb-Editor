using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.GameFlowScript.Resources;

namespace TombLib.Scripting.GameFlowScript.Objects
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
					Regex = new Regex(Patterns.Sections, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Sections.HtmlColor)),
						FontWeight = _scheme.Sections.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Sections.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.SpecialProperties, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.SpecialProperties.HtmlColor)),
						FontWeight = _scheme.SpecialProperties.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.SpecialProperties.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.Properties, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Properties.HtmlColor)),
						FontWeight = _scheme.Properties.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Properties.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.Constants, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Constants.HtmlColor)),
						FontWeight = _scheme.Constants.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Constants.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.Values),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Values.HtmlColor)),
						FontWeight = _scheme.Values.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Values.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Name = "GameFlowScript Rules";
				return ruleSet;
			}
		}

		#endregion Rules

		#region Other

		public string Name => "GameFlowScript Rules";

		public IEnumerable<HighlightingColor> NamedHighlightingColors => throw new NotImplementedException();
		public IDictionary<string, string> Properties => throw new NotImplementedException();

		public HighlightingColor GetNamedColor(string name)
			=> throw new NotImplementedException();

		public HighlightingRuleSet GetNamedRuleSet(string name)
			=> throw new NotImplementedException();

		#endregion Other
	}
}
