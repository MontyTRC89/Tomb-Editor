namespace TombIDE.ScriptingStudio.UI.DocumentModePresets
{
	internal static class MenuStripPresets
	{
		public static readonly UIElement[] None = null;

		public static readonly UIElement[] PlainText = new UIElement[]
		{
			UIElement.TrimWhiteSpace,

			UIElement.ToggleBookmark,
			UIElement.PrevBookmark,
			UIElement.NextBookmark,
			UIElement.ClearBookmarks
		};

		public static readonly UIElement[] ClassicScript = new UIElement[]
		{
			UIElement.Reindent,
			UIElement.TrimWhiteSpace,

			UIElement.CommentOut,
			UIElement.Uncomment,

			UIElement.ToggleBookmark,
			UIElement.PrevBookmark,
			UIElement.NextBookmark,
			UIElement.ClearBookmarks
		};

		public static readonly UIElement[] Lua = new UIElement[]
		{
			UIElement.Reindent,
			UIElement.TrimWhiteSpace,

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
