using System;

namespace TombIDE.Shared.NewStructure
{
	public interface ITrlvl
	{
		Version Version { get; }
		void WriteToFile(string filePath);
	}
}
