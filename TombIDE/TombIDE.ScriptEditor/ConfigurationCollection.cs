using System.Reflection;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.Lua;

namespace TombIDE.ScriptingStudio
{
	public class ConfigurationCollection
	{
		public CS_EditorConfiguration ClassicScript = new CS_EditorConfiguration().Load<CS_EditorConfiguration>();
		public Lua_EditorConfiguration Lua = new Lua_EditorConfiguration().Load<Lua_EditorConfiguration>();

		public void SaveAllConfigs()
		{
			foreach (FieldInfo field in GetType().GetRuntimeFields())
				(field.GetValue(this) as ConfigurationBase)?.Save();
		}
	}
}
