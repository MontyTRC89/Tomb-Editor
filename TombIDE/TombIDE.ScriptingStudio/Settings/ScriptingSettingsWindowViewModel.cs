#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.WorkspaceProfile;
using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.Lua.Themes;
using TombLib.Scripting.TRX;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Resources;
using TombLib.Utils;
using ClassicScriptDefaults = TombLib.Scripting.ClassicScript.Resources.ConfigurationDefaults;
using GameFlowDefaults = TombLib.Scripting.GameFlowScript.Resources.ConfigurationDefaults;
using TrxDefaults = TombLib.Scripting.TRX.Resources.ConfigurationDefaults;

namespace TombIDE.ScriptingStudio.Settings;

public sealed partial class ScriptingSettingsWindowViewModel : ObservableObject
{
	private readonly ConfigurationCollection _configs = new();

	public ScriptingSettingsWindowViewModel(ScriptingWorkspaceProfile workspaceProfile, DocumentMode documentMode)
	{
		ArgumentNullException.ThrowIfNull(workspaceProfile);

		string[] fontFamilies = Fonts.SystemFontFamilies
			.Select(static family => family.Source)
			.OrderBy(static family => family, StringComparer.OrdinalIgnoreCase)
			.ToArray();

		Pages = new ObservableCollection<ScriptingSettingsPageViewModel>(CreatePages(workspaceProfile, fontFamilies));
		SelectedPage = SelectInitialPage(workspaceProfile, documentMode) ?? Pages.FirstOrDefault();
	}

	public ObservableCollection<ScriptingSettingsPageViewModel> Pages { get; }

	public string WindowTitle => "Script Editor Settings";

	[ObservableProperty]
	private bool? _dialogResult;

	[ObservableProperty]
	private ScriptingSettingsPageViewModel? _selectedPage;

	[RelayCommand]
	private void Cancel()
		=> DialogResult = false;

	[RelayCommand]
	private void ResetCurrentPage()
		=> SelectedPage?.ResetToDefaults();

	[RelayCommand]
	private void Save()
	{
		foreach (ScriptingSettingsPageViewModel page in Pages)
			page.ApplyChanges();

		_configs.SaveAllConfigs();
		DialogResult = true;
	}

	public void RefreshSelectedPagePreview()
		=> SelectedPage?.RefreshPreviewPresentation();

	partial void OnSelectedPageChanged(ScriptingSettingsPageViewModel? value)
		=> value?.RefreshPreviewPresentation();

	private IEnumerable<ScriptingSettingsPageViewModel> CreatePages(ScriptingWorkspaceProfile workspaceProfile, IReadOnlyList<string> fontFamilies)
	{
		var orderedKinds = new List<ScriptingSettingsPageKind>();

		foreach (ScriptingWorkspaceSettingsPage page in workspaceProfile.SettingsPages)
		{
			if (!orderedKinds.Contains(page.Kind))
				orderedKinds.Add(page.Kind);
		}

		foreach (ScriptingSettingsPageKind kind in GetAllSettingsPageKinds())
		{
			if (!orderedKinds.Contains(kind))
				orderedKinds.Add(kind);
		}

		foreach (ScriptingSettingsPageKind kind in orderedKinds)
		{
			yield return new ScriptingSettingsPageViewModel(
				kind,
				GetTitle(workspaceProfile, kind),
				fontFamilies,
				GetConfiguration(kind));
		}
	}

	private ScriptingSettingsPageViewModel? SelectInitialPage(ScriptingWorkspaceProfile workspaceProfile, DocumentMode documentMode)
	{
		ScriptingSettingsPageKind? preferredKind = workspaceProfile.SettingsPages
			.FirstOrDefault(page => page.Matches(documentMode))?.Kind;

		if (!preferredKind.HasValue)
			preferredKind = GetSettingsPageKind(documentMode);

		return Pages.FirstOrDefault(page => page.Kind == preferredKind)
			?? Pages.FirstOrDefault();
	}

	private static IEnumerable<ScriptingSettingsPageKind> GetAllSettingsPageKinds()
	{
		yield return ScriptingSettingsPageKind.ClassicScript;
		yield return ScriptingSettingsPageKind.GameFlowScript;
		yield return ScriptingSettingsPageKind.TRX;
		yield return ScriptingSettingsPageKind.Lua;
	}

