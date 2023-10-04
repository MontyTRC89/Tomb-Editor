using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TombLib.Scripting.ClassicScript.Utils
{
	public static class NGCompiler
	{
		private static Stack<string> _visitedFiles = new();

		public static bool AreLibrariesRegistered()
		{
			bool requiredFilesExist = File.Exists(DefaultPaths.MscomctlSystemFile)
				&& File.Exists(DefaultPaths.Richtx32SystemFile)
				&& File.Exists(DefaultPaths.PicFormat32SystemFile)
				&& File.Exists(DefaultPaths.Comdlg32SystemFile);

			if (!requiredFilesExist)
			{
				try
				{
					var process = new ProcessStartInfo
					{
						FileName = DefaultPaths.LibraryRegistrationExecutable,
						UseShellExecute = true
					};

					Process.Start(process).WaitForExit();
				}
				catch
				{
					return false;
				}
			}

			return true;
		}

		public static bool Compile(string projectScriptPath, string projectEnginePath, bool newIncludeMethod = false)
		{
			if (!AreLibrariesRegistered())
				throw new Exception("The required libraries are not registered.");

			CopyFilesToVGEScriptDirectory(projectScriptPath, DefaultPaths.VGEScriptDirectory);

			if (newIncludeMethod)
				MergeIncludes();

			AdjustFormatting();

			var process = new ProcessStartInfo
			{
				FileName = DefaultPaths.NGCExecutable,
				Arguments = $"\"{DefaultPaths.VGEScriptDirectory}\\Script.txt\" -Log -NoMsgBox -NoWait -Concise",
				UseShellExecute = true
			};

			Process.Start(process).WaitForExit();

			FixLogs(projectEnginePath, out bool containsError);
			CopyCompiledFilesToProject(projectEnginePath);

			return !containsError;
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

		private static void MergeIncludes()
		{
			_visitedFiles.Clear();

			string vgeScriptFilePath = Path.Combine(DefaultPaths.VGEScriptDirectory, "Script.txt");

			_visitedFiles.Push(vgeScriptFilePath);

			string[] lines = File.ReadAllLines(vgeScriptFilePath, Encoding.GetEncoding(1252));
			lines = ReplaceIncludesWithFileContents(lines);

			string newFileContent = string.Join(Environment.NewLine, lines);
			File.WriteAllText(vgeScriptFilePath, newFileContent, Encoding.GetEncoding(1252));

			_visitedFiles.Clear();
		}

		private static void AdjustFormatting()
		{
			string vgeScriptFilePath = Path.Combine(DefaultPaths.VGEScriptDirectory, "Script.txt");

			string fileContent = File.ReadAllText(vgeScriptFilePath, Encoding.GetEncoding(1252));

			while (fileContent.Contains(" ="))
				fileContent = fileContent.Replace(" =", "=");

			File.WriteAllText(vgeScriptFilePath, fileContent, Encoding.GetEncoding(1252));
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

							string[] includeLines = File.ReadAllLines(includedFilePath, Encoding.GetEncoding(1252));
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

		private static void FixLogs(string projectEnginePath, out bool constainsError)
		{
			string logFilePath = Path.Combine(DefaultPaths.VGEDirectory, "LastCompilerLog.txt");
			string logFileContent = File.ReadAllText(logFilePath, Encoding.GetEncoding(1252));

			// Replace the VGE paths in the log file with the current project ones
			string newFileContent = logFileContent
				.Replace(DefaultPaths.VGEDirectory, projectEnginePath)
				.Replace("ERROR: unknonw ", "ERROR: unknown ");

			constainsError = newFileContent.Contains("ERROR:");
			File.WriteAllText(logFilePath, newFileContent, Encoding.GetEncoding(1252));
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
	}
}
