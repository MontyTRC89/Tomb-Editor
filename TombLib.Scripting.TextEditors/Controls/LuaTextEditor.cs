using TombLib.Scripting.TextEditors.SyntaxHighlighting;

namespace TombLib.Scripting.TextEditors.Controls
{
	public sealed class LuaTextEditor : TextEditorBase
	{
		public LuaTextEditor()
		{
			SyntaxHighlighting = new LuaSyntaxHighlighting();
		}

		// TODO
	}
}
