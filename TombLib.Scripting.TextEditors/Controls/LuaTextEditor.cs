using TombLib.Scripting.TextEditors.SyntaxHighlighting;

namespace TombLib.Scripting.TextEditors.Controls
{
	public sealed class LuaTextEditor : BaseTextEditor
	{
		public LuaTextEditor()
		{
			SyntaxHighlighting = new LuaSyntaxHighlighting();
		}

		// TODO
	}
}
