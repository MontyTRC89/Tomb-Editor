using DarkUI.Controls;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Extensions;
using TombLib.Scripting.GameFlowScript.Enums;
using TombLib.Scripting.GameFlowScript.Parsers;
using TombLib.Scripting.GameFlowScript.Resources;

namespace TombLib.Scripting.GameFlowScript.Workers
{
	public class GameFlowNodesProvider : ContentNodesProviderBase
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

				if (sectionNode != null)
					rootNodes[0].Nodes.Add(sectionNode);
				else if (levelNode != null)
					rootNodes[1].Nodes.Add(levelNode);
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
				new DarkTreeNode("Levels")
			};

		private DarkTreeNode GetSectionNode(string lineText)
		{
			if (LineParser.IsSectionHeaderLine(lineText))
			{
				string headerText = LineParser.GetSectionHeaderText(lineText);
				bool isLevelHeader = headerText.Equals("Level", StringComparison.OrdinalIgnoreCase);
				bool isEndHeader = headerText.Equals("END", StringComparison.OrdinalIgnoreCase);

				if (!isLevelHeader && !isEndHeader
					&& Keywords.Sections.ToList().Exists(x => x.Equals(headerText, StringComparison.OrdinalIgnoreCase))
					&& headerText.Contains(Filter, StringComparison.OrdinalIgnoreCase))
					return new DarkTreeNode($"{headerText}") { Tag = ObjectType.Section };
			}

			return null;
		}

		private DarkTreeNode GetLevelNode(string lineText)
		{
			var regex = new Regex(Patterns.LevelProperty, RegexOptions.IgnoreCase);

			if (regex.IsMatch(lineText))
			{
				lineText = LineParser.RemoveComments(lineText);
				string levelName = regex.Replace(lineText, string.Empty); // Removes "LEVEL: "

				if (!string.IsNullOrWhiteSpace(levelName) && levelName.Contains(Filter, StringComparison.OrdinalIgnoreCase))
					return new DarkTreeNode(levelName) { Tag = ObjectType.Level };
			}

			return null;
		}
	}
}
