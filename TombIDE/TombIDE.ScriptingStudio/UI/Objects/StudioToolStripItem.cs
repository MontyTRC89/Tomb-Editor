using System.Collections.Generic;

namespace TombIDE.ScriptingStudio.UI
{
	public class StudioToolStripItem
	{
		public string LangKey { get; set; } = string.Empty;
		public string Command { get; set; } = string.Empty;
		public string Icon { get; set; } = string.Empty;
		public bool CheckOnClick { get; set; } = false;

		public string Keys { get; set; } = string.Empty;
		public string KeysDisplay { get; set; } = string.Empty;

		public string Position { get; set; } = string.Empty;

		public List<StudioToolStripItem> DropDownItems { get; set; } = new List<StudioToolStripItem>();
	}

	/// <summary>
	/// Wide ToolStrip button (icon + text)
	/// </summary>
	public sealed class StudioToolStripButton : StudioToolStripItem
	{
		public static StudioToolStripButton FromNormalItem(StudioToolStripItem item)
			=> new StudioToolStripButton
			{
				LangKey = item.LangKey,
				Command = item.Command,
				Icon = item.Icon,
				Keys = item.Keys,
				KeysDisplay = item.KeysDisplay,
				CheckOnClick = item.CheckOnClick,
				Position = item.Position,
				DropDownItems = item.DropDownItems
			};
	}

	public sealed class StudioSeparator : StudioToolStripItem
	{ }
}
