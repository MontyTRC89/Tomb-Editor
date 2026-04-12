using System.Collections.Generic;
using TombLib.Scripting.Lua.Highlighting;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor
	{
		private LuaSemanticTokensColorizer? _semanticTokensColorizer;

		public void SetSemanticTokens(IReadOnlyList<LuaSemanticToken> tokens)
		{
			EnsureSemanticTokensColorizerAttached();
			_semanticTokensColorizer!.SetTokens(tokens);
		}

		public void ClearSemanticTokens()
			=> _semanticTokensColorizer?.ClearTokens();

		private void EnsureSemanticTokensColorizerAttached()
		{
			if (_semanticTokensColorizer is null)
				_semanticTokensColorizer = new LuaSemanticTokensColorizer(TextArea.TextView, GetThemeBrushSet());
			else
				_semanticTokensColorizer.UpdateTheme(GetThemeBrushSet());

			if (!TextArea.TextView.LineTransformers.Contains(_semanticTokensColorizer))
				TextArea.TextView.LineTransformers.Add(_semanticTokensColorizer);
		}
	}
}