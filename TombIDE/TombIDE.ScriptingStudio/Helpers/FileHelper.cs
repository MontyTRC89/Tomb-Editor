﻿using System;
using System.IO;
using System.Text;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Helpers;
using TombLib.Scripting.Interfaces;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Tomb1Main;

namespace TombIDE.ScriptingStudio.Helpers
{
	internal static class FileHelper
	{
		public static DocumentMode GetDocumentModeOfEditor(IEditorControl editor)
		{
			Type editorClassType = editor.GetType();

			if (editorClassType != null)
			{
				if (editorClassType == typeof(ClassicScriptEditor))
					return DocumentMode.ClassicScript;
				else if (editorClassType == typeof(StringEditor))
					return DocumentMode.Strings;
				else if (editorClassType == typeof(LuaEditor))
					return DocumentMode.Lua;
				else if (editorClassType == typeof(GameFlowEditor))
					return DocumentMode.GameFlowScript;
				else if (editorClassType == typeof(Tomb1MainEditor))
					return DocumentMode.Tomb1Main;
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

		public static bool IsJson5File(string filePath)
			=> Path.GetExtension(filePath).Equals(SupportedFormats.Json5, StringComparison.OrdinalIgnoreCase);

		public static string GetOriginalFilePathFromBackupFile(string backupFilePath)
			=> Path.Combine(Path.GetDirectoryName(backupFilePath), Path.GetFileNameWithoutExtension(backupFilePath));
	}
}
