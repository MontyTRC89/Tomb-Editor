using DarkUI.Forms;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Enums;
using TombLib.Scripting.Objects;

namespace TombLib.Scripting.Forms
{
	public partial class FormFindReplace : DarkForm
	{
		private TabControl _targetTabControl = null;
		private TextEditorBase _targetTextEditor = null;

		#region Construction and public methods

		public FormFindReplace(TextEditorBase targetTextEditor)
		{
			InitializeComponent();

			_targetTextEditor = targetTextEditor;

			radioButton_AllTabs.Enabled = false;
		}

		public FormFindReplace(TabControl targetTabControl)
		{
			InitializeComponent();

			_targetTabControl = targetTabControl;
		}

		public void Show(IWin32Window owner, string initialFindText)
		{
			if (!Visible)
			{
				Show(owner);

				if (!string.IsNullOrEmpty(initialFindText))
				{
					textBox_Find.Text = initialFindText;
					textBox_Find.SelectAll();
				}

				label_Status.Text = string.Empty;
			}
		}

		#endregion Construction and public methods

		#region Events

		protected override void OnClosing(CancelEventArgs e)
		{
			Hide();
			e.Cancel = true; // This form should never be closed during runtime

			base.OnClosing(e);
		}

		private void button_FindPrev_Click(object sender, EventArgs e) => Find(FindingOrder.Prev);
		private void button_FindNext_Click(object sender, EventArgs e) => Find(FindingOrder.Next);

		private void button_Find_Click(object sender, EventArgs e)
		{
			if (radioButton_Up.Checked)
				Find(FindingOrder.Prev);
			else if (radioButton_Down.Checked)
				Find(FindingOrder.Next);
		}

		private void button_ReplacePrev_Click(object sender, EventArgs e) => Replace(FindingOrder.Prev);
		private void button_ReplaceNext_Click(object sender, EventArgs e) => Replace(FindingOrder.Next);

		private void button_Replace_Click(object sender, EventArgs e)
		{
			if (radioButton_Up.Checked)
				Replace(FindingOrder.Prev);
			else if (radioButton_Down.Checked)
				Replace(FindingOrder.Next);
		}

		private void button_FindAll_Click(object sender, EventArgs e) => FindAll();
		private void button_ReplaceAll_Click(object sender, EventArgs e) => ReplaceAll();

		public event FindReplaceEventHandler FindAllPerformed;

		protected virtual void OnFindAllPerformed(FindReplaceEventArgs e)
			=> FindAllPerformed?.Invoke(this, e);

		#endregion Events

		#region Find methods

		private bool Find(FindingOrder order)
		{
			if (string.IsNullOrWhiteSpace(textBox_Find.Text))
			{
				ShowError("Invalid input.");
				return false;
			}

			TextEditorBase targetTextEditor = _targetTextEditor ?? GetTextEditorOfTab(_targetTabControl.SelectedTab);
			string pattern = GetCurrentPattern();
			RegexOptions options = GetCurrentRegexOptions();

			if (targetTextEditor != null && Regex.Matches(targetTextEditor.Text, pattern, options).Count == 0) // If no matches were found in the current document
			{
				if (radioButton_Current.Checked)
					ShowError("No matches found in the current document."); // Search cancelled
				else if (radioButton_AllTabs.Checked)
					FindMatchInAnotherTab(order, pattern, options);
			}
			else if (targetTextEditor == null)
			{
				if (radioButton_Current.Checked)
					ShowError("Unsupported document."); // Search cancelled
				else if (radioButton_AllTabs.Checked)
					FindMatchInAnotherTab(order, pattern, options);
			}
			else // Matches were found in the current document
				return FindMatch(order, targetTextEditor, pattern, options);

			return false;
		}

		private bool FindMatch(FindingOrder order, TextEditorBase textEditor, string pattern, RegexOptions options)
		{
			MatchCollection sectionMatchCollection = GetMatchCollectionFromSection(order, textEditor, pattern, options);

			if (sectionMatchCollection.Count == 0) // If no matches were found in that section of the document
			{
				if (radioButton_Current.Checked)
					EndSuccessfulSearch(order, textEditor);
				else if (radioButton_AllTabs.Checked)
					FindMatchInAnotherTab(order, pattern, options);
			}
			else // Matches were found in that section of the document
			{
				SelectMatch(order, textEditor, sectionMatchCollection);

				UpdateStatusLabel(textEditor.Text, pattern, options);
				return true;
			}

			return false;
		}