	private static ScriptingSettingsPageKind GetSettingsPageKind(DocumentMode documentMode)
		=> documentMode switch
		{
			DocumentMode.ClassicScript => ScriptingSettingsPageKind.ClassicScript,
			DocumentMode.Strings => ScriptingSettingsPageKind.ClassicScript,
			DocumentMode.GameFlowScript => ScriptingSettingsPageKind.GameFlowScript,
			DocumentMode.TRX => ScriptingSettingsPageKind.TRX,
			DocumentMode.Lua => ScriptingSettingsPageKind.Lua,
			_ => ScriptingSettingsPageKind.ClassicScript
		};

	private static string GetTitle(ScriptingWorkspaceProfile workspaceProfile, ScriptingSettingsPageKind kind)
		=> workspaceProfile.SettingsPages.FirstOrDefault(page => page.Kind == kind)?.Title
			?? kind switch
			{
				ScriptingSettingsPageKind.ClassicScript => "TR4 / TRNG Script",
				ScriptingSettingsPageKind.GameFlowScript => "TR2 / TR3 Script",
				ScriptingSettingsPageKind.TRX => "TRX Script",
				ScriptingSettingsPageKind.Lua => "Lua",
				_ => kind.ToString()
			};

	private TextEditorConfigBase GetConfiguration(ScriptingSettingsPageKind kind)
		=> kind switch
		{
			ScriptingSettingsPageKind.ClassicScript => _configs.ClassicScript,
			ScriptingSettingsPageKind.GameFlowScript => _configs.GameFlowScript,
			ScriptingSettingsPageKind.TRX => _configs.TRX,
			ScriptingSettingsPageKind.Lua => _configs.Lua,
			_ => throw new NotSupportedException($"Unsupported settings page kind: {kind}.")
		};
}

public sealed class ScriptingSettingsPageViewModel : ObservableObject
{
	private const string ClassicPreviewSection = "[Level]";
	private const string LuaPreviewText =
		"---@class Weapon\n"
		+ "local Weapon = {}\n"
		+ "global levelName = \"Lara\"\n\n"
		+ "function Weapon:new(name)\n"
		+ "    local damage = math.max(levelName and 1 or 0, 1)\n"
		+ "    self.name = name\n"
		+ "    return damage, \"mods\\\\ten\\\\preview\", true, TEN\n"
		+ "end";

	private static readonly string[] LuaPreviewLines = LuaPreviewText.Replace("\r", string.Empty).Split('\n');
	private static readonly IReadOnlyList<LuaSemanticToken> LuaPreviewTokens = CreateLuaPreviewTokens();

	private readonly TextEditorConfigBase _config;
	private readonly int _undoStackSize;

	private bool _autoAddCommas;
	private bool _autoCloseBraces;
	private bool _autoCloseBrackets;
	private bool _autoCloseDoubleQuotes;
	private bool _autoCloseParentheses;
	private bool _autoCloseSingleQuotes;
	private bool _autocompleteEnabled;
	private bool _diagnosticsUnderliningEnabled;
	private string _fontFamily = string.Empty;
	private double _fontSize;
	private bool _highlightCurrentLine;
	private bool _intellisenseEnabled;
	private string _selectedThemeName = string.Empty;
	private bool _showLineNumbers;
	private bool _showSectionSeparators;
	private bool _showVisibleSpaces;
	private bool _showVisibleTabs;
	private bool _signatureHelpPopupsEnabled;
	private bool _tidyPostCommaSpace;
	private bool _tidyPostEqualSpace;
	private bool _tidyPreCommaSpace;
	private bool _tidyPreEqualSpace;
	private bool _tidyReduceSpaces;
	private bool _wordWrapping;

