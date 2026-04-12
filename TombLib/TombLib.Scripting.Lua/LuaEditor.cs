using System;
using System.Windows.Media;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Highlighting;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".lua";

		private LuaTextMateInstallation _textMateHighlighting;

		public ILuaIntellisenseProvider IntellisenseProvider { get; set; }
		public event Action<LuaDefinitionLocation> DefinitionNavigationRequested;

		public LuaEditor(Version engineVersion) : base(engineVersion)
		{
			CommentPrefix = "--";
			InitializeSignaturePopup();
			BindLuaIntellisenseEvents();
		}

		public override void UpdateSettings(Bases.ConfigurationBase configuration)
		{
			var config = configuration as LuaEditorConfiguration;
			ColorScheme colorScheme = config?.ColorScheme ?? new ColorScheme();
			_textMateHighlighting?.Dispose();
			_textMateHighlighting = null;

			if (!LuaTextMateSyntaxHighlighting.TryInstall(this, out _textMateHighlighting))
				SyntaxHighlighting = new SyntaxHighlighting(colorScheme);
			else
				SyntaxHighlighting = null;

			EnsureSemanticTokensColorizerAttached();

			Background = CreateEditorBrush(colorScheme.Background, "#202020");
			Foreground = CreateEditorBrush(colorScheme.Foreground, "White");

			base.UpdateSettings(configuration);
			LiveErrorUnderlining = true;
		}

		private static SolidColorBrush CreateEditorBrush(string colorValue, string fallbackColorValue)
		{
			try
			{
				string effectiveColor = string.IsNullOrWhiteSpace(colorValue) ? fallbackColorValue : colorValue;
				return new SolidColorBrush((Color)ColorConverter.ConvertFromString(effectiveColor));
			}
			catch
			{
				return new SolidColorBrush((Color)ColorConverter.ConvertFromString(fallbackColorValue));
			}
		}
	}
}
