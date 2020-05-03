using System.IO;
using TombLib.Scripting.Helpers;

namespace TombLib.Scripting.Configuration.TextEditors.Colors
{
	public sealed class LuaColors : ConfigurationBase
	{
		public override string DefaultPath { get; }

		// TODO: Add color settings

		public LuaColors() =>
			DefaultPath = Path.Combine(PathHelper.GetTextEditorConfigsPath(), "Colors", "Lua.xml");
	}
}
