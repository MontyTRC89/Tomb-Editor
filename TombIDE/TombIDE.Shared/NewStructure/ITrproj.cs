using System;

namespace TombIDE.Shared.NewStructure
{
	public interface ITrproj
	{
		/// <summary>
		/// The path of the .trproj file.
		/// </summary>
		string FilePath { get; }

		Version Version { get; }

		void EncodeProjectPaths(string trprojFilePath);
		void DecodeProjectPaths(string trprojFilePath);

		void WriteToFile(string filePath);
	}
}
