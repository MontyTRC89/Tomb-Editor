using DarkUI.Controls;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.Scripting.Resources;

namespace TombIDE.ScriptEditor.Controls
{
	internal partial class ObjectBrowser : UserControl
	{
		// TODO: Refactor

		private IDE _ide;

		private string _editorText = string.Empty;

		#region Initialization

		public ObjectBrowser()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;
		}

		public void UpdateContent(string editorText)
		{
			_editorText = editorText;

			UpdateTreeView();
		}

		#endregion Initialization

		#region Events

		private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			List<string> data = (List<string>)e.Argument;

			string editorText = data[0];
			string filter = data[1];

			e.Result = GetNodes(editorText, filter);
		}

		private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			List<DarkTreeNode> nodes = (List<DarkTreeNode>)e.Result;

			treeView.Nodes.Clear();
			treeView.Nodes.AddRange(nodes);

			treeView.Invalidate();
		}

		private void treeView_Click(object sender, EventArgs e)
		{
			// If the user hasn't selected any node or the selected node is empty
			if (treeView.SelectedNodes.Count == 0 || string.IsNullOrWhiteSpace(treeView.SelectedNodes[0].Text))
				return;

			// If the selected node is a default item ("Sections", "Levels" etc.)
			foreach (DarkTreeNode node in treeView.Nodes)
			{
				if (treeView.SelectedNodes[0] == node)
					return;
			}

			string objectType = treeView.SelectedNodes[0].FullPath.Split('\\')[0];

			switch (objectType)
			{
				case "Sections":
					_ide.ScriptEditor_SelectObject(treeView.SelectedNodes[0].Text, ObjectType.Section);
					break;

				case "Levels":
					_ide.ScriptEditor_SelectObject(treeView.SelectedNodes[0].Text, ObjectType.Level);
					break;

				case "Includes":
					_ide.ScriptEditor_SelectObject(treeView.SelectedNodes[0].Text, ObjectType.Include);
					break;

				case "Defines":
					_ide.ScriptEditor_SelectObject(treeView.SelectedNodes[0].Text, ObjectType.Define);
					break;
			}
		}

		private void textBox_Search_Enter(object sender, EventArgs e)
		{
			if (textBox_Search.Text == "Search objects...")
				textBox_Search.Text = string.Empty;
		}

		private void textBox_Search_Leave(object sender, EventArgs e)
		{
			if (textBox_Search.Text == string.Empty)
				textBox_Search.Text = "Search objects...";
		}

		private void textBox_Search_TextChanged(object sender, EventArgs e) => UpdateTreeView();

		#endregion Events

		#region Methods

		private void UpdateTreeView()
		{
			timer.Stop();
			timer.Start();
		}

		private List<DarkTreeNode> GetNodes(string editorText, string filter)
		{
			List<DarkTreeNode> nodes = new List<DarkTreeNode>
			{
				new DarkTreeNode("Sections"),
				new DarkTreeNode("Levels"),
				new DarkTreeNode("Includes"),
				new DarkTreeNode("Defines")
			};

			TextDocument document = new TextDocument(editorText);

			// Add all subnodes
			for (int i = 1; i < document.LineCount; i++)
			{
				DocumentLine currentLine = document.GetLineByNumber(i);
				string line = document.GetText(currentLine.Offset, currentLine.Length);

				DarkTreeNode sectionNode = GetSectionNode(line, filter);
				DarkTreeNode levelNode = GetLevelNode(line, filter);
				DarkTreeNode includeNode = GetIncludeNode(line, filter);
				DarkTreeNode defineNode = GetDefineNode(line, filter);

				if (sectionNode != null)
					nodes[0].Nodes.Add(sectionNode);
				else if (levelNode != null)
					nodes[1].Nodes.Add(levelNode);
				else if (includeNode != null)
					nodes[2].Nodes.Add(includeNode);
				else if (defineNode != null)
					nodes[3].Nodes.Add(defineNode);
			}

			// Expand the default nodes
			for (int i = 0; i < nodes.Count; i++)
				nodes[i].Expanded = true;

			List<DarkTreeNode> nodesToRemove = new List<DarkTreeNode>();

			// Remove default nodes which don't contain any items
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i].Nodes.Count == 0)
					nodesToRemove.Add(nodes[i]);
			}

			foreach (DarkTreeNode node in nodesToRemove)
				nodes.Remove(node);

			return nodes;
		}

		private DarkTreeNode GetSectionNode(string line, string filter)
		{
			foreach (string section in ScriptKeywords.Sections)
			{
				// Exclude [Level] sections
				if (section.Equals("level", StringComparison.OrdinalIgnoreCase))
					continue;

				string sectionName = "[" + section + "]";

				// Check if the current line starts a section
				if (line.Trim().StartsWith(sectionName, StringComparison.OrdinalIgnoreCase))
				{
					// Add the node if the section name matches the filter (it always does if there's nothing in the search bar)
					if (sectionName.ToUpper().Contains(filter.ToUpper()))
						return new DarkTreeNode(sectionName);
				}
			}

			return null;
		}

		private DarkTreeNode GetLevelNode(string line, string filter)
		{
			// Remove comments from the line
			line = Regex.Replace(line, ";.*$", string.Empty).Trim();

			Regex regex = new Regex(@"\bName\s*?=\s?"); // Regex rule to find lines that start with "Name = "

			if (regex.IsMatch(line))
			{
				// Get the level name without "Name = " from the line string
				string levelName = regex.Replace(line, string.Empty).Trim();

				// Add the node if the level name matches the filter (it always does if there's nothing in the search bar)
				if (!string.IsNullOrWhiteSpace(levelName) && levelName.ToUpper().Contains(filter.ToUpper()))
					return new DarkTreeNode(levelName);
			}

			return null;
		}

		private DarkTreeNode GetIncludeNode(string line, string filter)
		{
			// Remove comments from the line
			line = Regex.Replace(line, ";.*$", string.Empty).Trim();

			Regex regex = new Regex(@"#include\s\s*?" + "\".*\"", RegexOptions.IgnoreCase); // Regex rule to find #include lines

			if (regex.IsMatch(line))
			{
				string includeFileName = line.Replace("#include", string.Empty).Trim().Split('"')[1];

				// Add the node if the included file name matches the filter (it always does if there's nothing in the search bar)
				if (!string.IsNullOrWhiteSpace(includeFileName) && includeFileName.ToUpper().Contains(filter.ToUpper()))
					return new DarkTreeNode(includeFileName);
			}

			return null;
		}

		private DarkTreeNode GetDefineNode(string line, string filter)
		{
			// Remove comments from the line
			line = Regex.Replace(line, ";.*$", string.Empty).Trim();

			Regex regex = new Regex(@"#define\s\s*?.*", RegexOptions.IgnoreCase); // Regex rule to find #define lines

			if (regex.IsMatch(line))
			{
				string definedConstantName = line.Replace("#define", string.Empty).Trim().Split(' ')[0];

				// Add the node if the defined constant name matches the filter (it always does if there's nothing in the search bar)
				if (!string.IsNullOrWhiteSpace(definedConstantName) && definedConstantName.ToUpper().Contains(filter.ToUpper()))
					return new DarkTreeNode(definedConstantName);
			}

			return null;
		}

		#endregion Methods

		private void timer_Tick(object sender, EventArgs e)
		{
			string filter = string.Empty;

			if (!string.IsNullOrWhiteSpace(textBox_Search.Text) && textBox_Search.Text != "Search objects...")
				filter = textBox_Search.Text.Trim();

			List<string> data = new List<string>
			{
				_editorText,
				filter
			};

			if (!backgroundWorker.IsBusy)
				backgroundWorker.RunWorkerAsync(data);
		}
	}
}
