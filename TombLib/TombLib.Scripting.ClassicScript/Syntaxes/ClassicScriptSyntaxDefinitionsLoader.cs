#nullable enable

using System.IO;
using System.Xml.Linq;

namespace TombLib.Scripting.ClassicScript.Syntaxes;

public sealed class ClassicScriptSyntaxDefinitionsLoader
{
	public IReadOnlyList<ClassicScriptSyntaxDefinition> Load(string filePath)
	{
		if (!File.Exists(filePath))
			return [];

		try
		{
			var document = XDocument.Load(filePath);

			return [..
				document.Root?
					.Elements("data")
					.Select(CreateDefinition)
					.Where(definition => definition is not null)
					.Select(definition => definition!)
					?? []];
		}
		catch (Exception)
		{
			return [];
		}
	}

	private static ClassicScriptSyntaxDefinition? CreateDefinition(XElement element)
	{
		string key = element.Attribute("name")?.Value ?? string.Empty;
		string syntaxText = element.Element("value")?.Value ?? string.Empty;

		if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(syntaxText))
			return null;

		return new ClassicScriptSyntaxDefinition(
			key,
			syntaxText,
			GetApplicableSection(syntaxText),
			GetArgumentCount(syntaxText),
			ContainsArrayArgument(syntaxText));
	}

	private static bool ContainsArrayArgument(string syntaxText)
		=> syntaxText.Contains("ARRAY", StringComparison.OrdinalIgnoreCase);

	private static string GetApplicableSection(string syntaxText)
	{
		int sectionStart = syntaxText.IndexOf('[');
		int sectionEnd = syntaxText.IndexOf(']');

		if (sectionStart < 0 || sectionEnd <= sectionStart)
			return string.Empty;

		return syntaxText.Substring(sectionStart + 1, sectionEnd - sectionStart - 1).Trim();
	}

	private static int GetArgumentCount(string syntaxText)
	{
		int sectionEnd = syntaxText.IndexOf(']');
		string argumentsText = sectionEnd >= 0 ? syntaxText[(sectionEnd + 1)..].Trim() : syntaxText.Trim();

		if (string.IsNullOrWhiteSpace(argumentsText))
			return 0;

		return argumentsText.Split(',').Length;
	}
}
