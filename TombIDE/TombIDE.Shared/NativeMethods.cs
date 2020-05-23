using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TombIDE.Shared
{
	internal static class NativeMethods
	{
		[DllImport("shell32.dll")]
		public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);
	}
}
