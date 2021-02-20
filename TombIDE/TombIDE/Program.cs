using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			UpdateNGCompilerPaths();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			IDEConfiguration ideConfiguration = IDEConfiguration.Load();
			List<Project> availableProjects = XmlHandling.GetProjectsFromXml();
			List<Plugin> availablePlugins = XmlHandling.GetPluginsFromXml();

			using (IDE ide = new IDE(ideConfiguration, availableProjects, availablePlugins))
			{
				IDE.Global = ide;

				using (FormStart form = new FormStart(ide))
				{
					if (args.Length > 0)
					{
						if (Path.GetExtension(args[0]).ToLower() != ".trproj")
						{
							MessageBox.Show("Invalid file type. TombIDE can only open .trproj files.", "Error",
								MessageBoxButtons.OK, MessageBoxIcon.Error);

							return;
						}

						ide.IDEConfiguration.RememberedProject = string.Empty;
						form.OpenTrprojWithTombIDE(args[0]); // Changes ide.Configuration.RememberedProject to the opened project on success

						if (string.IsNullOrEmpty(ide.IDEConfiguration.RememberedProject))
							return; // Opening project failed

						Application.Run(form);

						// Reset the RememberedProject setting after closing
						ide.IDEConfiguration.RememberedProject = string.Empty;
						ide.IDEConfiguration.Save();
					}
					else
						Application.Run(form);
				}
			}
		}

		private static void UpdateNGCompilerPaths()
		{
			try
			{
				string programPath = DefaultPaths.ProgramDirectory;

				if (IsUnicodePath(programPath))
					throw new ArgumentException(
						"Your executing path contains non-ASCII symbols. This will not allow the compilers to work correctly.\n" +
						"Please consider removing all non-ASCII symbols from the executing path before launching TombIDE.");

				string virtualEnginePath = DefaultPaths.VGEDirectory;

				if (virtualEnginePath.Length > 255)
					throw new PathTooLongException(
						"Your executing path is too long. Please consider shortening your executing path " +
						"to a maximum of 244 symbols before launching TombIDE.");

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

				string centerSettingsFilePath = Path.Combine(programPath, "TIDE", "NGC", "center_settings.bin");

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

		private static bool IsUnicodePath(string path) => path.Any(c => c > byte.MaxValue);
    }
}
