using NLog;
using System;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Highlighting;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Resources;
using TombLib.Scripting.Lua.Services;
using TombLib.Scripting.Lua.Utils;

namespace TombLib.Scripting.Lua;

/// <summary>
/// Provides a Lua-specific text editor with syntax highlighting, semantic coloring, and language-service integration.
/// </summary>
public sealed partial class LuaEditor : TextEditorBase
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();
	private readonly LuaCompletionController _completionController;
	private readonly LuaDefinitionNavigationController _definitionNavigationController;
	private readonly LuaHoverController _hoverController;
	private readonly LuaSignatureHelpController _signatureHelpController;

	/// <summary>
	/// Gets the default file extension associated with Lua documents.
	/// </summary>
	public override string DefaultFileExtension => ".lua";

	private LuaTextMateInstallation? _textMateHighlighting;
	private LuaThemeBrushSet? _themeBrushSet;
	private int _editorDocumentVersion;
	private int _editorRequestGeneration;

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
		TextArea.IndentationStrategy = new LuaAutoIndentationStrategy(Options);
		_completionController = new LuaCompletionController(this);
		_definitionNavigationController = new LuaDefinitionNavigationController(this);
		_hoverController = new LuaHoverController(this);
		_signatureHelpController = new LuaSignatureHelpController(this);
		_signatureHelpController.InitializePopup();
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
		SyntaxHighlighting = _textMateHighlighting is null
			? LuaTextMateSyntaxHighlighting.LoadFallbackHighlighting()
			: null;

		EnsureSemanticTokensColorizerAttached();

		Background = _themeBrushSet.EditorBackground;
		Foreground = _themeBrushSet.EditorForeground;

		base.UpdateSettings(configuration);
		LiveErrorUnderlining = true; // TEMP - Add as a setting later
	}

	private LuaThemeBrushSet GetThemeBrushSet()
	{
		_themeBrushSet ??= LuaEditorColorPalette.Create(LuaThemeRepository.GetTheme(ConfigurationDefaults.SelectedThemeName));
		return _themeBrushSet;
	}
}
