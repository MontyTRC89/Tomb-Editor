using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TombLib.Scripting.Lua.Highlighting;
using TombLib.Scripting.Lua;

namespace TombLib.Scripting.Lua;

public sealed partial class LuaEditor
{
	private LuaSemanticTokensColorizer? _semanticTokensColorizer;

	/// <summary>
	/// Replaces the current semantic token set used to colorize the document.
	/// </summary>
	/// <param name="tokens">The semantic tokens to apply to the editor.</param>
	public void SetSemanticTokens(IReadOnlyList<LuaSemanticToken> tokens)
	{
		EnsureSemanticTokensColorizerAttached();
		_semanticTokensColorizer.SetTokens(tokens);
	}

	/// <summary>
	/// Removes all semantic token formatting from the current document.
	/// </summary>
	public void ClearSemanticTokens()
		=> _semanticTokensColorizer?.ClearTokens();

	[MemberNotNull(nameof(_semanticTokensColorizer))]
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
