using TombLib.Scripting.Rendering;

namespace TombLib.Scripting.Controls
{
	public class LuaTextEditor : BaseTextEditor
	{
		public LuaTextEditor()
		{
			SyntaxHighlighting = new LuaSyntaxHighlighting();
		}

		// TODO
	}
}
