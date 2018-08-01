using FastColoredTextBoxNS;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ScriptEditor
{
	public class SyntaxHighlighting
	{
		private static TextStyle whitespaceColor = new TextStyle(new SolidBrush(Color.Gray), null, FontStyle.Regular);
		private static TextStyle commentColor = new TextStyle(new SolidBrush(Properties.Settings.Default.CommentColor), null, FontStyle.Regular);
		private static TextStyle regularColor = new TextStyle(null, null, FontStyle.Regular);
		private static TextStyle referenceColor = new TextStyle(new SolidBrush(Properties.Settings.Default.ReferenceColor), null, FontStyle.Regular);
		private static TextStyle valueColor = new TextStyle(new SolidBrush(Properties.Settings.Default.ValueColor), null, FontStyle.Regular);
		private static TextStyle headerColor = new TextStyle(new SolidBrush(Properties.Settings.Default.HeaderColor), null, FontStyle.Bold);
		private static TextStyle newCommandColor = new TextStyle(new SolidBrush(Properties.Settings.Default.NewCommandColor), null, FontStyle.Regular);
		private static TextStyle oldCommandColor = new TextStyle(new SolidBrush(Properties.Settings.Default.OldCommandColor), null, FontStyle.Regular);
		private static TextStyle unknownColor = new TextStyle(new SolidBrush(Properties.Settings.Default.UnknownColor), null, FontStyle.Bold);

		public static void DoSyntaxHighlighting(TextChangedEventArgs e)
		{
			// Clear styles
			e.ChangedRange.ClearStyle(
					commentColor, regularColor, referenceColor, valueColor,
					headerColor, newCommandColor, oldCommandColor, unknownColor);

			// Apply styles (THE ORDER IS IMPORTANT!)
			e.ChangedRange.SetStyle(whitespaceColor, "·");
			e.ChangedRange.SetStyle(commentColor, @";.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(regularColor, @"[\[\],=]");
			e.ChangedRange.SetStyle(referenceColor, @"\$[a-fA-F0-9][a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?[a-fA-F0-9]?");
			e.ChangedRange.SetStyle(valueColor, @"=\s?.*$", RegexOptions.Multiline);
			e.ChangedRange.SetStyle(headerColor, @"\[(" + string.Join("|", SyntaxKeyWords.Headers()) + @")\]");
			e.ChangedRange.SetStyle(newCommandColor, @"\b(" + string.Join("|", SyntaxKeyWords.NewCommands()) + @")[\s·]?=[\s·]?");
			e.ChangedRange.SetStyle(oldCommandColor, @"\b(" + string.Join("|", SyntaxKeyWords.OldCommands()) + @")[\s·]?=[\s·]?");
			e.ChangedRange.SetStyle(unknownColor, @"\b(" + string.Join("|", SyntaxKeyWords.Unknown()) + @")[\s·]?=[\s·]?");
		}
	}
}
