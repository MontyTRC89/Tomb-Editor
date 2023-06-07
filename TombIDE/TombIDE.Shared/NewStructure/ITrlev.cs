using System;

namespace TombIDE.Shared.NewStructure
{
	public interface ITrlev
	{
		Version Version { get; }
		void WriteToFile(string filePath);
	}
}
