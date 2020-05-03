using TombLib.Scripting.Configuration.TextEditors;

namespace TombLib.Scripting
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
