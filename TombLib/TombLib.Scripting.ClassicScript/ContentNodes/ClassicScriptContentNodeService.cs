using ICSharpCode.AvalonEdit.Document;
using System.Text.RegularExpressions;
using TombLib.Scripting.ClassicScript.Documents;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.Specifications.ClassicScript;

namespace TombLib.Scripting.ClassicScript.ContentNodes;

internal sealed class ClassicScriptContentNodeService
{
	private static readonly Regex DefineCommandRegex = new(Patterns.DefineCommand + Patterns.DefineValue, RegexOptions.IgnoreCase);
	private static readonly Regex IncludeCommandRegex = new($"{Patterns.IncludeCommand}({Patterns.FilePath})", RegexOptions.IgnoreCase);
	private static readonly Regex NameCommandRegex = new(Patterns.NameCommand, RegexOptions.IgnoreCase);

	public IReadOnlyList<ClassicScriptContentNodeGroup> GetNodeGroups(string content, string filter)
	{
		var document = new TextDocument(content ?? string.Empty);
		var sectionNodes = new List<ClassicScriptContentNode>();
		var levelNodes = new List<ClassicScriptContentNode>();
		var includeNodes = new List<ClassicScriptContentNode>();
		var defineNodes = new List<ClassicScriptContentNode>();

		foreach (DocumentLine line in document.Lines)
		{
			string lineText = document.GetText(line.Offset, line.Length);

			ClassicScriptContentNode? sectionNode = GetSectionNode(lineText, filter);

			if (sectionNode is ClassicScriptContentNode matchedSectionNode)
			{
				sectionNodes.Add(matchedSectionNode);
				continue;
			}

			ClassicScriptContentNode? levelNode = GetLevelNode(lineText, filter);

			if (levelNode is ClassicScriptContentNode matchedLevelNode)
			{
				levelNodes.Add(matchedLevelNode);
				continue;
			}

			ClassicScriptContentNode? includeNode = GetIncludeNode(lineText, filter);

			if (includeNode is ClassicScriptContentNode matchedIncludeNode)
			{
				includeNodes.Add(matchedIncludeNode);
				continue;
			}

			ClassicScriptContentNode? defineNode = GetDefineNode(lineText, filter);

			if (defineNode is ClassicScriptContentNode matchedDefineNode)
				defineNodes.Add(matchedDefineNode);
		}

		var groups = new List<ClassicScriptContentNodeGroup>(4);

		AddGroup(groups, "Sections", sectionNodes);
		AddGroup(groups, "Levels", levelNodes);
		AddGroup(groups, "Includes", includeNodes);
		AddGroup(groups, "Defines", defineNodes);

		return groups;
	}

	private static void AddGroup(List<ClassicScriptContentNodeGroup> groups, string header, List<ClassicScriptContentNode> nodes)
	{
		if (nodes.Count > 0)
			groups.Add(new ClassicScriptContentNodeGroup(header, nodes));
	}

	private static ClassicScriptContentNode? GetSectionNode(string lineText, string filter)
	{
		if (!LineParser.IsSectionHeaderLine(lineText))
			return null;

		string? headerText = LineParser.GetSectionHeaderText(lineText);

		if (string.IsNullOrWhiteSpace(headerText))
			return null;

		bool isLevelHeader = headerText.Equals("Level", StringComparison.OrdinalIgnoreCase);

		if (isLevelHeader
			|| !ClassicScriptKeywords.Sections.Any(x => x.Equals(headerText, StringComparison.OrdinalIgnoreCase))
			|| !headerText.Contains(filter, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		return new ClassicScriptContentNode($"[{headerText}]", ObjectType.Section);
	}

	private static ClassicScriptContentNode? GetLevelNode(string lineText, string filter)
	{
		if (!NameCommandRegex.IsMatch(lineText))
			return null;

		string sanitizedLineText = LineParser.RemoveComments(lineText);
		string levelName = NameCommandRegex.Replace(sanitizedLineText, string.Empty);

		if (string.IsNullOrWhiteSpace(levelName) || !levelName.Contains(filter, StringComparison.OrdinalIgnoreCase))
			return null;

		return new ClassicScriptContentNode(levelName, ObjectType.Level);
	}

	private static ClassicScriptContentNode? GetIncludeNode(string lineText, string filter)
	{
		if (!IncludeCommandRegex.IsMatch(lineText))
			return null;

		string includeFileName = IncludeCommandRegex.Match(lineText).Groups[1].Value.Trim('"');

		if (string.IsNullOrWhiteSpace(includeFileName) || !includeFileName.Contains(filter, StringComparison.OrdinalIgnoreCase))
			return null;

		return new ClassicScriptContentNode(includeFileName, ObjectType.Include);
	}

	private static ClassicScriptContentNode? GetDefineNode(string lineText, string filter)
	{
		if (!DefineCommandRegex.IsMatch(lineText))
			return null;

		string definedConstantName = DefineCommandRegex.Match(lineText).Groups[1].Value;

		if (string.IsNullOrWhiteSpace(definedConstantName) || !definedConstantName.Contains(filter, StringComparison.OrdinalIgnoreCase))
			return null;

		return new ClassicScriptContentNode(definedConstantName, ObjectType.Define);
	}
}

internal sealed class ClassicScriptContentNodeGroup(string header, IReadOnlyList<ClassicScriptContentNode> nodes)
{
	public string Header { get; } = header ?? string.Empty;
	public IReadOnlyList<ClassicScriptContentNode> Nodes { get; } = nodes ?? [];
}

internal readonly record struct ClassicScriptContentNode(string Text, ObjectType ObjectType);
