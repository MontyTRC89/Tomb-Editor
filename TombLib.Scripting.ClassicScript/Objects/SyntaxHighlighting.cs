using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.ClassicScript.Resources;

namespace TombLib.Scripting.ClassicScript.Objects
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

				/* Comments */
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

				/* Sections */
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

				/* Standard commands */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.StandardCommands, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.StandardCommands.HtmlColor)),
						FontWeight = _scheme.StandardCommands.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.StandardCommands.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* New commands */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.NewCommands, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.NewCommands.HtmlColor)),
						FontWeight = _scheme.NewCommands.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.NewCommands.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* Next line keys */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(">"),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.NewCommands.HtmlColor)),
						FontWeight = FontWeights.Bold // Always bold
					}
				});

				/* Mnemonics */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.Mnemonics, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.References.HtmlColor)),
						FontWeight = _scheme.References.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.References.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* Hex values */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.HexValues, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.References.HtmlColor)),
						FontWeight = _scheme.References.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.References.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* Directives (#...) */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(Patterns.Directives, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.References.HtmlColor)),
						FontWeight = _scheme.References.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.References.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				/* Values */
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

				ruleSet.Name = "ClassicScript Rules";
				return ruleSet;
			}
		}

		#endregion Rules

		#region Other

		public string Name => "ClassicScript Rules";

		public IEnumerable<HighlightingColor> NamedHighlightingColors => throw new NotImplementedException();
		public IDictionary<string, string> Properties => throw new NotImplementedException();

		public HighlightingColor GetNamedColor(string name)
			=> throw new NotImplementedException();

		public HighlightingRuleSet GetNamedRuleSet(string name)
			=> throw new NotImplementedException();

		#endregion Other
	}
}
