using System;
using TombIDE.ScriptingStudio.Bases;

namespace TombIDE.ScriptingStudio.WorkspaceProfile
{
	public static class ScriptingStudioFactory
	{
		public static StudioBase Create(ScriptingWorkspaceProfile workspaceProfile)
		{
			if (workspaceProfile is null)
				throw new ArgumentNullException(nameof(workspaceProfile));

			return workspaceProfile.Kind switch
			{
				ScriptingWorkspaceKind.ClassicScript => new ClassicScriptStudio(workspaceProfile),
				ScriptingWorkspaceKind.GameFlowScript => new GameFlowScriptStudio(workspaceProfile),
				ScriptingWorkspaceKind.TRX => new TRXStudio(workspaceProfile),
				ScriptingWorkspaceKind.Lua => new LuaStudio(workspaceProfile),
				_ => throw new NotSupportedException($"Unsupported scripting workspace kind: {workspaceProfile.Kind}.")
			};
		}
	}
}