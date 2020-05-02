using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;
using TombLib.Scripting.Helpers;

namespace TombLib.Scripting.Compilers
{
	public static class NGCompiler
	{
		// TODO: Refactor

		public static bool AreLibrariesRegistered()
		{
			string MSCOMCTL = Path.Combine(PathHelper.GetSystemDirectory(), "Mscomctl.ocx");
			string RICHTX32 = Path.Combine(PathHelper.GetSystemDirectory(), "Richtx32.ocx");
			string PICFORMAT32 = Path.Combine(PathHelper.GetSystemDirectory(), "PicFormat32.ocx");
			string COMDLG32 = Path.Combine(PathHelper.GetSystemDirectory(), "Comdlg32.ocx");

			if (!File.Exists(MSCOMCTL) || !File.Exists(RICHTX32) || !File.Exists(PICFORMAT32) || !File.Exists(COMDLG32))
			{
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = Path.Combine(PathHelper.GetProgramDirectory(), "TombIDE Library Registration.exe")
				};

				try
				{
					Process process = Process.Start(startInfo);
					process.WaitForExit();
				}
				catch { return false; }
			}

			return true;
		}

		public static bool Compile(string projectScriptPath, string projectEnginePath)
		{
			CopyFilesToVGEScriptFolder(projectScriptPath);
			AdjustFormatting(projectScriptPath);

			// Run NG_Center.exe
			var application = TestStack.White.Application.Launch(Path.Combine(PathHelper.GetInternalNGCPath(), "NG_Center.exe"));

			// Do some actions in NG Center
			return RunScriptedNGCenterEvents(projectEnginePath, application);
		}

		private static bool RunScriptedNGCenterEvents(string projectEnginePath, TestStack.White.Application application)
		{
			try
			{
				// Get a list of all windows belonging to the app
				var windowList = application.GetWindows();

				// Check if the list has the main NG Center window (it starts with "NG Center 1.5...")
				var ngWindow = windowList.Find(x => x.Title.Contains("NG Center 1.5"));

				if (ngWindow == null)
				{
					// Refresh the window list and check if a Updater message box appeared
					windowList = application.GetWindows();
					var ngMissingWindow = windowList.Find(x => x.Title.Contains("NG_CENTER"));

					if (ngMissingWindow != null)
						ngMissingWindow.KeyIn(TestStack.White.WindowsAPI.KeyboardInput.SpecialKeys.ESCAPE);

					// If not, then try again because we're most probably seeing the "Loading" window
					return RunScriptedNGCenterEvents(projectEnginePath, application);
				}

				Point cachedCursorPosition = new Point();

				// Refresh the window list and check if a Updater message box appeared
				windowList = application.GetWindows();
				var ngUpdaterWindow = windowList.Find(x => x.Title.Contains("NG_CENTER"));

				if (ngUpdaterWindow != null)
					ngUpdaterWindow.KeyIn(TestStack.White.WindowsAPI.KeyboardInput.SpecialKeys.ESCAPE);

				// Find the "Build" button
				var buildButton = ngWindow.Get<TestStack.White.UIItems.Button>("Build");

				// Click the button
				cachedCursorPosition = Cursor.Position;
				buildButton.Click();
				Cursor.Position = cachedCursorPosition; // Restore the previous cursor position

				// Refresh the window list and check if an error message box appeared
				windowList = application.GetWindows();
				var ngErrorWindow = windowList.Find(x => x.Title.Contains("NG_CENTER"));

				if (ngErrorWindow != null)
				{
					ngErrorWindow.KeyIn(TestStack.White.WindowsAPI.KeyboardInput.SpecialKeys.ESCAPE);

					// Click the button
					cachedCursorPosition = Cursor.Position;
					buildButton.Click();
					Cursor.Position = cachedCursorPosition; // Restore the previous cursor position

					ngErrorWindow = null;
				}

				// Refresh the window list and check if an error message box appeared
				windowList = application.GetWindows();
				ngErrorWindow = windowList.Find(x => x.Title.Contains("NG_CENTER"));

				if (ngErrorWindow != null)
					return false;

				// Find the "Show Log" button
				var logButton = ngWindow.Get<TestStack.White.UIItems.Button>("Show Log");

				// Click the button
				cachedCursorPosition = Cursor.Position;
				logButton.Click();
				Cursor.Position = cachedCursorPosition; // Restore the previous cursor position

				// Read the logs
				string logFilePath = Path.Combine(PathHelper.GetVGEScriptPath(), "script_log.txt");
				string logFileContent = File.ReadAllText(logFilePath);

				// Replace the VGE paths in the log file with the current project ones
				File.WriteAllText(logFilePath, logFileContent.Replace(PathHelper.GetVGEPath(), projectEnginePath), Encoding.GetEncoding(1252));

				application.Close(); // Done!

				// Copy the compiled files from the Virtual Game Engine folder to the current project folder
				string compiledScriptFilePath = Path.Combine(PathHelper.GetVGEPath(), "Script.dat");
				string compiledEnglishFilePath = Path.Combine(PathHelper.GetVGEPath(), "English.dat");

				if (File.Exists(compiledScriptFilePath))
					File.Copy(compiledScriptFilePath, Path.Combine(projectEnginePath, "Script.dat"), true);

				if (File.Exists(compiledEnglishFilePath))
					File.Copy(compiledEnglishFilePath, Path.Combine(projectEnginePath, "English.dat"), true);

				Thread.Sleep(100);

				foreach (Process notepadProcess in Process.GetProcessesByName("notepad"))
				{
					if (notepadProcess.MainWindowTitle.Contains("script_log"))
                        notepadProcess.Kill();
				}

				return true;
			}
			catch (ElementNotAvailableException)
			{
				// The "Loading" window just closed, so try again
				return RunScriptedNGCenterEvents(projectEnginePath, application);
			}
		}

