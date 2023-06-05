using System;

namespace TombIDE.Shared.NewStructure
{
	public interface ITrmap
	{
		Version Version { get; }
		void WriteToFile(string filePath);
	}
}
