using System.IO;
using TombLib.Scripting.ClassicScript.Services;

namespace TombLib.Scripting.ClassicScript.Documents;

public sealed class ClassicScriptFileClassificationService
{
	private readonly IClassicScriptLineService _lineService;

	public ClassicScriptFileClassificationService(IClassicScriptLineService lineService)
	{
		_lineService = lineService ?? throw new ArgumentNullException(nameof(lineService));
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

	private static readonly HashSet<string> StringSections = new(StringComparer.OrdinalIgnoreCase)
	{
		"Strings",
		"PSXStrings",
		"PCStrings",
		"ExtraNG"
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

			if (StringSections.Contains(sectionName))
				return ClassicScriptFileKind.Strings;

			if (ScriptSections.Contains(sectionName))
				return ClassicScriptFileKind.Script;
		}

		return ClassicScriptFileKind.Unknown;
	}
}
