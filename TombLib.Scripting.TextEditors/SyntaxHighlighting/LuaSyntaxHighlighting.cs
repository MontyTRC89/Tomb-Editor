using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.Resources;
using TombLib.Scripting.TextEditors.Configs;

namespace TombLib.Scripting.TextEditors.SyntaxHighlighting
{
	public sealed class LuaSyntaxHighlighting : IHighlightingDefinition
	{
		private readonly LuaEditorConfiguration _config = new LuaEditorConfiguration().Load<LuaEditorConfiguration>();

		public string Name { get { return "LUA Rules"; } }

		public HighlightingRuleSet MainRuleSet
		{
			get
			{
				HighlightingRuleSet ruleSet = new HighlightingRuleSet();

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.Comments),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Comments.HtmlColor)),
						FontWeight = _config.ColorScheme.Comments.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.Comments.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.Values, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Values.HtmlColor)),
						FontWeight = _config.ColorScheme.Values.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.Values.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.Statements, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Statements.HtmlColor)),
						FontWeight = _config.ColorScheme.Statements.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.Statements.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.Operators, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Operators.HtmlColor)),
						FontWeight = _config.ColorScheme.Operators.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.Operators.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.SpecialOperators, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.SpecialOperators.HtmlColor)),
						FontWeight = _config.ColorScheme.SpecialOperators.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _config.ColorScheme.SpecialOperators.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Name = "LUA Rules";
				return ruleSet;
			}
		}

		public IEnumerable<HighlightingColor> NamedHighlightingColors { get { throw new NotImplementedException(); } }
		public IDictionary<string, string> Properties { get { throw new NotImplementedException(); } }

		public HighlightingColor GetNamedColor(string name)
		{
			throw new NotImplementedException();
		}

		public HighlightingRuleSet GetNamedRuleSet(string name)
		{
			throw new NotImplementedException();
		}
	}
}
