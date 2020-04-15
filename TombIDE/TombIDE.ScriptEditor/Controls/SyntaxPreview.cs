using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TombIDE.ScriptEditor.Resources;
using TombIDE.Shared;

namespace TombIDE.ScriptEditor.Controls
{
	internal class SyntaxPreview : RichTextBox
	{
		public int CurrentArgumentIndex { get; set; }

		private Configuration _config;

		public SyntaxPreview()
		{
			_config = Configuration.Load();

			Font = new Font("Consolas", 9.75f, FontStyle.Bold);

			BackColor = Color.FromArgb(60, 63, 65);
			ForeColor = ColorTranslator.FromHtml(_config.ScriptColors_Value);

			BorderStyle = BorderStyle.None;
			ScrollBars = RichTextBoxScrollBars.None;

			ReadOnly = true;
			WordWrap = false;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);

			// Clear all styles
			SelectAll();
			SelectionColor = ColorTranslator.FromHtml(_config.ScriptColors_Value);

			HighlightSections();
			HighlightOldCommands();
			HighlightNewCommands();
			HighlightSomeMnemonics();
			HighlightCommandMnemonics();
			HighlightSpecialWordsAndSymbols();
			HighlightComments();

			UnderlineCurrentArgument();
		}

		private void UnderlineCurrentArgument()
		{
			if (string.IsNullOrWhiteSpace(Text) || !Text.Contains("="))
				return;

			string[] arguments = Text.Split(',');

			if (arguments.Length == 1)
				return;

			if (CurrentArgumentIndex >= arguments.Length)
				return;

			string currentArgument = Text.Split(',')[CurrentArgumentIndex];

			string token = string.Empty;

			if (CurrentArgumentIndex == 0)
				token = currentArgument.Split('=')[1].Trim();
			else
				token = currentArgument.Trim();

			Select(Text.IndexOf(token), token.Length);
			SelectionFont = new Font(SelectionFont.FontFamily, SelectionFont.Size, FontStyle.Underline | FontStyle.Bold);
		}

		private void HighlightSections()
		{
			string tokens = @"\[\b(" + string.Join("|", KeyWords.Sections) + @"|Any)\b\]";

			Regex regex = new Regex(tokens);
			MatchCollection collection = regex.Matches(Text);

			foreach (Match item in collection)
			{
				Select(item.Index, item.Length);
				SelectionColor = ColorTranslator.FromHtml(_config.ScriptColors_Section);
			}
		}

		private void HighlightOldCommands()
		{
			string tokens = "(" + string.Join("|", KeyWords.OldCommands) + @")\s*?=";

			Regex regex = new Regex(tokens);
			MatchCollection collection = regex.Matches(Text);

			foreach (Match item in collection)
			{
				Select(item.Index, item.Length);
				SelectionColor = ColorTranslator.FromHtml(_config.ScriptColors_OldCommand);
			}
		}

		private void HighlightNewCommands()
		{
			string tokens = "(" + string.Join("|", KeyWords.NewCommands) + @")\s*?=";

			Regex regex = new Regex(tokens);
			MatchCollection collection = regex.Matches(Text);

			foreach (Match item in collection)
			{
				Select(item.Index, item.Length);
				SelectionColor = ColorTranslator.FromHtml(_config.ScriptColors_NewCommand);
			}
		}

		private void HighlightSomeMnemonics()
		{
			string tokens = @"(ENABLED|DISABLED|#INCLUDE|#DEFINE|#FIRST_ID)";

			Regex regex = new Regex(tokens);
			MatchCollection collection = regex.Matches(Text);

			foreach (Match item in collection)
			{
				Select(item.Index, item.Length);
				SelectionColor = ColorTranslator.FromHtml(_config.ScriptColors_Reference);
			}
		}

		private void HighlightCommandMnemonics()
		{
			Regex regex = new Regex(@"\(.*?_\.*?\)");
			MatchCollection collection = regex.Matches(Text);

			foreach (Match item in collection)
			{
				Select(item.Index + 1, item.Length - 2);
				SelectionColor = ColorTranslator.FromHtml(_config.ScriptColors_Reference);
			}
		}

		private void HighlightSpecialWordsAndSymbols()
		{
			Regex regex = new Regex(@"(,|/|\(\*Array\*\))", RegexOptions.IgnoreCase);
			MatchCollection collection = regex.Matches(Text);

			foreach (Match item in collection)
			{
				Select(item.Index, item.Length);
				SelectionColor = Color.Gainsboro;
			}
		}

		private void HighlightComments()
		{
			Regex regex = new Regex(";.*$");
			MatchCollection collection = regex.Matches(Text);

			foreach (Match item in collection)
			{
				Select(item.Index, item.Length);
				SelectionColor = ColorTranslator.FromHtml(_config.ScriptColors_Comment);
			}
		}
	}
}
