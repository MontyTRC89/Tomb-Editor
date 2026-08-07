#nullable enable

using System.IO;
using System.Text.Json;

namespace TombLib.Scripting.ClassicScript.Commands;

// Parses the unified command catalog (Resources/ClassicScript/Commands.json) into a
// read-only model of sections and command entries. Every entry carries the command name,
// its kind (new/old/customize/parameter), the syntax variants keyed for lookup, and an
// optional description key that differs from the name. Missing or malformed files yield
// an empty catalog, matching the legacy command and syntax loaders.
public sealed class ClassicScriptCommandsLoader
{
	public ClassicScriptCommandsCatalog Load()
		=> Load(ClassicScriptResourcePaths.GetCommandsPath());

	public ClassicScriptCommandsCatalog Load(string filePath)
	{
		if (!File.Exists(filePath))
			return new ClassicScriptCommandsCatalog([], []);

		try
		{
			using JsonDocument document = JsonDocument.Parse(File.ReadAllText(filePath));

			if (document.RootElement.ValueKind != JsonValueKind.Object)
				return new ClassicScriptCommandsCatalog([], []);

			return new ClassicScriptCommandsCatalog(
				ReadSections(document.RootElement),
				ReadCommands(document.RootElement));
		}
		catch (Exception)
		{
			return new ClassicScriptCommandsCatalog([], []);
		}
	}

	private static IReadOnlyList<string> ReadSections(JsonElement root)
	{
		if (!root.TryGetProperty("Sections", out JsonElement sections) || sections.ValueKind != JsonValueKind.Array)
			return [];

		var result = new List<string>();

		foreach (JsonElement section in sections.EnumerateArray())
			if (section.ValueKind == JsonValueKind.String)
				result.Add(section.GetString() ?? string.Empty);

		return result;
	}

	private static IReadOnlyList<ClassicScriptCommandEntry> ReadCommands(JsonElement root)
	{
		if (!root.TryGetProperty("Commands", out JsonElement commands) || commands.ValueKind != JsonValueKind.Array)
			return [];

		var result = new List<ClassicScriptCommandEntry>();

		foreach (JsonElement command in commands.EnumerateArray())
		{
			ClassicScriptCommandEntry? entry = ReadCommand(command);

			if (entry is not null)
				result.Add(entry);
		}

		return result;
	}

	private static ClassicScriptCommandEntry? ReadCommand(JsonElement element)
	{
		if (element.ValueKind != JsonValueKind.Object)
			return null;

		string? name = GetStringProperty(element, "Name");

		if (string.IsNullOrWhiteSpace(name))
			return null;

		if (!TryGetKind(GetStringProperty(element, "Kind"), out ClassicScriptCommandKind kind))
			return null;

		return new ClassicScriptCommandEntry(
			name,
			kind,
			ReadSyntaxes(element),
			GetStringProperty(element, "DescriptionKey"));
	}

	private static IReadOnlyList<ClassicScriptSyntaxEntry> ReadSyntaxes(JsonElement element)
	{
		if (!element.TryGetProperty("Syntaxes", out JsonElement syntaxes) || syntaxes.ValueKind != JsonValueKind.Array)
			return [];

		var result = new List<ClassicScriptSyntaxEntry>();

		foreach (JsonElement syntax in syntaxes.EnumerateArray())
		{
			if (syntax.ValueKind != JsonValueKind.Object)
				continue;

			string? key = GetStringProperty(syntax, "Key");
			string? text = GetStringProperty(syntax, "Text");

			if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(text))
				continue;

			result.Add(new ClassicScriptSyntaxEntry(key, text));
		}

		return result;
	}

	private static string? GetStringProperty(JsonElement element, string propertyName)
	{
		if (!element.TryGetProperty(propertyName, out JsonElement property) || property.ValueKind != JsonValueKind.String)
			return null;

		return property.GetString();
	}

	private static bool TryGetKind(string? kind, out ClassicScriptCommandKind result)
	{
		if (string.Equals(kind, "new", StringComparison.OrdinalIgnoreCase))
		{
			result = ClassicScriptCommandKind.New;
			return true;
		}

		if (string.Equals(kind, "old", StringComparison.OrdinalIgnoreCase))
		{
			result = ClassicScriptCommandKind.Old;
			return true;
		}

		if (string.Equals(kind, "customize", StringComparison.OrdinalIgnoreCase))
		{
			result = ClassicScriptCommandKind.Customize;
			return true;
		}

		if (string.Equals(kind, "parameter", StringComparison.OrdinalIgnoreCase))
		{
			result = ClassicScriptCommandKind.Parameter;
			return true;
		}

		result = default;
		return false;
	}
}

public enum ClassicScriptCommandKind
{
	New,
	Old,
	Customize,
	Parameter
}

public sealed record class ClassicScriptSyntaxEntry(string Key, string Text);

public sealed record class ClassicScriptCommandEntry(
	string Name,
	ClassicScriptCommandKind Kind,
	IReadOnlyList<ClassicScriptSyntaxEntry> Syntaxes,
	string? DescriptionKey);

public sealed record class ClassicScriptCommandsCatalog(
	IReadOnlyList<string> Sections,
	IReadOnlyList<ClassicScriptCommandEntry> Commands);
