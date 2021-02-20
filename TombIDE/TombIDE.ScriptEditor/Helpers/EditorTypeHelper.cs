using System;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Lua;

namespace TombIDE.ScriptEditor.Helpers
{
	internal static class EditorTypeHelper
	{
		public static Type GetEditorClassType(string filePath, EditorType editorType = EditorType.Default)
		{
			if (editorType == EditorType.Default)
				editorType = GetDefaultEditorType(filePath);

			if (FileHelper.IsTextFile(filePath))
			{
				if (FileHelper.IsClassicScriptFile(filePath))
					return typeof(ClassicScriptEditor);
				else if (FileHelper.IsStringFile(filePath))
					return editorType == EditorType.Strings ? typeof(StringEditor) : typeof(ClassicScriptEditor);
				else
					return typeof(TextEditorBase);
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
