using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using TestStack.White;
using TestStack.White.UIItems;
using TestStack.White.UIItems.WindowItems;
using TestStack.White.WindowsAPI;

namespace TombLib.Scripting.Compilers
{
	public static class NGCompiler
	{
		public static async Task<bool> Compile(
			string projectScriptPath, string projectEnginePath, string ngcPath, string vgePath, bool newIncludeMethod = true)
		{
			string vgeScriptPath = Path.Combine(vgePath, "Script");

			CopyFilesToVGEScriptDirectory(projectScriptPath, vgeScriptPath);

			if (newIncludeMethod)
				MergeIncludes(vgeScriptPath);

			AdjustFormatting(vgeScriptPath);

			Application ngCenter = Application.Launch(Path.Combine(ngcPath, "NG_Center.exe")); // Runs NG_Center.exe
			return await RunScriptedNGCenterEvents(projectEnginePath, vgePath, ngCenter); // Does some actions in NG Center
		}

		private static async Task<bool> RunScriptedNGCenterEvents(string projectEnginePath, string vgePath, Application ngCenter)
		{
			try
			{
				// FIND THE MAIN NG CENTER WINDOW
				List<Window> windowList = ngCenter.GetWindows(); // Gets a list of all windows which belong to NG Center
				Window mainNGCenterWindow = windowList.Find(x => x.Title.StartsWith("NG Center 1.5"));

				if (mainNGCenterWindow == null) // If the main window wasn't found
				{
					// CLOSE PROBLEM MESSAGE BOXES (if there are any)
					windowList = ngCenter.GetWindows();
					Window ngcProblemMessageBox = windowList.Find(x => x.Title.Equals("NG_CENTER"));

					if (ngcProblemMessageBox != null)
						ngcProblemMessageBox.KeyIn(KeyboardInput.SpecialKeys.ESCAPE); // Closes the message box

					return await RunScriptedNGCenterEvents(projectEnginePath, vgePath, ngCenter);
				}

				// CLOSE THE UPDATER MESSAGE BOX (if there is one)
				windowList = ngCenter.GetWindows();
				Window ngcUpdateMessageBox = windowList.Find(x => x.Title.Equals("NG_CENTER"));

				if (ngcUpdateMessageBox != null)
					ngcUpdateMessageBox.KeyIn(KeyboardInput.SpecialKeys.ESCAPE); // Closes the message box

				// CLICK THE "Build" BUTTON
				Button buildButton = mainNGCenterWindow.Get<Button>("Build");
				buildButton.Click();

				// CHECK IF AN ERROR MESSAGE BOX APPEARED AND IF SO, TRY AGAIN
				windowList = ngCenter.GetWindows();
				Window ngcErrorMessageBox = windowList.Find(x => x.Title.Equals("NG_CENTER"));

				if (ngcErrorMessageBox != null) // If the first build attempt failed
				{
					ngcErrorMessageBox.KeyIn(KeyboardInput.SpecialKeys.ESCAPE); // Closes the message box

					buildButton.Click(); // Try again, because NG Center tends to throw non-existent errors on the first try

					ngcErrorMessageBox = null;
				}

				windowList = ngCenter.GetWindows();
				ngcErrorMessageBox = windowList.Find(x => x.Title.Equals("NG_CENTER"));

				if (ngcErrorMessageBox != null) // If the second build attempt failed
					return false;

				// CLICK THE "Show Log" BUTTON AND UPDATE PATHS FROM VGE TO PROJECT
				Button logButton = mainNGCenterWindow.Get<Button>("Show Log");
				logButton.Click();

				UpdatePathsInsideLogs(vgePath, projectEnginePath);

				// DONE
				ngCenter.Close();

				CopyCompiledFilesToProject(vgePath, projectEnginePath);

				await KillNotepadProcess();

				return true;
			}
			catch (ElementNotAvailableException)
			{
				// THE "Loading" WINDOW JUST CLOSED, SO TRY AGAIN
				return await RunScriptedNGCenterEvents(projectEnginePath, vgePath, ngCenter);
			}
		}

