using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Media;
using TombLib.Scripting.Resources;
using TombLib.Scripting.TextEditors.Configuration.Colors;

namespace TombLib.Scripting.TextEditors.SyntaxHighlighting
{
	public sealed class LuaSyntaxHighlighting : IHighlightingDefinition
	{
		private readonly LuaColors _colors = new LuaColors().Load<LuaColors>();

		public string Name { get { return "LUA Rules"; } }

		public HighlightingRuleSet MainRuleSet
		{
			get
			{
				HighlightingRuleSet ruleSet = new HighlightingRuleSet();

				ruleSet.Name = "LUA Rules";

                ruleSet.Rules.Add(new HighlightingRule
                {
                    Regex = new Regex(LuaPatterns.Values, RegexOptions.IgnoreCase),
                    Color = new HighlightingColor
                    {
                        Foreground = new SimpleHighlightingBrush(Colors.SpringGreen)
                    }
                });

                ruleSet.Rules.Add(new HighlightingRule
                {
                    Regex = new Regex(LuaPatterns.Keywords, RegexOptions.IgnoreCase),
                    Color = new HighlightingColor
                    {
                        Foreground = new SimpleHighlightingBrush(Colors.MediumAquamarine)
                    }
                });

                ruleSet.Rules.Add(new HighlightingRule
                {
                    Regex = new Regex(LuaPatterns.Operators, RegexOptions.IgnoreCase),
                    Color = new HighlightingColor
                    {
                        Foreground = new SimpleHighlightingBrush(Colors.Orange)
                    }
                });

                ruleSet.Rules.Add(new HighlightingRule
                {
                    Regex = new Regex(LuaPatterns.Comments),
                    Color = new HighlightingColor
                    {
                        Foreground = new SimpleHighlightingBrush(Colors.DarkGreen)
                    }
                });

                return ruleSet;
			}
		}

		public IEnumerable<HighlightingColor> NamedHighlightingColors { get { throw new NotImplementedException(); } }
		public IDictionary<string, string> Properties { get { throw new NotImplementedException(); } }

        public HighlightingColor GetNamedColor(string name) { throw new NotImplementedException(); }
        public HighlightingRuleSet GetNamedRuleSet(string name) { throw new NotImplementedException(); }
	}
}
