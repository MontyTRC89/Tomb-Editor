using System.IO;

namespace TombLib.Scripting.TextEditors.Configuration.Colors
{
	public sealed class LuaColors : ConfigurationBase
	{
		public override string DefaultPath { get; }

		// TODO: Add color settings

		public LuaColors() =>
			DefaultPath = Path.Combine(TextEditors.DefaultPaths.GetTextEditorConfigsPath(), "Colors", "Lua.xml");
	}
}
