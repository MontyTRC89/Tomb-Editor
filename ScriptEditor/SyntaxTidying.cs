using FastColoredTextBoxNS;
using System;
using System.Collections.Generic;

namespace ScriptEditor
{
	public class SyntaxTidying
	{
		public static void TidyScript(FastColoredTextBox textEditor, bool trimOnly = false)
		{
			// Start AutoUndo to allow the user to undo the tidying process using only a single stack
			textEditor.BeginAutoUndo();

			// Save set bookmarks and remove them from the editor to prevent a bug
			BaseBookmarks bookmarkedLines = textEditor.Bookmarks;
			textEditor.Bookmarks.Clear();

			// Store current scroll position
			int scrollPosition = textEditor.VerticalScroll.Value;

			// Store the editor content in a string and replace the "whitespace dots" (if there are any) with whitespace
			string editorContent = textEditor.Text.Replace("·", " ");

			// Setup a list to store all tidied lines
			List<string> tidiedlines = trimOnly ? TrimLines(editorContent) : ReindentLines(editorContent);

			// Scan all lines
			for (int i = 0; i < textEditor.LinesCount; i++)
			{
				// Also check if the user has "Show Spaces" enabled
				string currentTidiedLine = Properties.Settings.Default.ShowSpaces ? tidiedlines[i].Replace(" ", "·") : tidiedlines[i];

				// If a line has changed
				if (textEditor.GetLineText(i) != currentTidiedLine)
				{
					textEditor.Selection = new Range(textEditor, 0, i, textEditor.GetLineText(i).Length, i);
					textEditor.InsertText(tidiedlines[i]);
				}
			}

			// Go to the last scroll position
			textEditor.VerticalScroll.Value = scrollPosition;
			textEditor.UpdateScrollbars();

			// Add lost bookmarks
			textEditor.Bookmarks = bookmarkedLines;

			// End AutoUndo to stop recording the actions and put them into a single stack
			textEditor.EndAutoUndo();
		}

		private static List<string> ReindentLines(string editorContent)
		{
			editorContent = HandleSpacesBeforeEquals(editorContent);
			editorContent = HandleSpacesAfterEquals(editorContent);

			editorContent = HandleSpacesBeforeCommas(editorContent);
			editorContent = HandleSpacesAfterCommas(editorContent);

			editorContent = HandleSpaceReduction(editorContent);
			return TrimLines(editorContent);
		}

		private static string HandleSpacesBeforeEquals(string editorContent)
		{
			if (Properties.Settings.Default.PreEqualSpace)
			{
				editorContent = editorContent.Replace("=", " =");

				while (editorContent.Contains("  ="))
				{
					editorContent = editorContent.Replace("  =", " =");
				}
			}
			else
			{
				while (editorContent.Contains(" ="))
				{
					editorContent = editorContent.Replace(" =", "=");
				}
			}

			return editorContent;
		}

		private static string HandleSpacesAfterEquals(string editorContent)
		{
			if (Properties.Settings.Default.PostEqualSpace)
			{
				editorContent = editorContent.Replace("=", "= ");

				while (editorContent.Contains("=  "))
				{
					editorContent = editorContent.Replace("=  ", "= ");
				}
			}
			else
			{
				while (editorContent.Contains("= "))
				{
					editorContent = editorContent.Replace("= ", "=");
				}
			}

			return editorContent;
		}

		private static string HandleSpacesBeforeCommas(string editorContent)
		{
			if (Properties.Settings.Default.PreCommaSpace)
			{
				editorContent = editorContent.Replace(",", " ,");

				while (editorContent.Contains("  ,"))
				{
					editorContent = editorContent.Replace("  ,", " ,");
				}
			}
			else
			{
				while (editorContent.Contains(" ,"))
				{
					editorContent = editorContent.Replace(" ,", ",");
				}
			}

			return editorContent;
		}

		private static string HandleSpacesAfterCommas(string editorContent)
		{
			if (Properties.Settings.Default.PostCommaSpace)
			{
				editorContent = editorContent.Replace(",", ", ");

				while (editorContent.Contains(",  "))
				{
					editorContent = editorContent.Replace(",  ", ", ");
				}
			}
			else
			{
				while (editorContent.Contains(", "))
				{
					editorContent = editorContent.Replace(", ", ",");
				}
			}

			return editorContent;
		}

		private static string HandleSpaceReduction(string editorContent)
		{
			if (Properties.Settings.Default.ReduceSpaces)
			{
				while (editorContent.Contains("  "))
				{
					editorContent = editorContent.Replace("  ", " ");
				}
			}

			return editorContent;
		}

		private static List<string> TrimLines(string editorContent)
		{
			// Get every single line and create a list that will store them
			string[] lines = editorContent.Replace("\r", "").Split('\n');
			List<string> trimmedText = new List<string>();

			// Trim whitespace on every line and add it to the list
			for (int i = 0; i < lines.Length; i++)
			{
				string currentLineText = (lines.Length >= i) ? lines[i] : Environment.NewLine;
				trimmedText.Add(currentLineText.Trim());
			}

			return trimmedText;
		}
	}
}
