#nullable enable

using System;
using System.Text.RegularExpressions;

namespace TombLib.Scripting.Specifications.ClassicScript.Descriptions;

public sealed class ClassicScriptDescriptionArchiveService
{
	private const string MnemonicConstantsArchiveFileName = "Mnemonic Constants.rdda";
	private const string OldCommandsArchiveFileName = "OLD Commands.rdda";
	private const string NewCommandsArchiveFileName = "NEW Commands.rdda";
	private const string OcbArchiveFileName = "OCBs.rdda";

	public string GetMnemonicConstantDescription(string keyword)
		=> GetDescription(MnemonicConstantsArchiveFileName, keyword);

	public string GetOldCommandDescription(string keyword)
		=> GetDescription(OldCommandsArchiveFileName, keyword);

	public string GetNewCommandDescription(string keyword)
		=> GetDescription(NewCommandsArchiveFileName, keyword);

	public string GetOcbDescription(string keyword)
		=> GetDescription(OcbArchiveFileName, keyword);

	public bool IsOldCommand(string command)
		=> RddaArchiveReader.ContainsKeywordDescription(GetDescriptionPath(OldCommandsArchiveFileName), command);

	private static string GetDescription(string archiveFileName, string keyword)
	{
		ArgumentNullException.ThrowIfNull(keyword);

		string description = RddaArchiveReader.GetKeywordDescription(GetDescriptionPath(archiveFileName), keyword);
        return string.IsNullOrEmpty(description)
            ? string.Empty
            : Regex.Replace(description, @"\r\n?|\n", "\n");
	}

	private static string GetDescriptionPath(string archiveFileName)
		=> ClassicScriptResourcePaths.GetResourcePath("Descriptions", archiveFileName);
}