	public ScriptingSettingsPageViewModel(
		ScriptingSettingsPageKind kind,
		string title,
		IReadOnlyList<string> fontFamilies,
		TextEditorConfigBase config)
	{
		Kind = kind;
		Title = title;
		FontFamilies = fontFamilies;
		_config = config;
		_undoStackSize = Math.Max(config.UndoStackSize, TextEditorBaseDefaults.UndoStackSize);

		Description = kind switch
		{
			ScriptingSettingsPageKind.ClassicScript => "Tune readability, theme presets, and the simplified Classic Script formatting rules without opening a separate tool-specific dialog.",
			ScriptingSettingsPageKind.GameFlowScript => "Keep the TR2/TR3 script profile focused on editor clarity, typography, and preset-based theme selection.",
			ScriptingSettingsPageKind.TRX => "Configure the JSON5-oriented TRX editor while keeping its format-specific conveniences grouped in one place.",
			ScriptingSettingsPageKind.Lua => "Manage Lua intellisense, closing rules, editor presentation, and theme presets from one WPF surface.",
			_ => string.Empty
		};

		SupportsDiagnosticsUnderlining = kind != ScriptingSettingsPageKind.GameFlowScript;
		SupportsSignatureHelpPopups = kind == ScriptingSettingsPageKind.Lua;
		SupportsSectionSeparators = kind == ScriptingSettingsPageKind.ClassicScript;
		SupportsClassicFormatting = kind == ScriptingSettingsPageKind.ClassicScript;
		SupportsAutoAddCommas = kind == ScriptingSettingsPageKind.TRX;

		ThemeOptions = LoadThemeOptions(kind, config);
		PreviewEditor = CreatePreviewEditor(kind);

		LoadFromConfig(config);
		RefreshPreview();
	}

	public string Description { get; }

	public IReadOnlyList<string> FontFamilies { get; }

	public string FontFamily
	{
		get => _fontFamily;
		set => SetAndRefresh(ref _fontFamily, value);
	}

	public double FontSize
	{
		get => _fontSize;
		set => SetAndRefresh(ref _fontSize, value);
	}

	public ScriptingSettingsPageKind Kind { get; }

	public TextEditorBase PreviewEditor { get; }

	public string SelectedThemeName
	{
		get => _selectedThemeName;
		set => SetAndRefresh(ref _selectedThemeName, value);
	}

	public bool SupportsAutoAddCommas { get; }

	public bool SupportsClassicFormatting { get; }

	public bool SupportsDiagnosticsUnderlining { get; }

	public bool SupportsSectionSeparators { get; }

	public bool SupportsSignatureHelpPopups { get; }

	public IReadOnlyList<string> ThemeOptions { get; }

	public string Title { get; }

	public bool AutoAddCommas
	{
		get => _autoAddCommas;
		set => SetAndRefresh(ref _autoAddCommas, value);
	}

	public bool AutoCloseBraces
	{
		get => _autoCloseBraces;
		set => SetAndRefresh(ref _autoCloseBraces, value);
	}

	public bool AutoCloseBrackets
	{
		get => _autoCloseBrackets;
		set => SetAndRefresh(ref _autoCloseBrackets, value);
	}

	public bool AutoCloseDoubleQuotes
	{
		get => _autoCloseDoubleQuotes;
		set => SetAndRefresh(ref _autoCloseDoubleQuotes, value);
	}

	public bool AutoCloseParentheses
	{
		get => _autoCloseParentheses;
		set => SetAndRefresh(ref _autoCloseParentheses, value);
	}

	public bool AutoCloseSingleQuotes
	{
		get => _autoCloseSingleQuotes;
		set => SetAndRefresh(ref _autoCloseSingleQuotes, value);
	}

	public bool AutocompleteEnabled
	{
		get => _autocompleteEnabled;
		set => SetAndRefresh(ref _autocompleteEnabled, value);
	}

	public bool DiagnosticsUnderliningEnabled
	{
		get => _diagnosticsUnderliningEnabled;
		set => SetAndRefresh(ref _diagnosticsUnderliningEnabled, value);
	}

	public bool HighlightCurrentLine
	{
		get => _highlightCurrentLine;
		set => SetAndRefresh(ref _highlightCurrentLine, value);
	}

	public bool IntellisenseEnabled
	{
		get => _intellisenseEnabled;
		set
		{
			if (!SetAndRefresh(ref _intellisenseEnabled, value))
				return;

			OnPropertyChanged(nameof(IsIntellisenseDetailsEnabled));
		}
	}

	public bool IsIntellisenseDetailsEnabled => IntellisenseEnabled;

	public bool ShowLineNumbers
	{
		get => _showLineNumbers;
		set => SetAndRefresh(ref _showLineNumbers, value);
	}

	public bool ShowSectionSeparators
	{
		get => _showSectionSeparators;
		set => SetAndRefresh(ref _showSectionSeparators, value);
	}

