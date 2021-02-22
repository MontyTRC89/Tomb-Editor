namespace TombIDE.ScriptingStudio.UI.DocumentModePresets
{
	internal static class ToolStripPresets
	{
		public static readonly UIElement[] None = null;

		public static readonly UIElement[] PlainText = new UIElement[]
		{
			UIElement.ToggleBookmark,
			UIElement.PrevBookmark,
			UIElement.NextBookmark,
			UIElement.ClearBookmarks
		};

		public static readonly UIElement[] ClassicScript = new UIElement[]
		{
			UIElement.CommentOut,
			UIElement.Uncomment,

			UIElement.ToggleBookmark,
			UIElement.PrevBookmark,
			UIElement.NextBookmark,
			UIElement.ClearBookmarks
		};

		public static readonly UIElement[] Lua = new UIElement[]
		{
			UIElement.CommentOut,
			UIElement.Uncomment,

			UIElement.ToggleBookmark,
			UIElement.PrevBookmark,
			UIElement.NextBookmark,
			UIElement.ClearBookmarks
		};

		public static readonly UIElement[] Strings = null;
	}
}