		private static void CopyFilesToVGEScriptFolder(string projectScriptPath)
		{
			string vgeScriptPath = PathHelper.GetVGEScriptPath();

			// Delete the old /Script/ directory in the VGE if it exists
			if (Directory.Exists(vgeScriptPath))
				Directory.Delete(vgeScriptPath, true);

			// Recreate the directory
			Directory.CreateDirectory(vgeScriptPath);

			// Create all of the subdirectories
			foreach (string dirPath in Directory.GetDirectories(projectScriptPath, "*", SearchOption.AllDirectories))
				Directory.CreateDirectory(dirPath.Replace(projectScriptPath, vgeScriptPath));

			// Copy all the files into the VGE /Script/ folder
			foreach (string newPath in Directory.GetFiles(projectScriptPath, "*.*", SearchOption.AllDirectories))
				File.Copy(newPath, newPath.Replace(projectScriptPath, vgeScriptPath));
		}

		private static void AdjustFormatting(string projectScriptPath)
		{
			string vgeScriptFilePath = Path.Combine(PathHelper.GetVGEScriptPath(), "Script.txt");

			string[] lines = File.ReadAllLines(vgeScriptFilePath, Encoding.GetEncoding(1252));
			lines = ReplaceIncludesWithFileContents(lines, projectScriptPath);

			string newFileContent = string.Join(Environment.NewLine, lines);

			while (newFileContent.Contains(" ="))
				newFileContent = newFileContent.Replace(" =", "=");

			File.WriteAllText(Path.Combine(PathHelper.GetVGEScriptPath(), "Script.txt"), newFileContent, Encoding.GetEncoding(1252));
		}

		private static string[] ReplaceIncludesWithFileContents(string[] lines, string projectScriptPath)
		{
			List<string> newLines = new List<string>();

			foreach (string line in lines)
			{
				if (line.StartsWith("#include", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						string partialIncludePath = line.Split('"')[1].Trim();
						string includedFilePath = Path.Combine(projectScriptPath, partialIncludePath);

						if (File.Exists(includedFilePath))
						{
							newLines.Add("; // // // // <" + partialIncludePath.ToUpper() + "> // // // //");
							newLines.AddRange(File.ReadAllLines(includedFilePath, Encoding.GetEncoding(1252)));
							newLines.Add("; // // // // </" + partialIncludePath.ToUpper() + "> // // // //");
						}

						continue;
					}
					catch { }
				}

				newLines.Add(line);
			}

			return newLines.ToArray();
		}
	}
}
