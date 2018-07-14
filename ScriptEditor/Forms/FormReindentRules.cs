using DarkUI.Forms;
using FastColoredTextBoxNS;
using System;
using System.Text.RegularExpressions;

namespace ScriptEditor
{
	public partial class FormReindentRules : DarkForm
	{
		public FormReindentRules()
		{
			InitializeComponent();

			preEqualSpaceCheck.Checked = Properties.Settings.Default.PreEqualSpace;
			postEqualSpaceCheck.Checked = Properties.Settings.Default.PostEqualSpace;

			preCommaSpaceCheck.Checked = Properties.Settings.Default.PreCommaSpace;
			postCommaSpaceCheck.Checked = Properties.Settings.Default.PostCommaSpace;

			reduceSpacesCheck.Checked = Properties.Settings.Default.ReduceSpaces;
		}

		private void preview_TextChanged(object sender, TextChangedEventArgs e)
		{
			// Clear styles
			e.ChangedRange.ClearStyle(
				SyntaxColors.Comments, SyntaxColors.Regular, SyntaxColors.References, SyntaxColors.Values,
				SyntaxColors.Headers, SyntaxColors.NewCommands, SyntaxColors.OldCommands);

			// Apply styles (THE ORDER IS IMPORTANT!)
			e.ChangedRange.SetStyle(SyntaxColors.Comments, @";.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(SyntaxColors.Regular, @"[\[\],=]");
			e.ChangedRange.SetStyle(SyntaxColors.References, @"\$[a-fA-F0-9][a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?");
			e.ChangedRange.SetStyle(SyntaxColors.Values, @"=\s?.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(SyntaxColors.Headers, @"\[(" + string.Join("|", SyntaxKeyWords.Headers()) + @")\]");
			e.ChangedRange.SetStyle(SyntaxColors.NewCommands, @"\b(" + string.Join("|", SyntaxKeyWords.NewCommands()) + @")[\s·]?=[\s·]?");
			e.ChangedRange.SetStyle(SyntaxColors.OldCommands, @"\b(" + string.Join("|", SyntaxKeyWords.OldCommands()) + @")[\s·]?=[\s·]?");
		}

		private void defaultButton_Click(object sender, EventArgs e)
		{
			preEqualSpaceCheck.Checked = true;
			postEqualSpaceCheck.Checked = true;

			preCommaSpaceCheck.Checked = false;
			postCommaSpaceCheck.Checked = true;

			reduceSpacesCheck.Checked = true;
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			Properties.Settings.Default.PreEqualSpace = preEqualSpaceCheck.Checked;
			Properties.Settings.Default.PostEqualSpace = postEqualSpaceCheck.Checked;

			Properties.Settings.Default.PreCommaSpace = preCommaSpaceCheck.Checked;
			Properties.Settings.Default.PostCommaSpace = postCommaSpaceCheck.Checked;

			Properties.Settings.Default.ReduceSpaces = reduceSpacesCheck.Checked;

			Properties.Settings.Default.Save();
		}

		private void preCommaSpaceCheck_CheckedChanged(object sender, EventArgs e)
		{
			preview.Text = preCommaSpaceCheck.Checked ? preview.Text.Replace(",", " ,") : preview.Text.Replace(" ,", ",");
		}

		private void postCommaSpaceCheck_CheckedChanged(object sender, EventArgs e)
		{
			preview.Text = postCommaSpaceCheck.Checked ? preview.Text.Replace(",", ", ") : preview.Text.Replace(", ", ",");
		}

		private void preEqualSpaceCheck_CheckedChanged(object sender, EventArgs e)
		{
			preview.Text = preEqualSpaceCheck.Checked ? preview.Text.Replace("=", " =") : preview.Text.Replace(" =", "=");
		}

		private void postEqualSpaceCheck_CheckedChanged(object sender, EventArgs e)
		{
			preview.Text = postEqualSpaceCheck.Checked ? preview.Text.Replace("=", "= ") : preview.Text.Replace("= ", "=");
		}

		private void reduceSpacesCheck_CheckedChanged(object sender, EventArgs e)
		{
			preview.Text = reduceSpacesCheck.Checked ? preview.Text.Replace("  ;", ";") : preview.Text.Replace(";", "  ;");
		}
	}
}
