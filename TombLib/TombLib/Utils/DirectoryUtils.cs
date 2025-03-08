using System.IO;

namespace TombLib.Utils;

/// <summary>
/// Provides utility methods for working with directories.
/// </summary>
public static class DirectoryUtils
{
	/// <summary>
	/// Recursively copies the contents of a directory to another directory.
	/// </summary>
	/// <param name="sourcePath">The path of the source directory.</param>
	/// <param name="targetPath">The path of the target directory.</param>
	public static void DeepCopy(string sourcePath, string targetPath)
	{
		// Create all directories in the target path that exist in the source path
		foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
		{
			string targetDirPath = dirPath.Replace(sourcePath, targetPath);
			Directory.CreateDirectory(targetDirPath);
		}

		// Copy all files from the source path to the target path
		foreach (string filePath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
		{
			string targetFilePath = filePath.Replace(sourcePath, targetPath);
			File.Copy(filePath, targetFilePath, true);
		}
	}
}
