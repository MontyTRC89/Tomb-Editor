﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Projects;

namespace TombIDE.Shared
{
	public class SharedMethods
	{
		public static void OpenFolderInExplorer(string path)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo
			{
				FileName = "explorer.exe",
				Arguments = path
			};

			Process.Start(startInfo);
		}

		public static void UpdatePrj2GameSettings(string prj2FilePath, ProjectLevel destLevel, Project destProject)
		{
			Level level = Prj2Loader.LoadFromPrj2(prj2FilePath, null);

			string dataFileName = destLevel.Name.Replace(' ', '_') + destProject.GetLevelFileExtension();

			level.Settings.GameDirectory = destProject.ProjectPath;
			level.Settings.GameExecutableFilePath = Path.Combine(destProject.ProjectPath, destProject.GetExeFileName());
			level.Settings.GameLevelFilePath = Path.Combine(destProject.ProjectPath, "data", dataFileName);
			level.Settings.GameVersion = destProject.GameVersion;

			Prj2Writer.SaveToPrj2(prj2FilePath, level);
		}

		public static string RemoveIllegalSymbols(string fileName)
		{
			return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
		}

		[DllImport("shell32.dll")]
		public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out]StringBuilder lpszPath, int nFolder, bool fCreate);

		/// <summary>
		/// Returns either the "System32" path or the "SysWOW64" path.
		/// </summary>
		public static string GetSystemDirectory()
		{
			StringBuilder path = new StringBuilder(260);
			SHGetSpecialFolderPath(IntPtr.Zero, path, 0x0029, false);
			return path.ToString();
		}
	}
}
