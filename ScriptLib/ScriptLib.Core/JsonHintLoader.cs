using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ScriptLib.Core;

/// <summary>
/// Loads and provides access to localized hints from embedded JSON resources.
/// </summary>
public sealed class JsonHintLoader
{
	private static readonly ConcurrentDictionary<string, JsonHintLoader> _loaderCache = new();

	/// <summary>
	/// Gets the embedded resource path for this hint loader.
	/// </summary>
	public string EmbeddedResourcePath { get; }

	private readonly IReadOnlyDictionary<string, string> _content;

	private JsonHintLoader(string embeddedResourcePath, IReadOnlyDictionary<string, string> content)
	{
		EmbeddedResourcePath = embeddedResourcePath;
		_content = content;
	}

	/// <summary>
	/// Gets a hint for the specified key. Returns <see langword="null" /> if the key is not found.
	/// </summary>
	/// <param name="key">The key to look up.</param>
	/// <returns>The hint text if found; otherwise, <see langword="null" />.</returns>
	public string? GetHint(string key)
		=> _content.TryGetValue(key, out string? value) ? value : null;

	/// <summary>
	/// Gets a hint for the specified key using the specified string comparison. Returns <see langword="null" /> if the key is not found.
	/// </summary>
	/// <param name="key">The key to look up.</param>
	/// <param name="comparisonType">The string comparison to use when matching keys.</param>
	/// <returns>The hint text if found; otherwise, <see langword="null" />.</returns>
	public string? GetHint(string key, StringComparison comparisonType)
	{
		foreach (KeyValuePair<string, string> kvp in _content)
		{
			if (kvp.Key.Equals(key, comparisonType))
				return kvp.Value;
		}

		return null;
	}

	/// <summary>
	/// Loads a hint loader from an embedded JSON resource.
	/// </summary>
	/// <param name="embeddedResourcePath">The path to the embedded resource.</param>
	/// <returns>A new JsonHintLoader instance.</returns>
	/// <exception cref="InvalidOperationException">Thrown when the assembly name cannot be determined.</exception>
	/// <exception cref="FileNotFoundException">Thrown when the resource cannot be found.</exception>
	/// <exception cref="JsonException">Thrown when the JSON content cannot be deserialized.</exception>
	public static JsonHintLoader Load(string embeddedResourcePath)
		=> _loaderCache.GetOrAdd(embeddedResourcePath, LoadInternal);

	private static JsonHintLoader LoadInternal(string embeddedResourcePath)
	{
		var assembly = Assembly.GetExecutingAssembly();
		string assemblyName = assembly.GetName().Name ??
			throw new InvalidOperationException("Could not determine assembly name");

		string resourcePath = $"{assemblyName}.{embeddedResourcePath.Replace('/', '.')}";

		using Stream stream = assembly.GetManifestResourceStream(resourcePath)
			?? throw new FileNotFoundException($"Resource not found: {resourcePath}", resourcePath);

		using var reader = new StreamReader(stream);
		string jsonContent = reader.ReadToEnd();

		Dictionary<string, string> hints = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent)
			?? throw new JsonException($"JSON content from {resourcePath} deserialized to null");

		return new JsonHintLoader(embeddedResourcePath, hints);
	}
}