	public bool ShowVisibleSpaces
	{
		get => _showVisibleSpaces;
		set => SetAndRefresh(ref _showVisibleSpaces, value);
	}

	public bool ShowVisibleTabs
	{
		get => _showVisibleTabs;
		set => SetAndRefresh(ref _showVisibleTabs, value);
	}

	public bool SignatureHelpPopupsEnabled
	{
		get => _signatureHelpPopupsEnabled;
		set => SetAndRefresh(ref _signatureHelpPopupsEnabled, value);
	}

	public bool TidyPostCommaSpace
	{
		get => _tidyPostCommaSpace;
		set => SetAndRefresh(ref _tidyPostCommaSpace, value);
	}

	public bool TidyPostEqualSpace
	{
		get => _tidyPostEqualSpace;
		set => SetAndRefresh(ref _tidyPostEqualSpace, value);
	}

	public bool TidyPreCommaSpace
	{
		get => _tidyPreCommaSpace;
		set => SetAndRefresh(ref _tidyPreCommaSpace, value);
	}

	public bool TidyPreEqualSpace
	{
		get => _tidyPreEqualSpace;
		set => SetAndRefresh(ref _tidyPreEqualSpace, value);
	}

	public bool TidyReduceSpaces
	{
		get => _tidyReduceSpaces;
		set => SetAndRefresh(ref _tidyReduceSpaces, value);
	}

	public bool WordWrapping
	{
		get => _wordWrapping;
		set => SetAndRefresh(ref _wordWrapping, value);
	}

	public void ApplyChanges()
		=> ApplyToConfig(_config);

	public void RefreshPreviewPresentation()
		=> InvalidatePreview();

	public void ResetToDefaults()
	{
		switch (Kind)
		{
			case ScriptingSettingsPageKind.ClassicScript:
				LoadFromConfig(new ClassicScriptEditorConfiguration());
				break;

			case ScriptingSettingsPageKind.GameFlowScript:
				LoadFromConfig(new GameFlowEditorConfiguration());
				break;

			case ScriptingSettingsPageKind.TRX:
				LoadFromConfig(new TRXEditorConfiguration());
				break;

			case ScriptingSettingsPageKind.Lua:
				LoadFromConfig(new LuaEditorConfiguration());
				break;
		}

		RefreshPreview();
	}

	private void ApplySharedSettings(TextEditorConfigBase config)
	{
		config.FontSize = FontSize;
		config.FontFamily = string.IsNullOrWhiteSpace(FontFamily) ? TextEditorBaseDefaults.FontFamily : FontFamily;
		config.UndoStackSize = _undoStackSize;
		config.IntellisenseEnabled = IntellisenseEnabled;
		config.AutocompleteEnabled = AutocompleteEnabled;
		config.LiveErrorUnderlining = DiagnosticsUnderliningEnabled;
		config.SignatureHelpPopupsEnabled = SignatureHelpPopupsEnabled;
		config.AutoCloseParentheses = AutoCloseParentheses;
		config.AutoCloseBraces = AutoCloseBraces;
		config.AutoCloseBrackets = AutoCloseBrackets;
		config.AutoCloseDoubleQuotes = AutoCloseDoubleQuotes;
		config.AutoCloseSingleQuotes = AutoCloseSingleQuotes;
		config.WordWrapping = WordWrapping;
		config.HighlightCurrentLine = HighlightCurrentLine;
		config.ShowLineNumbers = ShowLineNumbers;
		config.ShowVisualSpaces = ShowVisibleSpaces;
		config.ShowVisualTabs = ShowVisibleTabs;
	}

	private void ApplyToConfig(TextEditorConfigBase config)
	{
		ApplySharedSettings(config);

		switch (config)
		{
			case ClassicScriptEditorConfiguration classicConfig:
				classicConfig.SelectedColorSchemeName = SelectedThemeName;
				classicConfig.ShowSectionSeparators = ShowSectionSeparators;
				classicConfig.Tidy_PreEqualSpace = TidyPreEqualSpace;
				classicConfig.Tidy_PostEqualSpace = TidyPostEqualSpace;
				classicConfig.Tidy_PreCommaSpace = TidyPreCommaSpace;
				classicConfig.Tidy_PostCommaSpace = TidyPostCommaSpace;
				classicConfig.Tidy_ReduceSpaces = TidyReduceSpaces;
				break;

			case GameFlowEditorConfiguration gameFlowConfig:
				gameFlowConfig.SelectedColorSchemeName = SelectedThemeName;
				break;

			case TRXEditorConfiguration trxConfig:
				trxConfig.SelectedColorSchemeName = SelectedThemeName;
				trxConfig.AutoAddCommas = AutoAddCommas;
				break;

			case LuaEditorConfiguration luaConfig:
				luaConfig.SelectedThemeName = SelectedThemeName;
				break;
		}
	}