		private void FindMatchInAnotherTab(FindingOrder order, string pattern, RegexOptions options)
		{
			if (GetAllTabsMatchCount(pattern, options) == 0) // If no matches were found in any tab
				ShowError("No matches found."); // Search cancelled
			else
				switch (order)
				{
					case FindingOrder.Prev:
						FindPrevInPrevTab(); // Go to the previous tab to find matches there
						break;

					case FindingOrder.Next:
						FindNextInNextTab(); // Go to the next tab to find matches there
						break;
				}
		}

		private void FindPrevInPrevTab()
		{
			if (_targetTabControl.SelectedIndex == 0)
			{
				TextEditorBase textEditor = GetTextEditorOfTab(_targetTabControl.SelectedTab);

				if (textEditor != null)
					MoveCaretToDocumentStart(textEditor);

				ShowWarning("Reached the start of the first tab document with no more matches found."); // Search ends here
			}
			else
			{
				_targetTabControl.SelectedIndex--;

				TextEditorBase nextTarget = GetTextEditorOfTab(_targetTabControl.SelectedTab); // The tab has changed, therefore we can get the current tab's TextEditor

				if (nextTarget == null)
					FindPrevInPrevTab();
				else
				{
					MoveCaretToDocumentEnd(nextTarget);

					Find(FindingOrder.Prev);
				}
			}
		}

		private void FindNextInNextTab()
		{
			if (_targetTabControl.SelectedIndex == _targetTabControl.TabCount - 1)
			{
				TextEditorBase textEditor = GetTextEditorOfTab(_targetTabControl.SelectedTab);

				if (textEditor != null)
					MoveCaretToDocumentEnd(textEditor);

				ShowWarning("Reached the end of the last tab document with no more matches found."); // Search ends here
			}
			else
			{
				_targetTabControl.SelectedIndex++;

				TextEditorBase nextTarget = GetTextEditorOfTab(_targetTabControl.SelectedTab); // The tab has changed, therefore we can get the current tab's TextEditor

				if (nextTarget == null)
					FindNextInNextTab();
				else
				{
					MoveCaretToDocumentStart(nextTarget);

					Find(FindingOrder.Next);
				}
			}
		}

		private void SelectMatch(FindingOrder order, TextEditorBase textEditor, MatchCollection sectionMatchCollection)
		{
			switch (order)
			{
				case FindingOrder.Prev:
				{
					// Get the last match of that section, since we're going upwards
					Match lastMatch = sectionMatchCollection[sectionMatchCollection.Count - 1];

					textEditor.Select(lastMatch.Index, lastMatch.Length); // Select the match
					break;
				}
				case FindingOrder.Next:
				{
					// Get the first match of that section, since we're going downwards
					Match firstMatch = sectionMatchCollection[0];

					int selectionEnd = textEditor.SelectionStart + textEditor.SelectionLength;
					string textAfterSelection = GetTextAfterSelection(textEditor.Text, selectionEnd);
					int cutStringLength = textEditor.Document.TextLength - textAfterSelection.Length;

					textEditor.Select(cutStringLength + firstMatch.Index, firstMatch.Length); // Select the match
					break;
				}
			}

			textEditor.ScrollTo(textEditor.TextArea.Caret.Position.Line, textEditor.TextArea.Caret.Position.Column);
		}

		private void EndSuccessfulSearch(FindingOrder order, TextEditorBase textEditor)
		{
			switch (order)
			{
				case FindingOrder.Prev:
					MoveCaretToDocumentStart(textEditor);
					ShowWarning("Reached the start of the document with no more matches found."); // Search ends here
					break;

				case FindingOrder.Next:
					MoveCaretToDocumentEnd(textEditor);
					ShowWarning("Reached the end of the document with no more matches found."); // Search ends here
					break;
			}
		}

		private MatchCollection GetMatchCollectionFromSection(FindingOrder order, TextEditorBase textEditor, string pattern, RegexOptions options)
		{
			switch (order)
			{
				case FindingOrder.Prev:
					string textBeforeSelection = GetTextBeforeSelection(textEditor.Text, textEditor.SelectionStart);
					return Regex.Matches(textBeforeSelection, pattern, options);

				case FindingOrder.Next:
					int selectionEnd = textEditor.SelectionStart + textEditor.SelectionLength;
					string textAfterSelection = GetTextAfterSelection(textEditor.Text, selectionEnd);
					return Regex.Matches(textAfterSelection, pattern, options);
			}

			return null;
		}

		#endregion Find methods

		#region Replace methods

		private bool Replace(FindingOrder order)
		{
			if (!Find(order))
				return false;

			ReplaceMatch();
			return true;
		}

