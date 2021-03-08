using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.UI
{
	public class StudioToolStripItem
	{
		public string LangKey { get; set; }
		public string Command { get; set; }
		public string Icon { get; set; }
		public bool? CheckOnClick { get; set; }

		public string OverrideText { get; set; }

		public string Keys { get; set; }
		public string KeysDisplay { get; set; }

		public string Position { get; set; }

		public List<StudioToolStripItem> DropDownItems { get; set; } = new List<StudioToolStripItem>();
	}
}
