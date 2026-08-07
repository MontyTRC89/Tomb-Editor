using System.IO;
using System.Text;

namespace TombLib.Scripting.ClassicScript.Descriptions;

// Reads command and constant descriptions from the Markdown description files
// (Resources/ClassicScript/Descriptions/*.md) into case-insensitive dictionaries.
// Each description lives under a "## <key>" section; the section body is the text
// between the header line and the next header (or end of file), minus exactly one
// trailing newline, so consecutive headers represent an intentionally empty description.
// The file is parsed once and lookups are O(1) afterwards.
public sealed class ClassicScriptDescriptionCatalogService
{
	private readonly Lazy<IReadOnlyDictionary<string, string>> _mnemonicConstants = new(() => Load("MnemonicConstants.md"));
	private readonly Lazy<IReadOnlyDictionary<string, string>> _oldCommands = new(() => Load("OldCommands.md"));
	private readonly Lazy<IReadOnlyDictionary<string, string>> _newCommands = new(() => Load("NewCommands.md"));
	private readonly Lazy<IReadOnlyDictionary<string, string>> _ocbs = new(() => Load("OCBs.md"));

	public string GetMnemonicConstantDescription(string keyword)
		=> GetDescription(_mnemonicConstants.Value, keyword);

	public string GetOldCommandDescription(string keyword)
		=> GetDescription(_oldCommands.Value, keyword);

	public string GetNewCommandDescription(string keyword)
		=> GetDescription(_newCommands.Value, keyword);

	public string GetOcbDescription(string keyword)
		=> GetDescription(_ocbs.Value, keyword);

	private static string GetDescription(IReadOnlyDictionary<string, string> catalog, string keyword)
	{
		ArgumentNullException.ThrowIfNull(keyword);

		if (catalog.TryGetValue(NormalizeKeyword(keyword), out string? description))
			return description;

		return string.Empty;
	}

	private static IReadOnlyDictionary<string, string> Load(string fileName)
	{
		string path = ClassicScriptResourcePaths.GetResourcePath("Descriptions", fileName);
		return ParseSections(File.ReadAllText(path));
	}

	// Same normalization the archive reader applied to build info_<keyword>.txt names:
	// leading underscores trimmed, spaces turned into underscores, slashes removed.
	private static string NormalizeKeyword(string keyword)
		=> keyword.TrimStart('_').Replace(" ", "_").Replace("/", string.Empty);

	private static IReadOnlyDictionary<string, string> ParseSections(string markdown)
	{
		markdown = NormalizeLineEndings(markdown);

		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		if (!markdown.StartsWith("## ", StringComparison.Ordinal))
			return result;

		int searchFrom = 0;

		while (true)
		{
			int headerLineEnd = markdown.IndexOf('\n', searchFrom);
			if (headerLineEnd < 0)
				break;

			string key = markdown[(searchFrom + 3)..headerLineEnd];
			int bodyStart = headerLineEnd + 1;
			int nextHeaderStart = FindNextHeaderStart(markdown, bodyStart);

			string body = nextHeaderStart < 0
				? markdown[bodyStart..]
				: markdown[bodyStart..nextHeaderStart];

			result[key] = TrimOneTrailingNewline(body);

			if (nextHeaderStart < 0)
				break;

			searchFrom = nextHeaderStart;
		}

		return result;
	}

	private static int FindNextHeaderStart(string markdown, int from)
	{
		// A header starts at 'from' when the previous header's newline is immediately
		// followed by "## " (consecutive headers represent an intentionally empty body).
		if (markdown.AsSpan(from).StartsWith("## ", StringComparison.Ordinal))
			return from;

		int match = markdown.IndexOf("\n## ", from, StringComparison.Ordinal);
		return match < 0 ? -1 : match + 1;
	}

	private static string TrimOneTrailingNewline(string text)
		=> text.EndsWith('\n') ? text[..^1] : text;

	private static string NormalizeLineEndings(string text)
	{
		var normalized = new StringBuilder(text.Length);

		for (int i = 0; i < text.Length; i++)
			if (text[i] == '\r')
			{
				normalized.Append('\n');
				if (i + 1 < text.Length && text[i + 1] == '\n')
					i++;
			}
			else
				normalized.Append(text[i]);

		return normalized.ToString();
	}
}