	private static TextEditorBase CreatePreviewEditor(ScriptingSettingsPageKind kind)
	{
		TextEditorBase editor = kind switch
		{
			ScriptingSettingsPageKind.ClassicScript => new ClassicScriptEditor(new Version(0, 0)),
			ScriptingSettingsPageKind.GameFlowScript => new GameFlowEditor(new Version(0, 0)),
			ScriptingSettingsPageKind.TRX => new TRXEditor(new Version(0, 0)),
			ScriptingSettingsPageKind.Lua => new LuaEditor(new Version(0, 0)),
			_ => throw new NotSupportedException($"Unsupported preview editor kind: {kind}.")
		};

		editor.IsReadOnly = true;
		editor.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
		editor.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
		editor.TextArea.Margin = new Thickness(6);
		editor.Options.EnableHyperlinks = false;
		editor.Options.EnableEmailHyperlinks = false;

		return editor;
	}

	private TextEditorConfigBase CreatePreviewConfiguration()
	{
		TextEditorConfigBase previewConfig = Kind switch
		{
			ScriptingSettingsPageKind.ClassicScript => new ClassicScriptEditorConfiguration(),
			ScriptingSettingsPageKind.GameFlowScript => new GameFlowEditorConfiguration(),
			ScriptingSettingsPageKind.TRX => new TRXEditorConfiguration(),
			ScriptingSettingsPageKind.Lua => new LuaEditorConfiguration(),
			_ => throw new NotSupportedException($"Unsupported preview config kind: {Kind}.")
		};

		ApplyToConfig(previewConfig);
		return previewConfig;
	}

	private string CreateClassicPreviewText()
	{
		string preEqual = TidyPreEqualSpace ? " " : string.Empty;
		string postEqual = TidyPostEqualSpace ? " " : string.Empty;
		string preComma = TidyPreCommaSpace ? " " : string.Empty;
		string postComma = TidyPostCommaSpace ? " " : string.Empty;
		string commentGap = TidyReduceSpaces ? " " : "  ";

		return ClassicPreviewSection + "\n"
			+ "Rain" + preEqual + "=" + postEqual + "ENABLED" + preComma + "," + postComma + "12" + commentGap + "; Has error\n"
			+ "Layer1" + preEqual + "=" + postEqual + "128" + preComma + "," + postComma + "128" + preComma + "," + postComma + ">\n"
			+ "\t\t128" + preComma + "," + postComma + "-8\n"
			+ "Mirror" + preEqual + "=" + postEqual + "69" + preComma + "," + postComma + "$2137\n"
			+ ClassicPreviewSection;
	}

	private string GetPreviewText()
		=> Kind switch
		{
			ScriptingSettingsPageKind.ClassicScript => CreateClassicPreviewText(),
			ScriptingSettingsPageKind.GameFlowScript => "DESCRIPTION: Tomb Raider 2 Script File\n\nLEVEL: Scotland Temple\n\tGAME: data\\temp.tr2 // Good level\n\tSECRETS: 21\n\tTRACK: 37\nEND:",
			ScriptingSettingsPageKind.TRX => "\"levels\": [\n\t{\n\t\t\"title\": \"Vatican City\",\n\t\t\"file\": \"data\\\\level21.phd\",\n\t\t\"type\": \"normal\",\n\t\t\"music\": 37,\n\t\t\"demo\": true,\n\t}\n]",
			ScriptingSettingsPageKind.Lua => LuaPreviewText,
			_ => string.Empty
		};

