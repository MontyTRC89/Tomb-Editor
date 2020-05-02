using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace TombLib.Scripting.Helpers
{
	internal class PathHelper
	{
		public static string GetTextEditorConfigsPath()
		{
			return Path.Combine(GetProgramDirectory(), "Configs", "TextEditors");
		}

		public static string GetInternalNGCPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "NGC");
		}

		public static string GetVGEPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "NGC", "VGE");
		}

		public static string GetVGEScriptPath()
		{
			return Path.Combine(GetProgramDirectory(), "TIDE", "NGC", "VGE", "Script");
		}

		public static string GetProgramDirectory()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