		private static void CopyFilesToVGEScriptDirectory(string projectScriptPath, string vgeScriptPath)
		{
			// Delete the old /Script/ directory in the VGE folder (if it exists)
			if (Directory.Exists(vgeScriptPath))
				Directory.Delete(vgeScriptPath, true);

			// Recreate the directory
			Directory.CreateDirectory(vgeScriptPath);

			// Create all of the subdirectories from the original project directory
			foreach (string dirPath in Directory.GetDirectories(projectScriptPath, "*", SearchOption.AllDirectories))
				Directory.CreateDirectory(dirPath.Replace(projectScriptPath, vgeScriptPath));

			// Copy all the files into the VGE /Script/ directory
			foreach (string newPath in Directory.GetFiles(projectScriptPath, "*.*", SearchOption.AllDirectories))
				File.Copy(newPath, newPath.Replace(projectScriptPath, vgeScriptPath));
		}

		private static void MergeIncludes(string vgeScriptPath)
		{
			string vgeScriptFilePath = Path.Combine(vgeScriptPath, "Script.txt");

			string[] lines = File.ReadAllLines(vgeScriptFilePath, Encoding.GetEncoding(1252));
			lines = ReplaceIncludesWithFileContents(lines, vgeScriptPath);

			string newFileContent = string.Join(Environment.NewLine, lines);
			File.WriteAllText(vgeScriptFilePath, newFileContent, Encoding.GetEncoding(1252));
		}

		private static void AdjustFormatting(string vgeScriptPath)
		{
			string vgeScriptFilePath = Path.Combine(vgeScriptPath, "Script.txt");

			string fileContent = File.ReadAllText(vgeScriptFilePath, Encoding.GetEncoding(1252));

			while (fileContent.Contains(" ="))
				fileContent = fileContent.Replace(" =", "=");

			File.WriteAllText(vgeScriptFilePath, fileContent, Encoding.GetEncoding(1252));
		}

		private static string[] ReplaceIncludesWithFileContents(string[] lines, string vgeScriptPath)
		{
			List<string> newLines = new List<string>();

			foreach (string line in lines)
			{
				if (line.StartsWith("#include", StringComparison.OrdinalIgnoreCase))
					try
					{
						string partialIncludePath = line.Split('"')[1].Trim();
						string includedFilePath = Path.Combine(vgeScriptPath, partialIncludePath);

						if (File.Exists(includedFilePath))
						{
							newLines.Add("; // // // // <" + partialIncludePath.ToUpper() + "> // // // //");
							newLines.AddRange(File.ReadAllLines(includedFilePath, Encoding.GetEncoding(1252)));
							newLines.Add("; // // // // </" + partialIncludePath.ToUpper() + "> // // // //");
						}

						continue;
					}
					catch { }

				newLines.Add(line);
			}

			return newLines.ToArray();
		}

		private static void UpdatePathsInsideLogs(string vgePath, string projectEnginePath)
		{
			string logFilePath = Path.Combine(vgePath, "Script", "script_log.txt");
			string logFileContent = File.ReadAllText(logFilePath);

			// Replace the VGE paths in the log file with the current project ones
			string newFileContent = logFileContent.Replace(vgePath, projectEnginePath);

			File.WriteAllText(logFilePath, newFileContent, Encoding.GetEncoding(1252));
		}

		private static void CopyCompiledFilesToProject(string vgePath, string projectEnginePath)
		{
			// Copy the compiled files from the Virtual Game Engine folder to the current project folder
			string compiledScriptFilePath = Path.Combine(vgePath, "Script.dat");
			string compiledEnglishFilePath = Path.Combine(vgePath, "English.dat");

			if (File.Exists(compiledScriptFilePath))
				File.Copy(compiledScriptFilePath, Path.Combine(projectEnginePath, "Script.dat"), true);

			if (File.Exists(compiledEnglishFilePath))
				File.Copy(compiledEnglishFilePath, Path.Combine(projectEnginePath, "English.dat"), true);
		}

		private static Task KillNotepadProcess()
		{
			CancellationToken token = new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token;

            Action KillProcess = () =>
            {
                while (true)
                    if (token.IsCancellationRequested)
                        return;
                    else
                        foreach (Process process in Process.GetProcessesByName("notepad"))
                            if (process.MainWindowTitle.Contains("script_log"))
                            {
                                process.Kill();
                                return;
                            }
            };

			return Task.Run(KillProcess, token);
		}
	}
}
