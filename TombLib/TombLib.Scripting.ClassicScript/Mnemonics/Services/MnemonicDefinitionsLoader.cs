#nullable enable

using System.IO;
using System.Xml.Linq;
using TombLib.Scripting.ClassicScript.Mnemonics.Models;

namespace TombLib.Scripting.ClassicScript.Mnemonics.Services;

public sealed class MnemonicDefinitionsLoader
{
	public MnemonicDefinitions Load(string mnemonicConstantsXmlPath, string pluginScriptsDirectoryPath)
		=> new MnemonicDefinitions(
			LoadStandardConstants(mnemonicConstantsXmlPath),
			LoadPluginMnemonics(pluginScriptsDirectoryPath));

	private static IReadOnlyList<MnemonicConstantDefinition> LoadStandardConstants(string mnemonicConstantsXmlPath)
	{
		try
		{
			var document = XDocument.Load(mnemonicConstantsXmlPath);
			var definitions = document.Root?
				.Elements("row")
				.Select(row => new MnemonicConstantDefinition(
					row.Element("decimal")?.Value ?? string.Empty,
					row.Element("hex")?.Value ?? string.Empty,
					row.Element("flag")?.Value ?? string.Empty))
				.Where(definition => !string.IsNullOrWhiteSpace(definition.FlagName))
				.ToList();

			return definitions ?? [];
		}
		catch (Exception)
		{
			return [];
		}
	}

	private static IReadOnlyList<PluginMnemonicDefinition> LoadPluginMnemonics(string pluginScriptsDirectoryPath)
	{
		var pluginMnemonics = new List<PluginMnemonicDefinition>();

		try
		{
			foreach (string file in Directory.GetFiles(pluginScriptsDirectoryPath, "plugin_*.script", SearchOption.TopDirectoryOnly))
			{
				string[] lines = File.ReadAllLines(file);

				foreach (string line in lines)
				{
					if (line.StartsWith("<start_constants>", StringComparison.OrdinalIgnoreCase))
						continue;

					if (line.StartsWith("<end>", StringComparison.OrdinalIgnoreCase))
						break;

					if (string.IsNullOrWhiteSpace(line))
						continue;

					int decimalStart = line.IndexOf(':');

					if (decimalStart == -1)
						continue;

					int descriptionStart = line.IndexOf(';');
					string decimalString;
					string description = string.Empty;

					if (descriptionStart != -1)
					{
						string[] descriptionLines = line.Substring(descriptionStart + 1).Trim().Split('>');
						description = string.Join(Environment.NewLine, descriptionLines.Select(descriptionLine => descriptionLine.TrimEnd()));

						int decimalLength = descriptionStart - decimalStart - 1;
						decimalString = decimalLength > 0 ? line.Substring(decimalStart + 1, decimalLength).Trim() : string.Empty;
					}
					else
					{
						decimalString = line.Substring(decimalStart + 1).Trim();
					}

					try
					{
						short decimalValue;

						if (!short.TryParse(decimalString, out decimalValue))
							decimalValue = Convert.ToInt16(decimalString.Replace("$", string.Empty), 16);

						pluginMnemonics.Add(new PluginMnemonicDefinition(
							line.Substring(0, decimalStart).Trim(),
							description,
							decimalValue,
							FormatHexValue(decimalValue)));
					}
					catch (Exception)
					{
					}
				}
			}

			return pluginMnemonics;
		}
		catch (Exception)
		{
			return [];
		}
	}

	private static string FormatHexValue(short decimalValue)
	{
		string hexValue = decimalValue.ToString("X");
		int zerosToAdd = 4 - hexValue.Length;

		return $"${new string('0', zerosToAdd)}{hexValue}";
	}
}
