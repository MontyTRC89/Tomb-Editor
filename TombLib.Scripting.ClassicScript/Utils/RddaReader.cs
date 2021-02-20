using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Enums;
using TombLib.Scripting.ClassicScript.Objects;
using TombLib.Scripting.ClassicScript.Resources;

namespace TombLib.Scripting.ClassicScript.Utils
{
	public static class RddaReader
	{
		public static string GetKeywordDescription(string keyword, ReferenceType type)
		{
			string result = string.Empty;

			try
			{
				result = GetDescriptionFromRDDA(keyword, type);

				if (string.IsNullOrEmpty(result) && type == ReferenceType.MnemonicConstant)
					result = GetDescriptionFromPlugin(keyword); // Might be a plugin constant

				if (!string.IsNullOrEmpty(result))
					result = Regex.Replace(result, @"\r\n?|\n", "\n");
			}
			catch { }

			return result;
		}

		private static string GetDescriptionFromRDDA(string keyword, ReferenceType type)
		{
			string archivePath = Path.Combine(DefaultPaths.ReferenceDescriptionsDirectory, GetRDDAFileName(type));
			string keywordDescriptionFileName = "info_" + keyword.TrimStart('_').Replace(" ", "_").Replace("/", string.Empty) + ".txt";

			using (FileStream file = File.OpenRead(archivePath))
			using (var archive = new ZipArchive(file))
				foreach (ZipArchiveEntry entry in archive.Entries)
					if (entry.Name.Equals(keywordDescriptionFileName, StringComparison.OrdinalIgnoreCase))
						using (Stream stream = entry.Open())
						using (var reader = new StreamReader(stream))
							return reader.ReadToEnd();

			return string.Empty;
		}

		private static string GetDescriptionFromPlugin(string keyword)
		{
			foreach (PluginConstant constant in MnemonicData.PluginConstants) // Search for a definition in the plugin mnemonics list
				if (constant.FlagName.Equals(keyword, StringComparison.OrdinalIgnoreCase))
					return constant.Description;

			return string.Empty;
		}

		public static ReferenceType GetCommandType(string command)
		{
			string oldCommand = GetDescriptionFromRDDA(command, ReferenceType.OldCommand);

			// Returns NewCommand if no description was found for the specified command in "OLD Commands.rdda"
			return !string.IsNullOrEmpty(oldCommand) ? ReferenceType.OldCommand : ReferenceType.NewCommand;
		}

		public static string GetRDDAFileName(ReferenceType type)
		{
			switch (type) // Hardcoded ._.
			{
				case ReferenceType.MnemonicConstant: return "Mnemonic Constants.rdda";
				case ReferenceType.OldCommand: return "OLD Commands.rdda";
				case ReferenceType.NewCommand: return "NEW Commands.rdda";
				case ReferenceType.OCB: return "OCBs.rdda";
				default: return string.Empty;
			}
		}
	}
}
