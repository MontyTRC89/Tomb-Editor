using System.IO;
using System.Xml.Serialization;
using TombLib.Scripting.Configuration.TextEditors.Colors;
using TombLib.Scripting.Helpers;

namespace TombLib.Scripting.Configuration.TextEditors
{
	public sealed class ClassicScriptConfiguration : GlobalTextEditorConfiguration
	{
		public override string DefaultPath { get; }

		[XmlIgnore]
		public ClassicScriptColors Colors = new ClassicScriptColors().Load<ClassicScriptColors>();

		public bool ShowSectionSeparators { get; set; } = true;

		public bool Tidy_PreEqualSpace { get; set; } = false;
		public bool Tidy_PostEqualSpace { get; set; } = true;
		public bool Tidy_PreCommaSpace { get; set; } = false;
		public bool Tidy_PostCommaSpace { get; set; } = true;
		public bool Tidy_ReduceSpaces { get; set; } = true;

		public ClassicScriptConfiguration()
		{
			DefaultPath = Path.Combine(PathHelper.GetTextEditorConfigsPath(), "ClassicScriptConfiguration.xml");

			// These type of brackets aren't being used while writing in Classic Script, therefore auto closing should be disabled for them
			AutoCloseParentheses = false;
			AutoCloseBraces = false;
		}

		public new void Save()
		{
			base.Save();
			Colors.Save();
		}
	}
}
