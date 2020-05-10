using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using TombLib.Scripting.Resources;
using TombLib.Scripting.TextEditors.ColorSchemes;

namespace TombLib.Scripting.TextEditors.SyntaxHighlighting
{
	public sealed class LuaSyntaxHighlighting : IHighlightingDefinition
	{
		private readonly LuaColorScheme _scheme;

		public LuaSyntaxHighlighting(LuaColorScheme scheme)
		{
			_scheme = scheme;
		}

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
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Comments.HtmlColor)),
						FontWeight = _scheme.Comments.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Comments.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.Values, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Values.HtmlColor)),
						FontWeight = _scheme.Values.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Values.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.Statements, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Statements.HtmlColor)),
						FontWeight = _scheme.Statements.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Statements.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.Operators, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.Operators.HtmlColor)),
						FontWeight = _scheme.Operators.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.Operators.IsItalic ? FontStyles.Italic : FontStyles.Normal
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.SpecialOperators, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_scheme.SpecialOperators.HtmlColor)),
						FontWeight = _scheme.SpecialOperators.IsBold ? FontWeights.Bold : FontWeights.Normal,
						FontStyle = _scheme.SpecialOperators.IsItalic ? FontStyles.Italic : FontStyles.Normal
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
