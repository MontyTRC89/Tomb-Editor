using FastColoredTextBoxNS;
using System.Drawing;

namespace ScriptEditor
{
	public class SyntaxColors
	{
		public static TextStyle Comments = new TextStyle(new SolidBrush(Properties.Settings.Default.CommentColor), null, FontStyle.Regular);
		public static TextStyle NewCommands = new TextStyle(new SolidBrush(Properties.Settings.Default.NewCommandColor), null, FontStyle.Regular);
		public static TextStyle OldCommands = new TextStyle(new SolidBrush(Properties.Settings.Default.OldCommandColor), null, FontStyle.Regular);
		public static TextStyle Headers = new TextStyle(new SolidBrush(Properties.Settings.Default.HeaderColor), null, FontStyle.Bold);
		public static TextStyle References = new TextStyle(new SolidBrush(Properties.Settings.Default.ReferenceColor), null, FontStyle.Regular);
		public static TextStyle Regular = new TextStyle(null, null, FontStyle.Regular);
		public static TextStyle Unknown = new TextStyle(new SolidBrush(Properties.Settings.Default.UnknownColor), null, FontStyle.Bold);
		public static TextStyle Values = new TextStyle(new SolidBrush(Properties.Settings.Default.ValueColor), null, FontStyle.Regular);
	}
}
