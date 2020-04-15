using DarkUI.Controls;
using DarkUI.Forms;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombIDE.ScriptEditor.Controls;

namespace TombIDE.ScriptEditor.Forms
{
	public partial class FormFindReplace : DarkForm
	{
		private CustomTabControl _editorTabControl;
		private CustomTabControl _infoTabControl;
		private DarkTreeView _searchResults;

		#region Initialization

		public FormFindReplace(CustomTabControl editorTabControl, CustomTabControl infoTabControl)
		{
			InitializeComponent();

			_editorTabControl = editorTabControl;
			_infoTabControl = infoTabControl;

			_searchResults = infoTabControl.TabPages[2].Controls.OfType<DarkTreeView>().First();
		}

		public void Show(IWin32Window owner, string selectedText)
		{
			if (!Visible)
			{
				Show(owner);

				label_Status.Text = string.Empty;

				textBox_Find.Text = selectedText;
				textBox_Find.SelectAll();
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Hide();
			e.Cancel = true;

			base.OnClosing(e);
		}

		#endregion Initialization

		#region Events

		private void button_FindPrev_Click(object sender, EventArgs e) => FindPrev();
		private void button_FindNext_Click(object sender, EventArgs e) => FindNext();

		private void button_Find_Click(object sender, EventArgs e)
		{
			if (radioButton_Up.Checked)
				FindPrev();
			else if (radioButton_Down.Checked)
				FindNext();
		}

		private void button_ReplacePrev_Click(object sender, EventArgs e) => ReplacePrev();
		private void button_ReplaceNext_Click(object sender, EventArgs e) => ReplaceNext();

		private void button_Replace_Click(object sender, EventArgs e)
		{
			if (radioButton_Up.Checked)
				ReplacePrev();
			else if (radioButton_Down.Checked)
				ReplaceNext();
		}

		private void button_FindAll_Click(object sender, EventArgs e) => FindAll();
		private void button_ReplaceAll_Click(object sender, EventArgs e) => ReplaceAll();

		#endregion Events

		#region Find methods

		private bool FindPrev()
		{
			if (string.IsNullOrWhiteSpace(textBox_Find.Text))
			{
				ShowError("Invalid input.");
				return false;
			}

			AvalonTextBox currentTabTextBox = GetCurrentTabTextBox();
			string pattern = GetCurrentPattern();
			RegexOptions options = GetCurrentRegexOptions();

			MatchCollection documentMatchCollection = Regex.Matches(currentTabTextBox.Text, pattern, options);

			if (documentMatchCollection.Count == 0) // If no matches were found in the current document
			{
				if (radioButton_Current.Checked)
					ShowError("No matches found in the current document."); // Search cancelled
				else if (radioButton_AllTabs.Checked)
				{
					if (GetAllTabsMatchCount(pattern, options) == 0) // If no matches were found in any tab
						ShowError("No matches found."); // Search cancelled
					else
						FindPrevInPrevTab(); // Go to the previous tab to find matches there
				}
			}
			else // Matches were found in the current document
			{
				string textBeforeSelection = GetTextBeforeSelection(currentTabTextBox.Text, currentTabTextBox.SelectionStart);
				MatchCollection sectionMatchCollection = Regex.Matches(textBeforeSelection, pattern, options);

				if (sectionMatchCollection.Count == 0) // If no matches were found in that section of the document
				{
					if (radioButton_Current.Checked)
					{
						MoveCaretToDocumentStart(currentTabTextBox);
						ShowWarning("Reached the start of the document with no more matches found."); // Search ends here
					}
					else if (radioButton_AllTabs.Checked)
						FindPrevInPrevTab(); // Go to the previous tab to find more matches
				}
				else // Matches were found in that section of the document
				{
					// Get the last match of that section, since we're going upwards
					Match match = sectionMatchCollection[sectionMatchCollection.Count - 1];

					// Select the match and scroll to its position
					currentTabTextBox.Select(match.Index, match.Length);
					currentTabTextBox.ScrollTo(currentTabTextBox.TextArea.Caret.Position.Line, currentTabTextBox.TextArea.Caret.Position.Column);

					UpdateStatusLabel(currentTabTextBox.Text, pattern, options);

					return true;
				}
			}

			return false;
		}

		private bool FindNext()
		{
			if (string.IsNullOrWhiteSpace(textBox_Find.Text))
			{
				ShowError("Invalid input.");
				return false;
			}

			AvalonTextBox currentTabTextBox = GetCurrentTabTextBox();
			string pattern = GetCurrentPattern();
			RegexOptions options = GetCurrentRegexOptions();

			MatchCollection documentMatchCollection = Regex.Matches(currentTabTextBox.Text, pattern, options);

			if (documentMatchCollection.Count == 0) // If no matches were found in the current document
			{
				if (radioButton_Current.Checked)
					ShowError("No matches found in the current document."); // Search cancelled
				else if (radioButton_AllTabs.Checked)
				{
					if (GetAllTabsMatchCount(pattern, options) == 0) // If no matches were found in any tab
						ShowError("No matches found."); // Search cancelled
					else
						FindNextInNextTab(); // Go to the next tab to find matches there
				}
			}
			else // Matches were found in the current document
			{
				int selectionEnd = currentTabTextBox.SelectionStart + currentTabTextBox.SelectionLength;
				string textAfterSelection = GetTextAfterSelection(currentTabTextBox.Text, selectionEnd);

				MatchCollection sectionMatchCollection = Regex.Matches(textAfterSelection, pattern, options);

				if (sectionMatchCollection.Count == 0) // If no matches were found in that section of the document
				{
					if (radioButton_Current.Checked)
					{
						MoveCaretToDocumentEnd(currentTabTextBox);
						ShowWarning("Reached the end of the document with no more matches found."); // Search ends here
					}
					else if (radioButton_AllTabs.Checked)
						FindNextInNextTab(); // Go to the next tab to find more matches there
				}
				else // Matches were found in that section of the document
				{
					// Get the first match of that section, since we're going downwards
					Match match = sectionMatchCollection[0];

					int cutStringLength = currentTabTextBox.Document.TextLength - textAfterSelection.Length;

					// Select the match and scroll to its position
					currentTabTextBox.Select(cutStringLength + match.Index, match.Length);
					currentTabTextBox.ScrollTo(currentTabTextBox.TextArea.Caret.Position.Line, currentTabTextBox.TextArea.Caret.Position.Column);

					UpdateStatusLabel(currentTabTextBox.Text, pattern, options);

					return true;
				}
			}

			return false;
		}

		private void FindPrevInPrevTab()
		{
			if (_editorTabControl.SelectedIndex == 0)
			{
				MoveCaretToDocumentStart(GetCurrentTabTextBox());
				ShowWarning("Reached the start of the first tab document with no more matches found."); // Search ends here
			}
			else
			{
				_editorTabControl.SelectedIndex -= 1;

				AvalonTextBox nextTarget = GetCurrentTabTextBox(); // The tab has changed, therefore we can get the current tab's TextBox

				MoveCaretToDocumentEnd(nextTarget);
				FindPrev();
			}
		}

		private void FindNextInNextTab()
		{
			if (_editorTabControl.SelectedIndex == _editorTabControl.TabCount - 1)
			{
				MoveCaretToDocumentEnd(GetCurrentTabTextBox());
				ShowWarning("Reached the end of the last tab document with no more matches found."); // Search ends here
			}
			else
			{
				_editorTabControl.SelectedIndex += 1;

				AvalonTextBox nextTarget = GetCurrentTabTextBox(); // The tab has changed, therefore we can get the current tab's TextBox

				MoveCaretToDocumentStart(nextTarget);
				FindNext();
			}
		}

		#endregion Find methods

		#region Replace methods

		private bool ReplacePrev()
		{
			if (!FindPrev())
				return false;

			ReplaceMatch();
			return true;
		}

		private bool ReplaceNext()
		{
			if (!FindNext())
				return false;

			ReplaceMatch();
			return true;
		}

		private void ReplaceMatch()
		{
			AvalonTextBox currentTextBox = GetCurrentTabTextBox();

			if (radioButton_Normal.Checked)
				currentTextBox.SelectedText = textBox_Replace.Text;
			else if (radioButton_Regex.Checked)
			{
				Match match = Regex.Match(currentTextBox.SelectedText, textBox_Find.Text, RegexOptions.IgnoreCase);

				currentTextBox.SelectedText = textBox_Replace.Text;

				if (match.Success)
				{
					GroupCollection groups = match.Groups;

					if (groups.Count > 0)
					{
						for (int i = 1; i < groups.Count; i++)
						{
							MatchCollection groupMatches = Regex.Matches(currentTextBox.SelectedText, @"\$\d*");

							if (groupMatches.Count == 0)
								break;

							Group group = groups[i];
							Match groupMatch = groupMatches[0];

							if (groupMatch.Value.Trim('$').Trim() == i.ToString())
								currentTextBox.SelectedText = currentTextBox.SelectedText.Replace(groupMatch.Value, group.Value);
						}
					}
				}
			}
		}

		#endregion Replace methods

		#region Bulk methods

		private void FindAll()
		{
			_searchResults.Nodes.Clear();

			if (string.IsNullOrWhiteSpace(textBox_Find.Text))
			{
				ShowError("Invalid input.");
				return;
			}

			_infoTabControl.SelectTab(2);

			string pattern = GetCurrentPattern();
			RegexOptions options = GetCurrentRegexOptions();

			if (radioButton_Current.Checked)
			{
				AvalonTextBox currentTabTextBox = GetCurrentTabTextBox();
				MatchCollection documentMatchCollection = Regex.Matches(currentTabTextBox.Text, pattern, options);

				if (documentMatchCollection.Count == 0)
				{
					ShowError("No matches found.");
					return;
				}

				DarkTreeNode fileNode = new DarkTreeNode(_editorTabControl.SelectedTab.Text + " (Matches: " + documentMatchCollection.Count + ")");

				foreach (Match match in documentMatchCollection)
				{
					DocumentLine line = currentTabTextBox.Document.GetLineByOffset(match.Index);
					string lineText = currentTabTextBox.Document.GetText(line.Offset, line.Length);

					fileNode.Nodes.Add(new DarkTreeNode
					{
						Text = "(" + line.LineNumber + ") " + lineText,
						Tag = match
					});
				}

				fileNode.Expanded = true;
				_searchResults.Nodes.Add(fileNode);

				ShowStatusInfo(fileNode.Nodes.Count + " matches found in the current document.");
			}
			else if (radioButton_AllTabs.Checked)
			{
				if (GetAllTabsMatchCount(pattern, options) == 0)
				{
					ShowError("No matches found.");
					return;
				}

				foreach (TabPage tab in _editorTabControl.TabPages)
				{
					AvalonTextBox tabTextBox = (AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child;
					MatchCollection documentMatchCollection = Regex.Matches(tabTextBox.Text, pattern, options);

					if (documentMatchCollection.Count == 0)
						continue;

					DarkTreeNode fileNode = new DarkTreeNode(tab.Text + " (Matches: " + documentMatchCollection.Count + ")");

					foreach (Match match in documentMatchCollection)
					{
						DocumentLine line = tabTextBox.Document.GetLineByOffset(match.Index);
						string lineText = tabTextBox.Document.GetText(line.Offset, line.Length);

						fileNode.Nodes.Add(new DarkTreeNode
						{
							Text = "(" + line.LineNumber + ") " + lineText,
							Tag = match
						});
					}

					fileNode.Expanded = true;
					_searchResults.Nodes.Add(fileNode);
				}

				ShowStatusInfo(GetAllTabsMatchCount(pattern, options) + " matches found in " + _searchResults.Nodes.Count + " tabs.");
			}
		}

		private void ReplaceAll()
		{
			if (string.IsNullOrWhiteSpace(textBox_Find.Text))
			{
				ShowError("Invalid input.");
				return;
			}

			string pattern = GetCurrentPattern();
			RegexOptions options = GetCurrentRegexOptions();

			int replacedItemCount = 0;

			if (radioButton_Current.Checked)
			{
				AvalonTextBox currentTabTextBox = GetCurrentTabTextBox();
				MatchCollection documentMatchCollection = Regex.Matches(currentTabTextBox.Text, pattern, options);

				MoveCaretToDocumentStart(currentTabTextBox);

				foreach (Match match in documentMatchCollection)
				{
					if (!ReplaceNext())
						break;

					replacedItemCount++;
				}
			}
			else if (radioButton_AllTabs.Checked)
			{
				if (GetAllTabsMatchCount(pattern, options) == 0)
				{
					ShowError("No matches found.");
					return;
				}

				DialogResult result = DarkMessageBox.Show(this, "Are you sure you want to replace all matching items across all tabs?", "Are you sure?",
					MessageBoxButtons.YesNo, MessageBoxIcon.Question);

				if (result == DialogResult.Yes)
				{
					TabPage cachedTab = _editorTabControl.SelectedTab;

					foreach (TabPage tab in _editorTabControl.TabPages)
					{
						_editorTabControl.SelectTab(tab);

						AvalonTextBox currentTabTextBox = GetCurrentTabTextBox();
						MatchCollection documentMatchCollection = Regex.Matches(currentTabTextBox.Text, pattern, options);

						MoveCaretToDocumentStart(currentTabTextBox);

						foreach (Match match in documentMatchCollection)
						{
							if (!ReplaceNext())
								break;

							replacedItemCount++;
						}
					}

					_editorTabControl.SelectTab(cachedTab);
				}
				else
					return;
			}

			if (replacedItemCount == 0)
				ShowError("No matches found.");
			else
				ShowStatusInfo("Replaced " + replacedItemCount + " matches.");
		}

		#endregion Bulk methods

		#region Other methods

		private void UpdateStatusLabel(string currentDocumentText, string pattern, RegexOptions options)
		{
			int currentDocumentMatchCount = Regex.Matches(currentDocumentText, pattern, options).Count;

			if (radioButton_Current.Checked)
				ShowStatusInfo(currentDocumentMatchCount + " matches found in the current document.");
			else if (radioButton_AllTabs.Checked)
				ShowStatusInfo(currentDocumentMatchCount + " matches found in the current document. " + GetAllTabsMatchCount(pattern, options) + " in all tabs combined.");
		}

		private int GetAllTabsMatchCount(string pattern, RegexOptions options)
		{
			int matchCount = 0;

			foreach (TabPage tab in _editorTabControl.TabPages)
			{
				AvalonTextBox tabTextBox = (AvalonTextBox)tab.Controls.OfType<ElementHost>().First().Child;
				matchCount += Regex.Matches(tabTextBox.Text, pattern, options).Count;
			}

			return matchCount;
		}

		private void ShowError(string message)
		{
			label_Status.ForeColor = Color.FromArgb(255, 128, 128); // Red
			label_Status.Text = message;
		}

		private void ShowWarning(string message)
		{
			label_Status.ForeColor = Color.FromArgb(255, 255, 128); // Yellow
			label_Status.Text = message;
		}

		private void ShowStatusInfo(string message)
		{
			label_Status.ForeColor = Color.FromArgb(128, 255, 128); // Green
			label_Status.Text = message;
		}

		private void MoveCaretToDocumentStart(AvalonTextBox documentTextBox)
		{
			documentTextBox.SelectionStart = 0;
			documentTextBox.SelectionLength = 0;

			documentTextBox.CaretOffset = 0;
		}

		private void MoveCaretToDocumentEnd(AvalonTextBox documentTextBox)
		{
			documentTextBox.SelectionStart = 0;
			documentTextBox.SelectionLength = 0;

			documentTextBox.CaretOffset = documentTextBox.Document.TextLength;
		}

		private AvalonTextBox GetCurrentTabTextBox()
		{
			return (AvalonTextBox)_editorTabControl.SelectedTab.Controls.OfType<ElementHost>().First().Child;
		}

		private string GetCurrentPattern()
		{
			// Check if "Regular Expressions" mode is enabled
			string pattern = radioButton_Regex.Checked ? textBox_Find.Text : Regex.Escape(textBox_Find.Text);

			// Check if "Match Whole Words" is checked
			pattern = checkBox_WholeWords.Checked ? @"\b" + pattern + @"\b" : pattern;

			return pattern;
		}

		private RegexOptions GetCurrentRegexOptions()
		{
			// Check if "Case Sensitive" is checked
			RegexOptions options = checkBox_CaseSensitive.Checked ? RegexOptions.None : RegexOptions.IgnoreCase;

			return options;
		}

		private string GetTextBeforeSelection(string documentText, int selectionStartIndex)
		{
			// Get a substring from the start of the document till the current SelectionStart index and use it to find the previous match.
			// Without such a substring, we would always end up in the last match occurrence of the entire document,
			// which means that we wouldn't move at all.

			return documentText.Substring(0, selectionStartIndex);
		}

		private string GetTextAfterSelection(string documentText, int selectionEndIndex)
		{
			// Get a substring from the SelectionEnd index till the end of the current document and use it to find the next match.
			// Without such a substring, we would always end up in the first match occurrence of the entire document,
			// which means that we wouldn't move at all.

			return documentText.Substring(selectionEndIndex, documentText.Length - selectionEndIndex);
		}

		#endregion Other methods
	}
}
