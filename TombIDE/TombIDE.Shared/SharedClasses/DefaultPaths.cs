using System;
using System.IO;
using System.Reflection;
using System.Text;
using TombLib.LevelData;

namespace TombIDE.Shared.SharedClasses
{
	public static class DefaultPaths
	{
		public static string GetConfigsPath()
		{ return Path.Combine(GetProgramDirectory(), "Configs"); }

		public static string GetInternalNGCPath()
		{ return Path.Combine(GetProgramDirectory(), "TIDE", "NGC"); }

		public static string GetVGEPath()
		{ return Path.Combine(GetProgramDirectory(), "TIDE", "NGC", "VGE"); }

		public static string GetVGEScriptPath()
		{ return Path.Combine(GetProgramDirectory(), "TIDE", "NGC", "VGE", "Script"); }

		public static string GetReferencesPath()
		{ return Path.Combine(GetProgramDirectory(), "TIDE", "References"); }

		public static string GetReferenceDescriptionsPath()
		{ return Path.Combine(GetProgramDirectory(), "TIDE", "References", "Descriptions"); }

		public static string GetEngineTemplatesPath()
		{ return Path.Combine(GetProgramDirectory(), "TIDE", "Templates", "Engines"); }

		public static string GetTRNGPluginsPath()
		{ return Path.Combine(GetProgramDirectory(), "TIDE", "TRNG Plugins"); }

		public static string GetProgramDirectory()
		{ return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }

		public static string GetSharedTemplatesPath(TRVersion.Game gameVersion)
		{
			switch (gameVersion)
			{
				case TRVersion.Game.TRNG: return Path.Combine(GetProgramDirectory(), "TIDE", "Templates", "TOMB4");
				case TRVersion.Game.TR5Main: return Path.Combine(GetProgramDirectory(), "TIDE", "Templates", "TOMB5");
				default: return null;
			}
		}

		public static string GetDefaultTemplatesPath(TRVersion.Game gameVersion)
		{
			switch (gameVersion)
			{
				case TRVersion.Game.TRNG: return Path.Combine(GetProgramDirectory(), "TIDE", "Templates", "TOMB4", "Defaults");
				case TRVersion.Game.TR5Main: return Path.Combine(GetProgramDirectory(), "TIDE", "Templates", "TOMB5", "Defaults");
				default: return null;
			}
		}

		/// <summary>
		/// Returns either the "System32" path or the "SysWOW64" path.
		/// </summary>
		public static string GetSystemDirectory()
		{
			StringBuilder path = new StringBuilder(260);
			NativeMethods.SHGetSpecialFolderPath(IntPtr.Zero, path, 0x0029, false);

			return path.ToString();
		}
	}
}
