using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Providers;

namespace TombLib.Scripting.TRX.Resources;

/// <summary>
/// Provides the TRX color schemes, including legacy file tiers, through the shared scripting color provider contract.
/// </summary>
public sealed class TRXColorSchemeProvider : ITextEditorColorProvider
{
	/// <inheritdoc />
	public IReadOnlyList<string> GetAvailableNames()
		=> TRXEditorConfiguration.GetAvailableColorSchemeFiles()
			.Select(static path => Path.GetFileNameWithoutExtension(path) ?? string.Empty)
			.Where(static name => !string.IsNullOrWhiteSpace(name))
			.OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
			.ToArray();

	/// <inheritdoc />
	public string GetSelectedName(TextEditorConfigBase config)
		=> ((TRXEditorConfiguration)config).SelectedColorSchemeName;

	/// <inheritdoc />
	public void SetSelectedName(TextEditorConfigBase config, string name)
		=> ((TRXEditorConfiguration)config).SelectedColorSchemeName = name;
}
