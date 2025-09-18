#nullable enable

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace TombLib.Forms;

public class Localizer : INotifyPropertyChanged
{
	public static Localizer Instance { get; set; } = new Localizer();

	private const string IndexerName = "Item";
	private const string IndexerArrayName = "Item[]";

	private Dictionary<string, string>? Strings;

	public bool LoadLanguage(string language)
	{
		Language = language;

		string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
			"Resources",
			"Localization",
			language.ToUpper(),
			"TombLib.Forms.json"
		);

		if (!File.Exists(filePath))
			return false;

		string fileContent = File.ReadAllText(filePath);
		Strings = JsonSerializer.Deserialize<Dictionary<string, string>>(fileContent, new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });

		Invalidate();
		return true;
	}

	public string? Language { get; private set; }

	public string this[string key]
	{
		get
		{
			if (Strings is not null && Strings.TryGetValue(key, out string? value))
				return value.Replace("\\n", "\n");

			return $"{Language}:{key}";
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public void Invalidate()
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerName));
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerArrayName));
	}
}
