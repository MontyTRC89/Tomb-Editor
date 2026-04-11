using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.IO;
using System.Windows.Media;
using System.Xml;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Lua.Services;

namespace TombLib.Scripting.Lua
{
	public sealed partial class LuaEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".lua";

		public ILuaIntellisenseProvider IntellisenseProvider { get; set; }
		public Action<LuaDefinitionLocation> DefinitionNavigationRequested { get; set; }

		public LuaEditor(Version engineVersion) : base(engineVersion)
		{
			CommentPrefix = "--";
			InitializeSignaturePopup();
			BindLuaIntellisenseEvents();
		}

		public override void UpdateSettings(Bases.ConfigurationBase configuration)
		{
			var config = configuration as LuaEditorConfiguration;

			string xmlFile = Path.Combine(DefaultPaths.LuaColorConfigsDirectory, "Default.xml");

			using (var stream = new FileStream(xmlFile, FileMode.Open, FileAccess.Read))
			using (var reader = new XmlTextReader(stream))
				SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#202020"));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("White"));

			base.UpdateSettings(configuration);
			LiveErrorUnderlining = true;
		}

		private static SolidColorBrush CreateFrozenBrush(Color color)
		{
			var brush = new SolidColorBrush(color);
			brush.Freeze();
			return brush;
		}
	}
}
