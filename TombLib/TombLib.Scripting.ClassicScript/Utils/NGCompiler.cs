using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Exceptions;
using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using FlaUI.UIA2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TombLib.Scripting.ClassicScript.Utils
{
	public static class NGCompiler
	{
		public static bool AreLibrariesRegistered()
		{
			bool requiredFilesExist = File.Exists(DefaultPaths.MscomctlSystemFile)
				&& File.Exists(DefaultPaths.Richtx32SystemFile)
				&& File.Exists(DefaultPaths.PicFormat32SystemFile)
				&& File.Exists(DefaultPaths.Comdlg32SystemFile);

			if (!requiredFilesExist)
				try { Process.Start(DefaultPaths.LibraryRegistrationExecutable).WaitForExit(); }
				catch { return false; }

			return true;
		}

		public static async Task<bool> Compile(string projectScriptPath, string projectEnginePath, bool newIncludeMethod = true)
		{
			if (!AreLibrariesRegistered())
				return false;

			CopyFilesToVGEScriptDirectory(projectScriptPath, DefaultPaths.VGEScriptDirectory);

			if (newIncludeMethod)
				MergeIncludes();

			AdjustFormatting();

			using UIA2Automation automation = new();
			var ngCenter = Application.Launch(DefaultPaths.NGCExecutable); // Runs NG_Center.exe
			return await RunScriptedNGCenterEvents(projectEnginePath, automation, ngCenter); // Does some actions in NG Center
		}

		private static async Task<bool> RunScriptedNGCenterEvents(string projectEnginePath, UIA2Automation automation, Application ngCenter)
		{
			try
			{
				Window window = ngCenter.GetAllTopLevelWindows(automation).FirstOrDefault();

				if (window == null)
					return await RunScriptedNGCenterEvents(projectEnginePath, automation, ngCenter);

				if (window.ModalWindows.Any(x => x.Title == "NG_CENTER"))
					Keyboard.Press(VirtualKeyShort.ESCAPE);

				Button buildButton = window.FindFirstDescendant(cf => cf.ByText("Build"))?.AsButton();

				if (buildButton == null)
					return await RunScriptedNGCenterEvents(projectEnginePath, automation, ngCenter);

				buildButton.Click();

				if (window.ModalWindows.Any(x => x.Title == "NG_CENTER"))
				{
					Keyboard.Press(VirtualKeyShort.ESCAPE);
					buildButton.Click();

					if (window.ModalWindows.Any(x => x.Title == "NG_CENTER"))
						return false;
				}

				Button logButton = window.FindFirstDescendant(cf => cf.ByText("Show Log"))?.AsButton();
				logButton.Click();

				UpdatePathsInsideLogs(projectEnginePath);

				ngCenter.Close();

				CopyCompiledFilesToProject(projectEnginePath);

				await KillNotepadProcess();

				return true;
			}
			catch (ElementNotAvailableException)
			{
				// THE "Loading" WINDOW JUST CLOSED, SO TRY AGAIN
				return await RunScriptedNGCenterEvents(projectEnginePath, automation, ngCenter);
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
			foreach (string newPath in Directory.GetFiles(projectScriptPath, "*.*", SearchOption.AllDirectories)
				.Where(x => !Path.GetExtension(x).Equals(".backup", StringComparison.OrdinalIgnoreCase)))
				File.Copy(newPath, newPath.Replace(projectScriptPath, vgeScriptPath));
		}

		private static Stack<string> _visitedFiles = new Stack<string>();

		private static void MergeIncludes()
		{
			_visitedFiles.Clear();

			string vgeScriptFilePath = Path.Combine(DefaultPaths.VGEScriptDirectory, "Script.txt");

			_visitedFiles.Push(vgeScriptFilePath);

			string[] lines = File.ReadAllLines(vgeScriptFilePath);
			lines = ReplaceIncludesWithFileContents(lines);

			string newFileContent = string.Join(Environment.NewLine, lines);
			File.WriteAllText(vgeScriptFilePath, newFileContent);

			_visitedFiles.Clear();
		}

		private static void AdjustFormatting()
		{
			string vgeScriptFilePath = Path.Combine(DefaultPaths.VGEScriptDirectory, "Script.txt");

			string fileContent = File.ReadAllText(vgeScriptFilePath);

			while (fileContent.Contains(" ="))
				fileContent = fileContent.Replace(" =", "=");

			File.WriteAllText(vgeScriptFilePath, fileContent);
		}

		private static string[] ReplaceIncludesWithFileContents(string[] lines)
		{
			var newLines = new List<string>();

			foreach (string line in lines)
			{
				if (line.TrimStart().StartsWith("#include", StringComparison.OrdinalIgnoreCase))
					try
					{
						string partialIncludePath = line.Split('"')[1].Trim();
						string includedFilePath = Path.Combine(DefaultPaths.VGEScriptDirectory, partialIncludePath);

						if (File.Exists(includedFilePath))
						{
							if (_visitedFiles.Any(x => x.Equals(includedFilePath, StringComparison.OrdinalIgnoreCase)))
								continue;

							_visitedFiles.Push(includedFilePath);

							newLines.Add("; // // // // <" + partialIncludePath.ToUpper() + "> // // // //");

							string[] includeLines = File.ReadAllLines(includedFilePath);
							includeLines = ReplaceIncludesWithFileContents(includeLines);

							newLines.AddRange(includeLines);

							newLines.Add("; // // // // </" + partialIncludePath.ToUpper() + "> // // // //");

							_visitedFiles.Pop();
						}

						continue;
					}
					catch { }

				newLines.Add(line);
			}

			return newLines.ToArray();
		}

		private static void UpdatePathsInsideLogs(string projectEnginePath)
		{
			string logFilePath = Path.Combine(DefaultPaths.VGEScriptDirectory, "script_log.txt");
			string logFileContent = File.ReadAllText(logFilePath);

			// Replace the VGE paths in the log file with the current project ones
			string newFileContent = logFileContent.Replace(DefaultPaths.VGEDirectory, projectEnginePath);

			File.WriteAllText(logFilePath, newFileContent);
		}

		private static void CopyCompiledFilesToProject(string projectEnginePath)
		{
			// Copy the compiled files from the Virtual Game Engine folder to the current project folder
			string compiledScriptFilePath = Path.Combine(DefaultPaths.VGEDirectory, "Script.dat");
			string compiledEnglishFilePath = Path.Combine(DefaultPaths.VGEDirectory, "English.dat");

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
				while (!token.IsCancellationRequested)
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
