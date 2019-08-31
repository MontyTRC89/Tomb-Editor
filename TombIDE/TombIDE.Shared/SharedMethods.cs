using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

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

		public static string RemoveIllegalPathSymbols(string fileName)
		{
			return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
		}

		public static string GetProgramDirectory()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		[DllImport("shell32.dll")]
		private static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out]StringBuilder lpszPath, int nFolder, bool fCreate);

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
