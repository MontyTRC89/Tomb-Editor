using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombIDE.ScriptEditor.Resources;
using TombIDE.Shared;

namespace TombIDE.ScriptEditor.Rendering
{
	public class ScriptSyntaxHighlighting : IHighlightingDefinition
	{
		private readonly Configuration _config = Configuration.Load();

		public string Name { get { return "Script Rules"; } }

		public HighlightingRuleSet MainRuleSet
		{
			get
			{
				HighlightingRuleSet ruleSet = new HighlightingRuleSet();

				/* Comments */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(@";.*$"),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ScriptColors_Comment))
					}
				});

				/* Sections */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(@"\[\b(" + string.Join("|", KeyWords.Sections) + @")\b\]", RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ScriptColors_Section)),
						FontWeight = FontWeights.Bold
					}
				});

				/* Old commands */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(@"\b(" + string.Join("|", KeyWords.OldCommands) + @")\b\s*?=", RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ScriptColors_OldCommand))
					}
				});

				/* New commands */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(@"\b(" + string.Join("|", KeyWords.NewCommands) + @")\b\s*?=", RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ScriptColors_NewCommand))
					}
				});

				/* Next line keys */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(">"),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ScriptColors_NewCommand)),
						FontWeight = FontWeights.Bold
					}
				});

				/* Commas */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(","),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush(Color.FromRgb(192, 192, 192)),
						FontWeight = FontWeights.Bold
					}
				});

				/* Mnemonc constants */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(@"\b(" + string.Join("|", KeyWords.AllMnemonics) + @")\b", RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ScriptColors_Reference))
					}
				});

				/* Hex values */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(@"\$[a-f0-9]*", RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ScriptColors_Reference))
					}
				});

				/* #... */
				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(@"#(define|first_id|include)\s", RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ScriptColors_Reference))
					}
				});

				ruleSet.Name = "Script Rules";
				return ruleSet;
			}
		}

		public IEnumerable<HighlightingColor> NamedHighlightingColors => throw new NotImplementedException();
		public IDictionary<string, string> Properties => throw new NotImplementedException();
		public HighlightingColor GetNamedColor(string name) => throw new NotImplementedException();
		public HighlightingRuleSet GetNamedRuleSet(string name) => throw new NotImplementedException();
	}
}
