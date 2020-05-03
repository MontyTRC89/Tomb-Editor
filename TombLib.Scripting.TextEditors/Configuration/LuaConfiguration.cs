using System.IO;
using System.Xml.Serialization;
using TombLib.Scripting.TextEditors.Configuration.Colors;

namespace TombLib.Scripting.TextEditors.Configuration
{
	public sealed class LuaConfiguration : GlobalTextEditorConfiguration
	{
		public override string DefaultPath { get; }

		[XmlIgnore]
		public LuaColors Colors = new LuaColors().Load<LuaColors>();

		// TODO

		public LuaConfiguration() =>
			DefaultPath = Path.Combine(TextEditors.DefaultPaths.GetTextEditorConfigsPath(), "LuaConfiguration.xml");

		public new void Save()
		{
			base.Save();
			Colors.Save();
		}
	}
}
