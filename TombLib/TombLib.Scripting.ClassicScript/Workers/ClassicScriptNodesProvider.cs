using DarkUI.Controls;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TombLib.Scripting.Bases;
using TombLib.Scripting.ClassicScript.Enums;
using TombLib.Scripting.ClassicScript.Parsers;
using TombLib.Scripting.ClassicScript.Resources;
using TombLib.Scripting.Extensions;

namespace TombLib.Scripting.ClassicScript.Workers
{
	public class ClassicScriptNodesProvider : ContentNodesProviderBase
	{
		protected override IEnumerable<DarkTreeNode> GetNodes(string content)
		{
			List<DarkTreeNode> rootNodes = GetRootNodes();

			var document = new TextDocument(content);

			// Add all subnodes
			foreach (DocumentLine line in document.Lines)
			{
				string lineText = document.GetText(line.Offset, line.Length);

				DarkTreeNode sectionNode = GetSectionNode(lineText);
				DarkTreeNode levelNode = GetLevelNode(lineText);
				DarkTreeNode includeNode = GetIncludeNode(lineText);
				DarkTreeNode defineNode = GetDefineNode(lineText);

				if (sectionNode != null)
					rootNodes[0].Nodes.Add(sectionNode);
				else if (levelNode != null)
					rootNodes[1].Nodes.Add(levelNode);
				else if (includeNode != null)
					rootNodes[2].Nodes.Add(includeNode);
				else if (defineNode != null)
					rootNodes[3].Nodes.Add(defineNode);
			}

			// Expand the root nodes
			foreach (DarkTreeNode node in rootNodes)
				node.Expanded = true;

			var nodesToRemove = new List<DarkTreeNode>();

			// Remove root nodes which don't contain any items
			foreach (DarkTreeNode node in rootNodes)
			{
				if (node.Nodes.Count == 0)
					nodesToRemove.Add(node);
			}

			foreach (DarkTreeNode node in nodesToRemove)
				rootNodes.Remove(node);

			return rootNodes;
		}

		private List<DarkTreeNode> GetRootNodes()
			=> new List<DarkTreeNode>
			{
				new DarkTreeNode("Sections"),
				new DarkTreeNode("Levels"),
				new DarkTreeNode("Includes"),
				new DarkTreeNode("Defines")
			};

		private DarkTreeNode GetSectionNode(string lineText)
		{
			if (LineParser.IsSectionHeaderLine(lineText))
			{
				string headerText = LineParser.GetSectionHeaderText(lineText);
				bool isLevelHeader = headerText.Equals("Level", StringComparison.OrdinalIgnoreCase);

				if (!isLevelHeader
					&& Keywords.Sections.ToList().Exists(x => x.Equals(headerText, StringComparison.OrdinalIgnoreCase))
					&& headerText.Contains(Filter, StringComparison.OrdinalIgnoreCase))
					return new DarkTreeNode($"[{headerText}]") { Tag = ObjectType.Section };
			}

			return null;
		}

		private DarkTreeNode GetLevelNode(string lineText)
		{
			var regex = new Regex(Patterns.NameCommand, RegexOptions.IgnoreCase); // (Name = )

			if (regex.IsMatch(lineText))
			{
				lineText = LineParser.RemoveComments(lineText);
				string levelName = regex.Replace(lineText, string.Empty); // Removes "Name = "

				if (!string.IsNullOrWhiteSpace(levelName) && levelName.Contains(Filter, StringComparison.OrdinalIgnoreCase))
					return new DarkTreeNode(levelName) { Tag = ObjectType.Level };
			}

			return null;
		}

		private DarkTreeNode GetIncludeNode(string lineText)
		{
			var regex = new Regex($"{Patterns.IncludeCommand}({Patterns.FilePath})", RegexOptions.IgnoreCase); // (#include "...")

			if (regex.IsMatch(lineText))
			{
				string includeFileName = regex.Match(lineText).Groups[1].Value.Trim('"');

				if (!string.IsNullOrWhiteSpace(includeFileName) && includeFileName.Contains(Filter, StringComparison.OrdinalIgnoreCase))
					return new DarkTreeNode(includeFileName) { Tag = ObjectType.Include };
			}

			return null;
		}

		private DarkTreeNode GetDefineNode(string lineText)
		{
			var regex = new Regex(Patterns.DefineCommand + Patterns.DefineValue, RegexOptions.IgnoreCase); // (#define CONSTANT VALUE)

			if (regex.IsMatch(lineText))
			{
				string definedConstantName = regex.Match(lineText).Groups[1].Value;

				if (!string.IsNullOrWhiteSpace(definedConstantName) && definedConstantName.Contains(Filter, StringComparison.OrdinalIgnoreCase))
					return new DarkTreeNode(definedConstantName) { Tag = ObjectType.Define };
			}

			return null;
		}
	}
}