		private void ReplaceMatch()
		{
			TextEditorBase targetTextEditor = _targetTextEditor ?? GetTextEditorOfTab(_targetTabControl.SelectedTab);

			if (radioButton_Normal.Checked)
				targetTextEditor.SelectedText = textBox_Replace.Text;
			else if (radioButton_Regex.Checked)
			{
				string pattern = GetCurrentPattern();
				RegexOptions options = GetCurrentRegexOptions();

				targetTextEditor.SelectedText = AdvancedRegexReplace(targetTextEditor.SelectedText, pattern, textBox_Replace.Text, options);
			}
		}

		/// <summary>
		/// Implements group replacement support.
		/// </summary>
		private string AdvancedRegexReplace(string input, string pattern, string replacement, RegexOptions options)
		{
			string result = input;

			foreach (Match match in Regex.Matches(input, pattern, options))
			{
				GroupCollection groups = match.Groups;

				for (int i = 0; i < groups.Count; i++)
				{
					if (i == 0)
					{
						result = Regex.Replace(result, pattern, replacement, options);
						continue;
					}

					Group group = groups[i];

					foreach (Match groupMatch in Regex.Matches(replacement, @"\$\d*"))
						if (groupMatch.Value.Trim('$').Trim() == i.ToString())
							result = Regex.Replace(result, pattern, replacement.Replace(groupMatch.Value, group.Value), options);
				}
			}

			return result;
		}

		#endregion Replace methods

		#region Find All methods

		private void FindAll()
		{
			if (string.IsNullOrWhiteSpace(textBox_Find.Text))
			{
				ShowError("Invalid input.");
				return;
			}

			string pattern = GetCurrentPattern();
			RegexOptions options = GetCurrentRegexOptions();

			if (radioButton_Current.Checked)
				FindAllInCurrentDocument(pattern, options);
			else if (radioButton_AllTabs.Checked)
				FindAllInAllTabs(pattern, options);
		}

		private void FindAllInCurrentDocument(string pattern, RegexOptions options)
		{
			TextEditorBase targetTextEditor = _targetTextEditor ?? GetTextEditorOfTab(_targetTabControl.SelectedTab);

			if (targetTextEditor == null)
			{
				ShowError("Unsupported document."); // Search cancelled
				return;
			}

			FindReplaceSource src = GetAllMatchesSource(targetTextEditor, pattern, options);

			if (src == null || src.Count == 0)
			{
				ShowError("No matches found.");
				return;
			}

			ShowStatusInfo(src.Count + " matches found in the current document.");

			OnFindAllPerformed(new FindReplaceEventArgs(new List<FindReplaceSource> { src }));
		}

		private void FindAllInAllTabs(string pattern, RegexOptions options)
		{
			int allMatchCount = GetAllTabsMatchCount(pattern, options);

			if (allMatchCount == 0)
				ShowError("No matches found.");
			else
			{
				var sources = new List<FindReplaceSource>();

				foreach (TabPage tab in _targetTabControl.TabPages)
				{
					FindReplaceSource src = GetAllMatchesSource(GetTextEditorOfTab(tab), pattern, options);

					if (src == null || src.Count == 0)
						continue;

					sources.Add(src);
				}

				ShowStatusInfo(allMatchCount + " matches found in " + sources.Count + " tabs.");

				OnFindAllPerformed(new FindReplaceEventArgs(sources));
			}
		}

		private FindReplaceSource GetAllMatchesSource(TextEditorBase textEditor, string pattern, RegexOptions options)
		{
			if (textEditor == null)
				return null;

			MatchCollection documentMatchCollection = Regex.Matches(textEditor.Text, pattern, options);

			if (documentMatchCollection.Count == 0)
				return null;

			var source = new FindReplaceSource { Name = textEditor.Document.FileName };

			foreach (Match match in documentMatchCollection)
			{
				DocumentLine line = textEditor.Document.GetLineByOffset(match.Index);
				string lineText = textEditor.Document.GetText(line.Offset, line.Length);
				string matchSegmentText = match.Value;
				int matchSegmentIndex = Regex.Matches(lineText.Substring(0, match.Index - line.Offset), Regex.Escape(match.Value)).Count;

				source.Add(new FindReplaceItem(line.LineNumber, lineText, matchSegmentText, matchSegmentIndex));
			}

			return source;
		}

		#endregion Find All methods

		#region Replace All methods

