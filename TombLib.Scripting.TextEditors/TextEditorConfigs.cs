using TombLib.Scripting.TextEditors.Configs;

namespace TombLib.Scripting.TextEditors
{
	public sealed class TextEditorConfigs
	{
		public ClassicScriptEditorConfiguration ClassicScript { get; private set; }
		public LuaEditorConfiguration Lua { get; private set; }

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
