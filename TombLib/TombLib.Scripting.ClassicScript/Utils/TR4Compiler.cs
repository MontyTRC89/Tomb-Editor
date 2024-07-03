using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.ClassicScript.Utils
{
	public static class TR4Compiler
	{
		public static string Compile(string projectScriptPath, string projectEnginePath)
		{
			if (DefaultPaths.TR4ScriptCompilerDirectory.Contains('\''))
				throw new Exception("The path to the TR4 script compiler contains an invalid character.\n' is not a valid path character for this operation.");

			CopyFilesToDOSScriptDirectory(projectScriptPath, DefaultPaths.TR4ScriptCompilerDirectory);

			var startInfo = new ProcessStartInfo
			{
				FileName = DefaultPaths.DOSBoxExecutable,
				WorkingDirectory = DefaultPaths.DOSDirectory,
				Arguments =
					$"-c \"mount C '{DefaultPaths.TR4ScriptCompilerDirectory}'\" " +
					"-c \"C:\" " +
					"-c \"script script.txt >> logs.txt\" " +
					"-c \"exit\" " +
					"-noconsole",
				UseShellExecute = true
			};

			Process.Start(startInfo).WaitForExit();

			string logFilePath = Path.Combine(DefaultPaths.TR4ScriptCompilerDirectory, "logs.txt");
			string logFileContent = File.ReadAllText(logFilePath);

			string compiledScriptFilePath = Path.Combine(DefaultPaths.TR4ScriptCompilerDirectory, "Script.dat");
			string compiledEnglishFilePath = Path.Combine(DefaultPaths.TR4ScriptCompilerDirectory, "English.dat");

			if (File.Exists(compiledScriptFilePath))
				File.Copy(compiledScriptFilePath, Path.Combine(projectEnginePath, "Script.dat"), true);

			if (File.Exists(compiledEnglishFilePath))
				File.Copy(compiledEnglishFilePath, Path.Combine(projectEnginePath, "English.dat"), true);

			var dosScriptDirectory = new DirectoryInfo(DefaultPaths.TR4ScriptCompilerDirectory);

			foreach (FileSystemInfo fileSystemInfo in dosScriptDirectory.EnumerateFileSystemInfos().Where(x
				=> !x.Name.Equals("SCRIPT.EXE", StringComparison.OrdinalIgnoreCase)
				&& !x.Name.Equals("DOS4GW.EXE", StringComparison.OrdinalIgnoreCase)))
			{
				if (fileSystemInfo is DirectoryInfo dir)
					dir.Delete(true);
				else
					fileSystemInfo.Delete();
			}

			return logFileContent;
		}

		private static void CopyFilesToDOSScriptDirectory(string projectScriptPath, string dosScriptPath)
		{
			foreach (string dirPath in Directory.GetDirectories(projectScriptPath, "*", SearchOption.AllDirectories))
				Directory.CreateDirectory(dirPath.Replace(projectScriptPath, dosScriptPath));

			foreach (string file in Directory.GetFiles(projectScriptPath, "*.*", SearchOption.AllDirectories)
				.Where(x => !Path.GetExtension(x).Equals(".backup", StringComparison.OrdinalIgnoreCase)))
			{
				string newPath = file.Replace(projectScriptPath, dosScriptPath);

				if (file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
				{
					string fileContent = File.ReadAllText(file);

					while (fileContent.Contains(" ="))
						fileContent = fileContent.Replace(" =", "=");

					fileContent = BasicCleaner.TrimEndingWhitespace(fileContent);
					File.WriteAllText(newPath, fileContent, Encoding.GetEncoding(1252));
				}
				else
					File.Copy(file, newPath);
			}
		}
	}
}