		private void ReplaceAll() // TODO: Refactor
		{
			if (string.IsNullOrWhiteSpace(textBox_Find.Text))
			{
				ShowError("Invalid input.");
				return;
			}

			string pattern = GetCurrentPattern();
			RegexOptions options = GetCurrentRegexOptions();

			int matchCount = 0;

			if (radioButton_Current.Checked)
			{
				TextEditorBase currentTabTextEditor = GetTextEditorOfTab(_targetTabControl.SelectedTab);

				if (currentTabTextEditor == null)
				{
					ShowError("Unsupported document.");
					return;
				}

				matchCount = Regex.Matches(currentTabTextEditor.Text, pattern, options).Count;

				if (matchCount > 0)
				{
					currentTabTextEditor.SelectAll();
					currentTabTextEditor.SelectedText = AdvancedRegexReplace(currentTabTextEditor.Text, pattern, textBox_Replace.Text, options);
					MoveCaretToDocumentStart(currentTabTextEditor);
				}
			}
			else if (radioButton_AllTabs.Checked)
			{
				matchCount = GetAllTabsMatchCount(pattern, options);

				if (matchCount > 0)
				{
					DialogResult result = DarkMessageBox.Show(this,
						"Are you sure you want to replace all matching items across all tabs?", "Are you sure?",
						MessageBoxButtons.YesNo, MessageBoxIcon.Question);

					if (result == DialogResult.Yes)
						foreach (TabPage tab in _targetTabControl.TabPages)
						{
							TextEditorBase tabTextEditor = GetTextEditorOfTab(tab);

							if (tabTextEditor == null)
								continue;

							tabTextEditor.SelectAll();
							tabTextEditor.SelectedText = AdvancedRegexReplace(tabTextEditor.Text, pattern, textBox_Replace.Text, options);
							MoveCaretToDocumentStart(tabTextEditor);
						}
					else
						return;
				}
			}

			if (matchCount == 0)
				ShowError("No matches found.");
			else
				ShowStatusInfo("Replaced " + matchCount + " matches.");
		}

		#endregion Replace All methods

		#region Other methods

		private void UpdateStatusLabel(string currentDocumentText, string pattern, RegexOptions options)
		{
			int currentDocumentMatchCount = Regex.Matches(currentDocumentText, pattern, options).Count;

			if (radioButton_Current.Checked)
				ShowStatusInfo(currentDocumentMatchCount + " matches found in the current document.");
			else if (radioButton_AllTabs.Checked)
				ShowStatusInfo(currentDocumentMatchCount + " matches found in the current document. "
					+ GetAllTabsMatchCount(pattern, options) + " in all tabs combined.");
		}

		private int GetAllTabsMatchCount(string pattern, RegexOptions options)
		{
			int matchCount = 0;

			foreach (TabPage tab in _targetTabControl.TabPages)
			{
				TextEditorBase textEditor = GetTextEditorOfTab(tab);

				if (textEditor != null)
					matchCount += Regex.Matches(textEditor.Text, pattern, options).Count;
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

		private void MoveCaretToDocumentStart(TextEditorBase documentTextEditor)
		{
			documentTextEditor.SelectionStart = 0;
			documentTextEditor.SelectionLength = 0;

			documentTextEditor.CaretOffset = 0;
		}

		private void MoveCaretToDocumentEnd(TextEditorBase documentTextEditor)
		{
			documentTextEditor.SelectionStart = 0;
			documentTextEditor.SelectionLength = 0;

			documentTextEditor.CaretOffset = documentTextEditor.Document.TextLength;
		}

		private TextEditorBase GetTextEditorOfTab(TabPage tab)
		{
			IEnumerable<ElementHost> elementHosts = tab.Controls.OfType<ElementHost>();
			return elementHosts.Count() > 0 ? elementHosts.First().Child as TextEditorBase : null;
		}

		private string GetCurrentPattern()
		{
			// Check if "Regular Expressions" mode is enabled
			string pattern = radioButton_Regex.Checked ? textBox_Find.Text : Regex.Escape(textBox_Find.Text);

			if (pattern.StartsWith("*"))
				return string.Empty;

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
			// Get a substring from the start of the document till the current SelectionStart index and use it to find the previous match.
			// Without such a substring, we would always end up in the last match occurrence of the entire document,
			// which means that we wouldn't move at all.
			=> documentText.Substring(0, selectionStartIndex);

		private string GetTextAfterSelection(string documentText, int selectionEndIndex)
			// Get a substring from the SelectionEnd index till the end of the current document and use it to find the next match.
			// Without such a substring, we would always end up in the first match occurrence of the entire document,
			// which means that we wouldn't move at all.
			=> documentText.Substring(selectionEndIndex);

		#endregion Other methods
	}
}
