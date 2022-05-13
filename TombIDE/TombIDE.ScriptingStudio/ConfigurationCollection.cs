using System.Reflection;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;

namespace TombIDE.ScriptingStudio
{
	public class ConfigurationCollection
	{
		public ClassicScriptEditorConfiguration ClassicScript = new ClassicScriptEditorConfiguration().Load<ClassicScriptEditorConfiguration>();
		public LuaEditorConfiguration Lua = new LuaEditorConfiguration().Load<LuaEditorConfiguration>();
		public GameFlowEditorConfiguration GameFlowScript = new GameFlowEditorConfiguration().Load<GameFlowEditorConfiguration>();

		public void SaveAllConfigs()
		{
			foreach (FieldInfo field in GetType().GetRuntimeFields())
				(field.GetValue(this) as ConfigurationBase)?.Save();
		}
	}
}
