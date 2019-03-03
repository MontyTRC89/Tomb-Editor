using FastColoredTextBoxNS;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ScriptEditor
{
	public partial class ScriptTextBox : FastColoredTextBox
	{
		private static Configuration _config = Configuration.Load();

		private TextStyle whitespaceColor = new TextStyle(new SolidBrush(Color.Gray), null, FontStyle.Regular);
		private TextStyle commentColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_Comment)), null, FontStyle.Regular);
		private TextStyle regularColor = new TextStyle(null, null, FontStyle.Regular);
		private TextStyle referenceColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_Reference)), null, FontStyle.Regular);
		private TextStyle valueColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_Value)), null, FontStyle.Regular);
		private TextStyle sectionColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_Section)), null, FontStyle.Bold);
		private TextStyle newCommandColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_NewCommand)), null, FontStyle.Regular);
		private TextStyle oldCommandColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_OldCommand)), null, FontStyle.Regular);
		private TextStyle unknownCommandColor = new TextStyle(new SolidBrush(ColorTranslator.FromHtml(_config.ScriptColors_UnknownCommand)), null, FontStyle.Bold);

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

			Dock = DockStyle.Fill;

			if (_config.Autocomplete)
				GenerateAutocompleteMenu();
		}

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

		protected override void OnTextChanged(TextChangedEventArgs e)
		{
			base.OnTextChanged(e);
			DoSyntaxHighlighting(e);
		}

		private void DoSyntaxHighlighting(TextChangedEventArgs e)
		{
			// Clear styles
			e.ChangedRange.ClearStyle(
					commentColor, regularColor, referenceColor, valueColor,
					sectionColor, newCommandColor, oldCommandColor, unknownCommandColor);

			// Apply styles (THE ORDER IS IMPORTANT!)
			e.ChangedRange.SetStyle(whitespaceColor, "·");
			e.ChangedRange.SetStyle(commentColor, @";.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(regularColor, @"[\[\],=]");
			e.ChangedRange.SetStyle(referenceColor, @"\$[a-fA-F0-9][a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?");
			e.ChangedRange.SetStyle(valueColor, @"=\s?.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(sectionColor, @"\[(" + string.Join("|", SyntaxKeyWords.Sections()) + @")\]");
			e.ChangedRange.SetStyle(newCommandColor, @"\b(" + string.Join("|", SyntaxKeyWords.NewCommands()) + @")[\s·]?=[\s·]?");
			e.ChangedRange.SetStyle(oldCommandColor, @"\b(" + string.Join("|", SyntaxKeyWords.OldCommands()) + @")[\s·]?=[\s·]?");
			e.ChangedRange.SetStyle(newCommandColor, @"\b(" + string.Join("|", SyntaxKeyWords.TR5MainCommands()) + @")[\s·]?=[\s·]?");
			e.ChangedRange.SetStyle(oldCommandColor, @"\b(" + string.Join("|", SyntaxKeyWords.TR5Commands()) + @")[\s·]?=[\s·]?");
			e.ChangedRange.SetStyle(unknownCommandColor, @"\b(" + string.Join("|", SyntaxKeyWords.Unknown()) + @")[\s·]?=[\s·]?");
		}
	}
}