	private static IReadOnlyList<string> LoadThemeOptions(ScriptingSettingsPageKind kind, TextEditorConfigBase config)
	{
		IEnumerable<string> options = kind switch
		{
			ScriptingSettingsPageKind.ClassicScript => GetThemeNames(DefaultPaths.ClassicScriptColorConfigsDirectory, "*" + ClassicScriptDefaults.ColorSchemeFileExtension),
			ScriptingSettingsPageKind.GameFlowScript => GetThemeNames(DefaultPaths.GameFlowColorConfigsDirectory, "*" + GameFlowDefaults.ColorSchemeFileExtension),
			ScriptingSettingsPageKind.TRX => TRXEditorConfiguration.GetAvailableColorSchemeFiles().Select(static path => Path.GetFileNameWithoutExtension(path) ?? string.Empty),
			ScriptingSettingsPageKind.Lua => LuaThemeRepository.GetAvailableThemes().Select(static theme => theme.Name),
			_ => Enumerable.Empty<string>()
		};

		List<string> values = options
			.Where(static option => !string.IsNullOrWhiteSpace(option))
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.OrderBy(static option => option, StringComparer.OrdinalIgnoreCase)
			.ToList();

		string selectedValue = kind switch
		{
			ScriptingSettingsPageKind.ClassicScript => ((ClassicScriptEditorConfiguration)config).SelectedColorSchemeName,
			ScriptingSettingsPageKind.GameFlowScript => ((GameFlowEditorConfiguration)config).SelectedColorSchemeName,
			ScriptingSettingsPageKind.TRX => ((TRXEditorConfiguration)config).SelectedColorSchemeName,
			ScriptingSettingsPageKind.Lua => ((LuaEditorConfiguration)config).SelectedThemeName,
			_ => string.Empty
		};

		if (!string.IsNullOrWhiteSpace(selectedValue) && !values.Contains(selectedValue, StringComparer.OrdinalIgnoreCase))
			values.Add(selectedValue);

		return values;
	}

	private void LoadFromConfig(TextEditorConfigBase config)
	{
		_fontSize = config.FontSize;
		_fontFamily = FontFamilies.Contains(config.FontFamily, StringComparer.OrdinalIgnoreCase)
			? FontFamilies.First(font => string.Equals(font, config.FontFamily, StringComparison.OrdinalIgnoreCase))
			: TextEditorBaseDefaults.FontFamily;
		_intellisenseEnabled = config.IntellisenseEnabled;
		_autocompleteEnabled = config.AutocompleteEnabled;
		_diagnosticsUnderliningEnabled = config.LiveErrorUnderlining;
		_signatureHelpPopupsEnabled = config.SignatureHelpPopupsEnabled;
		_autoCloseParentheses = config.AutoCloseParentheses;
		_autoCloseBraces = config.AutoCloseBraces;
		_autoCloseBrackets = config.AutoCloseBrackets;
		_autoCloseDoubleQuotes = config.AutoCloseDoubleQuotes;
		_autoCloseSingleQuotes = config.AutoCloseSingleQuotes;
		_wordWrapping = config.WordWrapping;
		_highlightCurrentLine = config.HighlightCurrentLine;
		_showLineNumbers = config.ShowLineNumbers;
		_showVisibleSpaces = config.ShowVisualSpaces;
		_showVisibleTabs = config.ShowVisualTabs;

		switch (config)
		{
			case ClassicScriptEditorConfiguration classicConfig:
				_selectedThemeName = classicConfig.SelectedColorSchemeName;
				_showSectionSeparators = classicConfig.ShowSectionSeparators;
				_tidyPreEqualSpace = classicConfig.Tidy_PreEqualSpace;
				_tidyPostEqualSpace = classicConfig.Tidy_PostEqualSpace;
				_tidyPreCommaSpace = classicConfig.Tidy_PreCommaSpace;
				_tidyPostCommaSpace = classicConfig.Tidy_PostCommaSpace;
				_tidyReduceSpaces = classicConfig.Tidy_ReduceSpaces;
				_autoAddCommas = false;
				break;

			case GameFlowEditorConfiguration gameFlowConfig:
				_selectedThemeName = gameFlowConfig.SelectedColorSchemeName;
				_showSectionSeparators = false;
				_tidyPreEqualSpace = false;
				_tidyPostEqualSpace = false;
				_tidyPreCommaSpace = false;
				_tidyPostCommaSpace = false;
				_tidyReduceSpaces = false;
				_autoAddCommas = false;
				break;

			case TRXEditorConfiguration trxConfig:
				_selectedThemeName = trxConfig.SelectedColorSchemeName;
				_showSectionSeparators = false;
				_tidyPreEqualSpace = false;
				_tidyPostEqualSpace = false;
				_tidyPreCommaSpace = false;
				_tidyPostCommaSpace = false;
				_tidyReduceSpaces = false;
				_autoAddCommas = trxConfig.AutoAddCommas;
				break;

			case LuaEditorConfiguration luaConfig:
				_selectedThemeName = luaConfig.SelectedThemeName;
				_showSectionSeparators = false;
				_tidyPreEqualSpace = false;
				_tidyPostEqualSpace = false;
				_tidyPreCommaSpace = false;
				_tidyPostCommaSpace = false;
				_tidyReduceSpaces = false;
				_autoAddCommas = false;
				break;
		}

		OnPropertyChanged(string.Empty);
	}

