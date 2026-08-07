using System.IO;
using TombLib.Scripting.ClassicScript.Services;

namespace TombLib.Scripting.ClassicScript.Documents;

public sealed class ClassicScriptFileClassificationService
{
	private readonly IClassicScriptLineService _lineService;

	public ClassicScriptFileClassificationService(IClassicScriptLineService lineService)
	{
		ArgumentNullException.ThrowIfNull(lineService);
		_lineService = lineService;
	}

	private static readonly HashSet<string> ScriptSections = new(StringComparer.OrdinalIgnoreCase)
	{
		"PSXExtensions",
		"PCExtensions",
		"Language",
		"Options",
		"Title",
		"Level"
	};

	public ClassicScriptFileKind GetFileKind(string filePath)
	{
		ArgumentNullException.ThrowIfNull(filePath);

		foreach (string line in File.ReadLines(filePath))
		{
			if (!_lineService.IsSectionHeaderLine(line))
				continue;

			string? sectionName = _lineService.GetSectionHeaderText(line);

			if (string.IsNullOrWhiteSpace(sectionName))
				continue;

			if (_lineService.IsStringSectionName(sectionName))
				return ClassicScriptFileKind.Strings;

			if (ScriptSections.Contains(sectionName))
				return ClassicScriptFileKind.Script;
		}

		return ClassicScriptFileKind.Unknown;
	}
}
