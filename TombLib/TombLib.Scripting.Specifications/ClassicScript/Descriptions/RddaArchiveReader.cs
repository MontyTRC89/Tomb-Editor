#nullable enable

using System;
using System.IO;
using System.IO.Compression;

namespace TombLib.Scripting.Specifications.ClassicScript.Descriptions;

public static class RddaArchiveReader
{
	public static string GetKeywordDescription(string archivePath, string keyword)
	{
		string keywordDescriptionFileName = GetKeywordDescriptionFileName(keyword);

		using (FileStream file = File.OpenRead(archivePath))
		using (var archive = new ZipArchive(file))
			foreach (ZipArchiveEntry entry in archive.Entries)
				if (entry.Name.Equals(keywordDescriptionFileName, StringComparison.OrdinalIgnoreCase))
					using (Stream stream = entry.Open())
					using (var reader = new StreamReader(stream))
						return reader.ReadToEnd();

		return string.Empty;
	}

	public static bool ContainsKeywordDescription(string archivePath, string keyword)
		=> !string.IsNullOrEmpty(GetKeywordDescription(archivePath, keyword));

	private static string GetKeywordDescriptionFileName(string keyword)
		=> "info_" + keyword.TrimStart('_').Replace(" ", "_").Replace("/", string.Empty) + ".txt";
}