using System.IO;
using System.Xml.Serialization;
using TombLib.Scripting.Configuration.TextEditors.Colors;
using TombLib.Scripting.Helpers;

namespace TombLib.Scripting.Configuration.TextEditors
{
	public sealed class LuaConfiguration : GlobalTextEditorConfiguration
	{
		public override string DefaultPath { get; }

		[XmlIgnore]
		public LuaColors Colors = new LuaColors().Load<LuaColors>();

		// TODO

		public LuaConfiguration() =>
			DefaultPath = Path.Combine(PathHelper.GetTextEditorConfigsPath(), "LuaConfiguration.xml");

		public new void Save()
		{
			base.Save();
			Colors.Save();
		}
	}
}
