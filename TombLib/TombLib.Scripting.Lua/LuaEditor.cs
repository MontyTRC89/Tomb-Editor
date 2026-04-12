using System;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Highlighting;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.Lua.Services;

namespace TombLib.Scripting.Lua;

/// <summary>
/// Provides a Lua-specific text editor with syntax highlighting, semantic coloring, and language-service integration.
/// </summary>
public sealed partial class LuaEditor : TextEditorBase
{
	/// <summary>
	/// Gets the default file extension associated with Lua documents.
	/// </summary>
	public override string DefaultFileExtension => ".lua";

	private LuaTextMateInstallation? _textMateHighlighting;
	private LuaThemeBrushSet? _themeBrushSet;

	/// <summary>
	/// Gets or sets the IntelliSense provider used to supply completions, hover text, diagnostics, and navigation results.
	/// </summary>
	public ILuaIntellisenseProvider? IntellisenseProvider { get; set; }

	/// <summary>
	/// Occurs when the editor resolves a definition location that should be opened by the host application.
	/// </summary>
	public event Action<LuaDefinitionLocation>? DefinitionNavigationRequested;

	/// <summary>
	/// Initializes a new instance of the <see cref="LuaEditor"/> class for the specified engine version.
	/// </summary>
	/// <param name="engineVersion">The engine version used to configure editor behavior.</param>
	public LuaEditor(Version engineVersion) : base(engineVersion)
	{
		CommentPrefix = "--";
		InitializeSignaturePopup();
		BindLuaIntellisenseEvents();
	}

	/// <summary>
	/// Applies the active Lua theme, refreshes syntax highlighting, and updates shared editor settings.
	/// </summary>
	/// <param name="configuration">The editor configuration to apply.</param>
	public override void UpdateSettings(Bases.ConfigurationBase configuration)
	{
		var config = configuration as LuaEditorConfiguration;
		var theme = config?.Theme ?? LuaThemeRepository.GetTheme(ConfigurationDefaults.SelectedThemeName);
		_themeBrushSet = LuaEditorColorPalette.Create(theme);
		_textMateHighlighting?.Dispose();
		_textMateHighlighting = null;

		LuaTextMateSyntaxHighlighting.TryInstall(this, theme.TextMateTheme, out _textMateHighlighting);
		SyntaxHighlighting = null;

		EnsureSemanticTokensColorizerAttached();

		Background = _themeBrushSet.EditorBackground;
		Foreground = _themeBrushSet.EditorForeground;

		base.UpdateSettings(configuration);
		LiveErrorUnderlining = true;
	}

	private LuaThemeBrushSet GetThemeBrushSet()
	{
		_themeBrushSet ??= LuaEditorColorPalette.Create(LuaThemeRepository.GetTheme(ConfigurationDefaults.SelectedThemeName));
		return _themeBrushSet;
	}
}
