using System.Collections.Generic;
using System.IO.Compression;
using TombIDE.Shared.NewStructure;

namespace TombIDE.ProjectMaster.Services.FileExtraction;

/// <summary>
/// Provides functionality for extracting files from ZIP archives.
/// </summary>
public interface IFileExtractionService
{
	/// <summary>
	/// Extracts ZIP archive entries to the target project directory.
	/// </summary>
	/// <param name="entries">The ZIP entries to extract.</param>
	/// <param name="targetProject">The target project where files should be extracted.</param>
	/// <param name="overwrite">Whether to overwrite existing files.</param>
	void ExtractEntries(IEnumerable<ZipArchiveEntry> entries, IGameProject targetProject, bool overwrite = true);
}
