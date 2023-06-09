using System;
using TombLib.Utils;

namespace TombIDE.Shared.NewStructure.Implementations
{
	public class TrlvlFile : ITrlvl
	{
		public Version Version => new(1, 0);

		public string LevelName { get; set; }
		public string TargetPrj2FileName { get; set; }

		public void WriteToFile(string filePath)
			=> XmlUtils.WriteXmlFile(filePath, this);
	}
}
