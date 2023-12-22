using System;
using System.Diagnostics;
using System.IO;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public sealed class TRNGGameProject : TR4GameProject
	{
		public override TRVersion.Game GameVersion => TRVersion.Game.TRNG;

		public override bool SupportsPlugins => true;

		public TRNGGameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }

		public TRNGGameProject(string name, string directoryPath, string levelsDirectoryPath, string scriptDirectoryPath, string pluginsDirectoryPath)
			: base(name, directoryPath, levelsDirectoryPath, scriptDirectoryPath, pluginsDirectoryPath)
		{ }

		public override Version GetCurrentEngineVersion()
		{
			try
			{
				string trngDllFilePath = Path.Combine(GetEngineRootDirectoryPath(), "Tomb_NextGeneration.dll");
				string versionInfo = FileVersionInfo.GetVersionInfo(trngDllFilePath).ProductVersion;
				return new Version(versionInfo.Replace(" ", string.Empty).Replace(',', '.'));
			}
			catch
			{
				return null;
			}
		}
	}
}
