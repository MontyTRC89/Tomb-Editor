using System.Diagnostics;

namespace TombIDE.Shared.SharedClasses
{
	public static class SharedMethods
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
	}
}