	private void RefreshPreview()
	{
		PreviewEditor.UpdateSettings(CreatePreviewConfiguration());

		string previewText = GetPreviewText();

		if (!string.Equals(PreviewEditor.Text, previewText, StringComparison.Ordinal))
			PreviewEditor.Text = previewText;

		if (PreviewEditor is LuaEditor luaEditor)
			luaEditor.SetSemanticTokens(LuaPreviewTokens);

		InvalidatePreview();
	}

	private void InvalidatePreview()
	{
		PreviewEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
		PreviewEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Selection);
		PreviewEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Caret);
		PreviewEditor.TextArea.TextView.Redraw();
		PreviewEditor.TextArea.TextView.InvalidateVisual();
		PreviewEditor.InvalidateVisual();
	}

	private bool SetAndRefresh<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (!SetProperty(ref field, value, propertyName))
			return false;

		RefreshPreview();
		return true;
	}

	private static IReadOnlyList<string> GetThemeNames(string directoryPath, string searchPattern)
	{
		if (!Directory.Exists(directoryPath))
			return Array.Empty<string>();

		return Directory.GetFiles(directoryPath, searchPattern, SearchOption.TopDirectoryOnly)
			.Select(static path => Path.GetFileNameWithoutExtension(path) ?? string.Empty)
			.Where(static name => !string.IsNullOrWhiteSpace(name))
			.OrderBy(static name => name, StringComparer.OrdinalIgnoreCase)
			.ToArray();
	}

	private static IReadOnlyList<LuaSemanticToken> CreateLuaPreviewTokens()
	{
		return new[]
		{
			CreateLuaPreviewToken(0, "Weapon", "class"),
			CreateLuaPreviewToken(1, "Weapon", "class"),
			CreateLuaPreviewToken(2, "levelName", "variable", 1, "global"),
			CreateLuaPreviewToken(4, "Weapon", "class"),
			CreateLuaPreviewToken(4, "new", "method", 1, "declaration"),
			CreateLuaPreviewToken(4, "name", "parameter"),
			CreateLuaPreviewToken(5, "math", "namespace", 1, "defaultLibrary"),
			CreateLuaPreviewToken(5, "max", "function", 1, "defaultLibrary"),
			CreateLuaPreviewToken(5, "levelName", "variable", 1, "global"),
			CreateLuaPreviewToken(6, "name", "property", 1),
			CreateLuaPreviewToken(6, "name", "parameter", 2)
		};
	}

	private static LuaSemanticToken CreateLuaPreviewToken(int lineIndex, string tokenText, string tokenType, params string[] modifiers)
		=> CreateLuaPreviewToken(lineIndex, tokenText, tokenType, 1, modifiers);

	private static LuaSemanticToken CreateLuaPreviewToken(int lineIndex, string tokenText, string tokenType, int occurrence, params string[] modifiers)
	{
		int characterIndex = GetOccurrenceIndex(LuaPreviewLines[lineIndex], tokenText, occurrence);
		return new LuaSemanticToken(lineIndex, characterIndex, tokenText.Length, tokenType, modifiers);
	}

	private static int GetOccurrenceIndex(string line, string tokenText, int occurrence)
	{
		int startIndex = -1;

		for (int currentOccurrence = 0; currentOccurrence < occurrence; currentOccurrence++)
		{
			startIndex = line.IndexOf(tokenText, startIndex + 1, StringComparison.Ordinal);

			if (startIndex < 0)
				throw new InvalidOperationException("Failed to locate preview token '" + tokenText + "'.");
		}

		return startIndex;
	}
}