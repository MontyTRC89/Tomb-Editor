using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombIDE.Shared;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense
{
	internal static class LuaLanguageServerLocator
	{
		private const string ExecutableFileName = "lua-language-server.exe";

		private static readonly string[] ExtensionRoots =
		{
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".vscode", "extensions"),
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".vscode-insiders", "extensions")
		};

		public static string ResolveExecutablePath(IDEConfiguration config)
		{
			if (config is not null && IsValidExecutablePath(config.LuaLanguageServerPath))
				return config.LuaLanguageServerPath;

			string executablePath = FindExecutableInPath();

			if (executablePath is null)
				executablePath = FindExecutableInExtensions();

			if (config is not null && !string.IsNullOrWhiteSpace(executablePath)
				&& !string.Equals(config.LuaLanguageServerPath, executablePath, StringComparison.OrdinalIgnoreCase))
			{
				config.LuaLanguageServerPath = executablePath;
				config.Save();
			}

			return executablePath;
		}

		private static bool IsValidExecutablePath(string executablePath)
			=> !string.IsNullOrWhiteSpace(executablePath)
			&& File.Exists(executablePath)
			&& Path.GetFileName(executablePath).Equals(ExecutableFileName, StringComparison.OrdinalIgnoreCase);

		private static string FindExecutableInPath()
		{
			string pathVariable = Environment.GetEnvironmentVariable("PATH");

			if (string.IsNullOrWhiteSpace(pathVariable))
				return null;

			foreach (string pathPart in pathVariable.Split(Path.PathSeparator).Where(part => !string.IsNullOrWhiteSpace(part)))
			{
				try
				{
					string candidatePath = Path.Combine(pathPart.Trim(), ExecutableFileName);

					if (IsValidExecutablePath(candidatePath))
						return candidatePath;
				}
				catch
				{
					// Ignore invalid PATH entries.
				}
			}

			return null;
		}

		private static string FindExecutableInExtensions()
		{
			var candidates = new List<string>();

			foreach (string extensionRoot in ExtensionRoots)
			{
				if (!Directory.Exists(extensionRoot))
					continue;

				foreach (string extensionDirectory in Directory.EnumerateDirectories(extensionRoot)
					.Where(path => Path.GetFileName(path).Contains("lua", StringComparison.OrdinalIgnoreCase))
					.OrderByDescending(path => Path.GetFileName(path), StringComparer.OrdinalIgnoreCase))
				{
					try
					{
						candidates.AddRange(Directory.EnumerateFiles(extensionDirectory, ExecutableFileName, SearchOption.AllDirectories));
					}
					catch
					{
						// Ignore directories we cannot access.
					}
				}
			}

			return candidates.FirstOrDefault(IsValidExecutablePath);
		}
	}
}