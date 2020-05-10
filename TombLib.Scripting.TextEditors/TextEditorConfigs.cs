using TombLib.Scripting.TextEditors.Configs;

namespace TombLib.Scripting.TextEditors
{
	public sealed class TextEditorConfigs
	{
		public ClassicScriptEditorConfiguration ClassicScript;
		public LuaEditorConfiguration Lua;

		public static TextEditorConfigs Load()
		{
			return new TextEditorConfigs
			{
				ClassicScript = new ClassicScriptEditorConfiguration().Load<ClassicScriptEditorConfiguration>(),
				Lua = new LuaEditorConfiguration().Load<LuaEditorConfiguration>()
			};
		}
	}
}
