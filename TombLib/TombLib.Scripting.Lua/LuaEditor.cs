using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.IO;
using System.Windows.Media;
using System.Xml;
using TombLib.Scripting.Bases;

namespace TombLib.Scripting.Lua
{
	public sealed class LuaEditor : TextEditorBase
	{
		public override string DefaultFileExtension => ".lua";

		public LuaEditor()
		{
			CommentPrefix = "--";
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
		}
	}
}
