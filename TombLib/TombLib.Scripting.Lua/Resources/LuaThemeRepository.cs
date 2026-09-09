using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TombLib.Scripting.Lua.Themes;
using TombLib.Scripting.UI.Resources;

namespace TombLib.Scripting.Lua.Resources;

/// <summary>
/// Loads and resolves Lua editor themes from disk, falling back to the built-in default theme when needed.
/// </summary>
public static class LuaThemeRepository
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();

	private static readonly Lazy<LuaThemeCatalog> Catalog = new(LoadCatalog);

	/// <summary>
	/// Gets all available Lua themes known to the repository.
	/// </summary>
	/// <returns>The ordered list of available themes.</returns>
	public static IReadOnlyList<LuaTheme> GetAvailableThemes() => Catalog.Value.Themes;

	/// <summary>
	/// Gets the theme matching the supplied name or alias, or the default theme when no match exists.
	/// </summary>
	/// <param name="themeName">The theme name or alias to resolve.</param>
	/// <returns>The resolved theme.</returns>
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
		string themesDirectory = ScriptingPaths.Default.LuaThemeConfigsDirectory;

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
				catch (Exception exception)
				{
					Log.Warn(exception, "Failed to load Lua theme '{FilePath}'.", filePath);
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

	private static void AddLookupName(Dictionary<string, LuaTheme> themesByLookupName, string lookupName, LuaTheme theme)
	{
		if (string.IsNullOrWhiteSpace(lookupName) || themesByLookupName.ContainsKey(lookupName))
			return;

		themesByLookupName[lookupName] = theme;
	}

	private sealed class LuaThemeCatalog(IReadOnlyList<LuaTheme> themes, IReadOnlyDictionary<string, LuaTheme> themesByLookupName, LuaTheme defaultTheme)
	{
		public IReadOnlyList<LuaTheme> Themes { get; } = themes;
		public IReadOnlyDictionary<string, LuaTheme> ThemesByLookupName { get; } = themesByLookupName;
		public LuaTheme DefaultTheme { get; } = defaultTheme;
	}
}
