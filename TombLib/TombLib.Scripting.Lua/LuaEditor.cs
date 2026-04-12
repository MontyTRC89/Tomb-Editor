using ICSharpCode.AvalonEdit.Highlighting;
using System;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Highlighting;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;
using TombLib.Scripting.Lua.Resources;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".lua";

		private LuaTextMateInstallation? _textMateHighlighting;
		private LuaThemeBrushSet? _themeBrushSet;

		public ILuaIntellisenseProvider? IntellisenseProvider { get; set; }
		public event Action<LuaDefinitionLocation>? DefinitionNavigationRequested;

		public LuaEditor(Version engineVersion) : base(engineVersion)
		{
			CommentPrefix = "--";
			InitializeSignaturePopup();
			BindLuaIntellisenseEvents();
		}

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
}
