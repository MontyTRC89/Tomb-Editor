using System;
using System.Runtime.InteropServices;
using System.Text;

namespace TombLib.Scripting
{
	internal static class NativeMethods
	{
		[DllImport("shell32.dll")]
		public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out]StringBuilder lpszPath, int nFolder, bool fCreate);

		[DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool DeleteObject([In] IntPtr hObject);
	}
}
