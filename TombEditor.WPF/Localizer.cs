using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace TombEditor.WPF
{
	public class Localizer : INotifyPropertyChanged
	{
		public static Localizer Instance { get; set; } = new Localizer();

		private const string IndexerName = "Item";
		private const string IndexerArrayName = "Item[]";

		private Dictionary<string, string>? Strings;

		public bool LoadLanguage(string language)
		{
			Language = language;
			Stream? resource = Assembly.GetExecutingAssembly().GetManifestResourceStream($"{GetType().Namespace}.Assets.i18n.{language}.json");

			if (resource is not null)
			{
				using var reader = new StreamReader(resource);
				Strings = JsonSerializer.Deserialize<Dictionary<string, string>>(reader.ReadToEnd(), new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip });

				Invalidate();
				return true;
			}

			return false;
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
}
