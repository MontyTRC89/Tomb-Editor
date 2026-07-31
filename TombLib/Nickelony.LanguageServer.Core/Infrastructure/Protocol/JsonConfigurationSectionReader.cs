using System.Text.Json;

namespace Nickelony.LanguageServer.Core;

/// <summary>
/// Resolves dotted workspace/configuration section paths against a serialized settings payload.
/// </summary>
internal static class JsonConfigurationSectionReader
{
	/// <summary>
	/// Extracts a nested configuration section from a serialized settings payload.
	/// </summary>
	/// <param name="settingsElement">The serialized root settings element.</param>
	/// <param name="section">The dotted configuration section path.</param>
	/// <returns>The extracted section object, or <see langword="null"/> when the section is missing.</returns>
	internal static object? GetSection(JsonElement settingsElement, string? section)
	{
		if (string.IsNullOrWhiteSpace(section))
			return settingsElement.Clone();

		JsonElement currentSection = settingsElement;
		string[] parts = section.Split('.');

		for (int i = 0; i < parts.Length; i++)
		{
			if (currentSection.ValueKind is not JsonValueKind.Object
				|| !TryGetProperty(currentSection, parts[i], out JsonElement nextSection))
			{
				return null;
			}

			currentSection = nextSection;
		}

		return currentSection.Clone();
	}

	private static bool TryGetProperty(JsonElement element, string propertyName, out JsonElement value)
	{
		if (element.TryGetProperty(propertyName, out value))
			return true;

		foreach (JsonProperty property in element.EnumerateObject())
		{
			if (!string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
				continue;

			value = property.Value;
			return true;
		}

		value = default;
		return false;
	}
}
