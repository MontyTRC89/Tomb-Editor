using System;
using System.IO;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Helpers;
using TombLib.Scripting.Lua;

namespace TombIDE.ScriptingStudio.Helpers
{
	internal static class FileHelper
	{
		public static DocumentMode GetDocumentModeForFile(string filePath, EditorType editorType = EditorType.Default)
		{
			Type editorClassType = EditorTypeHelper.GetEditorClassType(filePath, editorType);

			if (editorClassType != null)
			{
				if (editorClassType == typeof(ClassicScriptEditor))
					return DocumentMode.ClassicScript;
				else if (editorClassType == typeof(StringEditor))
					return DocumentMode.Strings;
				else if (editorClassType == typeof(LuaEditor))
					return DocumentMode.Lua;
			}

			return DocumentMode.PlainText;
		}

		public static bool IsStringFile(string filePath)
		{
			string[] lines = File.ReadAllLines(filePath);

			foreach (string line in lines)
				if (LineParser.IsSectionHeaderLine(line))
				{
					string text = LineParser.GetSectionHeaderText(line);

					if (StringHelper.BulkStringComparision(text, StringComparison.OrdinalIgnoreCase,
						"Strings", "PSXStrings", "PCStrings", "ExtraNG"))
						return true;
				}

			return false;
		}

		public static bool IsClassicScriptFile(string filePath)
		{
			string[] lines = File.ReadAllLines(filePath);

			foreach (string line in lines)
				if (LineParser.IsSectionHeaderLine(line))
				{
					string text = LineParser.GetSectionHeaderText(line);

					if (StringHelper.BulkStringComparision(text, StringComparison.OrdinalIgnoreCase,
						"PSXExtensions", "PCExtensions", "Language", "Options", "Title", "Level"))
						return true;
				}

			return false;
		}

		public static bool IsLuaFile(string filePath)
			=> Path.GetExtension(filePath).Equals(SupportedFormats.Lua, StringComparison.OrdinalIgnoreCase);

		public static bool IsTextFile(string filePath)
			=> Path.GetExtension(filePath).Equals(SupportedFormats.Text, StringComparison.OrdinalIgnoreCase);

		public static string GetOriginalFilePathFromBackupFile(string backupFilePath)
			=> Path.Combine(Path.GetDirectoryName(backupFilePath), Path.GetFileNameWithoutExtension(backupFilePath));
	}
}
