using System.Xml.Serialization;
using TombLib.Utils;

namespace TombIDE.Shared.NewStructure.Implementations
{
	[XmlRoot("LevelProject")]
	public class TrlvlFile : ITrlvl
	{
		[XmlAttribute]
		public string FileFormatVersion { get; set; } = "1.0";

		public string LevelName { get; set; }
		public string TargetPrj2FileName { get; set; }

		public void WriteToFile(string filePath)
			=> XmlUtils.WriteXmlFile(filePath, this);
	}
}
