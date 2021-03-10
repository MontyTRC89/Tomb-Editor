using System.Windows.Forms;

namespace TombIDE.ScriptingStudio.UI
{
	internal struct UIKeys
	{
		public const Keys NewFile = Keys.Control | Keys.N;
		public const Keys Save = Keys.Control | Keys.S;
		public const Keys SaveAll = Keys.Control | Keys.Shift | Keys.S;
		public const Keys Build = Keys.F9;

		public const Keys Undo = Keys.Control | Keys.Z;
		public const Keys Redo = Keys.Control | Keys.Y;
		public const Keys Cut = Keys.Control | Keys.X;
		public const Keys Copy = Keys.Control | Keys.C;
		public const Keys Paste = Keys.Control | Keys.V;
		public const Keys Find = Keys.Control | Keys.F;
		public const Keys Replace = Keys.Control | Keys.H;
		public const Keys SelectAll = Keys.Control | Keys.A;

		public const Keys Reindent = Keys.Control | Keys.R;
		public const Keys TrimWhitespace = Keys.Control | Keys.Shift | Keys.R;
		public const Keys CommentOut = Keys.Control | Keys.Shift | Keys.C;
		public const Keys Uncomment = Keys.Control | Keys.Shift | Keys.U;
		public const Keys ToggleBookmark = Keys.Control | Keys.B;
		public const Keys PrevBookmark = Keys.Control | Keys.Oemcomma;
		public const Keys NextBookmark = Keys.Control | Keys.OemPeriod;
		public const Keys ClearBookmarks = Keys.Control | Keys.Shift | Keys.B;

		public const Keys PrevSection = Keys.Control | Keys.Left;
		public const Keys NextSection = Keys.Control | Keys.Right;
		public const Keys ClearString = Keys.Delete;
		public const Keys RemoveLastString = Keys.Control | Keys.Delete;

		public Keys GetKeys(string flag)
		{
			object value = GetType().GetField(flag)?.GetValue(this);
			return value != null ? (Keys)value : Keys.None;
		}
	}
}
