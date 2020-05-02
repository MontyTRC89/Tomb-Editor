using TombLib.Scripting.Configuration;

namespace TombLib.Scripting
{
	public class TextEditorConfiguration
	{
		public ClassicScriptConfiguration ClassicScriptConfiguration;
		public LuaConfiguration LuaConfiguration;

		public static TextEditorConfiguration Load()
		{
			return new TextEditorConfiguration();
		}

		public TextEditorConfiguration()
		{
			ClassicScriptConfiguration = ClassicScriptConfiguration.Load();
			LuaConfiguration = LuaConfiguration.Load();
		}
	}
}
