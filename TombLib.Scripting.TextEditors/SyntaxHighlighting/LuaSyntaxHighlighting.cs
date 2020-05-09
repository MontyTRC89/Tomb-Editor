using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Comments))
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.Values, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Values))
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.Statements, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Statements))
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.Operators, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.Operators))
					}
				});

				ruleSet.Rules.Add(new HighlightingRule
				{
					Regex = new Regex(LuaPatterns.SpecialOperators, RegexOptions.IgnoreCase),
					Color = new HighlightingColor
					{
						Foreground = new SimpleHighlightingBrush((Color)ColorConverter.ConvertFromString(_config.ColorScheme.SpecialOperators))
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
