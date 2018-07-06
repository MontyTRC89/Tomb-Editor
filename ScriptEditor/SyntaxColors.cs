using FastColoredTextBoxNS;
using System.Drawing;

namespace ScriptEditor
{
	public class SyntaxColors
	{
		public static TextStyle Comments = new TextStyle(new SolidBrush(Properties.Settings.Default.CommentsColor), null, FontStyle.Regular);
		public static TextStyle Keys = new TextStyle(new SolidBrush(Properties.Settings.Default.KeyValueColor), null, FontStyle.Regular);
		public static TextStyle Headers = new TextStyle(new SolidBrush(Properties.Settings.Default.HeaderColor), null, FontStyle.Bold);
		public static TextStyle References = new TextStyle(new SolidBrush(Properties.Settings.Default.ReferencesColor), null, FontStyle.Regular);
		public static TextStyle Regular = new TextStyle(null, null, FontStyle.Regular);
		public static TextStyle Unknown = new TextStyle(new SolidBrush(Properties.Settings.Default.UnknownColor), null, FontStyle.Bold);
		public static TextStyle Values = new TextStyle(new SolidBrush(Properties.Settings.Default.ValuesColor), null, FontStyle.Regular);
	}
}
