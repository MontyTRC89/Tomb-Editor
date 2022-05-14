using System.Reflection;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Tomb1Main;

namespace TombIDE.ScriptingStudio
{
	public class ConfigurationCollection
	{
		public ClassicScriptEditorConfiguration ClassicScript = new ClassicScriptEditorConfiguration().Load<ClassicScriptEditorConfiguration>();
		public LuaEditorConfiguration Lua = new LuaEditorConfiguration().Load<LuaEditorConfiguration>();
		public GameFlowEditorConfiguration GameFlowScript = new GameFlowEditorConfiguration().Load<GameFlowEditorConfiguration>();
		public T1MEditorConfiguration Tomb1Main = new T1MEditorConfiguration().Load<T1MEditorConfiguration>();

		public void SaveAllConfigs()
		{
			foreach (FieldInfo field in GetType().GetRuntimeFields())
				(field.GetValue(this) as ConfigurationBase)?.Save();
		}
	}
}
