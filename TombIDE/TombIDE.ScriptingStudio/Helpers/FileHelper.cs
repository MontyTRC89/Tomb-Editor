using System;
using System.IO;
using TombLib.Scripting.ClassicScript.Documents;

namespace TombIDE.ScriptingStudio.Helpers
{
	internal static class FileHelper
	{
		private static readonly ClassicScriptFileClassificationService _classicScriptFileClassificationService = new();

		public static ClassicScriptFileKind GetClassicScriptFileKind(string filePath)
			=> _classicScriptFileClassificationService.GetFileKind(filePath);

		public static bool IsStringFile(string filePath)
			=> GetClassicScriptFileKind(filePath) == ClassicScriptFileKind.Strings;

		public static bool IsClassicScriptFile(string filePath)
			=> GetClassicScriptFileKind(filePath) == ClassicScriptFileKind.Script;

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
