using TombLib.Scripting.TextEditors.Configuration;

namespace TombLib.Scripting.TextEditors
{
	public sealed class TextEditorConfigurations
	{
		public ClassicScriptConfiguration ClassicScript = new ClassicScriptConfiguration().Load<ClassicScriptConfiguration>();
		public LuaConfiguration Lua = new LuaConfiguration().Load<LuaConfiguration>();

		public static TextEditorConfigurations Load()
		{
			return new TextEditorConfigurations();
		}
	}
}
