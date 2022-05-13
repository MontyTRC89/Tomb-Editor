using System;
using TombIDE.Shared;
using TombLib.LevelData;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.Enums;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;

namespace TombIDE.ScriptingStudio.Helpers
{
	internal static class EditorTypeHelper
	{
		public static Type GetEditorClassType(string filePath, EditorType editorType = EditorType.Default)
		{
			if (editorType == EditorType.Default)
				editorType = GetDefaultEditorType(filePath);

			if (FileHelper.IsTextFile(filePath))
			{
				if (IDE.Global.Project.GameVersion == TRVersion.Game.TR2 || IDE.Global.Project.GameVersion == TRVersion.Game.TR3)
				{
					return typeof(GameFlowEditor);
				}
				else
				{
					if (FileHelper.IsClassicScriptFile(filePath))
						return typeof(ClassicScriptEditor);
					else if (FileHelper.IsStringFile(filePath))
						return editorType == EditorType.Strings ? typeof(StringEditor) : typeof(ClassicScriptEditor);
					else
						return typeof(TextEditorBase);
				}
			}
			else if (FileHelper.IsLuaFile(filePath))
				return typeof(LuaEditor);
			else
				return null;
		}

		public static EditorType GetDefaultEditorType(string filePath)
		{
			if (FileHelper.IsStringFile(filePath))
				return EditorType.Strings;
			else
				return EditorType.Text;
		}
	}
}
