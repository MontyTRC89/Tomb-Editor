using DarkUI.Controls;
using FastColoredTextBoxNS;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ScriptEditor
{
	public class ObjectBrowser
	{
		public static void GoToSelectedObject(DarkTreeView objectBrowser, FastColoredTextBox textEditor)
		{
			// If the user hasn't selected any node or the selected node is empty
			if (objectBrowser.SelectedNodes.Count < 1 || string.IsNullOrWhiteSpace(objectBrowser.SelectedNodes[0].Text))
			{
				return;
			}

			// If the selected node is a default item
			if (objectBrowser.SelectedNodes[0] == objectBrowser.Nodes[0] || objectBrowser.SelectedNodes[0] == objectBrowser.Nodes[1])
			{
				return;
			}

			// Scan all lines
			for (int i = 0; i < textEditor.LinesCount; i++)
			{
				// Find the line that contains the node text
				if (textEditor.GetLineText(i).ToLower().Replace("·", " ").Contains(objectBrowser.SelectedNodes[0].Text.ToLower()))
				{
					textEditor.Focus();
					textEditor.Navigate(i); // Scroll to the line position

					// Select the line
					textEditor.Selection = new Range(textEditor, 0, i, textEditor.GetLineText(i).Length, i);
					return;
				}
			}
		}

		public static void ResetNodes(DarkTreeView objectBrowser)
		{
			bool headersNodeExpanded = false;
			bool levelsNodeExpanded = false;

			// If all default nodes are set
			if (objectBrowser.Nodes.Count == 2)
			{
				// Cache the current expand state
				headersNodeExpanded = objectBrowser.Nodes[0].Expanded;
				levelsNodeExpanded = objectBrowser.Nodes[1].Expanded;
			}

			objectBrowser.Nodes.Clear();
			objectBrowser.Nodes.Add(new DarkTreeNode("Headers"));
			objectBrowser.Nodes.Add(new DarkTreeNode("Levels"));

			// If all default nodes are set
			if (objectBrowser.Nodes.Count == 2)
			{
				// Set the previous expand state
				objectBrowser.Nodes[0].Expanded = headersNodeExpanded;
				objectBrowser.Nodes[1].Expanded = levelsNodeExpanded;
			}

			objectBrowser.Invalidate();
		}

		public static void UpdateNodes(DarkTreeView objectBrowser, FastColoredTextBox textEditor, string filter)
		{
			bool headersNodeExpanded = false;
			bool levelsNodeExpanded = false;

			// If all default nodes are set
			if (objectBrowser.Nodes.Count == 2)
			{
				// Cache the current expand state
				headersNodeExpanded = objectBrowser.Nodes[0].Expanded;
				levelsNodeExpanded = objectBrowser.Nodes[1].Expanded;
			}

			ResetNodes(objectBrowser);

			// Scan all lines
			for (int i = 0; i < textEditor.LinesCount; i++)
			{
				AddHeaderNode(objectBrowser, textEditor, i, filter);
				AddLevelNode(objectBrowser, textEditor, i, filter);
			}

			// If all default nodes are set
			if (objectBrowser.Nodes.Count == 2)
			{
				// Set the previous expand state
				objectBrowser.Nodes[0].Expanded = headersNodeExpanded;
				objectBrowser.Nodes[1].Expanded = levelsNodeExpanded;
			}

			objectBrowser.Invalidate();
		}

		private static void AddHeaderNode(DarkTreeView objectBrowser, FastColoredTextBox textEditor, int lineNumber, string filter)
		{
			// Get header key words
			List<string> headers = SyntaxKeyWords.Headers();

			// Loop through headers
			foreach (string header in headers)
			{
				// Add brackets to the header
				string fullHeader = "[" + header + "]";

				// If the current line starts with a header and the header is not a [Level] header
				if (textEditor.GetLineText(lineNumber).StartsWith(fullHeader) && fullHeader != "[Level]")
				{
					DarkTreeNode headerNode = new DarkTreeNode(fullHeader);

					// If the header name matches the filter (It always does if there's nothing in the search bar)
					if (headerNode.Text.ToLower().Contains(filter.ToLower()))
					{
						objectBrowser.Nodes[0].Nodes.Add(headerNode);
					}
				}
			}
		}

		private static void AddLevelNode(DarkTreeView objectBrowser, FastColoredTextBox textEditor, int lineNumber, string filter)
		{
			// Regex rule to find lines that start with "Name = "
			Regex rgx = new Regex(@"\bName[\s·]?=[\s·]?");

			// If we found a line that starts with our Regex rule ("Name = ")
			if (rgx.IsMatch(textEditor.GetLineText(lineNumber)))
			{
				// Create a new node, remove "Name = ", replace dots with spaces and trim it, so we only get the level name string
				DarkTreeNode levelNode = new DarkTreeNode(rgx.Replace(textEditor.GetLineText(lineNumber), string.Empty).Replace("·", " ").Trim());

				// If the level name matches the filter (It always does if there's nothing in the search bar)
				if (levelNode.Text.ToLower().Contains(filter.ToLower()))
				{
					objectBrowser.Nodes[1].Nodes.Add(levelNode);
				}
			}
		}
	}
}
