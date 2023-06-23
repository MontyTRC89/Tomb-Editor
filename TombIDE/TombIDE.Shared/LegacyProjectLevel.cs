using System.Xml.Serialization;

namespace TombIDE.Shared
{
	[XmlRoot("ProjectLevel")]
	public class LegacyProjectLevel
	{
		public string Name { get; set; }
		public string DataFileName { get; set; }
		public string FolderPath { get; set; }
		public string SpecificFile { get; set; } = "$(LatestFile)";
	}
}
