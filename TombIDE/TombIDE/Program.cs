using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Projects;

namespace TombIDE
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			UpdateNGCompilerPaths();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Configuration configuration = Configuration.Load();
			List<Project> availableProjects = XmlHandling.GetProjectsFromXml();

			using (IDE ide = new IDE(configuration, availableProjects))
			{
				using (FormStart form = new FormStart(ide))
					Application.Run(form);
			}
		}

		private static void UpdateNGCompilerPaths()
		{
			try
			{
				string programPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

				if (IsUnicodePath(programPath))
					throw new ArgumentException(
						"Your executing path contains non-ASCII symbols. This will not allow the compilers to work correctly.\n" +
						"Please consider removing all non-ASCII symbols from the executing path before launching TombIDE.");

				string virtualEnginePath = Path.Combine(programPath, "NGC", "VGE");

				if (virtualEnginePath.Length > 255)
					throw new PathTooLongException(
						"Your executing path is too long. Please consider shortening your executing path " +
						"to a maximum of 248 symbols before launching TombIDE.");

				byte[] stringBytes = Encoding.ASCII.GetBytes(virtualEnginePath);

				byte[] bytesToWrite_01 = Enumerable.Repeat((byte)0x20, 2560).ToArray();
				// Make room for the VGE path in the bytesToWrite_01 array
				bytesToWrite_01 = bytesToWrite_01.Skip(stringBytes.Length).ToArray();
				// Merge the stringBytes array with the leftover items from the bytesToWrite_01 array, so the full array size is still 2560
				bytesToWrite_01 = stringBytes.Concat(bytesToWrite_01).ToArray();

				byte[] bytesToWrite_02 = Enumerable.Repeat((byte)0x20, 256).ToArray();
				// Make room for the VGE path in the bytesToWrite_02 array
				bytesToWrite_02 = bytesToWrite_02.Skip(stringBytes.Length).ToArray();
				// Merge the stringBytes array with the leftover items from the bytesToWrite_02 array, so the full array size is still 256
				bytesToWrite_02 = stringBytes.Concat(bytesToWrite_02).ToArray();

				string centerSettingsFilePath = Path.Combine("NGC", "center_settings.bin");

				using (FileStream stream = File.OpenWrite(centerSettingsFilePath))
				{
					stream.Position = 4; // First game path offset
					stream.Write(bytesToWrite_01, 0, bytesToWrite_01.Length);

					stream.Position = 2820; // Second game path offset
					stream.Write(bytesToWrite_02, 0, bytesToWrite_02.Length);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Environment.Exit(-1);
			}
		}

		private static bool IsUnicodePath(string path)
		{
			int asciiByteCount = Encoding.ASCII.GetByteCount(path);
			int utf8ByteCount = Encoding.UTF8.GetByteCount(path);

			return asciiByteCount != utf8ByteCount;
		}
	}
}
