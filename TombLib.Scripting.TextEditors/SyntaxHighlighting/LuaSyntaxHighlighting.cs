using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
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

				// TODO: Add rules here

				ruleSet.Name = "LUA Rules";
				return ruleSet;
			}
		}

		public IEnumerable<HighlightingColor> NamedHighlightingColors => throw new NotImplementedException();
		public IDictionary<string, string> Properties => throw new NotImplementedException();
		public HighlightingColor GetNamedColor(string name) => throw new NotImplementedException();
		public HighlightingRuleSet GetNamedRuleSet(string name) => throw new NotImplementedException();
	}
}
