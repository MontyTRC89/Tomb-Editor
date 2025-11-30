using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.FileExtraction;

public sealed class FileExtractionService : IFileExtractionService
{
	public void ExtractEntries(IEnumerable<ZipArchiveEntry> entries, IGameProject targetProject, bool overwrite = true)
	{
		foreach (ZipArchiveEntry entry in entries)
		{
			if (entry.FullName.EndsWith("/"))
			{
				string dirPath = Path.Combine(targetProject.DirectoryPath, entry.FullName);

				if (!Directory.Exists(dirPath))
					Directory.CreateDirectory(dirPath);
			}
			else
			{
				string targetPath = Path.Combine(targetProject.DirectoryPath, entry.FullName);

				if (overwrite)
					entry.ExtractToFile(targetPath, true);
				else if (!File.Exists(targetPath))
					entry.ExtractToFile(targetPath, false);
			}
		}
	}
}
