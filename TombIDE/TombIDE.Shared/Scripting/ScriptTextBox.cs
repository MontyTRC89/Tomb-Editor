using DarkUI.Forms;
using FastColoredTextBoxNS;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TombIDE.Shared.Scripting
{
	public partial class ScriptTextBox : FastColoredTextBox
	{
		public bool ShowToolTips { get; set; }

		/// <summary>
		/// The path of the currently modified file.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string FilePath { get; set; }

		private static readonly Configuration _config = Configuration.Load();

		private TextStyle commentColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_Comment)), null, FontStyle.Regular);
		private TextStyle regularColor = new TextStyle(null, null, FontStyle.Regular);
		private TextStyle referenceColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_Reference)), null, FontStyle.Regular);
		private TextStyle valueColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_Value)), null, FontStyle.Regular);
		private TextStyle sectionColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_Section)), null, FontStyle.Bold);
		private TextStyle newCommandColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_NewCommand)), null, FontStyle.Regular);
		private TextStyle oldCommandColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_OldCommand)), null, FontStyle.Regular);
		private TextStyle unknownCommandColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_UnknownCommand)), null, FontStyle.Bold);

		#region Constructors

		public ScriptTextBox()
		{
			AutoCompleteBracketsList = new char[] { '[', ']' };
			AutoIndent = false;
			AutoIndentChars = false;
			AutoIndentExistingLines = false;
			BackColor = Color.FromArgb(32, 32, 32);
			BookmarkColor = Color.FromArgb(64, 64, 64);
			CaretColor = Color.Gainsboro;
			ChangedLineColor = Color.FromArgb(64, 64, 96);
			CommentPrefix = ";";
			CurrentLineColor = Color.FromArgb(64, 64, 64);
			DelayedTextChangedInterval = 1;
			Font = new Font("Consolas", 12F);
			ForeColor = SystemColors.ControlLight;
			IndentBackColor = Color.FromArgb(48, 48, 48);
			LeftPadding = 5;
			LineNumberColor = Color.FromArgb(160, 160, 160);
			Margin = new Padding(0);
			Paddings = new Padding(0);
			ReservedCountOfLineNumberChars = 2;
			SelectionColor = Color.FromArgb(60, 30, 144, 255);
			ServiceLinesColor = Color.FromArgb(32, 32, 32);

			ToolTipNeeded += OnToolTipNeeded;

			Dock = DockStyle.Fill;

			if (_config.Autocomplete)
				GenerateAutocompleteMenu();
		}

		#endregion Constructors

		#region Public methods

		public void SaveCurrentFile()
		{
			File.WriteAllText(FilePath, Text, Encoding.GetEncoding(1252));
			IsChanged = false;

			string backupFilePath = FilePath + ".backup";

			if (File.Exists(backupFilePath))
				File.Delete(backupFilePath); // We don't need the backup file when the original file is saved

			Invalidate();
		}

		public bool IsContentChanged()
		{
			if (string.IsNullOrEmpty(FilePath))
				return true;

			string fileContent = File.ReadAllText(FilePath, Encoding.GetEncoding(1252));

			if (Text == fileContent) // If the editor content is identical to the file content
			{
				IsChanged = false;

				string backupFilePath = FilePath + ".backup";

				if (File.Exists(backupFilePath))
					File.Delete(backupFilePath); // We don't need the backup file when there are no changes made to the original one

				return false;
			}

			return true;
		}

		public void ToggleBookmark()
		{
			int currentLine = Selection.Start.iLine;

			if (Bookmarks.Contains(currentLine))
				Bookmarks.Remove(currentLine);
			else
				Bookmarks.Add(currentLine);
		}

		public void ClearAllBookmarks()
		{
			DialogResult result = DarkMessageBox.Show(this,
				"Are you sure you want delete all bookmarks?", "Delete all bookmarks?",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (result == DialogResult.Yes)
				Bookmarks.Clear();
		}

		public void TidyDocument(bool trimOnly = false)
		{
			// Cache
			int scrollPosition = VerticalScroll.Value;
			Bookmark[] cachedBookmarks = Bookmarks.ToArray();

			Bookmarks.Clear();

			// Tidy
			string tidiedText = trimOnly ? SyntaxTidying.Trim(Text) : SyntaxTidying.Reindent(Text);
			Text = tidiedText;

			// Restore
			foreach (Bookmark bookmark in cachedBookmarks)
				Bookmarks.Add(bookmark);

			VerticalScroll.Value = scrollPosition;
			UpdateScrollbars();
		}

		#endregion Public methods

		#region Public new methods

		public new void OpenFile(string filePath)
		{
			try
			{
				string fileContent = File.ReadAllText(filePath, Encoding.GetEncoding(1252));

				Text = fileContent;
				FilePath = filePath;

				IsChanged = false;
				ClearUndo();

				Invalidate();
			}
			catch (Exception ex)
			{
				DarkMessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

				string backupFilePath = filePath + ".backup";

				if (File.Exists(backupFilePath))
					File.Delete(backupFilePath); // The main file doesn't exist, so the backup file shouldn't exist either
			}
		}

		public new void Undo()
		{
			Bookmark[] cachedBookmarks = Bookmarks.ToArray(); // Cache
			Bookmarks.Clear();

			base.Undo();

			foreach (Bookmark bookmark in cachedBookmarks) // Restore
				Bookmarks.Add(bookmark);
		}

		public new void Redo()
		{
			Bookmark[] cachedBookmarks = Bookmarks.ToArray(); // Cache
			Bookmarks.Clear();

			base.Redo();

			foreach (Bookmark bookmark in cachedBookmarks) // Restore
				Bookmarks.Add(bookmark);
		}

		#endregion Public new methods

		#region Override methods

		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);

			DoSyntaxHighlighting(e);

			if (!string.IsNullOrEmpty(FilePath)) // If the currently modified file is not "Untitled"
			{
				// Create a live backup file so the app can restore lost progress if it crashes
				string backupFilePath = FilePath + ".backup";
				File.WriteAllText(backupFilePath, Text, Encoding.GetEncoding(1252));
			}
		}

		protected override void OnZoomChanged()
		{
			base.OnZoomChanged();

			// Limit zoom to a minumum of 25 and a maximum of 500
			if (Zoom < 25)
				Zoom = 25;
			else if (Zoom > 500)
				Zoom = 500;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Right && !Selection.Contains(PointToPlace(e.Location)))
				Selection.Start = PointToPlace(e.Location); // Move the caret to the right-clicked position
		}

		#endregion Override methods

		#region ToolTips

		private void OnToolTipNeeded(object sender, ToolTipNeededEventArgs e)
		{
			// Check if toolTips are enabled and if the hovered text is not just whitespace
			if (ShowToolTips && !string.IsNullOrWhiteSpace(e.HoveredWord))
			{
				if (GetLineText(e.Place.iLine).StartsWith("[")) // If the word is a section header
					ShowSectionToolTip(e);
				else
					ShowCommandToolTip(e);
			}
		}

		private void ShowSectionToolTip(ToolTipNeededEventArgs e)
		{
			// Get the resources from SectionToolTips.resx
			ResourceManager sectionToolTipResource = new ResourceManager(typeof(ToolTips.SectionToolTips));
			ResourceSet resourceSet = sectionToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			// Check if the hovered word exists in the file, if so, display the toolTip
			foreach (DictionaryEntry entry in resourceSet)
			{
				if (e.HoveredWord == entry.Key.ToString())
				{
					e.ToolTipTitle = "[" + e.HoveredWord + "]";
					e.ToolTipText = entry.Value.ToString();
					return;
				}
			}
		}

		private void ShowCommandToolTip(ToolTipNeededEventArgs e)
		{
			// Get the resources from CommandToolTips.resx
			ResourceManager commandToolTipResource = new ResourceManager(typeof(ToolTips.CommandToolTips));
			ResourceSet resourceSet = commandToolTipResource.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

			if (e.HoveredWord == "Level")
				HandleLevelToolTip(e); // There are different definitions for the "Level" command, so handle them all
			else
			{
				// Check if the hovered word exists in the file, if so, display the toolTip
				foreach (DictionaryEntry entry in resourceSet)
				{
					if (e.HoveredWord == entry.Key.ToString())
					{
						e.ToolTipTitle = e.HoveredWord;
						e.ToolTipText = entry.Value.ToString();
						return;
					}
				}
			}
		}

		private void HandleLevelToolTip(ToolTipNeededEventArgs e)
		{
			int i = e.Place.iLine; // Current line number

			do
			{
				if (i < 0)
					return; // The line number might go to -1 and it will crash the app, so stop the loop to prevent it

				if (GetLineText(i).StartsWith("[PSXExtensions]"))
				{
					e.ToolTipTitle = "Level [PSXExtensions]";
					e.ToolTipText = ToolTips.CommandToolTips.LevelPSX;
					return;
				}
				else if (GetLineText(i).StartsWith("[PCExtensions]"))
				{
					e.ToolTipTitle = "Level [PCExtensions]";
					e.ToolTipText = ToolTips.CommandToolTips.LevelPC;
					return;
				}
				else if (GetLineText(i).StartsWith("[Title]"))
				{
					e.ToolTipTitle = "Level [Title]";
					e.ToolTipText = ToolTips.CommandToolTips.LevelTitle;
					return;
				}
				else if (GetLineText(i).StartsWith("[Level]"))
				{
					e.ToolTipTitle = "Level";
					e.ToolTipText = ToolTips.CommandToolTips.LevelLevel;
					return;
				}

				i--; // Go 1 line higher if no section header was found yet
			}
			while (!GetLineText(i + 1).StartsWith("["));
		}

		#endregion ToolTips

		#region Other methods

		private void GenerateAutocompleteMenu()
		{
			AutocompleteMenu popupMenu = new AutocompleteMenu(this)
			{
				AllowTabKey = true,
				BackColor = Color.FromArgb(64, 73, 74),
				ForeColor = Color.Gainsboro,
				SearchPattern = @"[\w\.:=!<>\[\]]",
				SelectedColor = Color.SteelBlue
			};

			popupMenu.Items.SetAutocompleteItems(AutocompleteItems.GetItems());
		}

		private void DoSyntaxHighlighting(TextChangedEventArgs e)
		{
			// Clear styles
			e.ChangedRange.ClearStyle(
					commentColor, regularColor, referenceColor, valueColor,
					sectionColor, newCommandColor, oldCommandColor, unknownCommandColor);

			// Apply styles (THE ORDER IS IMPORTANT!)
			e.ChangedRange.SetStyle(commentColor, @";.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(regularColor, @"[\[\],=]");
			e.ChangedRange.SetStyle(referenceColor, @"\$[a-fA-F0-9][a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?");
			e.ChangedRange.SetStyle(valueColor, @"=\s?.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(sectionColor, @"\[(" + string.Join("|", KeyWords.Sections) + @")\]");
			e.ChangedRange.SetStyle(newCommandColor, @"\b(" + string.Join("|", KeyWords.NewCommands) + @")\s?=\s?");
			e.ChangedRange.SetStyle(oldCommandColor, @"\b(" + string.Join("|", KeyWords.OldCommands) + @")\s?=\s?");
			e.ChangedRange.SetStyle(newCommandColor, @"\b(" + string.Join("|", KeyWords.TR5MainCommands) + @")\s?=\s?");
			e.ChangedRange.SetStyle(oldCommandColor, @"\b(" + string.Join("|", KeyWords.TR5Commands) + @")\s?=\s?");
			e.ChangedRange.SetStyle(unknownCommandColor, @"\b(" + string.Join("|", KeyWords.Unknown) + @")\s?=\s?");
		}

		#endregion Other methods
	}
}
