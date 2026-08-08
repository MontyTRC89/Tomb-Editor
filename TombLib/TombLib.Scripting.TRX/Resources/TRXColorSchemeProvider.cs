using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Scripting.UI.Providers;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.TRX.Resources;

/// <summary>
/// Provides the TRX color schemes, including legacy file tiers, through the shared scripting color provider contract.
/// </summary>
public sealed class TRXColorSchemeProvider : FileSystemColorSchemeProvider<TRXEditorConfiguration>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TRXColorSchemeProvider"/> class.
	/// </summary>
	public TRXColorSchemeProvider()
		: base(ScriptingPaths.Default.TRXColorConfigsDirectory)
	{
	}

	/// <inheritdoc />
	public override IReadOnlyList<string> GetAvailableNames()
		=> FilterNames(GetAvailableColorSchemeFiles());

	private static IReadOnlyList<string> GetAvailableColorSchemeFiles()
	{
		if (!Directory.Exists(ScriptingPaths.Default.TRXColorConfigsDirectory))
			return [];

		var filePathsByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		foreach (string filePath in Directory.GetFiles(ScriptingPaths.Default.TRXColorConfigsDirectory, "*" + ConfigurationDefaults.OldLegacyColorSchemeFileExtension, SearchOption.TopDirectoryOnly))
			filePathsByName[Path.GetFileNameWithoutExtension(filePath)] = filePath;

		foreach (string filePath in Directory.GetFiles(ScriptingPaths.Default.TRXColorConfigsDirectory, "*" + ConfigurationDefaults.LegacyColorSchemeFileExtension, SearchOption.TopDirectoryOnly))
			filePathsByName[Path.GetFileNameWithoutExtension(filePath)] = filePath;

		foreach (string filePath in Directory.GetFiles(ScriptingPaths.Default.TRXColorConfigsDirectory, "*" + ScriptingDefaults.ColorSchemeFileExtension, SearchOption.TopDirectoryOnly))
			filePathsByName[Path.GetFileNameWithoutExtension(filePath)] = filePath;

		return [..
			filePathsByName
				.OrderBy(entry => entry.Key, StringComparer.OrdinalIgnoreCase)
				.Select(entry => entry.Value)];
	}
}
