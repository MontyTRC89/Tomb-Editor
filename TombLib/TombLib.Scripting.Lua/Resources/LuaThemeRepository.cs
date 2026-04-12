using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TombLib.Scripting.Highlighting;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua.Resources
{
	public static class LuaThemeRepository
	{
		private static readonly Lazy<LuaThemeCatalog> Catalog = new Lazy<LuaThemeCatalog>(LoadCatalog);

		public static IReadOnlyList<LuaTheme> GetAvailableThemes()
			=> Catalog.Value.Themes;

		public static string ResolveThemeName(string themeName)
		{
			LuaTheme theme = GetTheme(themeName);
			return theme.Name;
		}

		public static LuaTheme GetTheme(string themeName)
		{
			LuaThemeCatalog catalog = Catalog.Value;

			if (!string.IsNullOrWhiteSpace(themeName)
				&& catalog.ThemesByLookupName.TryGetValue(themeName, out LuaTheme? theme)
				&& theme is not null)
			{
				return theme;
			}

			return catalog.DefaultTheme;
		}

		private static LuaThemeCatalog LoadCatalog()
		{
			var themes = new List<LuaTheme>();
			string themesDirectory = DefaultPaths.LuaThemeConfigsDirectory;
			var serializerOptions = new JsonSerializerOptions
			{
				AllowTrailingCommas = true,
				PropertyNameCaseInsensitive = true,
				ReadCommentHandling = JsonCommentHandling.Skip
			};

			if (Directory.Exists(themesDirectory))
			{
				foreach (string filePath in Directory.GetFiles(themesDirectory, "*.json", SearchOption.TopDirectoryOnly))
				{
					try
					{
						string fileContent = File.ReadAllText(filePath);
						LuaTheme? theme = JsonSerializer.Deserialize<LuaTheme>(fileContent, serializerOptions);

						if (theme is not null)
							themes.Add(theme.Normalize(Path.GetFileNameWithoutExtension(filePath)));
					}
					catch
					{
					}
				}
			}

			if (themes.Count == 0)
				themes.Add(LuaBuiltInThemes.CreateDefaultTheme().Normalize(ConfigurationDefaults.SelectedThemeName));

			var orderedThemes = themes
				.OrderByDescending(theme => string.Equals(theme.Name, ConfigurationDefaults.SelectedThemeName, StringComparison.OrdinalIgnoreCase))
				.ThenBy(theme => theme.Name, StringComparer.OrdinalIgnoreCase)
				.ToList();

			var themesByLookupName = new Dictionary<string, LuaTheme>(StringComparer.OrdinalIgnoreCase);

			for (int i = 0; i < orderedThemes.Count; i++)
			{
				LuaTheme theme = orderedThemes[i];
				AddLookupName(themesByLookupName, theme.Name, theme);

				for (int aliasIndex = 0; aliasIndex < theme.Aliases.Count; aliasIndex++)
					AddLookupName(themesByLookupName, theme.Aliases[aliasIndex], theme);
			}

			LuaTheme defaultTheme = themesByLookupName.TryGetValue(ConfigurationDefaults.SelectedThemeName, out LuaTheme? configuredTheme)
				&& configuredTheme is not null
				? configuredTheme
				: orderedThemes[0];

			return new LuaThemeCatalog(orderedThemes, themesByLookupName, defaultTheme);
		}

		private static void AddLookupName(IDictionary<string, LuaTheme> themesByLookupName, string lookupName, LuaTheme theme)
		{
			if (string.IsNullOrWhiteSpace(lookupName) || themesByLookupName.ContainsKey(lookupName))
				return;

			themesByLookupName[lookupName] = theme;
		}

		private sealed class LuaThemeCatalog
		{
			public LuaThemeCatalog(IReadOnlyList<LuaTheme> themes, IReadOnlyDictionary<string, LuaTheme> themesByLookupName, LuaTheme defaultTheme)
			{
				Themes = themes;
				ThemesByLookupName = themesByLookupName;
				DefaultTheme = defaultTheme;
			}

			public IReadOnlyList<LuaTheme> Themes { get; }
			public IReadOnlyDictionary<string, LuaTheme> ThemesByLookupName { get; }
			public LuaTheme DefaultTheme { get; }
		}
	}
}