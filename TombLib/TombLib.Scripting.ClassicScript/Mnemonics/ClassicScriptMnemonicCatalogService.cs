using System.Data;
using System.Text.RegularExpressions;
using TombLib.Scripting.Specifications.ClassicScript;
using TombLib.Scripting.Specifications.ClassicScript.Mnemonics.Models;
using TombLib.Scripting.Specifications.ClassicScript.Mnemonics.Services;

namespace TombLib.Scripting.ClassicScript.Mnemonics;

public sealed class ClassicScriptMnemonicCatalogService
{
	private static readonly object SyncRoot = new();
	private static readonly MnemonicDefinitionsLoader Loader = new();
	private static ClassicScriptMnemonicCatalogSnapshot _snapshot = LoadSnapshot(DefaultPaths.InternalNGCDirectory);

	public IReadOnlyList<string> GetAllFlags()
		=> _snapshot.AllFlags;

	public string GetMnemonicPattern()
		=> @"\b(" + string.Join("|", _snapshot.AllFlags) + @")\b";

	public bool ContainsFlag(string? flag)
		=> !string.IsNullOrWhiteSpace(flag) && _snapshot.EntriesByFlag.ContainsKey(flag);

	public bool TryGetDecimalValue(string flag, out int decimalValue)
	{
		decimalValue = 0;

		if (!_snapshot.EntriesByFlag.TryGetValue(flag, out ClassicScriptMnemonicEntry entry))
			return false;

		return int.TryParse(entry.DecimalValue, out decimalValue);
	}

	public bool TryResolveFlagByValue(string value, bool isHexValue, string? prefix, out string flagName)
	{
		flagName = string.Empty;

		if (string.IsNullOrWhiteSpace(value))
			return false;

		foreach (ClassicScriptMnemonicEntry entry in _snapshot.Entries)
		{
			string candidateValue = isHexValue ? entry.HexValue : entry.DecimalValue;

			if (!candidateValue.Equals(value, StringComparison.OrdinalIgnoreCase))
				continue;

			if (!string.IsNullOrWhiteSpace(prefix)
				&& !entry.FlagName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}

			flagName = entry.FlagName;
			return true;
		}

		return false;
	}

	public bool TryGetDescription(string flag, out string description)
	{
		description = string.Empty;

		if (!_snapshot.EntriesByFlag.TryGetValue(flag, out ClassicScriptMnemonicEntry entry))
			return false;

		description = entry.Description;
		return !string.IsNullOrWhiteSpace(description);
	}

	internal bool TryGetEntry(string flag, out ClassicScriptMnemonicEntry entry)
		=> _snapshot.EntriesByFlag.TryGetValue(flag, out entry);

	public bool TryGetPluginSyntax(string key, out string syntax)
	{
		syntax = string.Empty;

		if (!_snapshot.EntriesByFlag.TryGetValue(key, out ClassicScriptMnemonicEntry entry) || !entry.IsPlugin)
			return false;

		string? extractedSyntax = ExtractSyntax(entry.Description);

		if (string.IsNullOrWhiteSpace(extractedSyntax))
			return false;

		syntax = extractedSyntax;
		return true;
	}

	public DataTable CreateMnemonicTable()
		=> _snapshot.DataTable.Copy();

	public void Reload()
		=> Reload(DefaultPaths.InternalNGCDirectory);

	public void Reload(string pluginScriptsDirectoryPath)
	{
		lock (SyncRoot)
			_snapshot = LoadSnapshot(pluginScriptsDirectoryPath);
	}

	private static ClassicScriptMnemonicCatalogSnapshot LoadSnapshot(string pluginScriptsDirectoryPath)
	{
		MnemonicDefinitions definitions = Loader.Load(ClassicScriptResourcePaths.GetMnemonicConstantsPath(), pluginScriptsDirectoryPath);
		var entries = new List<ClassicScriptMnemonicEntry>(definitions.StandardConstants.Count + definitions.PluginConstants.Count);

		foreach (MnemonicConstantDefinition definition in definitions.StandardConstants)
			entries.Add(new ClassicScriptMnemonicEntry(definition.DecimalValue, definition.HexValue, definition.FlagName, string.Empty, false));

		foreach (PluginMnemonicDefinition definition in definitions.PluginConstants)
			entries.Add(new ClassicScriptMnemonicEntry(definition.DecimalValue.ToString(), definition.HexValue, definition.FlagName, definition.Description, true));

		var allFlags = entries.Select(entry => entry.FlagName).ToList();
		var entriesByFlag = new Dictionary<string, ClassicScriptMnemonicEntry>(StringComparer.OrdinalIgnoreCase);

		foreach (ClassicScriptMnemonicEntry entry in entries)
			entriesByFlag[entry.FlagName] = entry;

		return new ClassicScriptMnemonicCatalogSnapshot(entries, allFlags, entriesByFlag, BuildDataTable(entries));
	}

	private static DataTable BuildDataTable(IReadOnlyList<ClassicScriptMnemonicEntry> entries)
	{
		var dataTable = new DataTable();

		dataTable.Columns.Add("decimal", typeof(string));
		dataTable.Columns.Add("hex", typeof(string));
		dataTable.Columns.Add("flag", typeof(string));

		foreach (ClassicScriptMnemonicEntry entry in entries)
			dataTable.Rows.Add(entry.DecimalValue, entry.HexValue, entry.FlagName);

		return dataTable;
	}

	private static string? ExtractSyntax(string description)
	{
		if (string.IsNullOrWhiteSpace(description))
			return null;

		string[] parts = Regex.Split(description, "syntax:", RegexOptions.IgnoreCase);

		if (parts.Length <= 1)
			return null;

		return parts[1].Replace("\r", string.Empty).Split('\n')[0].Trim();
	}
}

internal sealed class ClassicScriptMnemonicCatalogSnapshot(
	IReadOnlyList<ClassicScriptMnemonicEntry> entries,
	IReadOnlyList<string> allFlags,
	IReadOnlyDictionary<string, ClassicScriptMnemonicEntry> entriesByFlag,
	DataTable dataTable)
{
	public IReadOnlyList<ClassicScriptMnemonicEntry> Entries { get; } = entries;

	public IReadOnlyList<string> AllFlags { get; } = allFlags;

	public IReadOnlyDictionary<string, ClassicScriptMnemonicEntry> EntriesByFlag { get; } = entriesByFlag;

	public DataTable DataTable { get; } = dataTable;
}

internal readonly record struct ClassicScriptMnemonicEntry(
	string DecimalValue,
	string HexValue,
	string FlagName,
	string Description,
	bool IsPlugin);
