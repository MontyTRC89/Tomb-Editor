using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.IO;
using System.Windows.Media;
using System.Xml;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Highlighting;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".lua";

		private LuaTextMateInstallation? _textMateHighlighting;

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
			ColorScheme colorScheme = config?.ColorScheme ?? new ColorScheme();
			_textMateHighlighting?.Dispose();
			_textMateHighlighting = null;

			if (!LuaTextMateSyntaxHighlighting.TryInstall(this, out _textMateHighlighting))
				SyntaxHighlighting = LoadFallbackSyntaxHighlighting() ?? new SyntaxHighlighting(colorScheme);
			else
				SyntaxHighlighting = null;

			EnsureSemanticTokensColorizerAttached();

			Background = CreateEditorBrush(colorScheme.Background, "#202020");
			Foreground = CreateEditorBrush(colorScheme.Foreground, "Gainsboro");

			base.UpdateSettings(configuration);
			LiveErrorUnderlining = true;
		}

		private static IHighlightingDefinition? LoadFallbackSyntaxHighlighting()
		{
			string fallbackDefinitionPath = Path.Combine(
				AppContext.BaseDirectory,
				"Configs",
				"TextEditors",
				"ColorSchemes",
				"Lua",
				"Default.xml");

			if (!File.Exists(fallbackDefinitionPath))
				return null;

			using var stream = new FileStream(fallbackDefinitionPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var reader = new XmlTextReader(stream);
			return HighlightingLoader.Load(reader, HighlightingManager.Instance);
		}

		private static SolidColorBrush CreateEditorBrush(string colorValue, string fallbackColorValue)
		{
			try
			{
				string effectiveColor = string.IsNullOrWhiteSpace(colorValue) ? fallbackColorValue : colorValue;
				var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(effectiveColor));

				if (brush.CanFreeze)
					brush.Freeze();

				return brush;
			}
			catch
			{
				var fallbackBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fallbackColorValue));

				if (fallbackBrush.CanFreeze)
					fallbackBrush.Freeze();

				return fallbackBrush;
			}
		}
	}
}
