using System;

namespace TombIDE.Shared.NewStructure.Implementations
{
	public class MapProject : IMapProject
	{
		public string TargetPrj2FilePath => throw new NotImplementedException();

		public string Name { get; protected set; }
		public string DirectoryPath { get; protected set; }

		public string[] GetPrj2FilePaths(bool includeBackups = false) => throw new NotImplementedException();
		public string GetTrmapFilePath() => throw new NotImplementedException();

		public bool IsValid(out string errorMessage) => throw new NotImplementedException();
		public void Rename(string newName, bool renameDirectory = false) => throw new NotImplementedException();
		public void Save() => throw new NotImplementedException();
	}
}
