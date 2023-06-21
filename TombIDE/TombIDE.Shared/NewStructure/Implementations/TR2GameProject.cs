﻿using System;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	public class TR2GameProject : CoreStructuredProjectBase
	{
		public override TRVersion.Game GameVersion => TRVersion.Game.TR2;

		public override string DataFileExtension => ".tr2";
		public override string EngineExecutableFileName => "Tomb2.exe";

		public override bool SupportsPlugins => false;

		public TR2GameProject(TrprojFile trproj, Version targetTrprojVersion) : base(trproj, targetTrprojVersion)
		{ }

		public TR2GameProject(string name, string directoryPath, string levelsDirectoryPath, string scriptDirectoryPath)
			: base(name, directoryPath, levelsDirectoryPath, scriptDirectoryPath)
		{ }
	}
}
